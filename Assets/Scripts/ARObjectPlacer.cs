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
    public Button placeButton;

    [Header("UI")]
    public TextMeshProUGUI bounceText;

    private bool objectPlaced = false;
    private GameObject placedObject;

    void Start()
    {
        placeButton.onClick.AddListener(PlaceObject);
        bounceText.gameObject.SetActive(false);  // Hide initially
    }

    void Update()
    {
        // Update bounce counter display if object is placed
        if (objectPlaced && bounceText.gameObject.activeInHierarchy)
        {
            bounceText.text = $"Bounces: {LaserData.globalBounce}";
        }
    }

    void PlaceObject()
    {
        if (objectPlaced) return;

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;

            placedObject = Instantiate(objectPrefab, pose.position, pose.rotation);
            objectPlaced = true;

            // Hide place button
            placeButton.gameObject.SetActive(false);

            // Show bounce counter UI
            bounceText.text = $"Bounces: {LaserData.globalBounce}";
            bounceText.gameObject.SetActive(true);

            Debug.Log($"AR Object placed! Initial bounce count: {LaserData.globalBounce}");
        }
        else
        {
            Debug.Log("No plane detected under center of screen.");
        }
    }
}