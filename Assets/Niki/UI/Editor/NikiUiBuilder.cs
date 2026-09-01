using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Niki.UI.Editor
{
    /// <summary>
    /// Editor tooling: builds a fully wired Ability Slot widget under the first
    /// Canvas in the active scene.
    ///
    /// Menu: Niki UI / Create Ability Slot (in active scene)
    /// </summary>
    public static class NikiUiBuilder
    {
        private const string RadialShaderName = "NikiUI/RadialCooldown";
        private const float SlotSize = 140f;
        private const string UndoGroupName = "Create Ability Slot";

        [MenuItem("Niki UI/Create Ability Slot (in active scene)")]
        public static void CreateAbilitySlot()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[Niki.UI] No Canvas found in the active scene.");
                return;
            }

            // ---- Root ---------------------------------------------------------
            var root = CreateChild(canvas.transform, "AbilitySlot");
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.sizeDelta = new Vector2(SlotSize, SlotSize);

            var widget = root.gameObject.AddComponent<AbilitySlotWidget>();
            var iconSprite = root.gameObject.AddComponent<BindableImageSprite>();
            var pressIndicator = root.gameObject.AddComponent<BindableBoolColor>();
            pressIndicator.SetColors(Color.white, new Color(1f, 1f, 1f, 0.5f));

            // ---- Icon -----------------------------------------------------------
            var iconTr = CreateChild(root, "Icon");
            Stretch(iconTr, 0f, 0f, 0f, 0f);
            var iconImage = iconTr.gameObject.AddComponent<Image>();
            iconImage.color = Color.white;
            iconImage.raycastTarget = false;
            iconSprite.SetImage(iconImage);
            pressIndicator.SetImage(iconImage); // press feedback tints the same icon

            // ---- Radial cooldown overlay ----------------------------------------
            var radialTr = CreateChild(root, "RadialCooldown");
            Stretch(radialTr, 0f, 0f, 0f, 0f);
            var radialImage = radialTr.gameObject.AddComponent<Image>();
            radialImage.color = new Color(0f, 0f, 0f, 0.55f); // dim wedge tint
            radialImage.raycastTarget = false;
            var radial = radialTr.gameObject.AddComponent<BindableRadialCooldown>();
            radial.SetImage(radialImage);
            radial.SetRadialMaterial(CreateRadialMaterial());

            // ---- Name label -----------------------------------------------------
            var nameGo = new GameObject("Name", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(nameGo, UndoGroupName);
            var nameTr = (RectTransform)nameGo.transform;
            nameTr.SetParent(root, false);
            Stretch(nameTr, 0f, -24f, 0f, 0f); // bottom strip
            var nameText = nameGo.AddComponent<TextMeshProUGUI>();
            nameText.text = "?";
            nameText.fontSize = 14;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.raycastTarget = false;
            var nameBindable = nameGo.gameObject.AddComponent<BindableText>();
            nameBindable.SetText(nameText);

            // ---- Wire up ----------------------------------------------------------
            widget.Configure(new AbilitySlotViewModel(), iconSprite, nameBindable, pressIndicator, radial);

            Selection.activeGameObject = root.gameObject;
            Debug.Log($"[Niki.UI] Ability Slot created under {canvas.name}. " +
                       "Drive it via widget.ViewModel (Icon, Name, CooldownRemaining, IsPressed).");
        }

        private static Material CreateRadialMaterial()
        {
            var shader = Shader.Find(RadialShaderName);
            if (shader == null)
            {
                Debug.LogError($"[Niki.UI] Shader '{RadialShaderName}' not found.");
                return null;
            }

            return new Material(shader);
        }

        private static RectTransform CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, UndoGroupName);
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Stretch a RectTransform to its parent with the given inset offsets.</summary>
        private static void Stretch(RectTransform rt, float left, float bottom, float right, float top)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }
    }
}
