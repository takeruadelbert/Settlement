using Newtonsoft.Json;
using Settlement.Classes.Other;
using System;
using System.IO;

namespace Settlement.Classes.Helper
{
    class TKHelper
    {
        public static string GetCurrentDatetime()
        {
            return DateTime.Now.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID")) + " " + DateTime.Now.ToString("HH:mm:ss");
        }

        public static string GetCurrentDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        // Default Format : yyyy-MM-dd HH:mm:ss
        public static string ConvertDatetimeToDefaultFormat(string dt)
        {
            string[] temp = dt.Split(' ');
            string date = temp[0];
            string month = GetMonthInNumber(temp[1]);
            string year = temp[2];
            string time = temp[3];
            return year + "-" + month + "-" + date + " " + time;
        }

        // Format : dd/MM/yyyy HH:mm:ss
        public static string ConvertDatetimeToDefaultFormatMySQL(string dt)
        {
            string[] temp = dt.Split(' ');
            string date = temp[0];
            string time = temp[1];
            string[] temp2 = date.Split('/');
            string day = temp2[0];
            string month = temp2[1];
            string year = temp2[2];
            return year + "-" + month + "-" + day + " " + time;
        }

        public static string GetApplicationExecutableDirectoryName()
        {
            string workingDirectory = Environment.CurrentDirectory;
            return Directory.GetParent(workingDirectory).Parent.FullName;
        }

        public static string GetDirectoryName()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory;
        }

        private static string GetMonthInNumber(string month, int digit = 2)
        {
            int month_in_number = -1;
            switch (month)
            {
                case "Januari":
                    month_in_number = 1;
                    break;
                case "Februari":
                    month_in_number = 2;
                    break;
                case "Maret":
                    month_in_number = 3;
                    break;
                case "April":
                    month_in_number = 4;
                    break;
                case "Mei":
                    month_in_number = 5;
                    break;
                case "Juni":
                    month_in_number = 6;
                    break;
                case "Juli":
                    month_in_number = 7;
                    break;
                case "Agustus":
                    month_in_number = 8;
                    break;
                case "September":
                    month_in_number = 9;
                    break;
                case "Oktober":
                    month_in_number = 10;
                    break;
                case "November":
                    month_in_number = 11;
                    break;
                case "Desember":
                    month_in_number = 12;
                    break;
                default:
                    month_in_number = -1;
                    break;
            }
            return month_in_number != -1 ? month_in_number.ToString("00") : "";
        }

        public static DataConfig ParseDataConfig(string config_file = "")
        {
            config_file = config_file == "" ? @GetApplicationExecutableDirectoryName() + @"\Configuration\config.json" : config_file;
            using (StreamReader r = new StreamReader(config_file))
            {
                string json = r.ReadToEnd();
                DataConfig config = JsonConvert.DeserializeObject<DataConfig>(json);
                return config;
            }
        }
    }
}
