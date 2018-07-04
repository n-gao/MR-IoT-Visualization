using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace IoTVisualization.Utils
{
    /// <summary>
    /// This component can be attached to the SpatialMapping GameObject in order to create an ripple effect when
    /// tapping on something. It also needs SpatialMappingTap shader to be used.
    /// </summary>
    public class OnTapEffect : MonoBehaviour, IInputClickHandler
    {
        private Material[] Materials
        {
            get { return GetComponentsInChildren<MeshRenderer>().SelectMany(m => m.materials).ToArray(); }
        }
        /// <summary>
        /// Speed of the wave
        /// </summary>
        public float Speed = 0.5f;
        /// <summary>
        /// Current radius
        /// </summary>
        private float _radius;

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
//	    if (!Enabled) return;
            for (int i = 0; i < Materials.Length; i++)
            {
                var material = Materials[i];
#if UNITY_EDITOR
                _radius += Time.unscaledDeltaTime * Speed * 10;
#else
                _radius += Time.unscaledDeltaTime * Speed;
#endif
                if (_radius > 25f)
                {
                    return;
                }
                material.SetFloat("_Radius", _radius);
            }
        }
        
        /// <summary>
        /// Starts the effect at the given point as origin.
        /// </summary>
        /// <param name="origin">Origin</param>
        public void StartEffect(Vector3 origin)
        {
            _radius = -0.5f;
            for (int i = 0; i < Materials.Length; i++)
            {
                var material = Materials[i];
                material.SetVector("_Center", origin);
            }
        }
        
        public void OnInputClicked(InputClickedEventData eventData)
        {
            //Casting a raycast in order to find the new origin.
            var camTransform = Camera.main.transform;
            Ray ray = new Ray(camTransform.position, camTransform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
                StartEffect(hit.point);
        }
    }
}