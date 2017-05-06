using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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

            using (WebClient client = new WebClient())
            {
                htmlCode = client.DownloadString("http://www.gsmarena.com/makers.php3");
            }

            Console.WriteLine(htmlCode);
            Console.ReadKey();
        }
    }
}