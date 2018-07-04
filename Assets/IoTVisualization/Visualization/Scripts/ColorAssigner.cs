using System.Collections;
using System.Collections.Generic;
using IoTVisualization.Utils;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    public class ColorAssigner : MonoBehaviour
    {
        private const int ColorStepSize = 15;
        private static int _index = 0;
        private static readonly Color[] Colors;

        static ColorAssigner()
        {
            List<Color> colors = new List<Color>();
            for (int i = 0; i < 360; i += ColorStepSize)
                colors.Add(Color.HSVToRGB(i / 360.0f, 0.75f, 0.75f));
            //Randomizing the colors
            colors.Shuffle();
            Colors = colors.ToArray();
        }

        public Color Color { get; set; }

        void Awake()
        {
            Color = Colors[_index++];
            if (_index >= Colors.Length)
                _index = 0;
        }
    }
}