using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 8;
    public int height = 8;
    public GameObject cellPrefab;
    public RectTransform gridParent;
    public Blueprint blueprint;

    void Start()
    {
        Generate();
    }

    char GetCharAt(int x, int y)
    {
        if (blueprint == null) return '.';

        var lines = blueprint.asciiLayout.Replace("\r", "").Split('\n');
        if (y < 0 || y >= lines.Length) return '.';
        var line = lines[y];
        if (x < 0 || x >= line.Length) return '.';
        return line[x];
    }

    public void Generate()
    {
        // Clear old cells
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);

        // Create grid
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cellGO = Instantiate(cellPrefab, gridParent);
                cellGO.name = $"Cell_{x}_{y}";

                var cell = cellGO.AddComponent<GridCell>();
                cell.x = x;
                cell.y = y;

                // Apply blueprint color
                var img = cellGO.GetComponent<Image>();
                if (img != null)
                {
                    bool isNeeded = (GetCharAt(x, y) == 'X');
                    img.color = isNeeded
                        ? new Color(1f, 0.95f, 0.8f)
                        : new Color(0.9f, 0.6f, 0.2f);
                }

                // Add button click
                var btn = cellGO.GetComponent<Button>();
                if (btn != null)
                {
                    GridCell localCell = cell;
                    int cx = x;
                    int cy = y;

                    btn.onClick.AddListener(() =>
                    {
                        localCell.Flash(new Color(0.8f, 1f, 0.8f));
                        Debug.Log($"Clicked cell {cx},{cy}");
                    });
                }
            }
        }
    }
}
