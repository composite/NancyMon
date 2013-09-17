using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Nancy.Hosting.Self;

namespace NancyDemon
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            AppDomain app = AppDomain.CurrentDomain;
            string basepath = Path.GetDirectoryName(asm.Location)
                ,asmpath=Path.Combine(basepath,Defaults.AssemblyPath);

            if (Directory.Exists(asmpath))
                foreach (string filename in Directory.EnumerateFiles(asmpath, "*.dll"))
                {
                    Assembly asmfile = Assembly.LoadFile(filename);
                    app.Load(asmfile.GetName());
                }

            NancyHost host = new NancyHost(new Uri("http://localhost:8800"));
            host.Start();
            Console.WriteLine("Starting Nancy Server.");

            Console.ReadLine();

            host.Stop();
            Console.WriteLine("Stopped Nancy Server.");
        }
    }
}
