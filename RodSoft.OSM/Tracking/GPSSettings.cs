using RodSoft.Core.Configuration;

namespace RodSoft.OSM.Tracking
{
    public enum GeoLocationRegistratorTypes
    {
        Off = 0,
        DeviceAPI = 1,
        LocationAPI = 2,
        AndroidLocationService = 3,
        NMEAComPort = 4
    }

    public class GPSSettings : ServiceSettings
    {
        public GeoLocationRegistratorTypes GeoLocationRegistratorType;

        public int IPPort;
    }
}
