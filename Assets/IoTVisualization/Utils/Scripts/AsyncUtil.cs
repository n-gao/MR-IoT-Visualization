using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HoloToolkit.Unity;

namespace IoTVisualization.Utils
{
    /// <summary>
    /// A small util singleton in order to execute Actions during game loop.
    /// </summary>
    public class AsyncUtil : Singleton<AsyncUtil>
    {
        private readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

        /// <summary>
        /// Enqueues action which will be executed during the next game loop.
        /// </summary>
        /// <param name="action">Action</param>
        public void Enqueue(Action action)
        {
            _actions.Enqueue(action);
        }

        void Update()
        {
            while (_actions.Count > 0)
                _actions.Dequeue()();
        }
    }
}
