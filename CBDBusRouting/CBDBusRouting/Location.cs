using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace CBDBusRouting
{
    public class Location
    {
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public double[] coords { get; set; }
        public string nickname { get; set; }

        public Dictionary<Location, double> distanceDict;

        // used when reading in groups from csv
        public Location()
        {

        }

        // used for creating home location
        public Location(string address, string city, string state, string nickname)
        {
            this.address = address;
            // England is my city
            this.city = city;
            this.state = state;
            this.nickname = nickname;

            retrieveLatAndLong();
            // origin
            coords = new double[] { 0, 0 };
            distanceDict = new Dictionary<Location, double>();
            distanceDict.Add(this, 0);
        }

        // used for creating destinations
        public Location(string address, string city, string state, string nickname, double homeLat, double homeLong)
        {
            this.address = address;
            this.nickname = nickname;
            this.city = city;
            this.state = state;

            retrieveLatAndLong();
            coords = new double[2];
            calculateCoords(homeLong, homeLat);
            distanceDict = new Dictionary<Location, double>();
        }

        // used for retrieving cached locations where nearly all properties' values are known
        public Location(string address, string city, string state, string nickname, double latitude, double longitude, double xCoord, double yCoord)
        {
            this.address = address;
            this.city = city;
            this.state = state;
            this.nickname = nickname;
            this.latitude = latitude;
            this.longitude = longitude;

            coords = new double[2];
            coords[0] = xCoord;
            coords[1] = yCoord;
            distanceDict = new Dictionary<Location, double>();
        }

        private void retrieveLatAndLong()
        {
            try
            {
                string geocodingApiUrl = ConfigurationManager.AppSettings["GoogleGeocodingApi"];
                string apiKey = ConfigurationManager.AppSettings["GeocodingApiServplicity"];

                string addressUrl = formatForGeocodingUrl(address + "," + city + "," + state);

                geocodingApiUrl += addressUrl + "&key=" + apiKey;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(geocodingApiUrl);
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader sreader = new StreamReader(dataStream);
                string responseReader = sreader.ReadToEnd();
                response.Close();
                DataSet ds = new DataSet();
                ds.ReadXml(new XmlTextReader(new StringReader(responseReader)));

                latitude = Convert.ToDouble(ds.Tables["location"].Rows[0].ItemArray[0]);
                longitude = Convert.ToDouble(ds.Tables["location"].Rows[0].ItemArray[1]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error calculating location's latitude & longitude: " + ex.Message);
            }
        }

        private string formatForGeocodingUrl(string rawAddress)
        {
            string urlAddress = rawAddress.Replace(" ", "+");
            return urlAddress;
        }

        public void calculateCoords(double homeLat, double homeLong)
        {
            // Dx - Wx
            double xDist = latitude - homeLat;

            //Dy - Wy
            double yDist = longitude - homeLong;

            // Calculating hypotenuse of triangle
            // Square root of xDist^2 + yDist^2
            double hypotenuse = Math.Sqrt(Math.Pow(xDist, 2) + Math.Pow(yDist, 2));

            // Calculating xCoord
            coords[0] = xDist / hypotenuse;

            // Calculating yCoord
            coords[1] = yDist / hypotenuse;
        }

        // calls Google Distance Matrix API, using address as origin and all other locations as destinations
        // *Limit on number of destinations at 25 per call
        public void runDistanceMatrix(List<Location> locs)
        {
            try
            {
                // Referencing App.config
                string distanceApiUrl = ConfigurationManager.AppSettings["GoogleDistanceMatrixApi"];
                string distanceMatrixApiKey = ConfigurationManager.AppSettings["DistanceMatrixApiServplicity"];

                string locations = "";

                // Formatting string to add in URL
                foreach (Location nextLocation in locs)
                {
                    locations += formatForGeocodingUrl(nextLocation.address + " " + nextLocation.city + " " + nextLocation.state) + "%7C";
                }

                // removing the last bar (%7C) after the last location
                locations = locations.Remove(locations.Length - 3, 3);

                string url = distanceApiUrl + "&origins=" + latitude + "," + longitude +
                    "&destinations=" + locations +
                    "&key=" + distanceMatrixApiKey;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader sreader = new StreamReader(dataStream);
                    string responseReader = sreader.ReadToEnd();
                    response.Close();
                    DataSet ds = new DataSet();
                    ds.ReadXml(new XmlTextReader(new StringReader(responseReader)));

                    Console.WriteLine(url);

                    if (ds.Tables.Count > 0)
                    {
                        Console.WriteLine("new location");

                        for (int destNum = 0; destNum < ds.Tables["destination_address"].Rows.Count; destNum++)
                        {
                            if (ds.Tables["element"].Rows[destNum]["status"].ToString() == "OK")
                            {
                                string duration = Convert.ToString(ds.Tables["duration"].Rows[destNum]["text"].ToString().Trim());
                                duration = duration.Substring(0, duration.IndexOf(" "));

                                string distance = Convert.ToString(ds.Tables["distance"].Rows[destNum]["text"].ToString().Trim());

                                string destinationAddress = Convert.ToString(ds.Tables["destination_address"].Rows[destNum].ItemArray[0].ToString().Trim());
                                Console.WriteLine(destinationAddress);

                                if (distance == "1 ft")
                                {
                                    duration = "0";
                                }

                                Location dest = locs[destNum];
                                double travelTime = Convert.ToDouble(duration);

                                // Adds travel time between locations to both of their distanceDict's
                                if (!distanceDict.ContainsKey(dest))
                                    distanceDict.Add(dest, travelTime);

                                if (!dest.distanceDict.ContainsKey(this))
                                    dest.distanceDict.Add(this, travelTime);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error calculating distance: " + ex.Message);
                Console.WriteLine("error");
            }
        }

        // gets subarrays for running Google Distance Matrix (limit on 25 origins/destinations)
        public void computeDistancesBetweenAllLocations(List<Location> allLocations)
        {
            // Limit of 25 origins or destinations on Google Distance Matrix API
            int numDestinationsInSubList = 25;
            int numLocationSubLists = (allLocations.Count / numDestinationsInSubList) + 1;

            for (int i = 1; i < numLocationSubLists; i++)
            {
                List<Location> subList = new List<Location>();
                // Retrieves the next set of 25 locations to set as destinations in Distance Matrix API call
                subList = allLocations.GetRange((numDestinationsInSubList * (i - 1)), numDestinationsInSubList);

                runDistanceMatrix(subList);
                System.Threading.Thread.Sleep(1000);
            }

            int remainder = allLocations.Count % numDestinationsInSubList;

            if (remainder != 0)
            {
                List<Location> remainderList = new List<Location>();
                remainderList = allLocations.GetRange((numDestinationsInSubList * (numLocationSubLists - 1)), remainder);

                runDistanceMatrix(remainderList);
            }
        }

        public override string ToString()
        {
            return nickname;
        }
    }
}
