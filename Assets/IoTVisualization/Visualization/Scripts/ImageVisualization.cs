using System;
using UnityEngine;

namespace IoTVisualization.Visualization
{
    /// <summary>
    /// Used for to visualize an image. The metadata must include information about width, height and format of the image.
    /// </summary>
    public class ImageVisualization : Visualization
    {
        /// <summary>
        /// Metadata key used to get the width of the image.
        /// </summary>
        public string WidthKey = "width";
        /// <summary>
        /// Metadata key used to get the height of the image.
        /// </summary>
        public string HeightKey = "height";
        /// <summary>
        /// Metadata key used to get the format of the image.
        /// </summary>
        public string ImageFormatKey = "format";

        private int _width;
        private int _height;
        private int _imageFormat;
        private Texture2D _texture;

        protected override void Start()
        {
            base.Start();
            ReadMetaData();
            Attribute.MetaDataModified += (key, value) =>
            {
                ReadMetaData();
            };
            Attribute.ValueModified += data =>
            {
                _texture = ToTexture(data.StringValue);
            };
        }

        /// <summary>
        /// Reads width, height and format from metadata.
        /// </summary>
        private void ReadMetaData()
        {
            _width = int.Parse(Attribute.MetaData[WidthKey]);
            _height = int.Parse(Attribute.MetaData[HeightKey]);
            _imageFormat = int.Parse(Attribute.MetaData[ImageFormatKey]);
        }

        /// <summary>
        /// Converts the given Base64 string to an image and returns it as a texture.
        /// </summary>
        /// <param name="dataString">Base64 data string</param>
        /// <returns>Image as Texture2D</returns>
        private Texture2D ToTexture(string dataString)
        {
            byte[] data = Convert.FromBase64String(dataString);
            Texture2D result = new Texture2D(_width, _height, (TextureFormat)_imageFormat, false);
            result.LoadImage(data);
            return result;
        }
    }
}
