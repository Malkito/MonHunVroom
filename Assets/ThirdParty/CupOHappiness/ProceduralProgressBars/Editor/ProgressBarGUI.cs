#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CupOHappiness.Editor
{
   
    public sealed class ProgressBarGUI : ShaderGUI
    {
        private static readonly Dictionary<string, bool> Expanded = new Dictionary<string, bool>();

        public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
        {
            var material = (Material)editor.target;
            EnforceHdrpDefaults(editor, properties);
            DrawDepthWriteOption(properties);

            var shader = material.shader;
            var groups = new Dictionary<string, List<int>>();
            var order = new List<string>();
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                string name = shader.GetPropertyName(i);
                if (!material.HasProperty(name) || (shader.GetPropertyFlags(i) & ShaderPropertyFlags.HideInInspector) != 0 || name.StartsWith("unity_", StringComparison.Ordinal) || name.StartsWith("_Queue", StringComparison.Ordinal)) continue;
                string category = Category(name, shader.GetPropertyDescription(i));
                if (!groups.TryGetValue(category, out var list)) { groups[category] = list = new List<int>(); order.Add(category); }
                list.Add(i);
            }

            foreach (string category in order) DrawCategory(editor, material, shader, properties, category, groups[category]);
        }

        private static void DrawDepthWriteOption(MaterialProperty[] properties)
        {
            var surface = FindProperty("_SurfaceType", properties, false);
            var depthWrite = FindProperty("_TransparentZWrite", properties, false);
            if (surface == null || depthWrite == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Surface Options", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Popup("Surface Type", 1, new[] { "Opaque", "Transparent" });
            EditorGUILayout.Toggle("Depth Write", false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private static void EnforceHdrpDefaults(MaterialEditor editor, MaterialProperty[] properties)
        {
            if (FindProperty("_SurfaceType", properties, false) == null || FindProperty("_TransparentZWrite", properties, false) == null) return;

            var hdrp = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("UnityEngine.Rendering.HighDefinition.HDMaterial"))
                .FirstOrDefault(type => type != null);
            var validate = hdrp?.GetMethod("ValidateMaterial", BindingFlags.Public | BindingFlags.Static);
            foreach (var target in editor.targets)
            {
                var material = (Material)target;
                bool changed = !Mathf.Approximately(material.GetFloat("_SurfaceType"), 1f) || !Mathf.Approximately(material.GetFloat("_TransparentZWrite"), 0f);
                material.SetFloat("_SurfaceType", 1f);
                material.SetFloat("_TransparentZWrite", 0f);
                validate?.Invoke(null, new object[] { material });
                if (changed) EditorUtility.SetDirty(material);
            }
        }

        private static void DrawCategory(MaterialEditor editor, Material material, Shader shader, MaterialProperty[] properties, string category, List<int> ids)
        {
            int enableIndex = ids.FirstOrDefault(i => Display(shader.GetPropertyDescription(i)).Equals("Enable", StringComparison.OrdinalIgnoreCase));
            bool hasEnable = ids.Any(i => i == enableIndex && Display(shader.GetPropertyDescription(i)).Equals("Enable", StringComparison.OrdinalIgnoreCase));
            bool enabled = !hasEnable || material.GetFloat(shader.GetPropertyName(enableIndex)) > .5f;
            string key = AssetDatabase.GetAssetPath(material) + "." + category;
            if (!Expanded.ContainsKey(key)) Expanded[key] = enabled;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            Expanded[key] = EditorGUILayout.Foldout(Expanded[key], category, true, EditorStyles.foldoutHeader);
            if (hasEnable)
            {
                EditorGUI.BeginChangeCheck();
                bool next = EditorGUILayout.Toggle(enabled, GUILayout.Width(18));
                if (EditorGUI.EndChangeCheck()) SetToggle(editor, material, shader, properties, enableIndex, next);
                if (!enabled) EditorGUILayout.LabelField("Disabled", EditorStyles.miniLabel, GUILayout.Width(52));
            }
            EditorGUILayout.EndHorizontal();
            if (enabled && Expanded[key])
            {
                EditorGUI.indentLevel++;
                var consumed = new HashSet<string>();
                if (category == "General Shape")
                {
                    DrawCorners(editor, material, shader, properties, ids, consumed);
                    DrawSkew(editor, material, shader, properties, ids, consumed);
                    DrawSkewOptions(editor, shader, properties, ids, consumed);
                }
                string previous = null;
                int heightIndex = category == "General Shape" ? ids.FirstOrDefault(i => shader.GetPropertyName(i) == "_General_Shape_Height") : -1;
                foreach (int i in ids)
                {
                    string name = shader.GetPropertyName(i);
                    if (i == heightIndex || consumed.Contains(name) || (hasEnable && i == enableIndex) || !Relevant(material, name, shader.GetPropertyType(i))) continue;
                    string label = Display(shader.GetPropertyDescription(i));
                    string logical = LogicalGroup(label);
                    if (logical != null && previous != null && logical != previous) EditorGUILayout.Space(8);
                    if (logical != null) previous = logical;
                    var property = FindProperty(name, properties, false);
                    if (property == null) continue;
                    if (name == "_General_Shape_Size_Offset") DrawSizeOffset(editor, material, property, key);
                    else editor.ShaderProperty(property, new GUIContent(label, Tooltip(name, label, category)));
                }
                if (heightIndex >= 0)
                {
                    var height = FindProperty(shader.GetPropertyName(heightIndex), properties, false);
                    if (height != null && Relevant(material, shader.GetPropertyName(heightIndex), shader.GetPropertyType(heightIndex)))
                    {
                        EditorGUILayout.Space(8);
                        editor.ShaderProperty(height, new GUIContent(Display(shader.GetPropertyDescription(heightIndex)), Tooltip(shader.GetPropertyName(heightIndex), Display(shader.GetPropertyDescription(heightIndex)), category)));
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private static void DrawCorners(MaterialEditor editor, Material material, Shader shader, MaterialProperty[] all, List<int> ids, HashSet<string> consumed)
        {
            string[] names = { "_General_Shape_Roudness_Top_Left", "_General_Shape_Roudness_Top_Right", "_General_Shape_Roudness_Bottom_Left", "_General_Shape_Roudness_Bottom_Right" };
            if (names.Any(n => !ids.Any(i => shader.GetPropertyName(i) == n))) return;
            foreach (string name in names) consumed.Add(name);
            string pref = "CupOHappiness.ProgressBarStudio.CornersLinked." + AssetDatabase.GetAssetPath(material);
            bool linked = EditorPrefs.GetBool(pref, true);
            DrawLinkHeader("Corner Roundness", ref linked, pref, "Proportions constrained. Click to edit independently.", "Proportions unconstrained. Click to constrain.");
            DrawNumberRow(editor, material, shader, all, names, new[] { "Top left", "Top right", "Bottom left", "Bottom right" }, linked, 2);
        }

        private static void DrawSkew(MaterialEditor editor, Material material, Shader shader, MaterialProperty[] all, List<int> ids, HashSet<string> consumed)
        {
            string[] names = { "_General_Shape_Skew_Left_Top", "_General_Shape_Skew_Left_Bottom", "_General_Shape_Skew_Right_Top", "_General_Shape_Skew_Right_Bottom" };
            if (names.Any(n => !ids.Any(i => shader.GetPropertyName(i) == n))) return;
            foreach (string name in names) consumed.Add(name);
            string key = "CupOHappiness.ProgressBarStudio.SkewLinked." + AssetDatabase.GetAssetPath(material);
            bool linked = EditorPrefs.GetBool(key, true);
            DrawLinkHeader("Skew", ref linked, key, "All skew proportions constrained. Click to edit independently.", "Skew proportions unconstrained. Click to constrain.");
            DrawNumberRow(editor, material, shader, all, names, names.Select(n => Display(shader.GetPropertyDescription(ids.First(i => shader.GetPropertyName(i) == n))).Replace("Skew ", string.Empty)).ToArray(), linked, 2);
        }

        private static void DrawSkewOptions(MaterialEditor editor, Shader shader, MaterialProperty[] all, List<int> ids, HashSet<string> consumed)
        {
            string[] names = { "_General_Shape_Skew_Left_Edge_Position", "_General_Shape_Skew_Right_Edge_Position", "_General_Shape_Skew_Left_Invert", "_General_Shape_Skew_Right_Invert" };
            foreach (string name in names)
            {
                int index = ids.FirstOrDefault(i => shader.GetPropertyName(i) == name);
                if (!ids.Any(i => shader.GetPropertyName(i) == name)) continue;
                consumed.Add(name);
                var property = FindProperty(name, all, false);
                if (property == null) continue;
                string label = Display(shader.GetPropertyDescription(index));
                float oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = Mathf.Max(oldLabelWidth, 175f);
                editor.ShaderProperty(property, new GUIContent(label, Tooltip(name, label, "General Shape")));
                EditorGUIUtility.labelWidth = oldLabelWidth;
            }
        }

        private static void DrawLinkHeader(string title, ref bool linked, string key, string linkedTip, string unlinkedTip)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            var icon = EditorGUIUtility.IconContent(linked ? "Linked" : "Unlinked");
            icon.tooltip = linked ? linkedTip : unlinkedTip;
            bool next = GUILayout.Toggle(linked, icon, EditorStyles.miniButton, GUILayout.Width(28), GUILayout.Height(20));
            if (next != linked) { linked = next; EditorPrefs.SetBool(key, linked); }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawNumberRow(MaterialEditor editor, Material material, Shader shader, MaterialProperty[] all, string[] names, string[] labels, bool linked, int columns)
        {
            int count = names.Length;
            int rowSize = EditorGUIUtility.currentViewWidth < 600 ? 1 : columns == 2 ? 2 : count;
            for (int start = 0; start < count; start += rowSize)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                float fieldWidth = Mathf.Max(55, (EditorGUIUtility.currentViewWidth - (rowSize == 1 ? 105 : 145)) / 3f);
                for (int j = start; j < Math.Min(start + rowSize, count); j++)
                {
                    int index = j;
                    var property = FindProperty(names[j], all, false);
                    if (property == null) continue;
                    string label = rowSize == 1 ? labels[j] : CompactLabel(labels[j]);
                    float labelWidth = rowSize == 1 ? 115 : 68;
                    var labelRect = GUILayoutUtility.GetRect(labelWidth, EditorGUIUtility.singleLineHeight, GUILayout.Width(labelWidth));
                    var labelContent = new GUIContent(label, Tooltip(names[j], labels[j], "General Shape") + "\nDrag the label horizontally to adjust.");
                    GUI.Label(labelRect, labelContent);
                    EditorGUIUtility.AddCursorRect(labelRect, MouseCursor.SlideArrow);
                    int controlId = GUIUtility.GetControlID(FocusType.Passive, labelRect);
                    var evt = Event.current;
                    if (evt.type == EventType.MouseDown && evt.button == 0 && labelRect.Contains(evt.mousePosition))
                    {
                        GUIUtility.hotControl = controlId;
                        evt.Use();
                    }
                    else if (evt.type == EventType.MouseDrag && GUIUtility.hotControl == controlId)
                    {
                        float sensitivity = evt.shift ? 0.001f : 0.01f;
                        SetLinked(editor, material, shader, all, names, index, property.floatValue + evt.delta.x * sensitivity, linked, "Adjust " + labels[j]);
                        GUI.changed = true;
                        evt.Use();
                    }
                    else if (evt.type == EventType.MouseUp && GUIUtility.hotControl == controlId)
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();
                    }
                    float value = EditorGUILayout.FloatField(property.floatValue, GUILayout.Width(fieldWidth));
                    if (!Mathf.Approximately(value, property.floatValue)) SetLinked(editor, material, shader, all, names, index, value, linked, "Set " + labels[j]);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void SetLinked(MaterialEditor editor, Material material, Shader shader, MaterialProperty[] all, string[] names, int edited, float input, bool linked, string undo)
        {
            var values = names.Select(n => material.GetFloat(n)).ToArray();
            float value = Mathf.Clamp(input, shader.GetPropertyRangeLimits(IndexOf(shader, names[edited])).x, shader.GetPropertyRangeLimits(IndexOf(shader, names[edited])).y);
            editor.RegisterPropertyChangeUndo(undo);
            float ratio = Mathf.Abs(values[edited]) > .00001f ? value / values[edited] : 0f;
            for (int i = 0; i < names.Length; i++)
            {
                var range = shader.GetPropertyRangeLimits(IndexOf(shader, names[i]));
                float next = i == edited ? value : linked ? Mathf.Clamp(values[edited] != 0f ? values[i] * ratio : value, range.x, range.y) : values[i];
                FindProperty(names[i], all, false).floatValue = next;
            }
        }

        private static int IndexOf(Shader shader, string name)
        {
            for (int i = 0; i < shader.GetPropertyCount(); i++) if (shader.GetPropertyName(i) == name) return i;
            return -1;
        }

        private static string CompactLabel(string label) => label.Replace("Skew ", string.Empty).Replace("Roundness ", string.Empty);

        private static void DrawSizeOffset(MaterialEditor editor, Material material, MaterialProperty property, string groupKey)
        {
            string key = "CupOHappiness.ProgressBarStudio.SizeOffsetLinked." + AssetDatabase.GetAssetPath(material);
            bool linked = EditorPrefs.GetBool(key, true);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Size Offset", "Changes the shape's size along X and Y. Constrain proportions to preserve their ratio."), EditorStyles.boldLabel);
            bool next = GUILayout.Toggle(linked, EditorGUIUtility.IconContent(linked ? "Linked" : "Unlinked"), EditorStyles.miniButton, GUILayout.Width(28));
            if (next != linked) { linked = next; EditorPrefs.SetBool(key, linked); }
            EditorGUILayout.EndHorizontal();
            Vector4 old = property.vectorValue;
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck(); float x = EditorGUILayout.FloatField(new GUIContent("X", "Horizontal size offset. When constrained, changing this also scales Y."), old.x);
            if (EditorGUI.EndChangeCheck()) SetSize(editor, property, old, 0, x, linked);
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(new GUIContent("Y", "Vertical size offset. When constrained, changing this also scales X."), GUILayout.Width(72));
            float y = EditorGUILayout.FloatField(property.vectorValue.y, GUILayout.Width(55));
            if (EditorGUI.EndChangeCheck()) SetSize(editor, property, property.vectorValue, 1, y, linked);
            EditorGUILayout.EndHorizontal();
        }

        private static void SetSize(MaterialEditor editor, MaterialProperty property, Vector4 old, int axis, float value, bool linked)
        {
            float previous = axis == 0 ? old.x : old.y;
            float other = axis == 0 ? old.y : old.x;
            if (linked && !Mathf.Approximately(previous, value)) other = Mathf.Abs(previous) > .00001f ? other * value / previous : value;
            editor.RegisterPropertyChangeUndo("Edit Shape Size Offset");
            property.vectorValue = axis == 0 ? new Vector4(value, other, old.z, old.w) : new Vector4(other, value, old.z, old.w);
        }

        private static void SetToggle(MaterialEditor editor, Material material, Shader shader, MaterialProperty[] properties, int index, bool enabled)
        {
            var property = FindProperty(shader.GetPropertyName(index), properties, false);
            if (property == null) return;
            editor.RegisterPropertyChangeUndo("Toggle " + Display(shader.GetPropertyDescription(index)));
            property.floatValue = enabled ? 1f : 0f;
            var attribute = shader.GetPropertyAttributes(index).FirstOrDefault(a => a.StartsWith("Toggle(", StringComparison.Ordinal));
            if (attribute == null) return;
            int open = attribute.IndexOf('('), close = attribute.IndexOf(')');
            if (open < 0 || close <= open) return;
            string keyword = attribute.Substring(open + 1, close - open - 1);
            foreach (var target in editor.targets)
            {
                var targetMaterial = (Material)target;
                if (enabled) targetMaterial.EnableKeyword(keyword); else targetMaterial.DisableKeyword(keyword);
            }
        }

        private static bool Relevant(Material m, string name, ShaderPropertyType type)
        {
            bool On(string field) => m.HasProperty(field) && m.GetFloat(field) > .5f;
            if (name.StartsWith("_Custom_Shape_Noise_Guide_2", StringComparison.Ordinal) && !On("_Custom_Shape_Use_Second_Texture")) return false;
            if (name.StartsWith("_Custom_Outline_Noise_Guide_", StringComparison.Ordinal) && !On("_Custom_Outline_Use_Noise_Guides")) return false;
            if (name.StartsWith("_Fill_Overlay_Noise_Guide_", StringComparison.Ordinal) && !On("_FILL_OVERLAY_USE_GUIDES")) return false;
            if (name.StartsWith("_Fill_Gradient_Color_", StringComparison.Ordinal) && !On("_Fill_Use_Horizontal_Gradient")) return false;
            if (name == "_Fill_Smooth_Edge_Value" && !On("_Fill_Enable_Smooth_Edge")) return false;
            if ((name == "_Fill_Texture_Guide" || name == "_Fill_Guide_Strength" || name == "_Fill_Scroll_speed_XY" || name.StartsWith("_Fill_Flip_Guide_Texture", StringComparison.Ordinal)) && !On("_Fill_Enable_Edge_Guide")) return false;
            if ((name == "_Fill_Shadow_Use_Only_Background_Color_Alpha" || name == "_Fill_Shadow_Background_Color") && !On("_Fill_Shadow_Use_On_Background")) return false;
            if ((name == "_Fill_Shadow_Texture_Guide" || name == "_Fill_Shadow_Guide_Strength" || name == "_Fill_Shadow_Scroll_Speed_XY" || name == "_Fill_Shadow_Flip_Guide_Texture") && !On("_Fill_Shadow_Use_Texture_Guide")) return false;
            string texture = GuideTexture(name);
            if (texture != null && type != ShaderPropertyType.Texture && (!m.HasProperty(texture) || !m.GetTexture(texture))) return false;
            string strength = GuideStrength(name);
            if ((name.Contains("Scroll_Speed") || name.EndsWith("Scroll_speed_XY", StringComparison.Ordinal)) && strength != null && m.HasProperty(strength) && m.GetFloat(strength) <= 0f) return false;
            string driver = name.StartsWith("_Fill_Overlay_", StringComparison.Ordinal) ? "_Fill_Overlay_Strength"
                : name.StartsWith("_Custom_Outline_", StringComparison.Ordinal) ? "_Custom_Outline_Strength"
                : name.StartsWith("_Fill_Vertical_Darken_", StringComparison.Ordinal) ? "_Fill_Vertical_Darken_Amount"
                : name.StartsWith("_Fill_Segments_", StringComparison.Ordinal) ? "_Fill_Segments_Segments_Number"
                : name.StartsWith("_Shadow_", StringComparison.Ordinal) ? "_Shadow_Strength"
                : name.StartsWith("_Fill_Shadow_", StringComparison.Ordinal) ? "_Fill_Shadow_Multiplier_Shadow_Amount" : null;
            return driver == null || name == driver || type == ShaderPropertyType.Texture || !m.HasProperty(driver) || m.GetFloat(driver) > 0f;
        }

        private static string GuideTexture(string name)
        {
            if (name == "_Fill_Guide_Strength" || name == "_Fill_Scroll_speed_XY") return "_Fill_Texture_Guide";
            if (name == "_Fill_Shadow_Guide_Strength" || name == "_Fill_Shadow_Scroll_Speed_XY") return "_Fill_Shadow_Texture_Guide";
            if (!name.Contains("_Noise_Guide_")) return null;
            if (name.EndsWith("_Strength", StringComparison.Ordinal)) return name.Substring(0, name.Length - 9);
            if (name.EndsWith("_Scroll_Speed", StringComparison.Ordinal)) return name.Substring(0, name.Length - 13);
            return null;
        }

        private static string GuideStrength(string name)
        {
            if (name.EndsWith("_Scroll_Speed", StringComparison.Ordinal)) return name.Substring(0, name.Length - 13) + "_Strength";
            if (name == "_Fill_Scroll_speed_XY") return "_Fill_Guide_Strength";
            if (name == "_Fill_Shadow_Scroll_Speed_XY") return "_Fill_Shadow_Guide_Strength";
            return null;
        }

        private static string Category(string name, string description)
        {
            int colon = description.IndexOf(':');
            if (colon > 0) return description.Substring(0, colon).Trim();
            if (name == "_End_Line_Visibility") return "End Line";
            if (name == "_Fill_Amount" || name == "_Main_Bar_Fill_Amount" || name == "_Second_Bar_Main_Blend" || name == "_Second_Bar_Invisible_Blend") return "Fill";
            return "General";
        }

        private static string Display(string description)
        {
            int colon = description.IndexOf(':');
            return colon >= 0 ? description.Substring(colon + 1).Trim() : description;
        }

        private static string LogicalGroup(string label)
        {
            string value = label.ToLowerInvariant();
            if (value.Contains("noise guide")) return "Noise";
            if (value.Contains("background")) return "Background";
            if (value.Contains("texture guide") || value.StartsWith("guide ") || value.Contains("guide rotation")) return "Guides";
            if (value.StartsWith("fill ") || value.StartsWith("fill color") || value.StartsWith("fill alpha")) return "Fill";
            if (value.Contains("shadow")) return "Shadow";
            if (value.Contains("outline")) return "Outline";
            return null;
        }

        private static string Tooltip(string name, string label, string category)
        {
            string v = label.ToLowerInvariant(), c = category.ToLowerInvariant();
            bool second = c.Contains("second bar") || v.Contains("second bar") || v.Contains("main bar fill");
            if (second) return v == "enable" ? "Shows the trailing ghost fill bar." : v.Contains("color") ? "Sets the trailing ghost fill bar's color." : v.Contains("invisible") ? "Controls how the trailing ghost fill bar fades away." : v.Contains("main blend") ? "Controls how the trailing ghost fill bar blends into the main fill." : v.Contains("fill amount") ? "Sets the trailing ghost fill bar's fill position." : "Controls this trailing ghost fill bar option.";
            if (v == "enable") return "Turns this effect on or off.";
            if (v.Contains("noise guide 2") && v.Contains("strength")) return "Sets how strongly the optional second noise layer changes the effect.";
            if (v.Contains("noise guide 2") && v.Contains("scroll speed")) return "Sets how quickly and in which direction the optional second noise layer moves.";
            if (v.Contains("noise guide 2")) return "Optional second noise texture for adding another layer of irregular detail.";
            if (v.Contains("noise guide")) return "Texture used to add irregular detail to the " + c + " effect.";
            if (v.Contains("scroll speed")) return "Sets the direction and speed that the guide texture moves.";
            if (v.Contains("strength")) return "Sets how strongly this effect or guide changes the bar's appearance.";
            if (v.Contains("roundness")) return "Sets how rounded the shape's corners appear.";
            if (v.Contains("skew")) return "Slants this edge. Larger values make the edge more angled.";
            if (v.Contains("size offset")) return "Changes the shape's size along X and Y. Constrain proportions to preserve their ratio.";
            if (v.Contains("segments count")) return "Sets how many separate segments appear in the filled bar.";
            if (v.Contains("segment")) return "Changes the spacing, edge, or color treatment of the filled segments.";
            if (v.Contains("alpha")) return "Sets how transparent this part of the effect appears.";
            if (v.Contains("impact")) return "Sets how strongly the effect changes the selected color or opacity.";
            if (v.Contains("smoothness")) return "Softens or sharpens the transition at this edge.";
            if (v.Contains("hardness") || v.Contains("power")) return "Changes the contrast and hardness of this effect.";
            if (v.Contains("rotation")) return "Rotates the guide that controls this effect.";
            if (v.Contains("texture guide") || v == "guide") return "Texture used as a mask or guide for this effect.";
            if (v.Contains("scroll")) return "Sets how the effect moves over time.";
            if (v.Contains("background")) return "Changes the bar's unfilled background area.";
            if (v.Contains("color")) return "Sets the color used by this part of the " + c + " effect.";
            if (v.Contains("fill amount")) return "Sets how much of the bar is filled.";
            if (v.Contains("visibility")) return "Sets how visible this effect is.";
            if (v.Contains("delay")) return "Sets how long this effect waits before it starts.";
            if (v.Contains("offset")) return "Moves the effect edge or guide without changing its overall size.";
            if (v.Contains("width")) return "Sets the width of this part of the effect.";
            if (v.Contains("height")) return "Sets the height of the bar shape.";
            if (v.Contains("blend")) return "Controls the transition between the bar layers.";
            if (v.Contains("shadow")) return "Changes the appearance and strength of the bar shadow.";
            if (v.Contains("overlay")) return "Changes how the overlay appears on the bar.";
            return "Controls " + v + " for the " + c + " effect.";
        }
    }
}
#endif
