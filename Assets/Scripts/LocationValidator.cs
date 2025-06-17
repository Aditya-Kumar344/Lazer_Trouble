using UnityEngine;
using Niantic.Lightship.AR.WorldPositioning;

public class LocationValidator : MonoBehaviour
{
    public ARWorldPositioningManager positioningManager;
    public GameObject uiToEnableIfNearby;
    public double targetLatitude = 28.6129;
    public double targetLongitude = 77.2295;
    public double allowedRadiusMeters = 50;

    void Start()
    {
        double currLat = positioningManager.WorldTransform.OriginLatitude;
        double currLon = positioningManager.WorldTransform.OriginLongitude;
        double dist = Haversine(currLat, currLon, targetLatitude, targetLongitude);

        uiToEnableIfNearby.SetActive(dist < allowedRadiusMeters);
    }

    double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        double R = 6371000;
        double dLat = Mathf.Deg2Rad * (float)(lat2 - lat1);
        double dLon = Mathf.Deg2Rad * (float)(lon2 - lon1);
        double a = Mathf.Sin((float)(dLat / 2)) * Mathf.Sin((float)(dLat / 2)) +
                   Mathf.Cos(Mathf.Deg2Rad * (float)lat1) * Mathf.Cos(Mathf.Deg2Rad * (float)lat2) *
                   Mathf.Sin((float)(dLon / 2)) * Mathf.Sin((float)(dLon / 2));
        double c = 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1 - a)));
        return R * c;
    }
}
