using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Settlement.Classes.Bank.BNI;
using Settlement.Classes.Helper;
using Settlement.Classes.Other;

namespace Settlement
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TKHelper tk = new TKHelper();
                DataConfig config = tk.ParseDataConfig();
                if(config != null)
                {
                    BNI bni = new BNI();
                    //Console.WriteLine(config.ServerEPayment);
                    //Console.WriteLine(config.ServerEPass);
                    //Console.WriteLine(config.DatabaseName);
                    //Console.WriteLine(config.DatabaseUsername);
                    //Console.WriteLine(config.DatabasePassword);
                    //Console.WriteLine(config.SettlementDestinationLinux);
                    //Console.WriteLine(config.SettlementDestinationWindows);
                    //Console.WriteLine(config.TIDSettlement);
                    //Console.WriteLine(config.MIDSettlement);
                    string start = "2019-03-21 10:00:00";
                    string end = "2019-03-21 11:00:00";
                    bni.FetchDeductResultWihtinPeriod(start, end);
                    bni.CreateSettlement(start, end);
                } else
                {
                    Console.WriteLine("Error occurred while parsing configuration file.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
    }
}
