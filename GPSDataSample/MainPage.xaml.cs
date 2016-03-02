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
        
        uint _desiredAccuracy = 1;  // give me data within one metre

        public MainPage()
        {
            this.InitializeComponent();
            
            setupGeoLocation();

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
                        //myGeo = new Geolocator();
                        myGeo = new Geolocator { DesiredAccuracyInMeters = _desiredAccuracy };
                        myGeo.ReportInterval = (uint)5000;
                        // set up the events
                        // status changed, position changed
                        myGeo.StatusChanged += MyGeo_StatusChanged;
                        myGeo.PositionChanged += MyGeo_PositionChanged;
                        // get our current position.
                        Geoposition pos = await myGeo.GetGeopositionAsync();
                        updateMainPage(pos);

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

        private void updateMainPage(Geoposition pos)
        {
            tblGeoData.Text  = "Latitude: " + pos.Coordinate.Latitude.ToString() + System.Environment.NewLine +
                         "Longitude: " + pos.Coordinate.Longitude.ToString() + System.Environment.NewLine +
                         "Heading: " + pos.Coordinate.Heading.ToString() + System.Environment.NewLine +
                         "Speed: " + pos.Coordinate.Speed.ToString();

        }

        private async void MyGeo_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tblGeoData.Text = "New Location Found: " +
                                  args.Position.Coordinate.Latitude.ToString() + ", " +
                                  args.Position.Coordinate.Longitude.ToString();
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
    }
}
