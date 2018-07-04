using UnityEngine;

namespace IoTVisualization.Utils
{
    /// <summary>
    /// This static class contains a few extension methods for Vector3.
    /// </summary>
    public static class Vector3Extension {
        /// <summary>
        /// Returns a vector with the same y and z values and the given x value.
        /// </summary>
        /// <param name="self">original vector</param>
        /// <param name="x">New x value</param>
        /// <returns>(x, self.y, self.z)</returns>
        public static Vector3 WithX(this Vector3 self, float x)
        {
            return new Vector3(x, self.y, self.z);
        }

        /// <summary>
        /// Returns a vector with the same x and z values and the given y value.
        /// </summary>
        /// <param name="self">original vector</param>
        /// <param name="x">New y value</param>
        /// <returns>(self.x, y, self.z)</returns>
        public static Vector3 WithY(this Vector3 self, float y)
        {
            return new Vector3(self.x, y, self.z);
        }

        /// <summary>
        /// Returns a vector with the same x and y values and the given z value.
        /// </summary>
        /// <param name="self">original vector</param>
        /// <param name="x">New z value</param>
        /// <returns>(self.x, self.y, z)</returns>
        public static Vector3 WithZ(this Vector3 self, float z)
        {
            return new Vector3(self.x, self.y, z);
        }

        /// <summary>
        /// A ease in out animation for vector3.
        /// </summary>
        /// <param name="self">Start vector</param>
        /// <param name="change">Change</param>
        /// <param name="time">Elapsed time</param>
        /// <param name="duration">Whole duration</param>
        /// <returns>Vector at the given time.</returns>
        public static Vector3 EaseInOut(this Vector3 self, Vector3 change, float time, float duration)
        {
            time /= duration / 2;
            if (time < 1) return change / 2 * time * time + self;
            time--;
            return -change / 2 * (time * (time - 2) - 1) + self;
        }
        
    }
}
