namespace RodSoft.OSM.Tracking
{
    public interface IVehicleGeoDataAgentFactory
    {
        VehicleGeoData CreateVehicleGeoDataAgent(double latitude, double longitude);
    }
}
