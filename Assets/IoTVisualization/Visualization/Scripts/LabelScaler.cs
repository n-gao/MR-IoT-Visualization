using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// Attaching this component causes the transform to always have a lossyScale of (1,1,1).
    /// This is used in order to display text properly.
    /// </summary>
    public class LabelScaler : MonoBehaviour
    {
        /// <summary>
        /// Adjusts the size properly.
        /// </summary>
        public void AdjustScale()
        {
            transform.localScale = Vector3.one.Div(transform.parent.lossyScale);
        }

        void OnWillRenderObject()
        {
            AdjustScale();
        }

        void OnValidate()
        {
            AdjustScale();
        }
    }

}