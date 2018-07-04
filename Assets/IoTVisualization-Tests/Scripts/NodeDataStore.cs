using System;
using System.Collections.Generic;
using IoTVisualization.Networking.Utils;

namespace IoTVisualization.Networking.EnEffCampus
{
    class NodeDataStore : Dictionary<string, AttributeStore>
    {
        public IoTData First { get; private set; }

        public IoTData Last { get; private set; }

        public void AddValue(IoTData data)
        {
            if (data == null)
                throw new ArgumentException("Data must not be null.");
            if (First == null)
                First = data;
            if (!ContainsKey(data.Attribute.AttributeName))
                this[data.Attribute.AttributeName] = new AttributeStore();
            Last = this[data.Attribute.AttributeName][data.Time] = data;
        }

    }
}
