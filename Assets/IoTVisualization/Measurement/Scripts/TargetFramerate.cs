using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using HoloToolkit.Unity;
using IoTVisualization.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace IoTVisualization.Measurement
{
    public class TargetFramerate: Singleton<TargetFramerate>
    {
        [SerializeField]private int _targetFramerate = 30;
        public int TargetFrameRate
        {
            get
            {
                return _targetFramerate;
                
            }
            set
            {
                _targetFramerate = value;
                if (_targetFramerate != 0)
                    _targetMilisec = 750 / _targetFramerate;
            }
        }

        private int _targetMilisec = 750 / 30;
        
        private readonly Stopwatch _stopwatch = new Stopwatch();

        void Start()
        {
            TargetFrameRate = _targetFramerate;
            var coroutine = WaitForTargetFramerate();
            StartCoroutine(coroutine);
        }

        public static void SetFrameRate(int frameRate)
        {
            if (AsyncUtil.IsInitialized)
            {
                AsyncUtil.Instance.Enqueue(() =>
                {
                    if (IsInitialized)
                        Instance.TargetFrameRate = frameRate;
                    else
                        Application.targetFrameRate = frameRate;
                });
            }
            else
            {
                if (IsInitialized)
                    Instance.TargetFrameRate = frameRate;
                else
                    Application.targetFrameRate = frameRate;
            }
            Debug.Log("Set framerate to " + frameRate);
        }
        
        private IEnumerator WaitForTargetFramerate()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (_targetFramerate > 0)
                {
                    long elapsed = _stopwatch.ElapsedMilliseconds;
                    int toWait = (int)(_targetMilisec - elapsed);
                    if (toWait > 0)
#if !WINDOWS_UWP
                        Thread.Sleep(toWait);
#else
                        System.Threading.Tasks.Task.Delay(toWait).Wait();
#endif
                    _stopwatch.Reset();
                    _stopwatch.Start();
                }
            }
        }
    }
}
