using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.IoTVisualization_Tests.Scripts
{
    public class PositionData
    {
        private float[] _rotation = {0, 0, 0, 0 };
        public Quaternion Rotation
        {
            get
            {
                return new Quaternion(_rotation[0], _rotation[1], _rotation[2], _rotation[3]);
            }
            set
            {
                _rotation = new[] { value.x, value.y, value.z, value.w };
            }
        }

        private float[] _position = {0, 0, 0};
        public Vector3 Position
        {
            get
            {
                return new Vector3(_position[0], _position[1], _position[2]);
            }
            set
            {
                _position = new []{ value.x, value.y , value.z};
            }
        }

        private class SavedData
        {
            public float[] pos;
            public float[] rot;
        }

        public PositionData()
        {
            
        }

        public PositionData(Transform t)
        {
            Rotation = t.rotation;
            Position = t.position;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(new SavedData { pos = _position, rot = _rotation });
        }

        public static PositionData Parse(string s)
        {
            PositionData result = new PositionData();
            SavedData sd = JsonConvert.DeserializeObject<SavedData>(s);
            result._position = sd.pos;
            result._rotation = sd.rot;
            return result;
        }
    }
}
