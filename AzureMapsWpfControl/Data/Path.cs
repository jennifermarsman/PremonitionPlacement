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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AzureMapsWpfControl.Data
{
    public class Path: ObservableCollection<Position>
    {
        public Path() : base()
        {
        }
        public Path(IEnumerable<Position> coordinates) : base(coordinates)
        {
        }

        public Path(IList<Position> coordinates): base()
        {
            foreach(var c in coordinates)
            {
                this.Add(c);
            }            
        }

        public string ToJson()
        {
            return ToJson(false);
        }

        public string ToJson(bool closeRing)
        {
            var sb = new StringBuilder();

            sb.Append("[");

            if (this.Count > 0)
            {
                foreach (var i in this)
                {
                    sb.Append(i.ToJson());
                    sb.Append(",");
                }

                if (closeRing && !this[0].Equals(this[this.Count - 1]))
                {
                    sb.Append(this[0].ToJson());
                    sb.Append(",");
                }

                sb.Length--;
            }
            sb.Append("]");

            return sb.ToString();
        }
    }
}
