using BNITapCashDLL;
using Newtonsoft.Json;
using Settlement.Classes.API;
using Settlement.Classes.API.Response;
using Settlement.Classes.Constant;
using Settlement.Classes.Database;
using Settlement.Classes.DataModel;
using Settlement.Classes.FileMonitor;
using Settlement.Classes.Helper;
using Settlement.Classes.Other;
using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Classes.Bank.BNI
{
    public class BNI
    {
        TapCashDLL bni = new TapCashDLL();
        private List<string> deductResults = new List<string>();
        private DBConnect db;
        private FileWatcher watcher;
        private RestApi restApi;

        private string SettlementPathInUbuntu;
        private string SettlementPathFromWindows;
        private string TIDSettlement;
        private string MIDSettlement;
        private string BankName;
        private string ApiUrl;

        public BNI()
        {
            TIDSettlement = DataConfig.TIDSettlement;
            MIDSettlement = DataConfig.MIDSettlement;
            SettlementPathInUbuntu = DataConfig.SettlementDestinationLinux;
            SettlementPathFromWindows = DataConfig.SettlementDestinationWindows;
            BankName = DataConfig.BankName;
            ApiUrl = DataConfig.ApiUrl;

            db = new DBConnect();
            watcher = new FileWatcher();
            restApi = new RestApi();
        }

        public List<string> FetchDeductResultWihtinPeriod(string start_dt, string end_dt)
        {
            this.deductResults = db.FetchDeductResult(start_dt, end_dt);
            return this.deductResults;
        }

        public void CreateSettlement(string start_dt, string end_dt)
        {
            if (this.deductResults.Count > 0)
            {
                List<Trx> listDebitLine = new List<Trx>();

                string[] filelines = deductResults.ToArray();
                for (int a = 0; a < filelines.Length; a++)
                {
                    string debitLine = filelines[a];

                    Trx transaksi = new Trx();
                    transaksi.TrxTID = debitLine.Substring(35, 8);
                    transaksi.TrxLine = debitLine;
                    listDebitLine.Add(transaksi);
                }

                listDebitLine.Sort(delegate (Trx c1, Trx c2) { return c1.TrxTID.CompareTo(c2.TrxTID); });

                List<string> settlementList = new List<string>();
                foreach (Trx elemen in listDebitLine)
                {
                    if (elemen.TrxTID.Equals(TIDSettlement))
                    {
                        settlementList.Add(elemen.TrxLine);
                    }
                }

                string result = bni.createSettlement(settlementList, MIDSettlement, TIDSettlement);
                Console.WriteLine(result);

                this.InsertCreatedSettlementFileToDB(start_dt, end_dt);

                this.ResetDeductResult();
            }
            else
            {
                Console.WriteLine(ConstantVariable.SETTLEMENT_UP_TO_DATE);
            }
        }

        private void ResetDeductResult()
        {
            this.deductResults = new List<string>();
        }

        private void InsertCreatedSettlementFileToDB(string start_dt, string end_dt)
        {
            long LastInsertID = -1;
            FileWatcher.newFile.Sort();

            // insert to local database
            for (int i = 0; i < FileWatcher.newFile.Count; i++)
            {
                string filename = @FileWatcher.newFile[i];
                string path = TKHelper.GetApplicationExecutableDirectoryName() + @"\bin\Debug\settlement\";
                string path_file = TKHelper.GetApplicationExecutableDirectoryName() + @"\bin\\Debug\settlement\" + FileWatcher.newFile[i];

                // get settlement path in server
                string serverSettlementPath = SettlementPathFromWindows + filename;

                string created = TKHelper.ConvertDatetimeToDefaultFormat(TKHelper.GetCurrentDatetime());
                string tableName = "settlements";
                string query = string.Format("INSERT INTO settlements (path_file, created) VALUES(@serverSettlementPath, @created)", tableName);

                // copy file to destination server

                string targetPath = SettlementPathFromWindows;
                ServerHelper.CopyFileToServer(filename, path, targetPath);

                // insert to server database
                try
                {
                    Dictionary<string, object> param = new Dictionary<string, object>()
                    {
                        {"@serverSettlementPath", serverSettlementPath },
                        {"@created", created }
                    };
                    LastInsertID = db.Insert(query, param);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                if (LastInsertID != -1)
                {
                    try
                    {
                        // update deduct_card_results table
                        tableName = "deduct_card_results";
                        query = string.Format("UPDATE {0} SET settlement_id=@lastInsertID, is_processed=@is_processed WHERE transaction_dt BETWEEN @start_dt AND @end_dt", tableName);
                        Dictionary<string, object> param = new Dictionary<string, object>()
                        {
                            {"@lastInsertID", LastInsertID },
                            {"@is_processed", 1 },
                            {"@start_dt", start_dt },
                            {"@end_dt", end_dt }
                        };
                        db.Update(query, param);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine(ConstantVariable.ERROR_MESSAGE_INSERT_SETTLEMENT_RECORD_INTO_DATABASE);
                }
            }
            this.CreateJSONFile(LastInsertID, start_dt, end_dt);
            this.SendCreatedJSONFileToServer();
        }

        private void CreateJSONFile(long LastInsertID, string start, string end)
        {
            try
            {
                Transaction temp = db.FetchAllTransactionWithinPeriod(LastInsertID.ToString(), start, end, BankName);
                var temp2 = JsonConvert.SerializeObject(temp, Formatting.Indented);
                string json_file_path = TKHelper.GetApplicationExecutableDirectoryName() + ConstantVariable.DIR_PATH_CREATE_SETTLEMENT_FILE;

                File.WriteAllText(json_file_path, temp2.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SendCreatedJSONFileToServer()
        {
            string ip_address = ConstantVariable.URL_PROTOCOL + DataConfig.ServerEPass;
            string path_file = TKHelper.GetApplicationExecutableDirectoryName() + ConstantVariable.DIR_PATH_CREATE_SETTLEMENT_FILE;
            DataResponse receivedData = restApi.post(ip_address, ApiUrl, path_file, false);
            if (receivedData != null)
            {
                if (receivedData.Status != 206)
                {
                    Console.WriteLine("Error : " + receivedData.Message);
                }
                else
                {
                    Console.WriteLine(ConstantVariable.UPLOAD_JSON_FILE_SUCCESS);
                }
            }
            else
            {
                Console.WriteLine(ConstantVariable.ERROR_MESSAGE_CANNOT_ESTABLISH_CONNECTION_TO_SERVER);
            }
        }
    }
}
