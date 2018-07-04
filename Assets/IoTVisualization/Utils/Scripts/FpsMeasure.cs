using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoloToolkit.Unity;
using IoTVisualization.Localization;
using UnityEngine;

namespace IoTVisualization.Utils
{
    public class FpsMeasure : Singleton<FpsMeasure>
    {
        private string _dir;
        public float Duration = 10;
        private string _fileName;
        private float _startTime = 0;
        private readonly List<float> _measures = new List<float>();
        public bool Capturing { get; set; }
        /// <summary>
        /// Starts the measurement of frame times.
        /// </summary>
        public void StartMeasure(string fileName = null)
        {
            if (Capturing) return;
            int count = DeviceObjectManager.Instance.Objects.Count;
            long timeStamp = (long)(DateTimeOffset.UtcNow - DateTimeHelper.UtcMin).TotalMilliseconds;
            _fileName = Path.Combine(_dir, string.IsNullOrEmpty(fileName) ? string.Format("measure_{0}_{1}.txt", count, timeStamp) : fileName);
            Debug.LogWarning(_fileName);
            _startTime = Time.time;
            Capturing = true;
            _measures.Clear();
        }

        void Start()
        {
            _dir = Path.Combine(Application.persistentDataPath, "Measures");
            Directory.CreateDirectory(_dir);
            Capturing = false;
        }

        void Update()
        {
            if (!Capturing) return;
            if (Time.time - _startTime <= Duration)
            {
                _measures.Add(Time.deltaTime);
            }
            else
            {
                File.WriteAllLines(_fileName, _measures.Select(m => m.ToString("R")).ToArray());
                print("finished capturing fps\n avg: " + _measures.Average(m => 1f/m).ToString("R"));
                Capturing = false;
            }
        }
    }

}