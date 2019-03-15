namespace RodSoft.OSM.Tracking
{
    public class TrackPointFactory : IVehicleGeoDataAgentFactory
    {
        public VehicleGeoData CreateVehicleGeoDataAgent(double latitude, double longitude)
        {
            return new TrackPoint(latitude, longitude);
        }
    }
}
