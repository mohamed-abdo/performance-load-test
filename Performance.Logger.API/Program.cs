using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Performance.Logger.API
{
    public class Program
    {
        static void Main(string[] args)
        {
            string baseAddress = ConfigurationManager.AppSettings["address"];
            // Start OWIN host 
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                Console.WriteLine($"Performance Test is loading on address:{baseAddress}");
                HttpClient client = new HttpClient();
                Console.WriteLine("Press enter to exit!");
                Console.ReadLine();
            }
        }
    }
}
