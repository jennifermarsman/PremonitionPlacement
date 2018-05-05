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

using AzureMapsWpfControl.Source;
using System.Text;
using System.Windows;

namespace AzureMapsWpfControl.Layer
{
    public class SymbolLayer : BaseMapLayer
    {
        public SymbolLayer(): base()
        {
        }

        public SymbolLayer(string id):base(id)
        {
        }

        public ISource Source { get; set; }

        public SymbolOptions Options { get; set; }

        internal override string GetJsonValue(string propertyName)
        {
            switch (propertyName)
            {
                case "visibility":
                    return (Visibility == Visibility.Visible) ? "\"visible\"" : "\"none\"";
                case "icon-image":
                    return ProcessStringValue(Options.IconImage);
                case "icon-size":
                    return Options.IconSize.ToString();
                case "icon-anchor":
                    return Options.IconAnchor.GetJsonName();
                case "text-anchor":
                    return Options.TextAnchor.GetJsonName();
                case "text-color":
                    return ProcessColorValue(Options.TextColor);
                case "text-field":
                    return ProcessStringValue(Options.Text);
                case "text-size":
                    return Options.TextSize.ToString();
                case "filter":
                    return ProcessStringValue(Options.Filter);
            }

            return string.Empty;
        }
        
        internal override string ToJson()
        {
            if (Map != null && !Map.Sources.Contains(Source))
            {
                Map.Sources.Add(Source);
            }

            var sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"id\":\"{0}\",\"type\":\"symbol\",\"source\":\"{1}\",", Id, Source?.Id);
            sb.Append("\"layout\":{");

            if (Options != null && !string.IsNullOrWhiteSpace(Options.IconImage))
            {
                sb.AppendFormat("\"icon-image\":{0},", GetJsonValue("icon-image"));
            }
            else
            {
                sb.Append("\"icon-image\":\"pin-darkblue\",");
            }

            if (Options != null)
            {
                sb.AppendFormat("\"icon-anchor\":\"{0}\",", GetJsonValue("icon-anchor"));

                if (Options.IconSize > 0)
                {
                    sb.AppendFormat("\"icon-size\":{0},", GetJsonValue("icon-size"));
                }

                //if (!string.IsNullOrWhiteSpace(Options.Text))
                //{
                //    sb.AppendFormat("\"text-field\":{0},", GetJsonValue("text-field"));
                    //sb.AppendFormat("\"text-anchor\":\"{0}\",", GetJsonValue("text-anchor"));

                    //if (Options.TextSize > 0)
                    //{
                    //    sb.AppendFormat("\"text-size\":{0},", GetJsonValue("text-size"));
                    //}
                //}
            }

            if (sb.ToString().EndsWith(","))
            {
                sb.Length--;
            }

            sb.Append("},\"paint\":{");

            if (Options != null)
            {
                //if (!string.IsNullOrWhiteSpace(Options.Text))
                //{
                //    if (Options.TextColor != null)
                //    {
                //        sb.AppendFormat("\"text-color\":{0},", GetJsonValue("text-color"));
                //    }
                //}
            }

            if (sb.ToString().EndsWith(","))
            {
                sb.Length--;
            }

            sb.Append("}");

            if (Options != null && !string.IsNullOrWhiteSpace(Options.Filter))
            {
                sb.AppendFormat(",\"filter\":{0},", GetJsonValue("filter"));
            }

            if (sb.ToString().EndsWith(","))
            {
                sb.Length--;
            }

            sb.Append("}");

            return sb.ToString();

          //  return "{'id':'" + Id + "','type':'symbol','source':'" + Source?.Id + "','layout':{'icon-image': 'pin-blue'}}";
        }
    }
}
