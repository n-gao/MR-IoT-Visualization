using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.VR.WSA;

namespace IoTVisualization.Utils
{
    public class SpatialMappingController : Singleton<SpatialMappingController>
    {
        public bool FreezeUpdates
        {
            get { return _spatialMappers.Length == 0 ? true : _spatialMappers[0].freezeUpdates; }
            set
            {
                for (int i = 0; i < _spatialMappers.Length; i++)
                    _spatialMappers[i].freezeUpdates = value;
            }
        }

        private SpatialMappingBase[] _spatialMappers; 
        // Use this for initialization
        void Start()
        {
            _spatialMappers = GetComponents<SpatialMappingBase>();
        }
    }
}

