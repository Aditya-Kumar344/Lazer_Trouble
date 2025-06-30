using UnityEngine;
using System.Collections.Generic;

public class LaserRedirector : MonoBehaviour
{
    [Header("Redirector Settings")]
    public Transform emissionPoint;
    public LayerMask obstacleLayerMask = -1;

    [Header("Label Matching")]
    public int expectedLabel = 8;

    [Header("Visual Settings")]
    public Material laserMaterial;
    public Color laserColor = Color.red;
    public float baseWidth = 0.2f;
    public float widthMultiplier = 2f;

    [Header("Debug Info")]
    [SerializeField] private bool isReceivingLaser = false;
    [SerializeField] private LaserData currentLaserData = null;

    private LineRenderer outputLineRenderer;
    private List<LaserRedirector> hitRedirectors = new List<LaserRedirector>();
    
    // Track if this redirector has already consumed a bounce in the current laser session
    private bool hasConsumedBounce = false;

    void Start()
    {
        SetupOutputLaser();

        if (!CompareTag("Laser"))
            Debug.LogWarning($"LaserRedirector on {gameObject.name} should have the 'Laser' tag!");

        if (GetComponent<Collider>() == null)
            Debug.LogWarning($"LaserRedirector on {gameObject.name} needs a Collider component!");
    }

    void SetupOutputLaser()
    {
        if (emissionPoint == null)
        {
            GameObject emissionChild = new GameObject("LaserEmissionPoint");
            emissionChild.transform.SetParent(transform);
            emissionChild.transform.localPosition = Vector3.zero;
            emissionChild.transform.localRotation = Quaternion.identity;
            emissionPoint = emissionChild.transform;
        }

        outputLineRenderer = emissionPoint.GetComponent<LineRenderer>();
        if (outputLineRenderer == null)
            outputLineRenderer = emissionPoint.gameObject.AddComponent<LineRenderer>();

        outputLineRenderer.material = laserMaterial;
        SetLineRendererColor(outputLineRenderer, laserColor);
        float initialWidth = baseWidth * widthMultiplier;
        outputLineRenderer.startWidth = initialWidth;
        outputLineRenderer.endWidth = initialWidth;
        outputLineRenderer.positionCount = 2;
        outputLineRenderer.useWorldSpace = true;
        outputLineRenderer.enabled = false;
    }

    public void OnLaserHit(LaserData incomingLaser, Vector3 hitPoint, Vector3 incomingDirection)
    {
        // Check if laser has correct label
        if (incomingLaser.label != expectedLabel)
        {
            return; // Silent ignore for wrong label
        }

        // Check if laser is valid
        if (!incomingLaser.IsValid())
        {
            Debug.Log($"❌ Redirector {gameObject.name} not activating: {incomingLaser}");
            return;
        }

        // If already receiving laser, just update without consuming bounce
        if (isReceivingLaser)
        {
            currentLaserData = new LaserData(incomingLaser.brightness * 0.5f, incomingLaser.label / 2);
            UpdateOutputLaser();
            return;
        }

        // Check if we have bounces left for NEW activation
        if (LaserData.globalBounce <= 0)
        {
            Debug.Log($"🚫 Redirector {gameObject.name} not activated. Global bounce exhausted.");
            return;
        }

        // NEW activation - consume a bounce
        LaserData.globalBounce--;
        hasConsumedBounce = true;
        isReceivingLaser = true;
        currentLaserData = new LaserData(incomingLaser.brightness * 0.5f, incomingLaser.label / 2);
        
        Debug.Log($"✅ Redirector {gameObject.name} activated. Remaining Bounce: {LaserData.globalBounce}");
        UpdateOutputLaser();
    }

    public void OnLaserHitStopped()
    {
        isReceivingLaser = false;
        currentLaserData = null;
        hasConsumedBounce = false; // Reset bounce consumption when laser stops

        foreach (var redirector in hitRedirectors)
        {
            if (redirector != null)
                redirector.OnLaserHitStopped();
        }
        hitRedirectors.Clear();

        if (outputLineRenderer != null)
            outputLineRenderer.enabled = false;
    }

    void UpdateOutputLaser()
    {
        if (!isReceivingLaser || currentLaserData == null || !currentLaserData.IsValid())
        {
            // Clean up when stopping
            if (hitRedirectors.Count > 0)
            {
                foreach (var redirector in hitRedirectors)
                {
                    if (redirector != null)
                        redirector.OnLaserHitStopped();
                }
                hitRedirectors.Clear();
            }
            outputLineRenderer.enabled = false;
            return;
        }

        outputLineRenderer.enabled = true;

        Vector3 startPos = emissionPoint.position;
        Vector3 direction = emissionPoint.forward;
        float maxDistance = 100f;
        Vector3 endPos;
        
        LaserRedirector currentHitRedirector = null;

        if (Physics.Raycast(startPos, direction, out RaycastHit hit, maxDistance, obstacleLayerMask))
        {
            endPos = hit.point;

            if (hit.collider.CompareTag("Laser"))
            {
                LaserRedirector redirector = hit.collider.GetComponent<LaserRedirector>();
                if (redirector != null && redirector != this)
                {
                    currentHitRedirector = redirector;
                }
            }
        }
        else
        {
            endPos = startPos + direction * maxDistance;
        }

        // Handle redirector state changes (same logic as LaserEmitter)
        LaserRedirector lastHitRedirector = hitRedirectors.Count > 0 ? hitRedirectors[0] : null;
        
        if (currentHitRedirector != lastHitRedirector)
        {
            // Stop the previous redirector if there was one
            if (lastHitRedirector != null)
            {
                lastHitRedirector.OnLaserHitStopped();
                hitRedirectors.Clear();
            }

            // Start the new redirector if there is one
            if (currentHitRedirector != null)
            {
                hitRedirectors.Add(currentHitRedirector);
                currentHitRedirector.OnLaserHit(currentLaserData, hit.point, direction);
            }
        }
        else if (currentHitRedirector != null)
        {
            // Same redirector, just update it (this should not consume bounces)
            currentHitRedirector.OnLaserHit(currentLaserData, hit.point, direction);
        }

        // Update visual
        outputLineRenderer.SetPosition(0, startPos);
        outputLineRenderer.SetPosition(1, endPos);

        float currentWidth = baseWidth * currentLaserData.brightness * widthMultiplier;
        outputLineRenderer.startWidth = currentWidth;
        outputLineRenderer.endWidth = currentWidth;

        Color currentColor = laserColor;
        currentColor.a = Mathf.Clamp01(currentLaserData.brightness);
        SetLineRendererColor(outputLineRenderer, currentColor);
    }

    void Update()
    {
        if (isReceivingLaser)
            UpdateOutputLaser();
    }

    void OnDrawGizmos()
    {
        if (isReceivingLaser && currentLaserData != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, 
                $"Label: {currentLaserData.label}\nGlobalBounce: {LaserData.globalBounce}");
#endif
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.1f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f, 
                $"Expects: {expectedLabel}\nGlobalBounce: {LaserData.globalBounce}");
#endif
        }
    }

    public void SetExpectedLabel(int newExpectedLabel)
    {
        expectedLabel = Mathf.Max(1, newExpectedLabel);
    }

    void SetLineRendererColor(LineRenderer lr, Color color)
    {
        if (lr.material != null)
            lr.material.color = color;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0), new GradientColorKey(color, 1) },
            new GradientAlphaKey[] { new GradientAlphaKey(color.a, 0), new GradientAlphaKey(color.a, 1) }
        );
        lr.colorGradient = gradient;
    }
}