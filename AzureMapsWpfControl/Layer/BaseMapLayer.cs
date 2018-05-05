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

using System.Windows;
using System.Windows.Media;

namespace AzureMapsWpfControl.Layer
{
    public abstract class BaseMapLayer : ObservableClass
    {
        public BaseMapLayer()
        {
            Id = Map.GetUniqueId("Layer");
        }

        public BaseMapLayer(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = Map.GetUniqueId("Layer");
            }
            Id = id;
        }

        public string Id { get; }

        internal Map Map { get; set; }

        private Visibility visibility;
        public Visibility Visibility
        {
            get
            {
                return visibility;
            }
            set
            {
                if (visibility != value)
                {
                    visibility = value;
                    SetLayoutProperty("visibility");
                }
            }
        }

        internal abstract string ToJson();

        internal abstract string GetJsonValue(string propertyName);

        internal async void SetLayoutProperty(string property)
        {
            if (Map != null)
            {
                await Map.EvaluateScript("map.map.setLayoutProperty('" + Id + "', '" + property + "', " + GetJsonValue(property) + ");", null);
            }
        }

        internal async void SetPaintProperty(string property)
        {
            if (Map != null)
            {
                await Map.EvaluateScript("map.map.setPaintProperty('" + Id + "', '" + property + "', " + GetJsonValue(property) + ");", null);
            }
        }

        //Checks to see if the string value may be an expression.
        internal string ProcessStringValue(string val)
        {
            if (!string.IsNullOrWhiteSpace(val))
            {
                if (val.IndexOf("[") >= 0)
                {
                    return val;
                }

                return "\"" + val + "\"";
            }

            return "\"\"";
        }

        internal string ProcessColorValue(Color val)
        {
            if (val != null)
            {
                return string.Format("\"rgba({0},{1},{2},{3})\"", val.R, val.G, val.B, val.A);
            }

            return null;
        }
    }
}
