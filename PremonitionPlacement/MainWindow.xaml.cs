using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PremonitionPlacement
{
    /// <summary>
    /// This code is a demonstration of the AI for Earth land cover 
    /// mapping API.  
    /// </summary>
    public partial class MainWindow : Window
    {
        // Subscription Key for Land Cover Mapping
        #region Hiding subscription key :)
        const string subscriptionKey = "<insert your key here>";
        #endregion
        const string uriBase = "https://aiforearth.azure-api.net/v0.1/landcover/classify";
        string imageFilePath = @"C:\Code\AIforEarth\Samples\images\north_redmond-2013-WA.tif";
        
        public MainWindow()
        {
            InitializeComponent();
            InitializeDefaultPicture();
        }

        private void InitializeDefaultPicture()
        {
            // TODO: put image in solution and load from here
            img.Source = new BitmapImage(new Uri(imageFilePath));
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofdPicture = new OpenFileDialog();
            ofdPicture.Filter = "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif|All files|*.*";
            ofdPicture.FilterIndex = 1;

            if (ofdPicture.ShowDialog() == true)
            {
                img.Source = new BitmapImage(new Uri(ofdPicture.FileName));
            }
        }

        private async void btnFindOptimalPlacement_Click(object sender, RoutedEventArgs e)
        {
            // Call the AI for Earth land cover API
            Bitmap returnImage = await AIforEarthLandCoverMappingAnalyze(imageFilePath);
            classifiedImg.Source = ConvertBitmapToBitmapImage(returnImage);

            // Using the land cover information, find the optimal spot to place Project Premonition traps.  
            //FindBestPlacement(returnImage); 
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
            // Find points where water and trees meet
            // TODO: start here

            // Otherwise find place where water meets non-water

            // Otherwise find place in trees

            // Otherwise field (can't place trap in water or in buildings)

            throw new NotImplementedException();
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
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

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
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }

    }
}
