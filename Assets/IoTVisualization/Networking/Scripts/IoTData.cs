using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using IoTVisualization.Utils;
using UnityEngine;

namespace IoTVisualization.Networking
{
    /// <summary>
    /// Data from an IoT-enviroment.
    /// </summary>
    [Serializable]
    public class IoTData
    {
        /// <summary>
        /// Attribute this data belongs to.
        /// </summary>
        public IAttribute Attribute { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTimeOffset Time { get; set; }
        /// <summary>
        /// Indicated whether this Data should be handled as string, otherwise it should be handled as float.
        /// </summary>
        public bool IsString
        {
            get { return StringValue != null; }
        }
        /// <summary>
        /// String value of this data. Is null when FloatValue != NaN.
        /// </summary>
        public string StringValue { get; set; }
        /// <summary>
        /// Float value of this data. Is NaN when it is not convertible to a float.
        /// </summary>
        public float FloatValue { get; set; }

        public IoTData()
        {
            FloatValue = float.NaN;
        }
        /// <summary>
        /// Returns StringValue or FloatValue as string.
        /// </summary>
        public string AsString
        {
            get { return IsString ? StringValue : FloatValue.ToString("F"); }
        }

        public override string ToString()
        {
            return Attribute.AttributeName + " " + (IsString ? StringValue : FloatValue.ToString("N")) + " " + Time;
        }
        
        /// <summary>
        /// Parses 2 objects. First one will be interpreted as time and should be a number and the second one represenets the value and should be a string or a number.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IoTData Parse(object time, object value)
        {
            try
            {
                if (time == null || value == null)
                    throw new ArgumentNullException();
                IoTData result = new IoTData();
                double d = Convert.ToDouble(time);
                result.Time = DateTimeHelper.UtcMin.AddSeconds(d);
                try
                {
                    result.FloatValue = Convert.ToSingle(value);
                }
                catch (Exception)
                {
                    result.StringValue = Convert.ToString(value);
                }
                return result;
            }
            catch (Exception)
            {
//                Debug.LogError("[Network] Could not parse to IoTData.\n" + time + "\n" + value + "\n" + e.Message + "\n" + e.StackTrace);
                throw;
            }
        }
    }
}
