using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Examples.InteractiveElements;
using HoloToolkit.Unity.SpatialMapping;
using IoTVisualization.Utils;
using UnityEngine;

namespace IoTVisualization.UserInterface
{
    public class ScanButton : MonoBehaviour
    {
        private InteractiveToggle _toggle;

        void Start()
        {
            _toggle = GetComponent<InteractiveToggle>();
            StartScan();
        }

        public void StartScan()
        {
            if (SpatialMappingController.IsInitialized)
                SpatialMappingController.Instance.FreezeUpdates = false;
            if (SpatialMappingManager.IsInitialized)
                SpatialMappingManager.Instance.SurfaceObserver.StartObserving();
            _toggle.HasSelection = true;
        }

        public void StopScan()
        {
            if (SpatialMappingController.IsInitialized)
                SpatialMappingController.Instance.FreezeUpdates = true;
            if (SpatialMappingManager.IsInitialized)
                SpatialMappingManager.Instance.SurfaceObserver.StopObserving();
            _toggle.HasSelection = false;
        }
    }

}