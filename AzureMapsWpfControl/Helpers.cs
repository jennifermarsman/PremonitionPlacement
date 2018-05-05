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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace AzureMapsWpfControl
{
    public static class Helpers
    { 
        public static string BitmapImageToBase64(BitmapImage image, string format)
        {
            byte[] data = null;
            string imageType = format.ToLowerInvariant();
            BitmapEncoder encoder = null;
            switch (imageType)
            {
                case "png":
                    encoder = new PngBitmapEncoder();
                    break;
                case "gif":
                    encoder = new GifBitmapEncoder();
                    break;
                case "bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                case "jpg":
                    encoder = new JpegBitmapEncoder();
                    break;
            }
            if (encoder != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(image));
                using (var ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    data = ms.ToArray();
                }
            }

            return "data:image/" + imageType + ";base64," + Convert.ToBase64String(data);
        }

        public static string DictionaryToJson(Dictionary<string, object> dictionary)
        {
            if (dictionary != null)
            {
                var sb = new StringBuilder();
                sb.Append("{");

                foreach (var key in dictionary.Keys)
                {
                    var val = dictionary[key];

                    if (val is double || val is bool)
                    {
                        sb.AppendFormat("'{0}':{1},", key, dictionary[key]);
                    }
                    else if (val is string)
                    {
                        sb.AppendFormat("'{0}':\"{1}\",", key, dictionary[key]);
                    }
                }

                sb.Append("}");
                return sb.ToString();
            }

            return "{}";
        }

        //public Dictionary<string, object> DynamicToDictionary(dynamic val)
        //{
        //    return null;
        //}
    }
}
