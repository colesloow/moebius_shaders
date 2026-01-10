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

    [Header("Keys (0..1)")]
    [Range(0f, 1f)][SerializeField] private float dawn = 0.23f;
    [Range(0f, 1f)][SerializeField] private float day = 0.35f;
    [Range(0f, 1f)][SerializeField] private float dusk = 0.70f;
    [Range(0f, 1f)][SerializeField] private float night = 0.85f;

    [Header("Global tint")]
    [SerializeField] private Color tintDawn = Color.white;
    [SerializeField] private Color tintDay = Color.white;
    [SerializeField] private Color tintDusk = Color.white;
    [SerializeField] private Color tintNight = Color.white;

    [Header("Skydome colors")]
    [SerializeField] private Color skyTopDawn = new Color(0.65f, 0.75f, 1f, 1f);
    [SerializeField] private Color skyTopDay = new Color(0.35f, 0.65f, 1f, 1f);
    [SerializeField] private Color skyTopDusk = new Color(0.95f, 0.45f, 0.35f, 1f);
    [SerializeField] private Color skyTopNight = new Color(0.02f, 0.03f, 0.08f, 1f);

    [SerializeField] private Color skyBottomDawn = new Color(1f, 0.7f, 0.45f, 1f);
    [SerializeField] private Color skyBottomDay = new Color(0.8f, 0.9f, 1f, 1f);
    [SerializeField] private Color skyBottomDusk = new Color(1f, 0.35f, 0.25f, 1f);
    [SerializeField] private Color skyBottomNight = new Color(0.01f, 0.01f, 0.02f, 1f);

    [Header("Sun (rotation only)")]
    [SerializeField] private Light sun;
    [SerializeField] private float offset = 25f;

    private static readonly int ID_TOD_Tint = Shader.PropertyToID("_TOD_Tint");
    private static readonly int ID_SkyTop = Shader.PropertyToID("_Sky_TopColor");
    private static readonly int ID_SkyBottom = Shader.PropertyToID("_Sky_BottomColor");

#if UNITY_EDITOR
    private double _editorStart;
#endif

    private void OnEnable()
    {
#if UNITY_EDITOR
        // in edit mode Time.time doesn't tick (use EditorApplication.timeSinceStartup instead)
        _editorStart = EditorApplication.timeSinceStartup;
#endif
        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }

    private void Update()
    {
        if (autoAdvance)
        {
            // cnovert seconds -> normalized
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
        // global tint
        Shader.SetGlobalColor(ID_TOD_Tint, Eval4(time, tintDawn, tintDay, tintDusk, tintNight));

        // skydome colors
        Shader.SetGlobalColor(ID_SkyTop, Eval4(time, skyTopDawn, skyTopDay, skyTopDusk, skyTopNight));
        Shader.SetGlobalColor(ID_SkyBottom, Eval4(time, skyBottomDawn, skyBottomDay, skyBottomDusk, skyBottomNight));

        if (sun == null)
            return;

        float yaw = (time * 360f) + offset;
        sun.transform.rotation = Quaternion.Euler(yaw, 0f, 0f);
    }

    private Color Eval4(float t, Color cDawn, Color cDay, Color cDusk, Color cNight)
    {
        // We split the day into 4 segments
        if (InWrap(t, dawn, day))
            return Color.Lerp(cDawn, cDay, LerpTWrap(t, dawn, day));

        if (InWrap(t, day, dusk))
            return Color.Lerp(cDay, cDusk, LerpTWrap(t, day, dusk));

        if (InWrap(t, dusk, night))
            return Color.Lerp(cDusk, cNight, LerpTWrap(t, dusk, night));

        // last segment wraps around (night -> dawn)
        return Color.Lerp(cNight, cDawn, LerpTWrap(t, night, dawn));
    }

    private static bool InWrap(float t, float a, float b)
    {
        // Checks if t is inside the interval [a, b) on a circular 0..1 timeline
        if (a <= b)
            return t >= a && t < b;

        return t >= a || t < b;
    }

    private static float LerpTWrap(float t, float a, float b)
    {
        // normal interval
        if (a <= b)
            return Mathf.InverseLerp(a, b, t);

        // wrap interval
        float len = (1f - a) + b;                               // dist from a to b
        float pos = (t >= a) ? (t - a) : ((1f - a) + t);        // t position
        return (len <= 0f) ? 0f : Mathf.Clamp01(pos / len);     // normalize
    }
}
