using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class ARObjectPlacer : MonoBehaviour
{
    public GameObject objectPrefab;
    public ARRaycastManager raycastManager;
    public Button placeButton;

    private bool objectPlaced = false;

    void Start()
    {
        placeButton.onClick.AddListener(PlaceObject);
    }

    void PlaceObject()
    {
        if (objectPlaced) return;

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.PlaneWithinPolygon);

        if (hits.Count > 0)
        {
            Pose pose = hits[0].pose;
            Instantiate(objectPrefab, pose.position, pose.rotation);
            objectPlaced = true;

            // Disable place button
            placeButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("No plane detected under center of screen.");
        }
    }
}
