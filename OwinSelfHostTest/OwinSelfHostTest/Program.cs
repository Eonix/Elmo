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
            app.UseElmoViewer("/elmo");
            app.UseElmo();
            app.Use(DoSomething);
            app.UseErrorPage();
            app.UseWelcomePage("/");
        }

        private async Task DoSomething(IOwinContext owinContext, Func<Task> func)
        {
            throw new Exception();
        }
    }
}
