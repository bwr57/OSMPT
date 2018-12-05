using System;

namespace RodSoft.OSMPT.PT.Online
{
    [Serializable]
    public class EttuVehiclePosition
    {
        public DateTime ATime;
        public uint DEV_ID;
        public double Lat;
        public double Lon;
        public string Route;
        public int Course;
        public int Velocity;
        public int ON_ROUTE;
        public int BOARD_ID;
        public int BOARD_NUM;
        public int DEPOT;

        public VehicleData ToVehicleData(VehicleData vd)
        {
            if (vd == null)
                vd = new VehicleData(this.Lat, this.Lon);
            vd.Time = this.ATime;
            vd.Id = BOARD_ID;
            vd.Number = this.BOARD_NUM.ToString();
            vd.Speed = this.Velocity;
            vd.Bearing = this.Course + 90;
            if (vd.Bearing > 360)
                vd.Bearing = vd.Bearing - 360;
            vd.Line = this.Route;
            vd.Operator = "ЕТТУ";
            return vd;
        }

        public VehicleData ToVehicleData()
        {
            return ToVehicleData(null);
        }
    }

    public class EttuErrorMessage
    {
        public uint ID;
        public string Message;
    }

    public class EttuServerMessage
    {
        public EttuErrorMessage Error;
        public EttuVehiclePosition[] Vehicles;
    }
}
