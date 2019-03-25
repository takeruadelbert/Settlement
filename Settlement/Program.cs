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
                string start = tk.GetCurrentDate() + " " + args[0];
                string end = tk.GetCurrentDate() + " " + args[1];
                DataConfig config = tk.ParseDataConfig();
                if (config != null)
                {
                    BNI bni = new BNI();
                    bni.FetchDeductResultWihtinPeriod(start, end);
                    bni.CreateSettlement(start, end);
                }
                else
                {
                    Console.WriteLine("Error occurred while parsing configuration file.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
