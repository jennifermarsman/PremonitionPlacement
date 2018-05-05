/*
 * Copyright(c) 2018 Microsoft Corporation. All rights reserved. 
 * 
 * This code is licensed under the MIT License (MIT). 
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal 
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
 * of the Software, and to permit persons to whom the Software is furnished to do 
 * so, subject to the following conditions: 
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software. 
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE. 
*/

using AzureMapsWpfControl.Data;
using System;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AzureMapsWpfControl.Layer
{
    public class ImageLayer : BaseMapLayer
    {
        private string imageUri = null;

        public ImageLayer(string imageSource, Path corners) : base()
        {
            Coordinates = corners;
            SetImageUri(imageSource);
        }

        public ImageLayer(BitmapImage imageSource, Path corners) : base()
        {
            Coordinates = corners;
            imageUri = Helpers.BitmapImageToBase64(imageSource, "jpg");
        }

        public ImageLayer(string id, string imageSource, Path corners) : base(id)
        {
            Coordinates = corners;
            SetImageUri(imageSource);
        }

        public ImageLayer(string id, BitmapImage imageSource, Path corners) : base(id)
        {       
            Coordinates = corners;
            imageUri = Helpers.BitmapImageToBase64(imageSource, "jpg");
        }

        private Path coordinates;

        public Path Coordinates
        {
            get { return coordinates; }
            set
            {
                coordinates = value;
                coordinates.CollectionChanged += Coordinates_CollectionChanged;
                RaisePropertyChangedEvent("Coordinates");
            }
        }

        private void Coordinates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChangedEvent("Coordinates");
        }

        private TimeSpan fadeDuration = new TimeSpan(0,0,0,0,300);
        public TimeSpan FadeDuration
        {
            get
            {
                return fadeDuration;
            }
            set
            {
                if (fadeDuration != value)
                {
                    fadeDuration = value;
                    SetPaintProperty("raster-fade-duration");
                }
            }
        }

        private double opacity = 1;
        public double Opacity
        {
            get
            {
                return opacity;
            }
            set
            {
                if (opacity != value)
                {
                    opacity = (value < 0) ? 0 : ((value > 1) ? 1 : value);
                    SetPaintProperty("raster-opacity");
                }
            }
        }

        internal override string ToJson()
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"id\":\"{0}\",\"type\":\"raster\",\"source\":", Id);
            sb.Append("{");
            sb.AppendFormat("\"type\":\"image\",\"url\":\"{0}\",\"coordinates\":{1}", imageUri, Coordinates.ToJson());
            sb.Append("},\"layout\":{");
            sb.AppendFormat("\"visibility\":{0}", GetJsonValue("visibility"));
            sb.Append("},\"paint\":{");
            sb.AppendFormat("\"raster-fade-duration\":{0},\"raster-opacity\":{1}", GetJsonValue("raster-fade-duration"), GetJsonValue("raster-opacity"));
            sb.Append("}}");

            return sb.ToString();
        }

        internal override string GetJsonValue(string propertyName)
        {
            switch (propertyName)
            {
                case "visibility":
                    return (Visibility == Visibility.Visible) ? "\"visible\"" : "\"none\"";
                case "raster-fade-duration":
                    return Math.Round(fadeDuration.TotalMilliseconds).ToString();
                case "raster-opacity":
                    return opacity.ToString();
            }

            return string.Empty;
        }

        private void SetImageUri(string imageSource)
        {
            if (imageSource.Contains("<svg") && !imageSource.StartsWith("data:"))
            {
                imageUri = "data:image/svg+xml;base64," + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(imageSource));
            }
            else
            {
                imageUri = imageSource;
            }
        }
    }
}