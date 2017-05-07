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
            List<phone_brand> PhoneBrands = getPhoneList();
        }

        private static List<phone_brand> getPhoneList()
        {
            List<phone_brand> PhoneBrands = new List<phone_brand>();

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
                    string temp_brand = line.Substring((line.IndexOf(".php>") + 5), ((line.IndexOf("<br>")) - (line.IndexOf(".php>") + 5)));
                    string temp_model_no = line.Substring((line.IndexOf("<span>") + 6), ((line.IndexOf(" devices")) - (line.IndexOf("<span>") + 6)));
                    string temp_url = "http://www.gsmarena.com/" + line.Substring((line.IndexOf("href=") + 5), ((line.IndexOf(".php>") + 4) - (line.IndexOf("href=") + 5)));

                    phone_brand Phone = new phone_brand(temp_brand, temp_model_no, temp_url);

                    PhoneBrands.Add(Phone);

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(Phone.brand);
                    Console.ResetColor();
                    Console.Write(":" + Phone.model_no + " devices");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" url:" + Phone.url + "\n");
                    Console.ResetColor();
                }

                if (line.IndexOf("<table>") == 0)
                    table = true;
            }

            Console.ReadKey();

            return PhoneBrands;
        }
    }
}