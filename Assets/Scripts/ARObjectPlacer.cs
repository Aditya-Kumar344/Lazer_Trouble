using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ARObjectPlacer : MonoBehaviour
{
    [Header("AR Placement")]
    public GameObject objectPrefab;
    public ARRaycastManager raycastManager;
    
    [Header("UI")]
    public TextMeshProUGUI bounceText;
    
    [HideInInspector] public bool objectPlaced = false;
    private GameObject placedObject;

    void Start()
    {
        bounceText.gameObject.SetActive(false);  // Hide initially
    }

    void Update()
    {
        if (objectPlaced && bounceText.gameObject.activeInHierarchy)
        {
            bounceText.text = $"Bounces: {LaserData.globalBounce}";
        }
    }

    public void PlaceObjectAtPosition(Vector3 position)
    {
        if (objectPlaced) return;

        // Check if position is on a detected plane
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(new Vector2(Screen.width/2, Screen.height/2), hits, TrackableType.PlaneWithinPolygon))
        {
            // Use the plane's rotation
            placedObject = Instantiate(objectPrefab, position, hits[0].pose.rotation);
        }
        else
        {
            // Fallback to horizontal placement
            placedObject = Instantiate(objectPrefab, position, Quaternion.identity);
        }
        
        objectPlaced = true;
        bounceText.text = $"Bounces: {LaserData.globalBounce}";
        bounceText.gameObject.SetActive(true);
        Debug.Log($"AR Object placed! Initial bounce count: {LaserData.globalBounce}");
    }
}