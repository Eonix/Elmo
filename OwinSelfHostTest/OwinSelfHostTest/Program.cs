using System;
using System.Threading.Tasks;
using Elmo;
using Elmo.Viewer;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Owin;

namespace OwinSelfHostTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://localhost:12345"))
            {
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
            //app.UseWelcomePage("/");
            app.Use(DoSomething);
        }

        private static Task DoSomething(IOwinContext owinContext, Func<Task> func)
        {
            throw new Exception();
        }
    }
}
