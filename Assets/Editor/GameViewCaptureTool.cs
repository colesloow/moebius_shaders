using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GameViewCaptureTool : EditorWindow
{
    private static string savePath = "Screenshots";
    private static string fileName = "capture";
    private static int width = 1920;
    private static int height = 1080;
    private static bool useTimestamp = true;

    [MenuItem("Tools/Game View Capture/Open Window")]
    public static void ShowWindow()
    {
        GetWindow<GameViewCaptureTool>("Game View Capture");
    }

    // Works in both Edit and Play mode
    [MenuItem("Tools/Game View Capture/Capture Now %#c")]
    public static void CaptureNow()
    {
        DoCapture();
    }

    private void OnGUI()
    {
        GUILayout.Label("Resolution", EditorStyles.boldLabel);
        width  = EditorGUILayout.IntField("Width",  width);
        height = EditorGUILayout.IntField("Height", height);

        GUILayout.Space(8);
        GUILayout.Label("Output", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        savePath = EditorGUILayout.TextField("Folder", savePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string chosen = EditorUtility.OpenFolderPanel("Select folder", savePath, "");
            if (!string.IsNullOrEmpty(chosen))
                savePath = chosen;
        }
        EditorGUILayout.EndHorizontal();

        fileName     = EditorGUILayout.TextField("File name", fileName);
        useTimestamp = EditorGUILayout.Toggle("Append timestamp", useTimestamp);

        GUILayout.Space(12);

        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Capture", GUILayout.Height(36)))
            DoCapture();
        GUI.backgroundColor = Color.white;

        GUILayout.Space(4);
        EditorGUILayout.HelpBox("Shortcut: Ctrl+Shift+C (works in Play Mode)", MessageType.None);
    }

    private static void DoCapture()
    {
        Camera cam = Camera.main;
        if (cam == null)
            cam = FindAnyObjectByType<Camera>();
        if (cam == null)
        {
            EditorUtility.DisplayDialog("Game View Capture",
                "No active camera found in the scene.", "OK");
            return;
        }

        string folder = Path.IsPathRooted(savePath)
            ? savePath
            : Path.GetFullPath(Path.Combine(Application.dataPath, "..", savePath));

        Directory.CreateDirectory(folder);

        string name = useTimestamp
            ? $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.png"
            : $"{fileName}.png";

        string fullPath = Path.Combine(folder, name);

        var rt  = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        var prevTarget = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = prevTarget;

        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        File.WriteAllBytes(fullPath, tex.EncodeToPNG());

        DestroyImmediate(rt);
        DestroyImmediate(tex);

        Debug.Log($"[GameViewCapture] Saved: {fullPath}");
        EditorUtility.RevealInFinder(fullPath);
    }
}
