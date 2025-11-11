#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

// editor for squarecutter
[CustomEditor(typeof(SquareCutter))]
public class SquareCutterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(8);

        if (GUILayout.Button("Auto-Fill Recipes 1×1..N×N"))
        {
            var sc = (SquareCutter)target;
            Undo.RecordObject(sc, "Auto-Fill Recipes");
            sc.EditorAutoGenerateRecipes();
            EditorUtility.SetDirty(sc);
            Debug.Log("[SquareCutterEditor] Recipes rebuilt.");
        }

        if (GUILayout.Button("Create ItemSOs For Recipes..."))
        {
            var sc = (SquareCutter)target;
            CreateItemsForRecipes(sc, onlyIfMissing: false);
        }

        if (GUILayout.Button("Build Recipes + Create ItemSOs..."))
        {
            var sc = (SquareCutter)target;
            Undo.RecordObject(sc, "Build + Create");
            sc.EditorAutoGenerateRecipes();
            EditorUtility.SetDirty(sc);
            CreateItemsForRecipes(sc, onlyIfMissing: false);
        }
    }

    // create items helper
    private void CreateItemsForRecipes(SquareCutter sc, bool onlyIfMissing)
    {
        // pick target folder
        string abs = EditorUtility.OpenFolderPanel("Choose Items Folder", Application.dataPath, "");
        if (string.IsNullOrEmpty(abs)) return;

        // validate project path
        string data = Application.dataPath.Replace("\\", "/");
        abs = abs.Replace("\\", "/");
        if (!abs.StartsWith(data))
        {
            EditorUtility.DisplayDialog("Invalid Folder", "Pick a folder under Assets/", "OK");
            return;
        }

        // convert relative path
        string rel = "Assets" + abs.Substring(data.Length);

        // choose base icon
        Sprite baseIcon = sc.defaultSquareItem ? sc.defaultSquareItem.icon : null;

        // iterate recipes list
        var list = sc.recipes;
        for (int i = 0; i < list.Count; i++)
        {
            var r = list[i];

            // normalize dims pair
            int a = Mathf.Min(r.width, r.height);
            int b = Mathf.Max(r.width, r.height);

            // compute asset paths
            string file = $"Square_{a}x{b}.asset";
            string path = Path.Combine(rel, file).Replace("\\", "/");

            // load existing asset
            var existing = AssetDatabase.LoadAssetAtPath<ItemSO>(path);

            // skip if required
            if (onlyIfMissing && existing != null)
            {
                r.outputItem = existing;
                list[i] = r;
                continue;
            }

            // create new asset
            ItemSO item = existing;
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemSO>(); // create new asset
                item.id = $"square_{a}x{b}";                      // set unique id
                item.displayName = $"Square {a}×{b}";             // set display name
                item.category = ItemCategory.Utility;             // set item category
                item.icon = baseIcon;                             // set fallback icon

                AssetDatabase.CreateAsset(item, path);            // create asset file
                AssetDatabase.SaveAssets();                       // flush database
            }

            // assign to recipe
            r.outputItem = item;
            list[i] = r;
        }

        // record changes now
        Undo.RecordObject(sc, "Assign Recipe Items");
        sc.recipes = list;
        EditorUtility.SetDirty(sc);

        AssetDatabase.SaveAssets(); // save assets now
        AssetDatabase.Refresh();    // refresh database now

        Debug.Log($"[SquareCutterEditor] Wired {list.Count} recipe outputs.");
    }
}
#endif
