using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IoTVisualization.Utils;
using IoTVisualization.Networking;
using UnityEngine;

namespace IoTVisualization.Networking.Utils
{
    /// <summary>
    /// This class can be used as a storage option for devices. It provides a fast way to get data in a certain timespan and
    /// a maximum capacity to improve performance.
    /// </summary>
    public class AttributeStore : SortedList<DateTimeOffset, IoTData>
    {
        private const int MaxEntries = 1000;

        private readonly Queue<DateTimeOffset> _addedValues = new Queue<DateTimeOffset>();

        public IoTData Oldest
        {
            get { return EarliestValueAfter(DateTimeOffset.MinValue); }
        }

        public IoTData Latest
        {
            get { return LatestValueBefore(DateTimeOffset.MaxValue); }
        }
        
        /// <summary>
        /// Adds a new value.
        /// </summary>
        /// <param name="data">Value to be added.</param>
        public void Add(IoTData data)
        {
            this[data.Time] = data;
            _addedValues.Enqueue(data.Time);
            while (_addedValues.Count > MaxEntries)
                Remove(_addedValues.Dequeue());
        }

        /// <summary>
        /// Return all entries between the given timestamps.
        /// </summary>
        /// <param name="from">Start</param>
        /// <param name="to">End</param>
        /// <returns>All entries between start and end.</returns>
        public IoTData[] GetDataFromTo(DateTimeOffset from, DateTimeOffset to)
        {
            int lower = GetIndexOfNextHigherKey(from);
            int higher = GetIndexOfNextLowerKey(to);
            IoTData[] result = new IoTData[higher - lower + 1];
            var vals = Values;
            int c = 0;
            for (int i = lower; i <= higher; i++)
                    result[c++] = vals[i];
            return result;
        }

        /// <summary>
        /// Returns the next lower entry.
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Next lower entry.</returns>
        public IoTData LatestValueBefore(DateTimeOffset time)
        {
            return Values[Count - 1];
        }
        
        /// <summary>
        /// Returns the next higher entry.
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Next higher entry.</returns>
        public IoTData EarliestValueAfter(DateTimeOffset time)
        {
            return Values[0];
        }

        /// <summary>
        /// Clears all stored data.
        /// </summary>
        public void Reset()
        {
            Clear();
            _addedValues.Clear();
        }

        /// <summary>
        /// Returns all entries between the given timestamps. This method also includes LatestValueBefore(start) and EarliestValueAfter(end).
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <returns>Data</returns>
        public IoTData[] GetDataFromToInclusive(DateTimeOffset start, DateTimeOffset end)
        {
            int lower = GetIndexOfNextLowerKey(start);
            int higher = GetIndexOfNextHigherKey(end);
            IoTData[] result = new IoTData[higher - lower + 1];
            var vals = Values;
            int c = 0;
            for (int i = lower; i <= higher; i++)
                result[c++] = vals[i];
            return result;
        }
        

        private int GetIndexOfNextHigherKey(DateTimeOffset key)
        {
            IList<DateTimeOffset> keys = Keys;
            int left = 0;
            int right = keys.Count - 1;

            while (left <= right)
            {
                int middle = left + ((right - left) / 2);

                if (keys[middle] == key)
                    return middle;

                if (keys[middle] > key)
                    right = middle - 1;
                else
                    left = middle + 1;

            }

            return left >= Count ? Count - 1 : left;
        }
        
        private int GetIndexOfNextLowerKey(DateTimeOffset key)
        {
            IList<DateTimeOffset> keys = Keys;
            int left = 0;
            int right = keys.Count - 1;
            while (left <= right)
            {
                int middle = left + ((right - left) / 2);
                var current = keys[middle];
                if (current == key)
                    return middle;

                if (current > key)
                    right = middle - 1;
                else
                    left = middle + 1;
            }

            return right < 0 ? 0 : right;
        }
    }

}
