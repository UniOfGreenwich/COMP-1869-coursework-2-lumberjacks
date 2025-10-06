using UnityEngine;
using System.Collections;

public class ProtoMachine : MonoBehaviour
{
    [Header("Machine Settings")]
    public float processTime = 3f;
    public float outputLifetime = 2f;  // how long the output stays visible
    public GameObject inputPrefab;     // e.g. Capsule (Wood)
    public GameObject outputPrefab;    // e.g. Cube (Plank)
    public Transform inputPoint;
    public Transform outputPoint;

    private bool isProcessing = false;

    void OnMouseDown()
    {
        if (!isProcessing)
            StartCoroutine(ProcessRoutine());
    }

    private IEnumerator ProcessRoutine()
    {
        isProcessing = true;

        // Spawn input
        GameObject inputObj = null;
        if (inputPrefab && inputPoint)
        {
            inputObj = Instantiate(inputPrefab, inputPoint.position, Quaternion.identity);
        }

        Debug.Log("Machine started!");

        yield return new WaitForSeconds(processTime);

        // Destroy input after processing
        if (inputObj != null) Destroy(inputObj);

        // Spawn output (temporary for now)
        if (outputPrefab && outputPoint)
        {
            GameObject outputObj = Instantiate(outputPrefab, outputPoint.position, Quaternion.identity);
            Destroy(outputObj, outputLifetime); // auto remove after X seconds
        }

        Debug.Log("Machine finished! (Plank created)");
        isProcessing = false;
    }
}
