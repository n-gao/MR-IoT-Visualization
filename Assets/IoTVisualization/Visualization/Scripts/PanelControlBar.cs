using HoloToolkit.Unity;
using IoTVisualization.Utils;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This component manages the controlbar at the top of each visualization window. It adjusts its size and position
    /// to fit to its window.
    /// 
    /// NOT USED ANYMORE
    /// </summary>
    public class PanelControlBar : MonoBehaviour
    {

        private Transform _panel;

        // Use this for initialization
        void Start ()
        {
            if (_panel == null)
                _panel = transform.parent;
            ColorAssigner assigner = GetComponentInParent<ColorAssigner>();
            if (assigner != null)
                GetComponent<Renderer>().material.color = assigner.Color;
        }
	
        // Update is called once per frame
        void Update ()
        {
            transform.localScale = transform.localScale.WithY(0.025f / _panel.localScale.y);
            transform.localPosition = new Vector3(0, transform.localScale.y/2 + 0.5f + 0.005f * 1 / _panel.localScale.y);
        }
    }
}
