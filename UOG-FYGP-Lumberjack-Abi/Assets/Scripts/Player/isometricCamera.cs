using UnityEngine;

public class isometricCamera : MonoBehaviour
{
    [Header("Pan settings")]
    public float panSpeed = 0.5f;
    [Header("Zoom settings")]
    public float zoomSpeed = 0.1f;
    public float zoomSmoothness = 5f;
    public float minZoom = 2f;
    public float maxZoom = 20f;
    public float rotationSpeed = 100f;
    [Header("Map settings")]
    public GameObject mapObject;
    public float mapWidth;
    public float mapHeight;
    private Camera _camera;
    private Vector2 _lastPanPosition;
    private int _panFingerId;
    private bool _isPanning;
    private float _currentZoom;
    private void Awake()
    {
        _camera = GetComponentInChildren<Camera>();
        _currentZoom = _camera.orthographicSize;

        if (mapObject != null) CalculateMapSize();
        RecalculateMaxZoom();
    }

    private void CalculateMapSize()
    {
        MeshRenderer renderer = mapObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            mapWidth = renderer.bounds.size.x;
            mapHeight = renderer.bounds.size.z;
        }
    }
    private void RecalculateMaxZoom()
    {
        float maxByHeight = mapHeight / 2f;
        float maxByWidth = (mapWidth / 2f) / _camera.aspect;
        maxZoom = Mathf.Min(maxByHeight, maxByWidth);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HandleEditorInput();
#else
            HandleTouchInput();
#endif
            ApplyZoom();
            ClampPosition();
        }
    }
    private void HandleEditorInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(h, 0, v) * panSpeed, Space.World);
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f) _currentZoom -= scroll * zoomSpeed * 100f * Time.deltaTime;
        if (Input.GetMouseButton(1))
        {
            float mouseDeltaX = Input.GetAxis("Mouse X");
            transform.Rotate(Vector3.up, mouseDeltaX * rotationSpeed * Time.deltaTime, Space.World);
        }
    }
    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                _lastPanPosition = touch.position;
                _panFingerId = touch.fingerId;
                _isPanning = true;
            }
            else if (touch.fingerId == _panFingerId && touch.phase == TouchPhase.Moved && _isPanning)
            {
                Vector2 touchDelta = touch.position - _lastPanPosition;
                Vector3 move = new Vector3(
                    -touchDelta.x * panSpeed * Time.deltaTime,
                    0,
                    -touchDelta.y * panSpeed * Time.deltaTime
                );

                transform.Translate(move, Space.World);
                _lastPanPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                _isPanning = false;
            }
        }
        else if (Input.touchCount == 2) // PINCH ZOOM + ROTATE
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            float prevDist = (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
            float currDist = (t0.position - t1.position).magnitude;
            float delta = prevDist - currDist;
            _currentZoom += delta * zoomSpeed * Time.deltaTime;
            Vector2 prevDir = (t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition);
            Vector2 currDir = t0.position - t1.position;
            float angle = Vector2.SignedAngle(prevDir, currDir);
            transform.Rotate(Vector3.up, angle * rotationSpeed * Time.deltaTime, Space.World);
        }
    }
    private void ApplyZoom()
    {
        _currentZoom = Mathf.Clamp(_currentZoom, minZoom, maxZoom);

        _camera.orthographicSize = Mathf.Lerp(
            _camera.orthographicSize,
            _currentZoom,
            Time.deltaTime * zoomSmoothness
        );
    }
    private void ClampPosition()
    {
        float vertExtent = _camera.orthographicSize;
        float horzExtent = vertExtent * _camera.aspect;

        if (mapWidth < horzExtent * 2f || mapHeight < vertExtent * 2f)
            return;

        float minX = -mapWidth / 2f + horzExtent;
        float maxX = mapWidth / 2f - horzExtent;
        float minZ = -mapHeight / 2f + vertExtent;
        float maxZ = mapHeight / 2f - vertExtent;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }
}
