using UnityEngine;
using Niantic.Lightship.AR.WorldPositioning;
using System;

public class PrePlaceWorldObjects : MonoBehaviour
{
    public CustomObject[] customObj;
    public ARWorldPositioningManager positioningManager;
    public ARWorldPositioningObjectHelper objectHelper;

    void Start()
    {
        foreach (var obj in customObj)
        {
            GameObject instance = Instantiate(obj.prefab);
            objectHelper.AddOrUpdateObject(
                instance,
                obj.latitude,
                obj.longitude,
                0,
                Quaternion.identity
            );
        }
    }
}

[Serializable]
public struct CustomObject
{
    public GameObject prefab;
    public float longitude;
    public float latitude;
}