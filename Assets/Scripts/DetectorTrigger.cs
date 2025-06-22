using UnityEngine;
using System.Collections;

public class DetectorTrigger : MonoBehaviour
{
    public LaserRelay relay;

    [Tooltip("Only trigger if incoming laser has exactly this label (e.g., 4, 2, 1)")]
    public int expectedLabel = 4;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Laser"))
        {
            triggered = true;

            LaserHitInfo hitInfo = other.GetComponent<LaserHitInfo>();
            if (hitInfo != null)
            {
                if (hitInfo.label == expectedLabel)
                {
                    float newBrightness = hitInfo.brightness * 0.5f;
                    int newBounce = hitInfo.bounce - 1;

                    Debug.Log($"✔ Triggered: Label {hitInfo.label}, Bounce left: {hitInfo.bounce}");
                    relay.ActivateLaser(newBrightness, hitInfo.label, hitInfo.bounce);
                }
                else
                {
                    Debug.Log($"✘ Label mismatch. Incoming: {hitInfo.label}, Required: {expectedLabel}");
                }
            }

            StartCoroutine(ResetTrigger());
        }
    }

    IEnumerator ResetTrigger(float delay = 0.2f)
    {
        yield return new WaitForSeconds(delay);
        triggered = false;
    }
}
