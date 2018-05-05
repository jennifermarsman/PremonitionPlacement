using AzureMapsWpfControl;
using AzureMapsWpfControl.Data;
using AzureMapsWpfControl.Layer;
using AzureMapsWpfControl.Source;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PremonitionPlacement
{
    /// <summary>
    /// This code is a demonstration of the AI for Earth land cover mapping API.  
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties

        #region Subscription keys

        // Subscription Key for Land Cover Mapping
        private static string AI4EarthSubscriptionKey = ConfigurationManager.AppSettings.Get("AiForEarthSubscriptionKey");

        // Subscription Key for Azure Maps
        private static string AzureMapsSubscriptionKey = ConfigurationManager.AppSettings.Get("AzureMapsSubscriptionKey");

        #endregion

        const string uriBase = "https://aiforearth.azure-api.net/v0.1/landcover/classify";

        private string imageFilePath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + @"\Resources\north_redmond-2013-WA.tif";

        private bool isMapLoaded = false;

        private SymbolLayer hospitalLayer = null;
        private BaseMapLayer originalMapLayer = null;
        private BaseMapLayer classifiedMapLayer = null;
        private SymbolLayer trapLayer = null;

        private string poiSearchUrlTemplate = "https://atlas.microsoft.com/search/poi/category/json?subscription-key={0}&api-version=1.0&query={1}&limit=100&lat={2}&lon={3}&radius={4}";

        private int poiSearchRadiusMeters = 12000;

        private List<Position> imagePosition = null;

        #endregion

        #region Construtor

        public MainWindow()
        {
            InitializeComponent();
            InitializeDefaultPicture();
            InitializeMap();
        }

        #endregion

        private void InitializeDefaultPicture()
        {
            //Load image.
            img.Source = new BitmapImage(new Uri(imageFilePath));
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofdPicture = new OpenFileDialog();
            ofdPicture.Filter = "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif;*.tiff|All files|*.*";
            ofdPicture.FilterIndex = 1;

            if (ofdPicture.ShowDialog() == true)
            {
                imageFilePath = ofdPicture.FileName;
                img.Source = new BitmapImage(new Uri(ofdPicture.FileName));

                LoadMapDataForImage();
            }
        }

        #region AI for Earth Functionality

        private async void btnFindOptimalPlacement_Click(object sender, RoutedEventArgs e)
        {
            // Call the AI for Earth land cover API
            Bitmap returnImage = await AIforEarthLandCoverMappingAnalyze(imageFilePath);
            classifiedImg.Source = ConvertBitmapToBitmapImage(returnImage);

            if (classifiedImg.Source != null)
            {
                //Add the image to the map if location information is available.
                if (imagePosition != null && imagePosition.Count == 4)
                {
                    classifiedMapLayer = new ImageLayer(classifiedImg.Source as BitmapImage, new AzureMapsWpfControl.Data.Path(imagePosition))
                    {
                        Opacity = 0
                    };
                    MyMap.Layers.Add(classifiedMapLayer);

                    MapLayerOpacity.IsEnabled = true;
                }

                // Using the land cover information, find the optimal spot to place Project Premonition traps.  
                FindBestPlacement(returnImage); 
            }
        }

        /// <summary>
        /// Business logic to place the Project Premonition trap in a 
        /// relevant location to attract mosquitos.  (Note that this 
        /// is not the real business logic used by Project Premonition
        /// research team; this is intended as an example/demo of using
        /// the AI for Earth land cover APIs.)
        /// </summary>
        /// <param name="x"></param>
        private void FindBestPlacement(Bitmap img)
        {
            var pixels = new List<System.Windows.Point>()
            {
                //Test data
                new System.Windows.Point(70, 30),
                new System.Windows.Point(80, 140),
                new System.Windows.Point(235, 180),
            };

            // Find points where water and trees meet
            // TODO: start here

            // Otherwise find place where water meets non-water

            // Otherwise find place in trees

            // Otherwise field (can't place trap in water or in buildings)

            //Display the traps on the map. Pass in the pixel of the image.
            ShowTrapsOnMap(pixels);
        }

        /// <summary>
        /// Classify the ESRI NAIP image file using the AI for Earth Land Cover Mapping REST API.
        /// </summary>
        /// <param name="imageFilePath">ESRI NAIP image file</param>
        static async Task<Bitmap> AIforEarthLandCoverMappingAnalyze(string imageFilePath)
        {
            Bitmap bitmap = null;

            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AI4EarthSubscriptionKey);

                // Request parameters. 
                string requestParameters = "type=tiff";

                // Assemble the URI for the REST API Call.
                string uri = uriBase + "?" + requestParameters;

                Console.WriteLine("Sending request to {0}", uri);
                HttpResponseMessage response;
                
                // Request body. Posts a locally stored TIFF image.
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("image/tiff");

                    // Call the AI for Earth API
                    response = await client.PostAsync(uri, content);
                    Debug.WriteLine(response.GetType().ToString());
                    Stream stream = await response.Content.ReadAsStreamAsync();
                    bitmap = new Bitmap(stream);

                    // Show the response status code
                    Debug.WriteLine("RESPONSE: {0}", response.StatusCode);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            return bitmap;
        }

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">Image file to read</param>
        /// <returns>Byte array of the image data</returns>
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        /// <summary>
        /// Converts a Bitmap to a BitmapImage
        /// </summary>
        /// <param name="bitmap">The bitmap to convert</param>
        /// <returns>A BitmapImage generated from the provided Bitmap</returns>
        static BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                try
                {
                    bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();
                    return bitmapimage;
                }
                catch (Exception e)
                {
                    MessageBox.Show("There was an error processing this image. Please try a different image.");
                    Debug.WriteLine(e.ToString());
                }
            }

            return null;
        }
        
        #endregion

        #region Map Functionality

        private void InitializeMap()
        {
            MyMap.MapLoaded += () =>
            {
                isMapLoaded = true;

                hospitalLayer = new SymbolLayer()
                {
                    Source = new GeoJsonSource(),
                    Options = new SymbolOptions
                    {
                        IconImage = "pin-blue"
                    }
                };
                MyMap.Layers.Add(hospitalLayer);

                LoadMapDataForImage();
            };

            //Set the Azure Maps subscription key, this will trigger the MapLoaded event once the map is initially rendered.
            MyMap.SubscriptionKey = AzureMapsSubscriptionKey;
        }

        private void LoadMapDataForImage()
        {
            if (isMapLoaded)
            {
                if(originalMapLayer != null)
                {
                    MyMap.Layers.Remove(originalMapLayer);
                    classifiedMapLayer = null;

                    MapLayerOpacity.Value = 0;

                    if (classifiedMapLayer != null)
                    {
                        MyMap.Layers.Remove(classifiedMapLayer);
                        classifiedMapLayer = null;
                        MapLayerOpacity.IsEnabled = false;
                    }

                    if (trapLayer != null)
                    {
                        MyMap.Layers.Remove(trapLayer);
                    }

                    (hospitalLayer.Source as GeoJsonSource).Clear();            
                }

                var locs = ImageLocationExactor.GetCoordinates(imageFilePath, img.Source as BitmapImage);
                Position center = null;

                if (locs != null && locs.Count > 0)
                {
                    if (locs.Count == 1)
                    {
                        //Only have a single location, create a pin.
                        originalMapLayer = new SymbolLayer()
                        {
                            Source = new GeoJsonSource()
                            {
                                new GeoPoint(locs[0])
                            }
                        };

                        center = locs[0];
                    }
                    else if (locs.Count == 4)
                    {
                        //Have all four corners of image; top-left, top-right, bottom-right, bottom-left.
                        originalMapLayer = new ImageLayer(img.Source as BitmapImage, new AzureMapsWpfControl.Data.Path(locs));

                        imagePosition = locs;

                        center = new Position((locs[0].Longitude + locs[2].Longitude) / 2, (locs[0].Latitude + locs[2].Latitude) / 2);
                    }
                }

                if (originalMapLayer != null)
                {
                    MyMap.Layers.Add(originalMapLayer);

                    MyMap.SetCamera(new CameraOptions()
                    {
                        Center = center,
                        Zoom = 15
                    }, new AnimationOptions()
                    {
                        AnimationType = AnimationType.Fly,
                        Duration = new TimeSpan(0,0,2)
                    });

                    LoadNearbyPOIs(center);
                }
            }
        }

        private async void ShowTrapsOnMap(List<System.Windows.Point> pixels)
        {
            if (pixels != null && pixels.Count > 0 && imagePosition != null && imagePosition.Count > 0)
            {
                var pins = new List<GeoPoint>();

                var topLeftPixel = await MyMap.PositionToPixel(imagePosition[0]);
                
                //Calculate approximate pixel coordinates. Since images cover small areas we can use linear approximations. 
                var imageWidth = img.Width;
                var imageHeight = img.Height;

                var west = imagePosition[0].Longitude;
                var dLng = imagePosition[2].Longitude - west;

                var north = imagePosition[0].Latitude;
                var dLat = north - imagePosition[2].Latitude;                

                if (topLeftPixel != null && topLeftPixel.HasValue)
                {
                    foreach (var p in pixels)
                    {
                        pins.Add(new GeoPoint(new Position(west + (p.X / imageWidth) * dLng, north - (p.Y / imageHeight) * dLat)));
                    }

                    trapLayer = new SymbolLayer()
                    {
                        Source = new GeoJsonSource(),
                        Options = new SymbolOptions
                        {
                            IconImage = "pin-round-red"
                        }
                    };
                    (trapLayer.Source as GeoJsonSource).AddRange(pins);
                    MyMap.Layers.Add(trapLayer);
                }
            }
        }

        private async void LoadNearbyPOIs(Position center)
        {            
            var pins = new List<GeoPoint>();

            var queryUrl = string.Format(poiSearchUrlTemplate, AzureMapsSubscriptionKey, "hospital", center.Latitude, center.Longitude, poiSearchRadiusMeters);

            var wc = new WebClient();
            var response = await wc.DownloadStringTaskAsync(new Uri(queryUrl));

            var d = JObject.Parse(response);

            var results = d["results"];

            if (results != null)
            {
                foreach (var r in results)
                {
                    var props = r["address"].ToObject<Dictionary<string, object>>();

                    if (r["poi"] != null)
                    {
                        props.Add("name", r["poi"]["name"].Value<string>());
                    }

                    pins.Add(new GeoPoint(new Position(r["position"]["lon"].Value<double>(), r["position"]["lat"].Value<double>()))
                    {
                        Properties = props
                    });
                }

                if (pins.Count > 0)
                {
                    (hospitalLayer.Source as GeoJsonSource).AddRange(pins);
                }
            }
        }

        private void PoiChecked(object sender, RoutedEventArgs e)
        {
            if (hospitalLayer != null)
            {
                if (HospitalCbx.IsChecked.HasValue && HospitalCbx.IsChecked.Value)
                {
                    hospitalLayer.Visibility = Visibility.Visible;
                }
                else
                {
                    hospitalLayer.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void OpacitySliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(originalMapLayer != null && originalMapLayer is ImageLayer &&
               classifiedMapLayer != null && classifiedMapLayer is ImageLayer)
            {
                (classifiedMapLayer as ImageLayer).Opacity = e.NewValue;
                (originalMapLayer as ImageLayer).Opacity = 1 - e.NewValue;
            }
        }

        #endregion
    }
}
