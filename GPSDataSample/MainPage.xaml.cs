using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GPSDataSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Geolocator myGeo;
        double[,] posArray = new double[4,2];
        uint _desiredAccuracy = 1;  // give me data within one metre
        Geoposition[] posTest = new Geoposition[3];

        public MainPage()
        {
            this.InitializeComponent();
            
            setupGeoLocation();
            _count = -1;
        }

        private async void setupGeoLocation()
        {
            // ask for permission 
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch(accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    {
                        MessageDialog myMsg = new MessageDialog("waiting for update from location data");
                        await myMsg.ShowAsync();
                        myGeo = new Geolocator();
                        myGeo.DesiredAccuracy = PositionAccuracy.High;
                        //myGeo = new Geolocator { DesiredAccuracyInMeters = _desiredAccuracy };
                        myGeo.ReportInterval = (uint)5000;
                        // set up the events
                        // status changed, position changed
                        myGeo.StatusChanged += MyGeo_StatusChanged;
                       // myGeo.PositionChanged += MyGeo_PositionChanged;
                        // get our current position.
                        //Geoposition pos = await myGeo.GetGeopositionAsync();
                   
                        //updateMainPage(pos);

                        break;
                    }
                case GeolocationAccessStatus.Denied:
                    {
                        MessageDialog myMsg = new MessageDialog("Please turn on location data");
                        await myMsg.ShowAsync();
                        break;
                    }
                default:
                    {
                        MessageDialog myMsg = new MessageDialog("Unspecified problem accessing location data");
                        await myMsg.ShowAsync();
                        break;
                    }
            }
        }

        private int _count;
        private void updateMainPage(Geoposition pos)
        {
            TextBlock tblCurr;

            _count++;
            posArray[_count,0] = pos.Coordinate.Latitude;
            posArray[_count, 1] = pos.Coordinate.Longitude;

            posTest[_count] = pos;

            tblCurr = new TextBlock();
            tblCurr.Name = "tblPosition" + _count.ToString();
            tblCurr.TextWrapping = TextWrapping.Wrap;
            tblCurr.Text = "Latitude: " + pos.Coordinate.Latitude.ToString() + System.Environment.NewLine +
                         "Longitude: " + pos.Coordinate.Longitude.ToString();
            spCoordinates.Children.Add(tblCurr);
            spCoordinates.UpdateLayout();
        }

        private async void MyGeo_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //tblGeoData.Text = "New Location Found: " +
                //                  args.Position.Coordinate.Latitude.ToString() + ", " +
                //                  args.Position.Coordinate.Longitude.ToString();
            });
        }

        private async void MyGeo_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            // use the dispatcher with lambda fuction to update the UI thread.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // code to run in method to update UI
                switch (args.Status )
                    {
                        case PositionStatus.Ready:
                            // what here?
                            tblStatusUpdates.Text = "Locations services normal";
                            break;
                        case PositionStatus.Disabled:
                            tblStatusUpdates.Text = "Turn on location services dummy";
                            break;
                        case PositionStatus.NoData:
                            tblStatusUpdates.Text = "No data received from Location services";
                            break;
                        case PositionStatus.Initializing:
                            tblStatusUpdates.Text = "Initialising Location services";
                            break;
                        default:
                            tblStatusUpdates.Text = "Unknown problem with your location services";
                            break;
                    }

                } );

        }

        private async void btnStore_Click(object sender, RoutedEventArgs e)
        {
            // get the current location,
            Geoposition pos = await myGeo.GetGeopositionAsync();
            updateMainPage(pos);



            // save it to the textblock and show in the stackpanel
            // when there are more than 3, give the option to caluclate the area

        }

        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            if(_count < 2)
            {
                tblStatusUpdates.Text = "Not enough points to calculate area.";
                return;
            }

            // first line is pos[0] to pos[1]
            // secondis pos[1] to pos[2]
            double distance1 = getDistance(posTest[0], posTest[1]);
            double distance2 = getDistance(posTest[1], posTest[2]);

            double area = distance1 * distance2;

            tblStatusUpdates.Text = "Area = " + area.ToString();
        }

        private double getDistance(Geoposition pos1, Geoposition pos2)
        {
            double theta = pos1.Coordinate.Longitude - pos2.Coordinate.Longitude;

            double dist = Math.Sin(deg2rad(pos1.Coordinate.Latitude)) * Math.Sin(deg2rad(pos2.Coordinate.Latitude)) + 
                          Math.Cos(deg2rad(pos1.Coordinate.Latitude)) * Math.Cos(deg2rad(pos2.Coordinate.Latitude)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);

            // this gets the distance in miles
            dist = dist * 60 * 1.1515;

            // distance in kilometers
            dist = dist * 1.609344;

            // getDistance in metres
            dist = dist * 1000;

            return (dist);
        }

        private double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }


    }
}
