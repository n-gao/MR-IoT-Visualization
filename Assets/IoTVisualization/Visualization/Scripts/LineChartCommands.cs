using System;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This class contains methodus which can be used as voice commands.
    /// </summary>
    public class LineChartCommands : MonoBehaviour {

        private LineChartController GetFocusedChart()
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                return hit.transform.GetComponent<LineChartController>();
            }
            return null;
        }

        /// <summary>
        /// If the user is not looking at a line chart this command will do nothing.
        /// Otherwise the line chart will be paused.
        /// </summary>
        public void Pause()
        {
            print("[VoiceCommand]Pause");
            var controller = GetFocusedChart();
            if (controller == null) return;
            controller.Status = LineChartController.PlayStatus.Paused;
        }


        /// <summary>
        /// If the user is not looking at a line chart this command will do nothing.
        /// Otherwise the line chart will continue to play.
        /// </summary>
        public void Play()
        {
            print("[VoiceCommand]Play");
            var controller = GetFocusedChart();
            if (controller == null) return;
            controller.Status = LineChartController.PlayStatus.Play;
            controller.OverrideVelocity = null;
        }


        /// <summary>
        /// If the user is not looking at a line chart this command will do nothing.
        /// Otherwise the line chart will go fast backwards. Calling this commands more often increases the speed.
        /// </summary>
        public void Backward()
        {
            print("[VoiceCommand]Backward");
            var controller = GetFocusedChart();
            if (controller == null) return;
            controller.Status = LineChartController.PlayStatus.Play;
            controller.OverrideVelocity = (controller.OverrideVelocity ?? 1) < 0 ? 2 * controller.OverrideVelocity ?? -2 : -2;
        }

        /// <summary>
        /// If the user is not looking at a line chart this command will do nothing.
        /// Otherwise the line chart will go fast forwards. Calling this commands more often increases the speed.
        /// </summary>
        public void Forward()
        {
            print("[VoiceCommand]Forward");
            var controller = GetFocusedChart();
            if (controller == null) return;
            controller.Status = LineChartController.PlayStatus.Play;
            controller.OverrideVelocity = (controller.OverrideVelocity ?? 1) > 0 ? 2 * controller.OverrideVelocity ?? 2 : 2;
        }


        /// <summary>
        /// If the user is not looking at a line chart this command will do nothing.
        /// Otherwise the line chart will jump to realtime.
        /// </summary>
        public void Realtime()
        {
            print("[VoiceCommand]Realtime");
            var controller = GetFocusedChart();
            if (controller == null) return;
            controller.Status = LineChartController.PlayStatus.Play;
            controller.OverrideVelocity = null;
            controller.GetComponent<LineChart>().XMax = controller.Max;
        }
    }
}
