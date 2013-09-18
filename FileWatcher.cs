using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace NancyDemon
{
    [Serializable]
    class FileWatcher : IDisposable
    {
        public static readonly AppDomain CurrentApp = AppDomain.CurrentDomain;
        public static readonly Assembly CurrentAssembly = Assembly.GetExecutingAssembly();
        public static readonly string CurrentPath = Path.GetDirectoryName(CurrentAssembly.Location);

        private AppDomain app;
        private Program prog;
        private bool waiting = false;
        private string host = "localhost";
        private int port = 3000;

        private IDictionary<string, Assembly> asms = new Dictionary<string, Assembly>(0);
        private Random rand = new Random();

        private string binpath = Path.Combine(CurrentPath, Defaults.AssemblyPath);
        private FileSystemWatcher binwatch;
        private IDictionary<string, Assembly> binasms = new Dictionary<string, Assembly>(0);
        private string codepath = Path.Combine(CurrentPath, Defaults.CodesPath);
        private FileSystemWatcher codewatch;
        private string workpath = Path.Combine(CurrentPath, ".binwork");
        private FileSystemWatcher workwatch;

        public string AppName { get; private set; }

        public FileWatcher()
        {
            InitAppDoamin();
            InitBinWatcher();
        }

        private void InitAppDoamin()
        {
            AppName = "NancyServ#" + Math.Abs(rand.Next().GetHashCode());
            Console.WriteLine("AppDomain will be created as {0}", AppName);
            app = AppDomain.CreateDomain(AppName, CurrentApp.Evidence, new AppDomainSetup() { ApplicationBase = CurrentPath, PrivateBinPath = Path.PathSeparator + "bin" });
            app.DomainUnload += (sender, arg) =>
            {
                Console.WriteLine("AppDomain is unloading {0}", AppName);
                if (prog != null) prog.End();
                binwatch.WaitForChanged(WatcherChangeTypes.All);
            };
            app.UnhandledException += (sender, arg) =>
            {
                Console.WriteLine(arg.ExceptionObject);
                EndAppDomain();
                Console.WriteLine("Exception Thrown. Waiting file changes to start.");
                binwatch.WaitForChanged(WatcherChangeTypes.All);
            };
            app.AssemblyResolve += (sender, arg) =>
            {
                Console.WriteLine("{0}.dll file not found? I'll find it.",arg.Name);
                string binfile = Path.Combine(binpath, arg.Name + ".dll");
                if (File.Exists(binfile)) return Assembly.LoadFile(binfile);

                return null;
            };
            Type pt = typeof(Program);
            prog = (Program)app.CreateInstanceAndUnwrap(pt.Assembly.FullName, pt.FullName);
            prog.Start(string.Format("http://{0}:{1}", host, port));
        }

        private void EndAppDomain()
        {
            AppDomain.Unload(app);
            app = null;
        }

        private void InitBinWatcher()
        {
            if (!Directory.Exists(binpath)) Directory.CreateDirectory(binpath);

            Type fsw = typeof(FileSystemWatcher);
            binwatch = new FileSystemWatcher();
            binwatch.Path = binpath;
            binwatch.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            IList<Regex> ignores = new List<Regex>(0);
            string igfile = Path.Combine(binpath, ".nancyignore");
            if (File.Exists(igfile))
            {
                using (var fileread = new StreamReader(igfile))
                {
                    for (string line = fileread.ReadLine(); line != null; )
                    {
                        if (line.Trim().StartsWith("#")) continue;
                        ignores.Add(Wildcard2Regex(Path.Combine(binpath, line)));
                    }
                }
            }

            Action comminit = () =>
            {
                foreach (string fullname in binasms.Keys)
                {
                    if (ignores.Count > 0 && ignores.Any(rx => rx.IsMatch(fullname))) continue;

                    app.Load(binasms[fullname].GetName());
                }

                Retry();
            };

            FileSystemEventHandler evt = (sender, arg) =>
            {
                if (!Regex.IsMatch(@"\.dll$", arg.FullPath)) return;

                switch (arg.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        Assembly asm = Assembly.LoadFile(arg.FullPath);
                        binasms.Add(arg.FullPath, asm);
                        app.Load(asm.GetName());
                        return;
                    case WatcherChangeTypes.Deleted:
                        binasms.Remove(arg.FullPath);
                        break;
                    case WatcherChangeTypes.Changed:
                        binasms[arg.FullPath] = Assembly.LoadFile(arg.FullPath);
                        break;
                }

                comminit.Invoke();
            };

            binwatch.Created += evt;
            binwatch.Changed += evt;
            binwatch.Deleted += evt;
            binwatch.Renamed += (sender, arg) =>
            {
                if (!Regex.IsMatch(@"\.dll$", arg.FullPath)) return;

                binasms.Remove(arg.OldFullPath);

                Assembly asm = Assembly.LoadFile(arg.FullPath);
                binasms.Add(arg.FullPath, asm);
                app.Load(asm.GetName());

                comminit.Invoke();
            };

            foreach (string fullname in Directory.EnumerateFiles(binpath, "*.dll", SearchOption.AllDirectories))
            {
                if (ignores.Count > 0 && ignores.Any(rx => rx.IsMatch(fullname))) continue;

                Assembly asm = Assembly.LoadFile(fullname);
                binasms.Add(fullname, asm);
                app.Load(asm.GetName());
            }

            binwatch.EnableRaisingEvents = true;
            binwatch.WaitForChanged(WatcherChangeTypes.All);
        }

        private void Retry()
        {
            if (app != null) EndAppDomain();
            if (waiting) waiting = false;
            InitAppDoamin();
        }

        public void Dispose()
        {
            EndAppDomain();
            if (binwatch != null) binwatch.Dispose();
            if (codewatch != null) codewatch.Dispose();
            if (workwatch != null) workwatch.Dispose();
        }

        public static bool IsPortValid(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
                if (tcpi.LocalEndPoint.Port == port) return false;

            return true;
        }

        public static Regex Wildcard2Regex(string ptn)
        {
            return new Regex("^" + Regex.Escape(ptn).
                       Replace(@"\*", ".*").
                       Replace(@"\?", ".") + "$", RegexOptions.Compiled);
        }
    }
}
