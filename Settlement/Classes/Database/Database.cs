using MySql.Data.MySqlClient;
using Settlement.Classes.Constant;
using Settlement.Classes.DataModel;
using Settlement.Classes.Helper;
using Settlement.Classes.Other;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Settlement.Classes.Database
{
    public class DBConnect
    {
        private MySqlConnection connection;
        public string server;
        public string database;
        public string uid;
        public string password;
        private TKHelper tk;

        public DBConnect()
        {
            tk = new TKHelper();
            Initialize();
        }

        private void Initialize()
        {
            server = DataConfig.ServerEPayment;
            database = DataConfig.DatabaseName;
            uid = DataConfig.DatabaseUsername;
            password = DataConfig.DatabasePassword;
            string connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            connection = new MySqlConnection(connectionString);
        }

        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine(ConstantVariable.ERROR_MESSAGE_CANNOT_ESTABLISH_CONNECTION_TO_SERVER);
                        break;

                    case 1045:
                        Console.WriteLine(ConstantVariable.ERROR_MESSAGE_INVALID_USERNAME_PASSWORD);
                        break;
                }
                return false;
            }
        }

        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public long Insert(string query_cmd)
        {
            string query = query_cmd;
            long insert_last_id = -1;
            if (this.OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                insert_last_id = cmd.LastInsertedId;
                this.CloseConnection();
            }
            return insert_last_id;
        }

        public void Update(string query_cmd)
        {
            string query = query_cmd;
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = query;
                cmd.Connection = connection;

                cmd.ExecuteNonQuery();

                this.CloseConnection();
            }
        }

        public void Delete(string query_cmd)
        {
            string query = query_cmd;
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        public List<string> FetchDeductResult(string start_dt, string end_dt)
        {
            string query = "SELECT * FROM deduct_card_results WHERE is_processed=0 AND transaction_dt BETWEEN '" + start_dt + "' AND '" + end_dt + "'";
            List<string> list = new List<string>();

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    list.Add(dataReader["result"] + "");
                }

                dataReader.Close();

                this.CloseConnection();

                return list;
            }
            else
            {
                return list;
            }
        }

        public int Count()
        {
            string query = "SELECT Count(*) FROM settlements";
            int Count = -1;

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                Count = int.Parse(cmd.ExecuteScalar() + "");

                this.CloseConnection();

                return Count;
            }
            else
            {
                return Count;
            }
        }

        public void Backup()
        {
            try
            {
                DateTime Time = DateTime.Now;
                int year = Time.Year;
                int month = Time.Month;
                int day = Time.Day;
                int hour = Time.Hour;
                int minute = Time.Minute;
                int second = Time.Second;
                int millisecond = Time.Millisecond;

                string path;
                path = "C:\\MySqlBackup" + year + "-" + month + "-" + day + "-" + hour + "-" + minute + "-" + second + "-" + millisecond + ".sql";
                StreamWriter file = new StreamWriter(path);


                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysqldump";
                psi.RedirectStandardInput = false;
                psi.RedirectStandardOutput = true;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}",
                    uid, password, server, database);
                psi.UseShellExecute = false;

                Process process = Process.Start(psi);

                string output;
                output = process.StandardOutput.ReadToEnd();
                file.WriteLine(output);
                process.WaitForExit();
                file.Close();
                process.Close();
            }
            catch (IOException)
            {
                Console.WriteLine(ConstantVariable.ERROR_MESSAGE_UNABLE_TO_BACKUP);
            }
        }

        public void Restore()
        {
            try
            {
                string path;
                path = "C:\\MySqlBackup.sql";
                StreamReader file = new StreamReader(path);
                string input = file.ReadToEnd();
                file.Close();

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysql";
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = false;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}",
                    uid, password, server, database);
                psi.UseShellExecute = false;


                Process process = Process.Start(psi);
                process.StandardInput.WriteLine(input);
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();
            }
            catch (IOException)
            {
                Console.WriteLine(ConstantVariable.ERROR_MESSAGE_UNABLE_TO_RESTORE);
            }
        }

        public bool CheckMySQLConnection()
        {
            bool successful = true;
            try
            {
                successful = this.OpenConnection();
                if (successful)
                {
                    this.CloseConnection();
                }
            }
            catch (MySqlException)
            {
                successful = false;
            }
            return successful;
        }

        public Transaction FetchAllTransactionWithinPeriod(string settlement_id, string start_dt, string end_dt, string bank)
        {
            // detail transaction process
            string query = "select * from deduct_card_results where is_processed=1 and settlement_id = '" + settlement_id + "' and transaction_dt between '" + start_dt + "' and '" + end_dt + "'";
            Transaction transaction = null;
            List<DetailTransaction> detailTransactions = new List<DetailTransaction>();

            if (this.OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    string transactionDt = TKHelper.ConvertDatetimeToDefaultFormatMySQL(dataReader["transaction_dt"].ToString());
                    int amount = Convert.ToInt32(dataReader["amount"].ToString());
                    string ipv4 = dataReader["ipv4"].ToString();
                    string operatorName = dataReader["operator"].ToString();
                    string idReader = dataReader["ID_reader"].ToString();
                    DetailTransaction detailTransaction = new DetailTransaction(amount, transactionDt, ipv4, operatorName, idReader);
                    detailTransactions.Add(detailTransaction);
                }
                dataReader.Close();
                this.CloseConnection();
            }

            if (this.OpenConnection())
            {
                // settlement process
                query = "select * from settlements where id = '" + settlement_id + "'";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    string created = dataReader["created"].ToString();
                    string createdSettlementDt = TKHelper.ConvertDatetimeToDefaultFormatMySQL(created);

                    transaction = new Transaction(createdSettlementDt, bank, detailTransactions);
                }
                dataReader.Close();
                this.CloseConnection();
            }
            return transaction;
        }
    }
}
