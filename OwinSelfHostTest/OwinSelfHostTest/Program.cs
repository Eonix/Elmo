using System;
using System.Threading.Tasks;
using Elmo;
using Elmo.Viewer;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;

namespace OwinSelfHostTest
{
    internal class Program
    {
        private static void Main()
        {
            using (WebApp.Start<Startup>("http://+:48784/"))
            {
                Console.WriteLine("Started");
                Console.ReadLine();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseElmoMemoryLog();
            app.UseElmoViewer();
            //app.UseErrorPage();
            app.UseWelcomePage("/");
        }
    }
}
