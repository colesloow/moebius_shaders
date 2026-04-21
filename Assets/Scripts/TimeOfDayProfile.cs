using UnityEngine;

[CreateAssetMenu(fileName = "TOD_Profile", menuName = "Time Of Day/Profile")]
public class TimeOfDayProfile : ScriptableObject
{
    public Color globalTint = Color.white;
    public Color skyTop    = new Color(0.35f, 0.65f, 1f, 1f);
    public Color skyBottom = new Color(0.80f, 0.90f, 1f, 1f);

    [Range(0f, 360f)]
    public float sunPitch = 45f;
}
