using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace IoTVisualization.Utils
{
    public class MeasureOnTap : MonoBehaviour, IInputClickHandler
    {
        public void OnInputClicked(InputClickedEventData eventData)
        {
            if (FpsMeasure.IsInitialized)
                FpsMeasure.Instance.StartMeasure();
        }
    }
}
