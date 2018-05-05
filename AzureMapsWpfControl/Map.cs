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
using AzureMapsWpfControl.Layer;
using AzureMapsWpfControl.Source;
using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;

namespace AzureMapsWpfControl
{
    //Using WinForm version of Chromium as WPF version doesn't have full touch support and GPU acceleration.
    public class Map : WindowsFormsHost
    {
        private ChromiumWebBrowser WebView;

        public Action MapLoaded;
        private bool isMapLoaded = false;
        private bool isPageLoaded = false;

        public Action<CameraOptions> ViewChange;

        public Map(): base()
        {
            var resourcePath = LoadMapResources();
            
            WebView = new ChromiumWebBrowser(resourcePath + "\\Map.html");

            CefSharpSettings.LegacyJavascriptBindingEnabled = true;
            WebView.RegisterJsObject("jsInterface", this);

#if DEBUG
            WebView.ConsoleMessage += (s, e) =>
            {
                Console.WriteLine(e.Message);
            };
#endif

            this.Child = WebView;

            Sources = new EnhancedObservableCollection<ISource>();
            Sources.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (var n in e.NewItems)
                    {
                        var source = (n as ISource);

                        source.Map = this;
                        WebView.ExecuteScriptAsync("map.map.addSource('" + source.Id + "'," + source.ToJson() + ")");

                        if (n is GeoJsonSource)
                        {
                            (n as GeoJsonSource).CollectionChanged += GeoJsonSourceDataChanged;
                        }
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    foreach (var o in e.OldItems)
                    {
                        if (o is GeoJsonSource)
                        {
                            (o as GeoJsonSource).CollectionChanged -= GeoJsonSourceDataChanged;
                        }

                        var source = (o as ISource);
                        source.Map = null;
                        WebView.ExecuteScriptAsync("if(map.map.getSource('" + source.Id + "')){map.map.removeSource('" + source.Id + "')}");
                    }
                }
            };

            Layers = new EnhancedObservableCollection<BaseMapLayer>();
            Layers.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    if (e.NewItems != null)
                    {
                        foreach (var n in e.NewItems)
                        {
                            (n as BaseMapLayer).Map = this;
                            WebView.ExecuteScriptAsync("map.map.addLayer(" + (n as BaseMapLayer).ToJson() + ")");
                        }
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    if (e.OldItems != null)
                    {
                        foreach (var o in e.OldItems)
                        {
                            (o as BaseMapLayer).Map = null;
                            WebView.ExecuteScriptAsync("map.map.removeLayer('" + (o as BaseMapLayer).Id + "')");
                        }
                    }
                }
            };
        }

        #region Internal Resources

        private static int UniqueIdNumber = 0;

        internal static string GetUniqueId(string type)
        {
            UniqueIdNumber++;
            return type + "_" + UniqueIdNumber;
        }
        
        public void _PageLoaded()
        {
            this.Dispatcher.Invoke(() =>
            {
                isPageLoaded = true;

                if (!string.IsNullOrWhiteSpace(SubscriptionKey))
                {
                    WebView.ExecuteScriptAsync("GetMap('" + SubscriptionKey + "'," + ((this.camera != null) ? this.camera.ToJson() : "{}") + ")");
                }
            });
        }

        public void _ViewChanged(dynamic c)
        {
            var cVals = new System.Collections.Generic.Dictionary<string, object>(c);
            var center = (object[])cVals["center"];

            this.camera = new CameraOptions()
            {
                Zoom = System.Convert.ToDouble(cVals["zoom"]),
                MinZoom = System.Convert.ToDouble(cVals["minZoom"]),
                MaxZoom = System.Convert.ToDouble(cVals["maxZoom"]),
                Pitch = System.Convert.ToDouble(cVals["pitch"]),
                Bearing = System.Convert.ToDouble(cVals["bearing"]),
                Center = new Position(System.Convert.ToDouble(center[0]), System.Convert.ToDouble(center[0]))
            }; 

            this.Dispatcher.Invoke(() =>
            {
                ViewChange?.Invoke(c);
            });
        }

        public void _MapLoaded(dynamic camera)
        {
            this.Dispatcher.Invoke(() =>
            {
                isMapLoaded = true;

                WebView.ExecuteScriptAsync("map.map.on('move', function(){window.jsInterface._ViewChanged(map.getCamera())});window.jsInterface._ViewChanged(map.getCamera());");

                MapLoaded?.Invoke();
            });
        }
        
        private string LoadMapResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var appPath = System.IO.Path.GetDirectoryName(assembly.GetModules()[0].FullyQualifiedName) + "\\AzureMapsResources";

            string[] resources = new string[]
            {
                "Map.html"
            };

            var resourcePath = "AzureMapsWpfControl.Resources.";

            if (!Directory.Exists(appPath))
            {
                Directory.CreateDirectory(appPath);
            }

            foreach (var r in resources)
            {
                if (File.Exists(appPath + "\\" + r))
                {
                    File.Delete(appPath + "\\" + r);
                }

                using (var stream = assembly.GetManifestResourceStream(resourcePath + r)) { 
                    using (var file = new FileStream(appPath + "\\" + r, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(file);
                    }
                }
            }

            return appPath;
        }

        static Map()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Map), new FrameworkPropertyMetadata(typeof(Map)));
        }

        #endregion

        #region Properties

        private CameraOptions camera = new CameraOptions()
        {
            Center = new Position(0, 0),
            Zoom = 1,
            Bearing = 0,
            Pitch = 0,
            MinZoom = 0,
            MaxZoom = 22
        };

        public Position Center
        {
            get { return camera.Center; }
            set
            {
                if (value != null)
                {
                    camera.Center = value;

                    if (isMapLoaded)
                    {
                        WebView.ExecuteScriptAsync("map.map.setCenter(" + value.ToJson() + ");");
                    }
                }
            }
        }

        public double Zoom
        {
            get { return (camera.Zoom.HasValue)? camera.Zoom.Value : 0; }
            set
            {
                if (value >= 0 && value < 25)
                {
                    camera.Zoom = value;

                    if (isMapLoaded)
                    {
                        WebView.ExecuteScriptAsync("map.map.setZoom(" + value + ");");
                    }
                }
            }
        }

        public EnhancedObservableCollection<ISource> Sources { get; }

        public EnhancedObservableCollection<BaseMapLayer> Layers { get; }

        public readonly static DependencyProperty SubscriptionKeyProperty = DependencyProperty.Register("SubscriptionKey", typeof(string), typeof(Map), new PropertyMetadata(""));

        public string SubscriptionKey
        {
            get { return (string)GetValue(SubscriptionKeyProperty); }
            set {
                SetValue(SubscriptionKeyProperty, value);

                if(isPageLoaded && !isMapLoaded)
                {
                    _PageLoaded();
                }
            }
        }

        #endregion

        #region Public Method

        public void Clear()
        {
            Layers.Clear();
            Sources.Clear();
        }

        public void SetCamera(CameraOptions camera)
        {
            SetCamera(camera, null);
        }

        public void SetCamera(CameraOptions camera, AnimationOptions animation)
        {
            if(camera != null)
            {
                WebView.ExecuteScriptAsync("map.setCamera(" + camera.ToJson(animation) + ");");
            }
        }

        public async Task<System.Windows.Point?> PositionToPixel(Position position)
        {
            var p = await EvaluateScript("map.map.project(" + position.ToJson() + ")", null);
            if (p != null)
            {
                var vals = new System.Collections.Generic.Dictionary<string, object>(p);

                if (vals.ContainsKey("x") && vals.ContainsKey("y"))
                {
                    return new System.Windows.Point(System.Convert.ToDouble(vals["x"]), System.Convert.ToDouble(vals["y"]));
                }
            }

            return null;
        }

        public async Task<Position> PixelToPosition(System.Windows.Point point)
        {
            var p = await EvaluateScript("map.map.unproject([" + point.X + "," + point.Y + "])", null);
            if (p != null)
            {
                var vals = new System.Collections.Generic.Dictionary<string, object>(p);

                if (vals.ContainsKey("lat") && vals.ContainsKey("lng"))
                {
                    return new Position(System.Convert.ToDouble(vals["lng"]), System.Convert.ToDouble(vals["lat"]));
                }
            }
            return null;
        }

        #endregion

        #region Private Methods

        private void GeoJsonSourceDataChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            WebView.ExecuteScriptAsync("map.map.getSource('" + (sender as ISource).Id + "').setData(" + (sender as GeoJsonSource).ToJson(true) + ")");
        }

        //Asynchronously retrieves a value from the web view.
        internal async Task<dynamic> EvaluateScript(string script, object defaultValue)
        {
            object result = defaultValue;
            if (WebView.IsBrowserInitialized && !WebView.IsDisposed)
            {
                try
                {
                    var task = WebView.EvaluateScriptAsync(script);
                    await task.ContinueWith(res => {
                        if (!res.IsFaulted)
                        {
                            var response = res.Result;
                            result = response.Success ? (response.Result ?? "null") : response.Message;
                        }
                    }).ConfigureAwait(false); // <-- This makes the task to synchronize on a different context
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.InnerException.Message);
                }
            }
            return result;
        }

        #endregion
    }
}
