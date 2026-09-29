using System;
using UnityEngine;

namespace CupOHappiness.ProceduralProgressBars.Documentation
{
public class Readme : ScriptableObject
{
    public Texture2D icon;
    public string title;
    public Section[] sections;
    public bool loadedLayout;

    [Serializable]
    public class Section
    {
        public string heading;
        [TextArea(3, 20)] public string text;
        public Texture2D[] images;
        public string linkText;
        public string url;
    }
}
}