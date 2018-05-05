using AzureMapsWpfControl.Data;
using MetadataExtractor;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace PremonitionPlacement
{
    public class ImageLocationExactor
    {
        //For demo purposes, we have stored the location information for some none GeoTiff images here. 
        private static Dictionary<string, List<Position>> KnownImageLocations = new Dictionary<string, List<Position>>()
        {
            { "north_redmond-2013-WA.tif", new List<Position>(){
                new Position(-122.113604543373, 47.7081880071627),
                new Position(-122.106713307923, 47.7081880071627),
                new Position(-122.106713307923, 47.7035673682899),
                new Position(-122.113604543373, 47.7035673682899) } }
        };

        public static List<Position> GetCoordinates(string imagePath, BitmapImage image)
        {
            var fileName = imagePath.Substring(imagePath.LastIndexOf(@"\")+1);

            List<Position> locs = null;

            if (KnownImageLocations.ContainsKey(fileName))
            {
                locs = KnownImageLocations[fileName];
            }
            else
            {
                var directories = ImageMetadataReader.ReadMetadata(imagePath);

                double? latitude = null, longitude = null;
                bool isSouth = false, isWest = false;

                foreach (var directory in directories)
                {
                    if (string.Compare(directory.Name, "GPS") == 0)
                    {
                        foreach (var tag in directory.Tags)
                        {
                            switch (tag.Name)
                            {
                                case "GPS Longitude":
                                    longitude = TryParseStringNumber(tag.Description);
                                    break;
                                case "GPS Longitude Ref":
                                    isWest = (string.Compare(tag.Description, "W", true) == 0);
                                    break;
                                case "GPS Latitude":
                                    latitude = TryParseStringNumber(tag.Description);
                                    break;
                                case "GPS Latitude Ref":
                                    isSouth = (string.Compare(tag.Description, "S", true) == 0);
                                    break;
                            }
                        }
                    }
                }

                if(latitude.HasValue && longitude.HasValue)
                {
                    if (isSouth && latitude > 0)
                    {
                        latitude *= -1;
                    }

                    if (isWest && longitude > 0)
                    {
                        longitude *= -1;
                    }

                    locs = new List<Position>()
                    {
                        new Position(longitude.Value, latitude.Value)
                    };
                }
            } 

            return locs;
        }

        private static double? TryParseStringNumber(string stNum)
        {
            double temp;

            if(Double.TryParse(stNum, out temp))
            {
                return temp;
            }
            else
            {
                temp = DMS2Decimal(stNum);

                if (!double.IsNaN(temp))
                {
                    return temp;
                }
            }

            return null;
        }

        private static double DMS2Decimal(string dms)
        {
            var d = dms.IndexOf('°');
            var m = dms.IndexOf('\'');
            var s = dms.IndexOf('"');

            double dec = double.NaN;
            bool isNeg = false;

            if (d > 0)
            {
                dec = double.Parse(dms.Substring(0, d));

                if(dec < 0)
                {
                    isNeg = true;
                    dec *= -1;
                }

                if (m > 0)
                {
                    var min = double.Parse(dms.Substring(d+1, m-d-1));

                    dec += min / 60;

                    if(s > 0)
                    {
                        var sec = double.Parse(dms.Substring(m + 1, s-m-1));

                        dec += sec / 3600;
                    }
                }
            }

            if (isNeg)
            {
                dec *= -1;
            }

            return dec;
        }
    }
}
