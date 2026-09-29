using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CupOHappiness.ProceduralProgressBars.Documentation
{
[CustomEditor(typeof(Readme))]
public class ReadmeEditor : UnityEditor.Editor
{
    private const string ReadmeDirectory = "Assets/ThirdParty/CupOHappiness/ProceduralProgressBars/Readme";
    private GUIStyle _titleStyle;
    private GUIStyle _headingStyle;
    private GUIStyle _bodyStyle;

    protected override void OnHeaderGUI()
    {
        InitializeStyles();
        var readme = (Readme)target;
        var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);
        GUILayout.BeginHorizontal("In BigTitle");
        if (readme.icon != null) GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
        GUILayout.Label(readme.title, _titleStyle);
        GUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        InitializeStyles();
        foreach (var section in ((Readme)target).sections)
        {
            if (!string.IsNullOrEmpty(section.heading)) GUILayout.Label(section.heading, _headingStyle);
            DrawTextAndImages(section);
            if (!string.IsNullOrEmpty(section.linkText) && GUILayout.Button(section.linkText, EditorStyles.linkLabel))
                Application.OpenURL(section.url);
            GUILayout.Space(16f);
        }

        if (GUILayout.Button("Delete Readme Files", EditorStyles.miniButton) &&
            EditorUtility.DisplayDialog("Delete Readme Files", "Delete all files in the Procedural Progress Bars Readme folder?", "Delete", "Cancel"))
        {
            AssetDatabase.DeleteAsset(ReadmeDirectory);
            AssetDatabase.Refresh();
            GUIUtility.ExitGUI();
        }
    }

    private void DrawTextAndImages(Readme.Section section)
    {
        var shownImages = new HashSet<int>();
        var text = section.text ?? string.Empty;
        var matches = Regex.Matches(text, @"\[\[IMAGE:(\d+)\]\]");
        int start = 0;
        foreach (Match match in matches)
        {
            DrawText(text.Substring(start, match.Index - start));
            if (int.TryParse(match.Groups[1].Value, out int index) && section.images != null && index >= 0 && index < section.images.Length)
            {
                DrawImage(section.images[index]);
                shownImages.Add(index);
            }
            start = match.Index + match.Length;
        }
        DrawText(text.Substring(start));

        if (section.images == null) return;
        for (int i = 0; i < section.images.Length; i++)
            if (!shownImages.Contains(i)) DrawImage(section.images[i]);
    }

    private void DrawText(string text)
    {
        if (!string.IsNullOrWhiteSpace(text)) GUILayout.Label(text, _bodyStyle);
    }

    private static void DrawImage(Texture2D image)
    {
        if (image == null) return;
        float width = Mathf.Min(EditorGUIUtility.currentViewWidth - 40f, image.width);
        float height = width * image.height / image.width;
        Rect imageRect = GUILayoutUtility.GetRect(width, height, GUILayout.Width(width), GUILayout.Height(height));
        GUI.DrawTexture(imageRect, image, ScaleMode.ScaleToFit, true);
        GUILayout.Space(6f);
    }

    private void InitializeStyles()
    {
        if (_bodyStyle != null) return;
        _bodyStyle = new GUIStyle(EditorStyles.label) { wordWrap = true, fontSize = 14 };
        _titleStyle = new GUIStyle(_bodyStyle) { fontSize = 26 };
        _headingStyle = new GUIStyle(_bodyStyle) { fontSize = 18 };
    }
}
}