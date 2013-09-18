using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using Nancy.Hosting.Self;
using System.Net.NetworkInformation;

namespace NancyDemon
{
    class Program : MarshalByRefObject
    {
        private NancyHost host;
        private Uri uri;

        public Program Start(string uri)
        {
            host = new NancyHost(this.uri = new Uri(uri));
            host.Start();
            Console.WriteLine("Starting Nancy Server at http://{0}:{1}.", this.uri.Host, this.uri.Port);

            return this;
        }

        public void End()
        {
            host.Stop();
            Console.WriteLine("Stopped Nancy Server.");
        }

        

        [STAThread]
        static void Main(string[] args)
        {
            using (var watch = new FileWatcher()) { }
        }

    }
}
