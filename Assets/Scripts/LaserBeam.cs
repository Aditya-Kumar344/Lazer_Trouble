using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float brightness = 1.0f;
    public int startLabel = 8;
    public int startBounce = 3;
    public float range = 100f;

    void Start()
    {
        EmitLaser();
    }

    void EmitLaser()
    {
        Vector3 start = transform.position;
        Vector3 dir = transform.forward;

        lineRenderer.SetPosition(0, start);

        if (Physics.Raycast(start, dir, out RaycastHit hit, range))
        {
            lineRenderer.SetPosition(1, hit.point);
            Debug.Log("Laser hit: " + hit.collider.name);

            GameObject marker = new GameObject("LaserTag");
            marker.tag = "Laser";
            marker.transform.position = hit.point;

            LaserHitInfo info = marker.AddComponent<LaserHitInfo>();
            info.brightness = brightness;
            info.label = startLabel;
            info.bounce = startBounce;

            SphereCollider col = marker.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.1f;

            Rigidbody rb = marker.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            Destroy(marker, 0.1f);
        }
        else
        {
            lineRenderer.SetPosition(1, start + dir * range);
        }

        SetLaserVisuals(brightness);
    }

    void SetLaserVisuals(float intensity)
    {
        float width = Mathf.Clamp(intensity * 0.1f, 0.01f, 0.2f);
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        lineRenderer.material.SetColor("_EmissionColor", Color.red);
    }
}
