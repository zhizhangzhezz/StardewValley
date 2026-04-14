using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "lightingSchedule", menuName = "Scriptable Object/LightingSchedule")]
public class LightingSchedule : ScriptableObject
{
    public LightingBrightness[] lightingBrightnessArray;
}

[System.Serializable]
public struct LightingBrightness
{
    public Season season;
    public int hour;
    public float lightIntensity;
}
