namespace RodSoft.OSM.Tracking
{
    public interface IVehicleGeoDataAgentFactory
    {
        VehicleGeoData CreateVehicleGeoDataAgent(double Latitude, double Longitude);
    }
}
