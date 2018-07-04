using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This component sets the color of all renderers to the color of the linked device color.
    /// </summary>
    public class PointerColor : MonoBehaviour
    {
        private Renderer[] _renderers;
        
        void Start()
        {
            DeviceWrapperHook hook = GetComponentInParent<DeviceWrapperHook>();
            if (hook == null) return;
            ColorAssigner colorAssigner = hook.DeviceWrapper.GetComponent<ColorAssigner>();
            if (colorAssigner == null) return;
            _renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].material.color = colorAssigner.Color;
            }
        }
    }
}