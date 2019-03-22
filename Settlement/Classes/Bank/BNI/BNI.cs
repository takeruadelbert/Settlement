using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BNITapCashDLL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Settlement.Classes.Database;
using Settlement.Classes.FileMonitor;
using Settlement.Classes.Helper;
using Settlement.Classes.API;
using Settlement.Classes.Other;

namespace Settlement.Classes.Bank.BNI
{
    public class BNI
    {
        TapCashDLL bni = new TapCashDLL();
        private string TIDSettlement;
        private string MIDSettlement;
        private List<string> deductResults = new List<string>();
        private TKHelper tk;
        private DBConnect db;
        private FileWatcher watcher;
        private ServerHelper serverHelper;

        private string SettlementPathInUbuntu;
        private string SettlementPathFromWindows;

        public BNI()
        {
            this.TIDSettlement = DataConfig.TIDSettlement;
            this.MIDSettlement = DataConfig.MIDSettlement;
            this.SettlementPathInUbuntu = DataConfig.SettlementDestinationLinux;
            this.SettlementPathFromWindows = DataConfig.SettlementDestinationWindows;

            db = new DBConnect();
            watcher = new FileWatcher();
            tk = new TKHelper();
            serverHelper = new ServerHelper();
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
                Console.WriteLine("Up to Date : No Process Settlement Needed.");
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
                string path = @tk.GetApplicationExecutableDirectoryName() + @"\bin\Debug\settlement\";
                string path_file = @tk.GetApplicationExecutableDirectoryName() + @"\bin\\Debug\settlement\" + FileWatcher.newFile[i];

                // get settlement path in server
                string serverSettlementPath = SettlementPathInUbuntu + filename;

                string created = tk.ConvertDatetimeToDefaultFormat(tk.GetCurrentDatetime());
                string query = "INSERT INTO settlements (path_file, created) VALUES('" + serverSettlementPath + "', '" + created + "')";

                // copy file to destination server
                string targetPath = @"\\" + db.server + SettlementPathFromWindows;
                serverHelper.CopyFileToServer(filename, path, targetPath);

                // insert to server database
                try
                {
                    LastInsertID = db.Insert(query);
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
                        query = "UPDATE deduct_card_results SET settlement_id = " + LastInsertID + ", is_processed = 1 WHERE transaction_dt BETWEEN '" + start_dt + "' AND '" + end_dt + "'";
                        db.Update(query);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Error : something's wrong while inserting new record of settlement file.");
                }
            }
            this.CreateJSONFile(LastInsertID, start_dt, end_dt);
            this.SendCreatedJSONFileToServer();
        }

        private void CreateJSONFile(long LastInsertID, string start, string end)
        {
            try
            {
                JObject temp = db.FetchAllTransactionWithinPeriod(LastInsertID.ToString(), start, end, "BNI");
                var temp2 = JsonConvert.SerializeObject(temp, Formatting.Indented);
                string json_file_path = @tk.GetApplicationExecutableDirectoryName() + @"/src/settlement.json";

                File.WriteAllText(json_file_path, temp2.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SendCreatedJSONFileToServer()
        {
            RESTAPI api = new RESTAPI();
            string ip_address = "http://" + DataConfig.ServerEPass;
            string url = "/epass2018/ws/parking_outs/get_transaction";
            string path_file = @tk.GetApplicationExecutableDirectoryName() + @"\src\settlement.json";
            DataResponse receivedData = api.API_Post_UploadFile(ip_address, url, path_file);
            if(receivedData != null)
            {
                if (receivedData.Status != 206)
                {
                    Console.WriteLine("Error : " + receivedData.Message);
                } else
                {
                    Console.WriteLine("JSON File has been uploaded successfully.");
                }
            } else
            {
                Console.WriteLine("Error : can't establish connection to server.");
            }
        }
    }
}
