using UnityEngine;
using Niantic.Lightship.AR; // Only if using Lightship
using Input = UnityEngine.Input; // Explicitly choose Unity's Input

public class FinalLocationValidator : MonoBehaviour
{
    [Header("Location Settings")]
    public double targetLatitude = 28.6129;
    public double targetLongitude = 77.2295;
    [Tooltip("How close you need to be (meters)")] 
    public double activationRadius = 50;
    
    [Header("AR Objects")]
    public GameObject anchorPrefab;
    public GameObject puzzlePrefab;
    [Range(1f, 20f)] public float spawnAreaSize = 5f;
    
    private bool _isActive;

    void Update()
    {
        // Method 1: Use Unity's location (works everywhere)
        if (UnityEngine.Input.location.status == LocationServiceStatus.Running)
        {
            CheckLocation(
                UnityEngine.Input.location.lastData.latitude,
                UnityEngine.Input.location.lastData.longitude
            );
        }
        // Method 2: Uncomment if using Lightship
        /*
        else if (TryGetLightshipPosition(out var lat, out var lon))
        {
            CheckLocation(lat, lon);
        }
        */
    }

    void CheckLocation(double currentLat, double currentLon)
    {
        double distance = CalculateDistance(
            currentLat,
            currentLon,
            targetLatitude,
            targetLongitude
        );
        
        if (distance <= activationRadius && !_isActive)
        {
            Instantiate(anchorPrefab, transform.position, Quaternion.identity);
            
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnAreaSize/2, spawnAreaSize/2),
                0,
                Random.Range(-spawnAreaSize/2, spawnAreaSize/2)
            );
            Instantiate(puzzlePrefab, transform.position + randomOffset, Quaternion.identity);
            
            _isActive = true;
        }
        else if (distance > activationRadius && _isActive)
        {
            _isActive = false;
        }
    }

    // Uncomment if using Lightship
    /*
    bool TryGetLightshipPosition(out double lat, out double lon)
    {
        var pos = FindObjectOfType<ARWorldPositioningManager>()?.GetEarthPosition();
        lat = pos?.LatitudeInDegrees ?? 0;
        lon = pos?.LongitudeInDegrees ?? 0;
        return pos.HasValue;
    }
    */

    double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        double R = 6371000; // Earth radius in meters
        double φ1 = lat1 * Mathf.Deg2Rad;
        double φ2 = lat2 * Mathf.Deg2Rad;
        double Δφ = (lat2-lat1) * Mathf.Deg2Rad;
        double Δλ = (lon2-lon1) * Mathf.Deg2Rad;

        double a = Mathf.Sin((float)Δφ/2) * Mathf.Sin((float)Δφ/2) +
                 Mathf.Cos((float)φ1) * Mathf.Cos((float)φ2) *
                 Mathf.Sin((float)Δλ/2) * Mathf.Sin((float)Δλ/2);
        
        return 2 * R * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1-a)));
    }
}