using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace IoTVisualization.Utils
{
    /// <summary>
    /// This class contains extension methods for Lists.
    /// </summary>
    public static class ListExtension
    {
        private static readonly Random Rng = new Random();
        /// <summary>
        /// Shuffles all elements inside the list using Fisher-Yates-shuffle.
        /// </summary>
        /// <typeparam name="T">Type of the list</typeparam>
        /// <param name="list">List to shuffle</param>
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n >= 1)
            {
                n--;
                int k = Rng.Next(n + 1);
                T tmp = list[n];
                list[n] = list[k];
                list[k] = tmp;
            }
        }
    }
}
