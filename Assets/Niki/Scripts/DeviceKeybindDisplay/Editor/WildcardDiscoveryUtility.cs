using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CupOHappiness.DeviceKeybindDisplay.Editor
{
    /// <summary>
    /// Output format:
    /// {Usage} (DisplayName) → Device → BindingDisplay (Resolved String)
    /// </summary>
    public static class WildcardDiscoveryUtility
    {
        private class UsageInfo
        {
            public string displayName;
            public SortedDictionary<string, List<BindingInfo>> devices = new();
        }

        private class BindingInfo
        {
            public string resolved;
            public string displayName;
        }

        [MenuItem("Window/CupOHappiness/Input/Discover ALL Possible Wildcard Resolutions")]
        public static void DiscoverAll()
        {
            var allLayoutNames = UnityEngine.InputSystem.InputSystem.ListLayouts()
                .OrderBy(n => n)
                .ToList();

            var usageMap = new SortedDictionary<string, UsageInfo>();
            var uniqueStrings = new SortedSet<string>();

            foreach (var layoutName in allLayoutNames)
            {
                try
                {
                    var layout = UnityEngine.InputSystem.InputSystem.LoadLayout(layoutName);
                    if (layout == null) continue;

                    if (layout.type == null || !typeof(InputDevice).IsAssignableFrom(layout.type))
                        continue;

                    InputDevice tempDevice = null;

                    try
                    {
                        tempDevice = UnityEngine.InputSystem.InputSystem.AddDevice(layoutName);
                        if (tempDevice == null) continue;

                        var controls = tempDevice.allControls
                            .Where(c => c.usages.Count > 0)
                            .ToList();

                        foreach (var control in controls)
                        {
                            string resolved = UnityEngine.InputSystem.InputControlPath.ToHumanReadableString(
                                control.path,
                                UnityEngine.InputSystem.InputControlPath.HumanReadableStringOptions.OmitDevice);

                            string bindingDisplay = !string.IsNullOrEmpty(control.displayName)
                                ? control.displayName
                                : control.name;

                            uniqueStrings.Add(resolved);

                            foreach (var usage in control.usages)
                            {
                                // Ensure usage entry
                                if (!usageMap.TryGetValue(usage, out var usageInfo))
                                {
                                    usageInfo = new UsageInfo();
                                    usageMap[usage] = usageInfo;
                                }

                                // Capture usage display name (first valid one wins)
                                if (string.IsNullOrEmpty(usageInfo.displayName))
                                {
                                    usageInfo.displayName = bindingDisplay;
                                }

                                // Ensure device entry
                                if (!usageInfo.devices.TryGetValue(layoutName, out var bindingList))
                                {
                                    bindingList = new List<BindingInfo>();
                                    usageInfo.devices[layoutName] = bindingList;
                                }

                                // Avoid duplicates
                                if (!bindingList.Any(b => b.resolved == resolved && b.displayName == bindingDisplay))
                                {
                                    bindingList.Add(new BindingInfo
                                    {
                                        resolved = resolved,
                                        displayName = bindingDisplay
                                    });
                                }
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        if (tempDevice != null)
                            UnityEngine.InputSystem.InputSystem.RemoveDevice(tempDevice);
                    }
                }
                catch { }
            }

            // Sort bindings per device
            foreach (var usage in usageMap.Values)
            {
                foreach (var device in usage.devices)
                {
                    device.Value.Sort((a, b) =>
                    {
                        int cmp = string.Compare(a.displayName, b.displayName, StringComparison.Ordinal);
                        if (cmp != 0) return cmp;
                        return string.Compare(a.resolved, b.resolved, StringComparison.Ordinal);
                    });
                }
            }

            // -------------------------
            // BUILD REPORT
            // -------------------------

            var report = new List<string>();

            report.Add("======================================================================================");
            report.Add("WILDCARD → DEVICE → RESOLUTION REPORT (WITH DISPLAY NAMES)");
            report.Add($"Generated on: {DateTime.Now}");
            report.Add($"Total Layouts Scanned: {allLayoutNames.Count}");
            report.Add("======================================================================================");

            foreach (var usageEntry in usageMap)
            {
                string usage = usageEntry.Key;
                string usageDisplay = usageEntry.Value.displayName ?? "Unknown";

                report.Add($"\nUSAGE: {{{usage}}} ({usageDisplay})");

                foreach (var deviceEntry in usageEntry.Value.devices)
                {
                    report.Add($"  DEVICE: {deviceEntry.Key}");

                    foreach (var binding in deviceEntry.Value)
                    {
                        report.Add($"    -> {binding.displayName} (\"{binding.resolved}\")");
                    }
                }
            }

            report.Add("\n======================================================================================");
            report.Add("SUMMARY: ALL DISCOVERED UNIQUE RESOLUTION STRINGS");
            report.Add("======================================================================================");

            foreach (var s in uniqueStrings)
            {
                report.Add($"  \"{s}\"");
            }

            report.Add("======================================================================================");

            Debug.Log(string.Join("\n", report));

            EditorUtility.DisplayDialog(
                "Discovery Complete",
                "Wildcard resolution report with display names generated. Check Console.",
                "OK");
        }
    }
}