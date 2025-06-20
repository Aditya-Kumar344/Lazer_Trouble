using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class LaserEmitter : MonoBehaviour
{
    public LineRenderer laser; 
    public float beamLength = 10f;
    public LayerMask hitLayers;  // Set in inspector to include only hittable layers (like "DirectorBlock")
    public string targetTag = "Director";  // Optional, if you're tagging the block

    void Start()
    {
        if (laser == null)
            laser = GetComponent<LineRenderer>();

        laser.positionCount = 2;  // Start and end points
    }

    void Update()
    {
        Vector3 start = transform.position;
        Vector3 direction = transform.forward;

        laser.SetPosition(0, start);  // Start of beam

        if (Physics.Raycast(start, direction, out RaycastHit hit, beamLength, hitLayers))
        {
            laser.SetPosition(1, hit.point);  // Stop at the surface

            if (hit.collider.CompareTag(targetTag))
            {
                Redirector db = hit.collider.GetComponent<Redirector>();
                if (db != null)
                {
                    db.OnLaserHit();
                }
            }
        }
        else
        {
            // No hit, draw full length
            laser.SetPosition(1, start + direction * beamLength);
        }
    }
}
