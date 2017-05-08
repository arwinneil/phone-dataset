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
            Console.WriteLine("Retrieving phone brands from GSM arena... \n");
            List<phone_brand> PhoneBrands = getBrandList();

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("\n\nRetrived " + PhoneBrands.Count + " phone brands\n");
            Console.ResetColor();
            Console.WriteLine("Retrieving phone models by brand... \n");

            foreach (phone_brand Phone in PhoneBrands)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\n" + Phone.brand);
                Console.ResetColor();
                Console.Write(" : " + Phone.model_no + " devices ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" url:" + Phone.url + "\n");
                Console.ResetColor();

                Console.WriteLine("Fetching models...");
                getModelList(Phone.url, false);
            }
        }

        private static List<phone_brand> getBrandList()
        {
            List<phone_brand> PhoneBrands = new List<phone_brand>();

            WebClient client = new WebClient();

            StreamWriter rawhtml = new StreamWriter("RawListPage.html");
            rawhtml.WriteLine(client.DownloadString("http://www.gsmarena.com/makers.php3"));
            rawhtml.Close();

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
                    Console.Write(";");
                    ;
                }

                if (line.IndexOf("<table>") == 0)
                    table = true;
            }
            sr.Close();
            File.Delete("RawListPage.html");
            return PhoneBrands;
        }

        private static List<phone_model> getModelList(string url, bool isRecursion)
        {
            List<phone_model> PhoneModels = new List<phone_model>();

            List<string> navigation_pages = new List<string>();

            WebClient client = new WebClient();

            StreamWriter rawhtml = new StreamWriter("RawModelPage.html");
            rawhtml.WriteLine(client.DownloadString(url));

            rawhtml.Close();

            StreamReader sr = new StreamReader("RawModelPage.html");

            string line = null;
            bool model_class_found = false;
            bool page_class_found = false;

            while ((line = sr.ReadLine()) != null)

            {
                if (line == "<div class=\"makers\">")
                    model_class_found = true;

                if ((line.IndexOf("<li>") == 0) && (model_class_found == true))
                {
                    string raw_models = line;
                }

                if (!isRecursion) //index search pages only if not recursion
                {
                    if (line == "<div class=\"nav-pages\">")
                        page_class_found = true;

                    if ((line.IndexOf("<strong>") == 0) && (page_class_found == true))
                    {
                        do
                        {
                            if (line.Contains("href") == true)
                            {
                                string temp_page = "http://http://www.gsmarena.com/" + line.Substring((line.IndexOf("href =\"") + 6), ((line.IndexOf(".php") + 4) - (line.IndexOf("href=\"") + 6)));

                                navigation_pages.Add(temp_page);
                            }

                            line = line.Remove(0, (line.IndexOf(".php") + 4));
                        } while (line.Contains("href") == true);
                    }
                }
            }

            foreach (string page_url in navigation_pages)
            {
                Console.WriteLine(page_url);
            }

            Console.WriteLine();
            sr.Close();
            File.Delete("RawModelPage.html");
            return PhoneModels;
        }
    }
}