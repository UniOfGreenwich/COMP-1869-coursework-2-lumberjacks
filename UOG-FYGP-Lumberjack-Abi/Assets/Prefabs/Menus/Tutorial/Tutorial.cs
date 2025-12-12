using TMPro;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    private TMP_Text tutorialText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tutorialText = GetComponentInChildren<TMP_Text>();
        Stage1();
    }

    void Stage1()
    {
        tutorialText.text = "Welcome to LumberJill's Carpenter Shop! Explore the workshop by clicking around, to go to another room walk over the yellow boxes on the floor.";
        // on click hide text, enable task panel
        // once each room has been visited, show final message
        // activate stage 2

    }

    //("Welcome to LumberJill's Carpenter Shop! Explore the workshop by clicking around, to go to another room walk over the yellow boxes on the floor.")
}
