using MyPoiHub.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Core;

namespace MyPoiHub.Common
{
    public sealed class LocalizationHelper
    {
        private static LocalizationHelper _localizationHelper;
        private CoreDispatcher _cd = CoreWindow.GetForCurrentThread().Dispatcher;
        private Geoposition myGeoposition;
        private Geolocator myGeolocator = null;
        public static async Task<LocalizationHelper> GetInstance()
        {
            if (_localizationHelper == null)
            {
                _localizationHelper = new LocalizationHelper();
                await _localizationHelper.SetMyGeopositionAsync();  
            }
            return _localizationHelper;
        }

        static LocalizationHelper()
        {

        }
        public Geoposition GetMyGeoposition()
        {
            return this.myGeoposition;
        }
        public async Task<LocalizationHelper> SetMyGeopositionAsync()
        {

            if (myGeolocator == null)
            {
                myGeolocator = new Geolocator();
                myGeolocator.DesiredAccuracyInMeters = 50;
                try
                {
                    myGeoposition = await myGeolocator.GetGeopositionAsync(maximumAge: TimeSpan.FromMinutes(5), timeout: TimeSpan.FromSeconds(10));
                }
                catch (Exception ex)
                {
                    MessageDialogHelper.Show(ex.Message, ex.GetType().ToString());
                }
            }

            if (myGeolocator != null)
            {
                myGeolocator.MovementThreshold = 3.0;
                myGeolocator.PositionChanged += new TypedEventHandler<Geolocator, PositionChangedEventArgs>(myGeolocator_PositionChanged);
            }
            return this;
        }
        async private void myGeolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            await _cd.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                myGeoposition = e.Position;
            });
        }
    }
}
