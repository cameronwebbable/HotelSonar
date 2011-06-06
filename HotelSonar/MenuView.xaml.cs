using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Xml.Linq;
using System.Device.Location;

namespace HotelSonar
{
    public partial class MenuView : PhoneApplicationPage
    {
        private readonly GeoCoordinateWatcher _geoWatcher;

        public MenuView()
        {
            InitializeComponent();
            _geoWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
            
            //Don't enable geolocation tracking until the button gets pressed
            _geoWatcher.PositionChanged += GeoWatcherPositionChanged;
                        _geoWatcher.Start();
        }

        private void GeoWatcherPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            var accuracy = e.Position.Location.HorizontalAccuracy;
            Dispatcher.BeginInvoke(() =>
            {
                loadData(e.Position.Location.Latitude.ToString(), e.Position.Location.Longitude.ToString());
                _geoWatcher.Stop();
            });
        }

        //True for success, false for fail
        public bool loadData(string lat, string lng)
        {
            WebClient hotelDeals = new WebClient();
            hotelDeals.DownloadStringCompleted += new DownloadStringCompletedEventHandler(hotelDeals_complete);
            hotelDeals.DownloadStringAsync(new Uri(String.Format("http://api.hotwire.com/v1/deal/hotel?dest={0},{1}&apikey=24hmgkzxavta9fma9zgxftw2&limit=8&tonight=true&starrating=3~*", lat, lng)));
            Console.WriteLine(String.Format("http://api.hotwire.com/v1/deal/hotel?dest={0},{1}&apikey=24hmgkzxavta9fma9zgxftw2&limit=8&tonight=true&starrating=3~*", lat, lng));
            return true;
        }

        void hotelDeals_complete (object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
                return;

            XElement xmlDeals = XElement.Parse(e.Result);

            listBox.ItemsSource = from hotel in xmlDeals.Element("Result").Descendants("HotelDeal")
                                  select new Hotel
                                  {
                                       description = "Garbage",
                                       neighborhood = hotel.Element("Neighborhood").Value,
                                       rating = hotel.Element("StarRating").Value,
                                       price = hotel.Element("Price").Value,
                                       redirectURL = hotel.Element("Url").Value
                                  };

        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

            Dispatcher.BeginInvoke(() =>
            {
                var result = MessageBox.Show(
                    "This application uses your location. Do you wish to give it permission to use your location?",
                    "User Location Data", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.Cancel)
                {
                    MessageBox.Show(
                        "You realize that this app won't do anything now that you've declined location services, right?");
                }
            });
        }
    }
}
