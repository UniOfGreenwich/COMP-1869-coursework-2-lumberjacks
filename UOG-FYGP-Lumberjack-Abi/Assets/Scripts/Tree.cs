using UnityEngine;

public class Tree : MonoBehaviour
{
    public int growthState { get; private set; } //0 = sapling, 1 = young tree, 2 = mature tree
    public int hitsToChop { get; private set; }
    public int currentHits { get; private set; }
    public int lumberYield { get; private set; }
    private bool choppable = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
