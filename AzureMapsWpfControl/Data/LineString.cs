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

namespace AzureMapsWpfControl.Data
{
    public class LineString: Geometry
    {
        public LineString()
        {
            Coordinates = new Path();
        }

        public LineString(Path path)
        {
            Coordinates = path;
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

        internal override string ToJson()
        {
            return "{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":" + Coordinates.ToJson() + "},\"properties\":" + Helpers.DictionaryToJson(Properties) + "}";
        }
    }
}
