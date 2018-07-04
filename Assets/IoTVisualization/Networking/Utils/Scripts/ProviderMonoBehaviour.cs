using System;
using IoTVisualization.Networking.EnEffCampus;
using UnityEngine;
using System.Linq;
using IoTVisualization.Utils;

namespace IoTVisualization.Networking.Utils
{
    /// <summary>
    /// This abstract class provides easier access to the current IoT-Provider.
    /// </summary>
	public abstract class ProviderMonoBehaviour : MonoBehaviour
    {
        public static IProvider Provider { get; set; }
	}
}
