using UnityEngine;
using System.Collections.Generic;

public class LaserEmitter : MonoBehaviour
{
    [Header("Laser Settings")]
    public float maxDistance = 100f;
    public float brightness = 1f;
    public int label = 8;
    public LayerMask obstacleLayerMask = -1;

    [Header("Visual")]
    public Material laserMaterial;
    public Color laserColor = Color.red;
    public float baseWidth = 0.2f;
    public float widthMultiplier = 2f;

    private LineRenderer lineRenderer;
    private List<LaserRedirector> hitRedirectors = new List<LaserRedirector>();
    private LaserData currentLaserData;
    
    // Track the last hit redirector to avoid repeated stops/starts
    private LaserRedirector lastHitRedirector = null;

    void Start()
    {
        SetupLineRenderer();
    }

    void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.material = laserMaterial;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        UpdateLaser();
    }

    void UpdateLaser()
    {
        currentLaserData = new LaserData(brightness, label);

        Vector3 start = transform.position;
        Vector3 dir = transform.forward;
        Vector3 end = start + dir * maxDistance;
        
        LaserRedirector currentHitRedirector = null;

        if (Physics.Raycast(start, dir, out RaycastHit hit, maxDistance, obstacleLayerMask))
        {
            end = hit.point;

            if (hit.collider.CompareTag("Laser"))
            {
                currentHitRedirector = hit.collider.GetComponent<LaserRedirector>();
            }
        }

        // Handle redirector state changes
        if (currentHitRedirector != lastHitRedirector)
        {
            // Stop the previous redirector if there was one
            if (lastHitRedirector != null)
            {
                lastHitRedirector.OnLaserHitStopped();
                hitRedirectors.Remove(lastHitRedirector);
            }

            // Start the new redirector if there is one
            if (currentHitRedirector != null)
            {
                hitRedirectors.Add(currentHitRedirector);
                currentHitRedirector.OnLaserHit(currentLaserData, hit.point, dir);
            }

            lastHitRedirector = currentHitRedirector;
        }
        else if (currentHitRedirector != null)
        {
            // Same redirector, just update it (this should not consume bounces)
            currentHitRedirector.OnLaserHit(currentLaserData, hit.point, dir);
        }

        // Update visual
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        float width = baseWidth * brightness * widthMultiplier;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        Color color = laserColor;
        color.a = Mathf.Clamp01(brightness);
        SetLineColor(lineRenderer, color);
    }

    void SetLineColor(LineRenderer lr, Color color)
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

    void OnDisable()
    {
        // Clean up when emitter is disabled
        if (lastHitRedirector != null)
        {
            lastHitRedirector.OnLaserHitStopped();
            lastHitRedirector = null;
        }
        hitRedirectors.Clear();
    }
}