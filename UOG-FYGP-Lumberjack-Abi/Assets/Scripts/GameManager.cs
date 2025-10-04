using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager current;
    public Canvas canvas;

    private void Awake()
    {
        current = this;
    }
}
