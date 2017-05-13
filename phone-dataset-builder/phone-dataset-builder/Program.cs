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
                Console.Write(" : " + Phone.model_no + " reported devices ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(" url:" + Phone.url + "\n");
                Console.ResetColor();

                Console.WriteLine("Fetching phone models...");
                List<phone_model> Model = getModelList(Phone.url, false, Phone.model_no);

                Console.WriteLine("Reading Specs...");
                foreach (phone_model model in Model)
                {
                    getSpecs(model.url, model.model);
                }
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

        private static List<phone_model> getModelList(string url, bool isRecursion, string model_no)
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
            string raw_models = null;

            while ((line = sr.ReadLine()) != null)

            {
                if (line == "<div class=\"makers\">")
                    model_class_found = true;

                if ((line.IndexOf("<li>") == 0) && (model_class_found == true))
                {
                    raw_models = line;
                    model_class_found = false;
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
                                string temp_page = line.Substring((line.IndexOf("href=\"") + 6), ((line.IndexOf(".php") + 4) - (line.IndexOf("href=\"") + 6)));

                                navigation_pages.Add("http://www.gsmarena.com/" + temp_page);
                            }

                            line = line.Remove(0, (line.IndexOf(".php") + 4));
                        } while (line.Contains("href") == true);
                    }
                }
            }

            if (page_class_found)
            {
                Console.Write("Recursively fetching phone models from ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine((navigation_pages.Count + 1) + " result pages:");
                Console.ResetColor();
            }

            do

            {
                if (raw_models.Contains("<span>") == true)
                {
                    string temp_url = raw_models.Substring((raw_models.IndexOf("href=\"") + 6), ((raw_models.IndexOf(".php") + 4) - (raw_models.IndexOf("href=\"") + 6)));

                    string temp_model = raw_models.Substring((raw_models.IndexOf("<span>") + 6), ((raw_models.IndexOf("</span>") - (raw_models.IndexOf("<span>") + 6))));

                    temp_url = "http://www.gsmarena.com/" + temp_url;

                    phone_model model = new phone_model(temp_model, temp_url);
                    PhoneModels.Add(model);
                }

                raw_models = raw_models.Remove(0, (raw_models.IndexOf("</span>") + 4));
            } while (raw_models.Contains("<span>") == true);

            sr.Close();
            File.Delete("RawModelPage.html");

            if (!isRecursion)

            {
                Console.Write("\rPopulating model list :" + PhoneModels.Count + "/" + model_no + "...");

                foreach (string result_url in navigation_pages)
                {
                    PhoneModels.AddRange(getModelList(result_url, true, model_no)); //Recursicely get phone models from other result pages and add to list

                    Console.Write("\rPopulating model list :" + PhoneModels.Count + "/" + model_no + "...");
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Done!");
                Console.ResetColor();
            }

            return PhoneModels;
        }

        private static void getSpecs(string url, string model)
        {
            WebClient client = new WebClient();

            StreamWriter rawhtml = new StreamWriter("RawSpecs.html");
            rawhtml.WriteLine(client.DownloadString(url));

            rawhtml.Close();

            StreamReader sr = new StreamReader("RawSpecs.html");

            string line = null;
            bool specs_list_found = false;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(model + ":");
            Console.ResetColor();

            while ((line = sr.ReadLine()) != null)
            {
                if (line.IndexOf("<div id=\"specs-list\">") == 0)
                    specs_list_found = true;

                if (line.IndexOf("<p class=\"note\">") == 0)
                    break;

                if (specs_list_found)
                {
                    if ((line.IndexOf("ttl") > -1) && (line.IndexOf("nbsp") == -1))
                    {
                        line = line.Remove(0, line.IndexOf(">") + 1);

                        line = line.Remove(0, line.IndexOf(">") + 1);

                        line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                        Console.Write(line + ",");
                    }
                }
            }
            Console.WriteLine();

            sr.Close();
            File.Delete("RawSpecs.html");
        }
    }
}