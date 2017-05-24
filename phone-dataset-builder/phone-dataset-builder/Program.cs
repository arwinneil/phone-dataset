using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

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
                    specs Specs = getSpecs(model.url, model.model);
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

        private static specs getSpecs(string url, string model)
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

            string network_technology = "";
            string twoG_bands = "";
            string fourG_bands = "";
            string threeG_bands = "";
            string network_speed = "";
            string GPRS = "";
            string EDGE = "";
            string announced = "";
            string status = "";
            string dimentions = "";
            string weight = "";
            string SIM = "";
            string display_type = "";
            string display_resolution = "";
            string display_size = "";
            string OS = "";
            string CPU = "";
            string Chipset = "";
            string GPU = "";
            string memory_card = "";
            string internal_memory = "";
            string RAM = "";
            string primary_camera = "";
            string secondary_camera = "";
            string loud_speaker = "";
            string audio_jack = "";
            string WLAN = "";
            string bluetooth = "";
            string GPS = "";
            string NFC = "";
            string radio = "";
            string USB = "";
            string sensors = "";
            string battery = "";
            string colors = "";
            string price_group = "";
            string img_url = "";
            while ((line = sr.ReadLine()) != null)
            {
                if (line.IndexOf("<div id=\"specs-list\">") == 0)
                    specs_list_found = true;

                if (line.IndexOf("<p class=\"note\">") == 0)
                    break;

                if (line.IndexOf("HISTORY_ITEM_IMAGE") == 0)
                {
                    line = line.Remove(0, line.IndexOf("\"") + 1);
                    line = line.Substring(0, line.IndexOf("\""));
                    img_url = line;
                }

                if (specs_list_found)
                {
                    if ((line.IndexOf("ttl") > -1) && (line.IndexOf("nbsp") == -1) && (line.IndexOf("lab") == -1))
                    {
                        if (line.IndexOf("Technology") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            network_technology = line;
                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("2G bands") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            twoG_bands = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("3G bands") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            threeG_bands = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("4G bands") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            fourG_bands = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("Speed") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            network_speed = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("GPRS") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            GPRS = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("EDGE") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            EDGE = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("Announced") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            announced = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("Status") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            status = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("Dimensions") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            dimentions = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("Weight") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            display_resolution = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("SIM") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            SIM = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("Type") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            display_type = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("Size") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            display_size = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("Resolution") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            display_resolution = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("OS") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            OS = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("Chipset") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            Chipset = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("CPU") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            CPU = line;

                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("GPU") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            GPU = line;

                            Console.WriteLine(line);
                            continue;
                        }

                        if (line.IndexOf("Card slot") > -1)
                        {
                            do
                            {
                                line = sr.ReadLine();
                            } while (line.IndexOf("nfo") == -1);

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            memory_card = line;
                            Console.WriteLine(line);

                            continue;
                        }

                        if (line.IndexOf("Internal") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            RAM = line;

                            Console.WriteLine(line);
                            continue;
                        }

                        if (line.IndexOf("Primary") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            primary_camera = line;
                            Console.WriteLine(line);
                        }

                        if (line.IndexOf("Secondary") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            secondary_camera = line;
                            Console.WriteLine(line);
                        }
                        if (line.IndexOf("Loudspeaker") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            loud_speaker = line;
                            Console.WriteLine(line);
                        }

                        if (line.IndexOf("3.5mm jack") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            audio_jack = line;
                            Console.WriteLine(line);
                        }

                        if (line.IndexOf("Loudspeaker") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            loud_speaker = line;
                            Console.WriteLine(line);
                        }

                        if (line.IndexOf("WLAN") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            WLAN = line;
                            Console.WriteLine(line);
                        }

                        if (line.IndexOf("Bluetooth") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            bluetooth = line;
                            Console.WriteLine(line);
                        }
                        if (line.IndexOf("GPS") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            GPS = line;
                            Console.WriteLine(line);
                        }

                        if (line.IndexOf("NFC") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            NFC = line;
                            Console.WriteLine(line);
                        }

                        if (line.IndexOf("Radio") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            radio = line;
                            Console.WriteLine(line);
                        }

                        if (line.IndexOf("USB") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            bluetooth = line;
                            Console.WriteLine(line);
                        }

                        if (line.IndexOf("Sensors") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                            bluetooth = line;
                            Console.WriteLine(line);
                        }

                        if (line.IndexOf("Battery") > -1)
                        {
                            line = sr.ReadLine();
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));
                            Console.WriteLine(line);

                            battery = line;
                        }

                        if (line.IndexOf("Colors") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));
                            Console.WriteLine(line);

                            colors = line;
                        }

                        if (line.IndexOf("Price group") > -1)
                        {
                            line = sr.ReadLine();

                            line = line.Remove(0, line.IndexOf(">") + 1);
                            line = line.Remove(0, line.IndexOf(">") + 1);

                            line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));
                            Console.WriteLine(line);

                            price_group = line;
                        }
                        //line = line.Remove(0, line.IndexOf(">") + 1);

                        //line = line.Remove(0, line.IndexOf(">") + 1);

                        //line = line.Remove(line.IndexOf("<"), ((line.Length) - (line.IndexOf("<"))));

                        //Console.Write(line + ",");
                    }
                }
            }
            Console.WriteLine();

            sr.Close();
            File.Delete("RawSpecs.html");

            specs Specs = new specs(network_technology,
             twoG_bands,
             threeG_bands,
             fourG_bands,
             network_speed,
             GPRS,
             EDGE,
             announced,
             status,
             dimentions,
             weight,
             SIM,
             display_type,
             display_resolution,
             display_size,
             OS,
             CPU,
             Chipset,
             GPU,
             memory_card,
             internal_memory,
             RAM,
             primary_camera,
             secondary_camera,
             loud_speaker,
             audio_jack,
             WLAN,
             bluetooth,
             GPS,
             NFC,
             radio,
             USB,
             sensors,
             battery,
             colors,
             price_group,
             img_url
                );

            return Specs;
        }
    }
}