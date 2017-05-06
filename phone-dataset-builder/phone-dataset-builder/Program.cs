using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace phone_dataset_builder
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            getPhoneList();
        }

        private static void getPhoneList()
        {
            string htmlCode = null;

            WebClient client = new WebClient();

            StreamWriter file = new StreamWriter("RawListPage.html");
            file.WriteLine(client.DownloadString("http://www.gsmarena.com/makers.php3"));
            file.Close();

            Console.WriteLine("Getting page done..");
            Console.ReadKey();
        }
    }
}