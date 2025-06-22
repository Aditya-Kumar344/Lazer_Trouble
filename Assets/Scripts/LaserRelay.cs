using UnityEngine;

public class LaserRelay : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float range = 100f;

    private float brightness;
    private int currentLabel = 8;
    private int currentBounce = 3;
    private bool active = false;

    private GameObject lastHit = null;

    public void ActivateLaser(float newBrightness, int incomingLabel, int incomingBounce)
    {
        if (newBrightness < 0.05f || incomingLabel <= 1 || incomingBounce <= 0)
        {
            Debug.Log("❌ Laser too dim, label too low, or bounce exhausted.");
            return;
        }

        brightness = newBrightness;
        currentLabel = incomingLabel / 2;
        currentBounce = incomingBounce - 1;

        active = true;
        gameObject.SetActive(true);
        lineRenderer.enabled = true;

        Debug.Log($"✅ Relay Laser Activated — Label: {currentLabel}, Bounce: {currentBounce}, Brightness: {brightness}");
    }

    private void OnEnable()
    {
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (!active) return;

        Vector3 start = transform.position;
        Vector3 dir = transform.forward;

        lineRenderer.SetPosition(0, start);

        if (Physics.Raycast(start, dir, out RaycastHit hit, range))
        {
            GameObject hitObj = hit.collider.gameObject;
            if (hitObj.CompareTag("Laser")) return;

            if (hitObj != lastHit)
            {
                Debug.Log("Relay laser hit: " + hitObj.name);
                lastHit = hitObj;
            }

            lineRenderer.SetPosition(1, hit.point);

            GameObject marker = new GameObject("LaserTag");
            marker.tag = "Laser";
            marker.transform.position = hit.point - dir * 0.05f;

            LaserHitInfo info = marker.AddComponent<LaserHitInfo>();
            info.brightness = brightness;
            info.label = currentLabel;
            info.bounce = currentBounce;

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

        float width = Mathf.Clamp(brightness * 0.1f, 0.01f, 0.2f);
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        lineRenderer.material.SetColor("_EmissionColor", Color.red);
    }
}
