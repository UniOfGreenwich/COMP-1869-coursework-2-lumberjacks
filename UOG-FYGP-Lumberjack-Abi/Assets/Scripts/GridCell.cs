using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    public int x;
    public int y;
    Image _img;
    private Color _originalColor;
    private bool _isFlashing;
    private float _fadeSpeed=3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _img = GetComponent<Image>();
        _originalColor = _img.color;
    }
    public void Flash(Color flashColor)
    {
        _img.color = flashColor;
        _isFlashing = true;

    }

    // Update is called once per frame
    void Update()
    {
        if (_isFlashing)
        {
            _img.color = Color.Lerp(_img.color, _originalColor, Time.deltaTime * _fadeSpeed);
        }
        if (Vector4.Distance(_img.color, _originalColor) < 0.02f)
        {
            _img.color=_originalColor;
            _isFlashing = false;
        }
        
    }
}
