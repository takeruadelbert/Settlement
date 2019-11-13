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
                //string currentDate = TKHelper.GetCurrentDate();
                string currentDate = "2019-11-12";
                string start = currentDate + " " + args[0];
                string end = currentDate + " " + args[1];
                DataConfig config = TKHelper.ParseDataConfig();
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
