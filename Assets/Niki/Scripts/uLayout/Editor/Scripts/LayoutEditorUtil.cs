/*
    Copyright (c) 2026 Nikita Mitrovich

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
*/
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CupOHappiness.UI
{
    public static class LayoutEditorUtil
    {
#if UNITY_6000_3_OR_NEWER
        [MenuItem("GameObject/UI (Canvas)/Layout/Layout", false, 9)]
#else
        [MenuItem("GameObject/UI/Layout/Layout", false, 9)]
#endif
        public static void CreateLayoutObject(MenuCommand command) {
            GameObject g = new GameObject("Layout");
            GameObjectUtility.SetParentAndAlign(g, command.context as GameObject);
            
            g.AddComponent<RectTransform>();
            g.AddComponent<Layout>();
            
            Undo.RegisterCreatedObjectUndo(g, "Create " + g.name);
            Selection.activeObject = g;
        }
        
#if UNITY_6000_3_OR_NEWER
        [MenuItem("GameObject/UI (Canvas)/Layout/Layout Text", false, 10)]
#else
        [MenuItem("GameObject/UI/Layout/Layout Text", false, 10)]
#endif
        public static void CreateLayoutTextObject(MenuCommand command) {
            GameObject g = new GameObject("LayoutText");
            GameObjectUtility.SetParentAndAlign(g, command.context as GameObject);
            
            g.AddComponent<RectTransform>();
            TextMeshProUGUI t = g.AddComponent<TextMeshProUGUI>();
            t.text = "New Text";
            t.alignment = TextAlignmentOptions.Capline;
            g.AddComponent<LayoutText>();
            
            Undo.RegisterCreatedObjectUndo(g, "Create " + g.name);
            Selection.activeObject = g;
        }
        
#if UNITY_6000_3_OR_NEWER
        [MenuItem("GameObject/UI (Canvas)/Layout/Layout Item", false, 11)]
#else
        [MenuItem("GameObject/UI/Layout/Layout Item", false, 11)]
#endif
        public static void CreateLayoutItemObject(MenuCommand command) {
            GameObject g = new GameObject("LayoutItem");
            GameObjectUtility.SetParentAndAlign(g, command.context as GameObject);
            
            g.AddComponent<RectTransform>();
            g.AddComponent<LayoutItem>();
            
            Undo.RegisterCreatedObjectUndo(g, "Create " + g.name);
            Selection.activeObject = g;
        }

#if UNITY_6000_3_OR_NEWER
        [MenuItem("GameObject/UI (Canvas)/Layout/Grid Layout", false, 12)]
#else
        [MenuItem("GameObject/UI/Layout/Grid Layout", false, 12)]
#endif
        public static void CreateGridLayoutObject(MenuCommand command) {
            GameObject g = new GameObject("GridLayout");
            GameObjectUtility.SetParentAndAlign(g, command.context as GameObject);

            g.AddComponent<RectTransform>();
            g.AddComponent<GridLayout>();

            Undo.RegisterCreatedObjectUndo(g, "Create " + g.name);
            Selection.activeObject = g;
        }

#if UNITY_6000_3_OR_NEWER
        [MenuItem("GameObject/UI (Canvas)/Layout/Tabs/Tab Group", false, 20)]
#else
        [MenuItem("GameObject/UI/Layout/Tabs/Tab Group", false, 20)]
#endif
        public static void CreateTabGroupObject(MenuCommand command) {
            GameObject g = new GameObject("Tab Group");
            GameObjectUtility.SetParentAndAlign(g, command.context as GameObject);

            g.AddComponent<RectTransform>();
            g.AddComponent<Layout>();
            g.AddComponent<ToggleGroup>();
            g.AddComponent<TabGroup>();

            Undo.RegisterCreatedObjectUndo(g, "Create " + g.name);
            Selection.activeObject = g;
        }

#if UNITY_6000_3_OR_NEWER
        [MenuItem("GameObject/UI (Canvas)/Layout/Tabs/Tab Button", false, 21)]
#else
        [MenuItem("GameObject/UI/Layout/Tabs/Tab Button", false, 21)]
#endif
        public static void CreateTabButtonObject(MenuCommand command) {
            GameObject g = new GameObject("Tab Button");
            GameObjectUtility.SetParentAndAlign(g, command.context as GameObject);

            g.AddComponent<RectTransform>();
            g.AddComponent<CanvasRenderer>();
            Image image = g.AddComponent<Image>();
            g.AddComponent<Toggle>();
            g.AddComponent<LayoutItem>();
            TabButton button = g.AddComponent<TabButton>();
            button.SetTargetGraphic(image);

            TabGroup group = g.GetComponentInParent<TabGroup>();
            if(group)
                group.Register(button);

            Undo.RegisterCreatedObjectUndo(g, "Create " + g.name);
            Selection.activeObject = g;
        }
    }
}

