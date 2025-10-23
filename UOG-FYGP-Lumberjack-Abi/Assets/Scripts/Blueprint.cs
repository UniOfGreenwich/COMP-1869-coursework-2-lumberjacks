using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Blueprint", menuName = "Scriptable Objects/Blueprint")]
public class Blueprint : ScriptableObject
{
    public string blueprintName = "chair";
    public int width = 8;
    public int height = 8;
    [TextArea(5, 20)]
    public string asciiLayout =
@"........
..XXXX..
..X..X..
..XXXX..
..X..X..
..X..X..
........
........"; 
}
