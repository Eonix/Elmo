using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elmo;
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
            app.UseElmoViewer("/elmo");
            //app.UseErrorPage();
            //app.UseWelcomePage("/");
            app.Use(DoSomething);
        }

        private Task DoSomething(IOwinContext owinContext, Func<Task> func)
        {
            throw new Exception();
        }
    }
}
