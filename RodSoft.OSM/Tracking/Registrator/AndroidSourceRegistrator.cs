using RodSoft.Core.Communications;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RodSoft.OSM.Tracking.Registrator
{
    public class AndroidSourceRegistrator : ActiveRemoteDeviceDriverBase, IGeoPositionRegistrator, IDisposable
    {

        public IVehicleGeoDataAgentFactory VehicleGeoDataAgentFactory { get; set; }
        public int LocalPort
        {
            get
            {
                return _ClientEndPoint == null ? 0 : _ClientEndPoint.Port;
            }
            set { CreateEndPoint(value); }
        }

        public double Latitude;
        public double Longitude;
        public double Altitude;
        public double Speed;
        public double Course;

        protected IPEndPoint _ClientEndPoint;
        protected Socket _WinSocket;
        protected EndPoint _Sender =  new IPEndPoint(IPAddress.Any, 0);

        protected bool IsError;


        public AndroidSourceRegistrator(GPSSettings settings)
            : base()
        {
            if (settings != null)
                CreateEndPoint(settings.IPPort);
        }

        public AndroidSourceRegistrator(string name, GPSSettings settings)
            :base(name)
        {
            if(settings != null)
                CreateEndPoint(settings.IPPort);
        }

        public virtual bool CreateEndPoint(int port)
        {
            _ClientEndPoint = new IPEndPoint(IPAddress.Any, port);
            try
            {
                _WinSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _WinSocket.Bind(_ClientEndPoint);
                _WinSocket.ReceiveTimeout = 100;
            }
            catch (Exception ex)
            {
                if (_WinSocket != null)
                {
                    _WinSocket.Dispose();
                    _WinSocket = null;
                }
                _ClientEndPoint = null;
                return false;
            }
            return true;
        }


        protected override void Process()
        {
            if (_WinSocket == null)
                return;
            byte[] data = new byte[1024];
            int recv = 0;
            try
            {
                lock (_WinSocket)
                    recv = _WinSocket.ReceiveFrom(data, ref _Sender);
                if (recv == 0)
                    return;
                string strLocation = Encoding.ASCII.GetString(data, 0, recv);
                string[] strLocationParameters = strLocation.Split(',');
                Latitude = double.Parse(strLocationParameters[0]);
                Longitude = double.Parse(strLocationParameters[1]);
                Altitude = double.Parse(strLocationParameters[2]);
                Speed = double.Parse(strLocationParameters[3]);
                Course = double.Parse(strLocationParameters[4]);
                LastRegistrationTime = DateTime.Now;
                IsError = false;
            }
            catch (Exception ex)
            {
                IsError = true;
            }
        }


        public VehicleGeoData GetCurrentPosition()
        {
            VehicleGeoData vehicleGeoData = VehicleGeoDataAgentFactory == null ? new VehicleGeoData(Latitude, Longitude) : VehicleGeoDataAgentFactory.CreateVehicleGeoDataAgent(Latitude, Longitude);
            vehicleGeoData.Altitude = Convert.ToSingle(Altitude);
            vehicleGeoData.Speed = Convert.ToInt16(Speed);
            vehicleGeoData.Course = Convert.ToInt16(Course);
            return vehicleGeoData;
        }

        public override void Dispose()
        {
            base.Dispose();
            Thread.Sleep(200);
            if (_WinSocket != null)
            {
                _WinSocket.Dispose();
                _WinSocket = null;
            }
        }

        public GeoPositionRegistratorStatus GetRegistratorStatus()
        {
            if (IsActive)
                return GeoPositionRegistratorStatus.Active;
            if (IsError)
                return GeoPositionRegistratorStatus.Error;
            return GeoPositionRegistratorStatus.Initializing;
        }
    }
}
