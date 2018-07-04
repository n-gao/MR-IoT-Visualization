using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using IoTVisualization.Localization;
using IoTVisualization.Networking;
using IoTVisualization.Networking.Utils;
using IoTVisualization.Visualization;
using UnityEngine;

namespace IoTVisualization.UserInterface
{
    /// <summary>
    /// This singleton creates for each displayed device an indicator to make it easier for the user to
    /// find all placed devices.
    /// </summary>
    public class HeadsUpDirectionIndicatorManager : Singleton<HeadsUpDirectionIndicatorManager>
    {
        public float Depth = 2;
        public float IndicatorMarginPercent = 0.1f;
        public Vector3 Pivot = new Vector3(0, 0.05f, 0);

        [SerializeField] private GameObject _indicatorPrefab;
        private readonly Dictionary<GameObject, HeadsUpDirectionIndicator> _indicators = new Dictionary<GameObject, HeadsUpDirectionIndicator>();
        
        // Use this for initialization
        void Start()
        {
            foreach (var pair in DeviceObjectManager.Instance.Objects)
            {
                InstantiateIndicator(pair.Key, pair.Value);
            }
            DeviceObjectManager.Instance.ObjectPlaced += InstantiateIndicator;
            DeviceObjectManager.Instance.ObjectRemoved += RemoveIndicator;
        }
        
        private void RemoveIndicator(IDevice device, GameObject target)
        {
            if (!_indicators.ContainsKey(target)) return;
            HeadsUpDirectionIndicator indicator = _indicators[target];
            Destroy(indicator.gameObject);
        }

        private void InstantiateIndicator(IDevice device, GameObject target)
        {
            GameObject indicatorObj = new GameObject("indicator_" + target.name);
            indicatorObj.transform.parent = transform;

            HeadsUpDirectionIndicator indicator = indicatorObj.AddComponent<HeadsUpDirectionIndicator>();
            indicator.Depth = Depth;
            indicator.TargetObject = target;
            indicator.IndicatorMarginPercent = IndicatorMarginPercent;
            indicator.PointerPrefab = _indicatorPrefab;
            indicator.Pivot = Pivot;
            _indicators[target] = indicator;

            DeviceWrapperHook deviceInfo = indicatorObj.AddComponent<DeviceWrapperHook>();
            deviceInfo.DeviceWrapper = target.GetComponent<DeviceWrapper>();
        }
    }
}
