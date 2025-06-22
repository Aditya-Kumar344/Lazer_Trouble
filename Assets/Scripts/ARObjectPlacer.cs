using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro; // ✅ TextMeshPro support
using System.Collections.Generic;

public class ARObjectPlacer : MonoBehaviour
{
    [Header("AR Placement")]
    public GameObject objectPrefab;
    public ARRaycastManager raycastManager;
    public Button placeButton;

    [Header("UI")]
    public TextMeshProUGUI bounceText; // ✅ Use TMP for display

    private bool objectPlaced = false;

    void Start()
    {
        placeButton.onClick.AddListener(PlaceObject);
        bounceText.gameObject.SetActive(false);  // Hide initially
    }

    void PlaceObject()
    {
        if (objectPlaced) return;

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;

            GameObject placedObject = Instantiate(objectPrefab, pose.position, pose.rotation);
            objectPlaced = true;

            // Hide place button
            placeButton.gameObject.SetActive(false);

            // ✅ Try to get LaserBeam from prefab or children
            LaserBeam beam = placedObject.GetComponentInChildren<LaserBeam>();
            int bounceCount = beam != null ? beam.startBounce : 0;

            // ✅ Update and show bounce text
            bounceText.text = $"Bounces: {bounceCount}";
            bounceText.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("No plane detected under center of screen.");
        }
    }
}
