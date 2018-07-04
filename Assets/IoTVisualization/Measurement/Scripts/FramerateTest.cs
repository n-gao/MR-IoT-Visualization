using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace IoTVisualization.Measurement
{
    /// <summary>
    /// This component changes the target framerate of this application by each click on its colliders.
    /// It should be placed on the spatial mapping GameObject.
    /// </summary>
    public class FramerateTest : MonoBehaviour, IInputClickHandler
    {
        public int[] Steps = { 60, 30, 15, 8 };
        private int _step = 0;

        private int Current { get { return Steps[_step]; } }

        // Use this for initialization
        void Start()
        {
            Application.targetFrameRate = Current;
        }

        public void NextStep()
        {
            _step++;
            _step = _step >= Steps.Length ? 0 : _step;
            Application.targetFrameRate = Current;
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            NextStep();
        }
    }
}
