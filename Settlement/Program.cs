using Settlement.Classes.Bank.BNI;
using Settlement.Classes.Constant;
using Settlement.Classes.Helper;
using Settlement.Classes.Other;
using System;

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
                    Console.WriteLine(ConstantVariable.ERROR_MESSAGE_FAIL_TO_PARSE_FILE_CONFIG);
                }
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
