using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public sealed class TimeOfDayController : MonoBehaviour
{
    [Header("Time")]
    [SerializeField] private bool autoAdvance = true;
    [Min(1f)][SerializeField] private float dayLengthSeconds = 300f;
    [Range(0f, 1f)][SerializeField] private float time = 0f;

    [Header("Profiles")]
    [SerializeField] private TimeOfDayProfile dawn;
    [SerializeField] private TimeOfDayProfile day;
    [SerializeField] private TimeOfDayProfile dusk;
    [SerializeField] private TimeOfDayProfile night;

    [Header("Keys (0..1)")]
    [Range(0f, 1f)][SerializeField] private float dawnKey  = 0.23f;
    [Range(0f, 1f)][SerializeField] private float dayKey   = 0.35f;
    [Range(0f, 1f)][SerializeField] private float duskKey  = 0.70f;
    [Range(0f, 1f)][SerializeField] private float nightKey = 0.85f;

    [Header("Sun")]
    [SerializeField] private Light sun;

    [Header("Capture Override")]
    [Tooltip("When enabled, bypasses profiles and uses the values below for screenshots.")]
    [SerializeField] private bool captureOverride = false;
    [SerializeField] private Color overrideTint      = Color.white;
    [SerializeField] private Color overrideSkyTop    = new Color(0.35f, 0.65f, 1f, 1f);
    [SerializeField] private Color overrideSkyBottom = new Color(0.80f, 0.90f, 1f, 1f);
    [Range(0f, 360f)][SerializeField] private float overrideSunPitch = 45f;
    [Range(0f, 360f)][SerializeField] private float overrideSunYaw   = 0f;

    private static readonly int ID_TOD_Tint   = Shader.PropertyToID("_TOD_Tint");
    private static readonly int ID_SkyTop     = Shader.PropertyToID("_Sky_TopColor");
    private static readonly int ID_SkyBottom  = Shader.PropertyToID("_Sky_BottomColor");

#if UNITY_EDITOR
    private double _editorStart;
#endif

    private void OnEnable()
    {
#if UNITY_EDITOR
        _editorStart = EditorApplication.timeSinceStartup;
#endif
        Apply();
    }

    private void OnValidate() => Apply();

    private void Update()
    {
        if (autoAdvance && !captureOverride)
        {
            float t = GetTimeSeconds() / Mathf.Max(1f, dayLengthSeconds);
            time = t - Mathf.Floor(t);
        }

        Apply();
    }

    private float GetTimeSeconds()
    {
        if (Application.isPlaying)
            return Time.time;

#if UNITY_EDITOR
        return (float)(EditorApplication.timeSinceStartup - _editorStart);
#else
        return 0f;
#endif
    }

    private void Apply()
    {
        if (captureOverride)
        {
            Shader.SetGlobalColor(ID_TOD_Tint,  overrideTint);
            Shader.SetGlobalColor(ID_SkyTop,    overrideSkyTop);
            Shader.SetGlobalColor(ID_SkyBottom, overrideSkyBottom);
            if (sun != null)
                sun.transform.rotation = Quaternion.Euler(overrideSunPitch, overrideSunYaw, 0f);
            return;
        }

        if (dawn == null || day == null || dusk == null || night == null)
            return;

        Shader.SetGlobalColor(ID_TOD_Tint,  EvalColor(time,
            dawn.globalTint, day.globalTint, dusk.globalTint, night.globalTint));
        Shader.SetGlobalColor(ID_SkyTop,    EvalColor(time,
            dawn.skyTop, day.skyTop, dusk.skyTop, night.skyTop));
        Shader.SetGlobalColor(ID_SkyBottom, EvalColor(time,
            dawn.skyBottom, day.skyBottom, dusk.skyBottom, night.skyBottom));

        if (sun != null)
        {
            float pitch = EvalFloat(time,
                dawn.sunPitch, day.sunPitch, dusk.sunPitch, night.sunPitch);
            sun.transform.rotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    private Color EvalColor(float t, Color a, Color b, Color c, Color d)
    {
        if (InWrap(t, dawnKey, dayKey))   return Color.Lerp(a, b, LerpTWrap(t, dawnKey, dayKey));
        if (InWrap(t, dayKey, duskKey))   return Color.Lerp(b, c, LerpTWrap(t, dayKey, duskKey));
        if (InWrap(t, duskKey, nightKey)) return Color.Lerp(c, d, LerpTWrap(t, duskKey, nightKey));
        return Color.Lerp(d, a, LerpTWrap(t, nightKey, dawnKey));
    }

    private float EvalFloat(float t, float a, float b, float c, float d)
    {
        if (InWrap(t, dawnKey, dayKey))   return Mathf.Lerp(a, b, LerpTWrap(t, dawnKey, dayKey));
        if (InWrap(t, dayKey, duskKey))   return Mathf.Lerp(b, c, LerpTWrap(t, dayKey, duskKey));
        if (InWrap(t, duskKey, nightKey)) return Mathf.Lerp(c, d, LerpTWrap(t, duskKey, nightKey));
        return Mathf.Lerp(d, a, LerpTWrap(t, nightKey, dawnKey));
    }

    private static bool InWrap(float t, float a, float b)
    {
        if (a <= b) return t >= a && t < b;
        return t >= a || t < b;
    }

    private static float LerpTWrap(float t, float a, float b)
    {
        if (a <= b) return Mathf.InverseLerp(a, b, t);
        float len = (1f - a) + b;
        float pos = (t >= a) ? (t - a) : ((1f - a) + t);
        return (len <= 0f) ? 0f : Mathf.Clamp01(pos / len);
    }
}
