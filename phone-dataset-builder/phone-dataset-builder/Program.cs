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
            WebClient client = new WebClient();

            StreamWriter rawhtml = new StreamWriter("RawListPage.html");
            rawhtml.WriteLine(client.DownloadString("http://www.gsmarena.com/makers.php3"));
            rawhtml.Close();

            Console.WriteLine("Getting page done..");
            Console.ReadKey();

            StreamReader sr = new StreamReader("RawListPage.html");

            string line;
            bool table = false;

            while ((line = sr.ReadLine()) != null)

            {
                if (line.IndexOf("</table>") == 0)
                    break;

                if (table == true && line != "")
                {
                    line = line.Substring((line.IndexOf(".php>") + 5), ((line.IndexOf("<br>")) - (line.IndexOf(".php>") + 5)));

                    Console.Write(line + ",");
                }

                if (line.IndexOf("<table>") == 0)
                    table = true;
            }

            Console.ReadKey();
        }
    }
}