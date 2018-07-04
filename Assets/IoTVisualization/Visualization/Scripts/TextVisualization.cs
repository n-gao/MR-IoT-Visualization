using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HoloToolkit.Unity;
using IoTVisualization.Networking;
using IoTVisualization.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// This visualization is used to visualize text data. If the given string is a JSON, it will be split down into a more readable key value listing.
    /// </summary>
	public class TextVisualization : Visualization
    {
        private const string NullValue = "null";

        private TextMesh _labelText;
		private TextMesh _valueText;

		private string _value = "";
        
	    private int _maxLength = 0;

		// Use this for initialization
		protected override void Start ()
		{
			base.Start();
			_labelText = transform.Find("Label").GetComponent<TextMesh>();
			_valueText = transform.Find("Value").GetComponent<TextMesh>();
			if (_labelText == null || _valueText == null)
			{
				Destroy(this);
				return;
			}

            _labelText.text = Attribute.DisplayName;
            
		    Attribute.ValueModified += SourceValueModified;

		    SourceValueModified(Attribute.LatestValue);
		}

        void OnDestroy()
        {
            Attribute.ValueModified -= SourceValueModified;
        }

        /// <summary>
        /// Parses a given data, if it is a JSON it will be broken down into a more readable format.
        /// In addition to that the size of the visualization is updated because longer value could cause in a otherwise 
        /// inapproriate displaying.
        /// </summary>
        /// <param name="ioTData">Data</param>
		private void SourceValueModified(IoTData ioTData)
		{
		    string valueString = ioTData.AsString;
		    int highestLength = _maxLength,
                lineCount = 0;
            //Checking for JSON Objects
            try
		    {
		        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(valueString);
                StringBuilder builder = new StringBuilder();
		        foreach (var pair in dict)
		        {
		            int length = pair.Key.Length + 4 + (pair.Value != null ? pair.Value.Length : NullValue.Length);
		            if (length > highestLength)
		            {
		                highestLength = length;
		                _maxLength = length;
		            }
		        }
		        foreach (var pair in dict)
		        {
		            string value = pair.Value ?? NullValue;
		            builder.AppendLine(pair.Key.PadRight(highestLength - value.Length, ' ') + value);
		        }
		        valueString = builder.ToString();
		        lineCount = dict.Count;
                
		    } catch(Exception)
            {
                var lines = valueString.Split('\n');
                highestLength = lines.Max(l => l.Length);
                lineCount = lines.Length;
            }
			_value = valueString;
            //A few a bit strange multiplications to determin the size. Needs to be done due to text scaling
            float labelNormFontSize = _labelText.fontSize / 64.0f;
		    float valNormFontSize = _valueText.fontSize / 64.0f;
		    string name = Attribute.DisplayName;
		    float width = name.Length > highestLength ? 0.013f * (name.Length * labelNormFontSize) : 0.015f * (highestLength * valNormFontSize);
            Size = new Vector2(width, 0.055f * labelNormFontSize + 0.0265f * valNormFontSize * lineCount);
        }

		// Update is called once per frame
		void Update ()
		{
			_valueText.text = _value;
		}
	}
}
