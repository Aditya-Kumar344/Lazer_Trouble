using UnityEngine;

[System.Serializable]
public class LaserData
{
    public float brightness;
    public int label;
    
    // Single global bounce counter
    public static int globalBounce = 4;
    
    public LaserData(float brightness, int label)
    {
        this.brightness = brightness;
        this.label = label;
    }
    
    public bool IsValid()
    {
        return brightness >= 0.05f && label >= 1;
    }
    
    public override string ToString()
    {
        return $"Brightness: {brightness:F2}, Label: {label}, GlobalBounce: {globalBounce}";
    }
}