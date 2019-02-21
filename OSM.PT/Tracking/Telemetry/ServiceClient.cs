using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Demo.WindowsPresentation.Tracking.Telemetry
{
    public class ServiceClient : WebClient
    {
        public int Timeout
        {
             get; set;
        }

        public ServiceClient()
            : base()
        { }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest webRequest = base.GetWebRequest(address);
            webRequest.Timeout = this.Timeout;
            return webRequest;
        }
    }
}
