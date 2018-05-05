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

namespace AzureMapsWpfControl
{
    public class CameraOptions
    {
        public Position Center { get; set; }

        public double? Zoom { get; set; }

        public double? MinZoom { get; set; }

        public double? MaxZoom { get; set; }

        public double? Pitch { get; set; }

        public double? Bearing { get; set; }

        //TODO: bounds;

        internal string ToJson()
        {
            return ToJson(null);
        }

        internal string ToJson(AnimationOptions animation)
        {
            var sb = new StringBuilder();

            sb.Append("{");

            if(animation != null)
            {
                if (animation.Duration != null)
                {
                    sb.AppendFormat("'duration':{0},", Math.Round(animation.Duration.TotalMilliseconds));
                }

                string animationType;

                switch(animation.AnimationType)
                {
                    case AnimationType.Ease:
                        animationType = "ease";
                        break;
                    case AnimationType.Fly:
                        animationType = "fly";
                        break;
                    case AnimationType.Jump:
                    default:
                        animationType = "jump";
                        break;
                }

                sb.AppendFormat("'type':'{0}',", animationType);
            }

            if (Center != null)
            {
                sb.AppendFormat("'center':{0},", Center.ToJson());
            }

            if (Zoom.HasValue)
            {
                sb.AppendFormat("'zoom':{0},", Zoom.Value);
            }

            if (MinZoom.HasValue)
            {
                sb.AppendFormat("'minZoom':{0},", MinZoom.Value);
            }

            if (MaxZoom.HasValue)
            {
                sb.AppendFormat("'maxZoom':{0},", MaxZoom.Value);
            }

            if (Pitch.HasValue)
            {
                sb.AppendFormat("'pitch':{0},", Pitch.Value);
            }

            if (Bearing.HasValue)
            {
                sb.AppendFormat("'bearing':{0},", Bearing.Value);
            }

            if(sb[sb.Length - 1] == ',')
            {
                sb.Length--;
            }

            sb.Append("}");

            return sb.ToString();
        }
    }
}
