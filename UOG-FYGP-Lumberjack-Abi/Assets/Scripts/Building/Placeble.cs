using UnityEngine;

public class Placeble : MonoBehaviour
{
    public bool placed { get; private set; }
    public Vector3Int Size { get; private set; }
    public Vector3[] Vertices { get; private set; }

    private Vector3[] localBottomCorners;
    private MeshRenderer renderer;
    private Material originalMaterial;
    private Material ghostMaterial;

    private void Awake()
    {
        // Cache MeshRenderer + Materials
        renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
            ghostMaterial = new Material(originalMaterial); // clone for tinting
            renderer.material = ghostMaterial;
        }
        else
        {
            Debug.LogError("[Placeble] No MeshRenderer found on " + gameObject.name);
        }

        // Cache collider corners
        CacheBottomCorners();
    }
    public void Load()
    {
        placed = true;                     // mark as permanently placed
        RestoreOriginalMaterial();         // use the normal material
        Destroy(GetComponent<ObjectDrag>()); // remove dragging if present
    }
    private void Start()
    {
        CalculateSize();
    }
    public void Rotate()
    {
        Debug.Log("Rotating: " + gameObject.name);
        transform.Rotate(Vector3.up, 90f, Space.World); // global Y rotation
        CalculateSize();
    }

    private void Update()
    {
        if (!placed)
        {
            bool canPlace = BuildingSystem.instance.CanBePlaced(this);
            SetGhostColor(canPlace ? Color.green : Color.red);
        }
    }
    private void CacheBottomCorners()
    {
        BoxCollider b = GetComponent<BoxCollider>();
        if (b == null)
        {
            Debug.LogError("[Placeble] No BoxCollider found on " + gameObject.name);
            Size = new Vector3Int(1, 1, 1);
            return;
        }

        float hx = b.size.x * 0.5f;
        float hz = b.size.z * 0.5f;
        float y = b.center.y - b.size.y * 0.5f;

        localBottomCorners = new Vector3[4];
        localBottomCorners[0] = new Vector3(b.center.x - hx, y, b.center.z - hz);
        localBottomCorners[1] = new Vector3(b.center.x + hx, y, b.center.z - hz);
        localBottomCorners[2] = new Vector3(b.center.x + hx, y, b.center.z + hz);
        localBottomCorners[3] = new Vector3(b.center.x - hx, y, b.center.z + hz);
    }
    private void CalculateSize()
    {
        if (localBottomCorners == null || localBottomCorners.Length == 0)
        {
            Size = new Vector3Int(1, 1, 1);
            return;
        }

        var grid = BuildingSystem.instance.gridLayout;
        Vector3Int[] cells = new Vector3Int[localBottomCorners.Length];

        for (int i = 0; i < localBottomCorners.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(localBottomCorners[i]);
            cells[i] = grid.WorldToCell(worldPos);
        }

        int minX = cells[0].x, maxX = cells[0].x;
        int minY = cells[0].y, maxY = cells[0].y;

        for (int i = 1; i < cells.Length; i++)
        {
            if (cells[i].x < minX) minX = cells[i].x;
            if (cells[i].x > maxX) maxX = cells[i].x;
            if (cells[i].y < minY) minY = cells[i].y;
            if (cells[i].y > maxY) maxY = cells[i].y;
        }

        Size = new Vector3Int(maxX - minX + 1, maxY - minY + 1, 1);
    }
    public Vector3 GetStartPosition()
    {
        var grid = BuildingSystem.instance.gridLayout;
        Vector3Int minCell = grid.WorldToCell(transform.TransformPoint(localBottomCorners[0]));

        for (int i = 1; i < localBottomCorners.Length; i++)
        {
            var c = grid.WorldToCell(transform.TransformPoint(localBottomCorners[i]));
            if (c.x < minCell.x) minCell.x = c.x;
            if (c.y < minCell.y) minCell.y = c.y;
        }

        return grid.CellToWorld(new Vector3Int(minCell.x, minCell.y, 0));
    }

    public void Place()
    {
        placed = true;
        RestoreOriginalMaterial();
    }
    private void SetGhostColor(Color color)
    {
        if (ghostMaterial != null && !placed)
        {
            color.a = 0.6f; // semi-transparent ghost
            ghostMaterial.color = color;
        }
    }
    private void RestoreOriginalMaterial()
    {
        if (renderer != null && originalMaterial != null)
        {
            renderer.material = originalMaterial;
        }
    }
}
