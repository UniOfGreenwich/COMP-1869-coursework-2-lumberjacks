#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SquarePieceGenerator
{
    [MenuItem("FGYP/Generate Square Pieces (1x1..8x8)")]
    public static void Generate()
    {
        string folder = EditorUtility.OpenFolderPanel("Create items in...", "Assets", "");
        if (string.IsNullOrEmpty(folder)) return;
        if (!folder.StartsWith(Application.dataPath))
        { EditorUtility.DisplayDialog("Pick an Assets/ folder", "Choose a folder under Assets/.", "OK"); return; }

        string assetRoot = "Assets" + folder.Substring(Application.dataPath.Length);

        for (int w = 1; w <= 8; w++)
            for (int h = 1; h <= 8; h++)
            {
                var it = ScriptableObject.CreateInstance<ItemSO>();
                it.id = $"square_{w}x{h}".ToLower();    // used later to read W×H
                it.displayName = $"Square {w}x{h}";
                it.category = ItemCategory.Utility;
                it.maxStack = 20;

                string path = $"{assetRoot}/Square_{w}x{h}.asset";
                AssetDatabase.CreateAsset(it, path);
            }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done", "Created 1x1..8x8 square items.", "Nice");
    }
}
#endif
