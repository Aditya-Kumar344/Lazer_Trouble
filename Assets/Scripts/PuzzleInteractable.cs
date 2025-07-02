using UnityEngine;

public class PuzzleInteractable : MonoBehaviour
{
    private ARObjectPlacer objectPlacer;
    
    void Start()
    {
        objectPlacer = FindObjectOfType<ARObjectPlacer>();
    }
    
    void OnMouseDown()
    {
        if (objectPlacer != null && !objectPlacer.objectPlaced)
        {
            objectPlacer.PlaceObjectAtPosition(transform.position);
        }
    }
}