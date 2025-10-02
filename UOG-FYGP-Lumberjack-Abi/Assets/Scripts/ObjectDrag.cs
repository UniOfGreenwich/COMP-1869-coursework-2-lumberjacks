using UnityEngine;

public class ObjectDrag : MonoBehaviour
{
    private Vector3 offSet;
    private void OnMouseDown()
    {
        offSet=transform.position-BuildingSystem.GetMouseWorldPosition();
    }
    private void OnMouseDrag()
    {
        Vector3 pos = BuildingSystem.GetMouseWorldPosition() + offSet;
        transform.position = BuildingSystem.instance.SnapCoordinateToGrid(pos);
    }
}
