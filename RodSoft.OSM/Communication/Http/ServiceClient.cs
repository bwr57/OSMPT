using System;
using System.Net;

namespace RodSoft.Communications.Http
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
