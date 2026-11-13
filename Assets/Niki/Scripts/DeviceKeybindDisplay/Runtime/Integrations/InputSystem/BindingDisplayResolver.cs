using CupOHappiness.DeviceKeybindDisplay.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using InputSystemAPI = UnityEngine.InputSystem.InputSystem;

namespace CupOHappiness.DeviceKeybindDisplay.Integrations.InputSystem
{
    internal static class BindingDisplayResolver
    {
        internal readonly struct Element
        {
            public readonly string Text;
            public readonly string DeviceLayout;
            public readonly string IconKey;
            public readonly bool IsSeparator;

            public Element(string text, string deviceLayout = null, string iconKey = null, bool isSeparator = false)
            {
                Text = text;
                DeviceLayout = deviceLayout;
                IconKey = iconKey;
                IsSeparator = isSeparator;
            }
        }

        internal readonly struct Presentation
        {
            public readonly string Text;
            public readonly string DeviceLayout;
            public readonly string IconKey;

            public readonly Element[] Elements;
            public bool IsComposite => Elements != null;

            public Presentation(string text, string deviceLayout, string iconKey, Element[] elements = null)
            {
                Text = text;
                DeviceLayout = deviceLayout;
                IconKey = iconKey;
                Elements = elements;
            }
        }

        internal static InputAction FindAction(InputActionAsset actions, string mapName, string actionName)
        {
            if (actions == null || string.IsNullOrEmpty(actionName))
                return null;

            return (!string.IsNullOrEmpty(mapName)
                ? actions.FindAction($"{mapName}/{actionName}", throwIfNotFound: false)
                : null) ?? actions.FindAction(actionName, throwIfNotFound: false);
        }

        internal static bool TryResolve(InputAction action, int bindingIndex, InputDevice preferredDevice,
            Func<InputDevice, bool> allowedDevice, out Presentation presentation,
            CompositeBindingDisplayMode mode = CompositeBindingDisplayMode.SeparateIcons,
            DeviceDisplayConfigurator configurator = null)
        {
            presentation = default;
            if (action == null)
                return false;

            // Automatic prompts show the whole composite; explicit row targets show only their part.
            if (bindingIndex < 0)
            {
                var control = SelectControl(action, null, preferredDevice, allowedDevice);
                if (control == null)
                    return false;

                bindingIndex = action.GetBindingIndexForControl(control);
                if (bindingIndex < 0)
                    return false;

                preferredDevice = control.device;
                while (bindingIndex > 0 && action.bindings[bindingIndex].isPartOfComposite)
                    bindingIndex--;
            }

            if (bindingIndex >= action.bindings.Count)
                return false;

            var binding = action.bindings[bindingIndex];
            if (string.IsNullOrEmpty(binding.effectivePath))
                return false;

            return binding.isComposite
                ? TryResolveComposite(action, bindingIndex, preferredDevice, allowedDevice, mode, configurator, out presentation)
                : TryResolveBinding(action, bindingIndex, preferredDevice, allowedDevice, out presentation);
        }

        private static bool TryResolveBinding(InputAction action, int index, InputDevice preferredDevice,
            Func<InputDevice, bool> allowedDevice, out Presentation presentation)
        {
            presentation = default;
            var binding = action.bindings[index];
            if (string.IsNullOrEmpty(binding.effectivePath))
                return false;

            var control = SelectControl(action, binding.effectivePath, preferredDevice, allowedDevice);
            if (control == null)
            {
                // A player-scoped prompt must not fall back to another player's device.
                if (allowedDevice != null || binding.effectivePath.IndexOfAny(new[] { '*', '{' }) >= 0)
                    return false;

                string text = binding.ToDisplayString(out var layout, out var path,
                    InputBinding.DisplayStringOptions.DontIncludeInteractions);
                presentation = new Presentation(text, layout, path);
                return !string.IsNullOrEmpty(text);
            }

            string label = binding.ToDisplayString(
                InputBinding.DisplayStringOptions.DontIncludeInteractions, control);
            string controlPath = control.path.Substring(control.device.path.Length).TrimStart('/');
            presentation = new Presentation(label, control.device.layout, controlPath);
            return !string.IsNullOrEmpty(label);
        }

        private static InputControl SelectControl(InputAction action, string path, InputDevice preferredDevice,
            Func<InputDevice, bool> allowedDevice)
        {
            InputControl best = null;
            foreach (var candidate in action.controls)
            {
                if (candidate?.device == null || (allowedDevice != null && !allowedDevice(candidate.device)) ||
                    (path != null && !InputControlPath.Matches(path, candidate)))
                    continue;

                bool preferred = candidate.device == preferredDevice;
                bool bestPreferred = best?.device == preferredDevice;
                if (best == null || (preferred && !bestPreferred) ||
                    (preferred == bestPreferred && CompareControls(candidate, best) < 0))
                    best = candidate;
            }
            return best;
        }

        private static int CompareControls(InputControl left, InputControl right)
        {
            int result = string.Compare(left.path, right.path, StringComparison.OrdinalIgnoreCase);
            return result != 0 ? result : left.device.deviceId.CompareTo(right.device.deviceId);
        }

        private static bool TryResolveComposite(InputAction action, int index, InputDevice preferredDevice,
            Func<InputDevice, bool> allowedDevice, CompositeBindingDisplayMode mode,
            DeviceDisplayConfigurator configurator, out Presentation presentation)
        {
            presentation = default;
            var parts = new Dictionary<string, List<Presentation>>(StringComparer.OrdinalIgnoreCase);
            int alternativeCount = 0;
            bool anyResolved = false;
            for (int i = index + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
            {
                var part = action.bindings[i];
                bool resolved = TryResolveBinding(action, i, preferredDevice, allowedDevice, out var value);
                anyResolved |= resolved;
                string name = part.name ?? string.Empty;
                if (!parts.TryGetValue(name, out var alternatives))
                    parts[name] = alternatives = new List<Presentation>();
                alternatives.Add(resolved ? value : new Presentation("—", null, null));
                alternativeCount = Math.Max(alternativeCount, alternatives.Count);
            }
            if (!anyResolved)
                return false;

            string registeredName = action.bindings[index].effectivePath.Split('(')[0].Trim();
            var type = InputSystemAPI.TryGetBindingComposite(registeredName);
            string compositeName = (type?.Name ?? registeredName).ToLowerInvariant();
            var format = type?.GetCustomAttribute<DisplayStringFormatAttribute>()?.formatString;
            if (string.IsNullOrEmpty(format))
                format = string.Join("/", System.Linq.Enumerable.Select(parts.Keys, name => "{" + name + "}"));

            if (mode == CompositeBindingDisplayMode.FirstInList)
                alternativeCount = Math.Min(1, alternativeCount);

            var labels = new List<string>();
            string firstLayout = null;
            string firstKey = null;
            var elements = new List<Element>();
            for (int alternative = 0; alternative < alternativeCount; alternative++)
            {
                // Parallel entries select alternatives; singleton parts (e.g. Shift) remain required.
                var selected = new Dictionary<string, Presentation>(StringComparer.OrdinalIgnoreCase);
                foreach (var part in parts)
                    selected[part.Key] = part.Value.Count == 1 ? part.Value[0] :
                        alternative < part.Value.Count ? part.Value[alternative] : new Presentation("—", null, null);

                var group = BuildCompositeGroup(compositeName, format, selected, configurator);
                labels.Add(group.Text);
                if (alternative == 0)
                {
                    firstLayout = group.DeviceLayout;
                    firstKey = group.IconKey;
                }
                if (elements.Count > 0)
                    elements.Add(new Element(" / ", isSeparator: true));
                elements.AddRange(group.Elements);
            }

            string text = string.Join(" / ", labels);
            if (mode == CompositeBindingDisplayMode.CompactText)
                elements = new List<Element> { new Element(text, isSeparator: true) };
            bool singleIcon = alternativeCount == 1 && mode != CompositeBindingDisplayMode.CompactText;
            presentation = new Presentation(text, singleIcon ? firstLayout : null,
                singleIcon ? firstKey : null, elements.ToArray());
            return true;
        }

        private static Presentation BuildCompositeGroup(string compositeName, string format,
            Dictionary<string, Presentation> parts, DeviceDisplayConfigurator configurator)
        {
            string layout = null;
            bool complete = true;
            var keys = new List<string>();
            foreach (var part in parts)
            {
                var value = part.Value;
                if (string.IsNullOrEmpty(value.DeviceLayout) || string.IsNullOrEmpty(value.IconKey))
                    complete = false;
                if (layout == null)
                    layout = value.DeviceLayout;
                else if (!string.Equals(layout, value.DeviceLayout, StringComparison.OrdinalIgnoreCase))
                    complete = false;
                keys.Add($"{part.Key.ToLowerInvariant()}={value.IconKey?.ToLowerInvariant()}");
            }
            keys.Sort(StringComparer.Ordinal);
            string key = complete ? $"{compositeName}({string.Join(",", keys)})" : null;
            string text = Regex.Replace(format, @"\{([^{}]+)\}", match =>
                parts.TryGetValue(match.Groups[1].Value, out var value) ? value.Text : "—");
            string compactLabel = configurator != null ? configurator.GetDeviceBindingLabel(layout, key) : null;
            if (!string.IsNullOrEmpty(compactLabel))
                text = compactLabel;
            if (!string.IsNullOrEmpty(compactLabel) ||
                (configurator != null && configurator.GetDeviceBindingIcon(layout, key) != null))
                return new Presentation(text, layout, key, new[] { new Element(text, layout, key) });

            // Use Unity's format to keep required parts separate without modifier-specific rules.
            var elements = new List<Element>();
            int position = 0;
            foreach (Match match in Regex.Matches(format, @"\{([^{}]+)\}"))
            {
                if (match.Index > position)
                    elements.Add(new Element(format.Substring(position, match.Index - position), isSeparator: true));
                elements.Add(parts.TryGetValue(match.Groups[1].Value, out var part)
                    ? new Element(part.Text, part.DeviceLayout, part.IconKey)
                    : new Element("—"));
                position = match.Index + match.Length;
            }
            if (position < format.Length)
                elements.Add(new Element(format.Substring(position), isSeparator: true));
            return new Presentation(text, layout, key, elements.ToArray());
        }

        internal static bool OwnsChange(object changed, InputActionAsset actions)
        {
            if (actions == null)
                return false;
            return ReferenceEquals(changed, actions) ||
                (changed is InputActionMap map && ReferenceEquals(map.asset, actions)) ||
                (changed is InputAction action && ReferenceEquals(action.actionMap?.asset, actions));
        }
    }
}
