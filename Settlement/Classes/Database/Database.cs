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

        public long Insert(string query_cmd, Dictionary<string, object> paramValues = null)
        {
            string query = query_cmd;
            long insert_last_id = -1;
            if (this.OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                if (paramValues.Count > 0)
                {
                    foreach (var param in paramValues)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                cmd.ExecuteNonQuery();
                insert_last_id = cmd.LastInsertedId;
                this.CloseConnection();
            }
            return insert_last_id;
        }

        public void Update(string query_cmd, Dictionary<string, object> paramValues = null)
        {
            string query = query_cmd;
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand();

                if (paramValues.Count > 0)
                {
                    foreach (var param in paramValues)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

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
            string tableName = "deduct_card_results";
            string query = string.Format("SELECT * FROM {0} WHERE is_processed=@is_processed AND transaction_dt BETWEEN @start_dt AND @end_dt", tableName);
            List<string> list = new List<string>();

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@is_processed", 0);
                cmd.Parameters.AddWithValue("@start_dt", start_dt);
                cmd.Parameters.AddWithValue("@end_dt", end_dt);
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
            string tableName = "deduct_card_results";
            string query = string.Format("SELECT * FROM {0} WHERE is_processed=@is_processed AND settlement_id=@settlement_id AND transaction_dt between @start_dt and @end_dt", tableName);
            Transaction transaction = null;
            List<DetailTransaction> detailTransactions = new List<DetailTransaction>();

            if (this.OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@is_processed", 1);
                cmd.Parameters.AddWithValue("@settlement_id", settlement_id);
                cmd.Parameters.AddWithValue("@start_dt", start_dt);
                cmd.Parameters.AddWithValue("@end_dt", end_dt);
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
                tableName = "settlements";
                query = string.Format("SELECT * FROM {0} WHERE id=@id", tableName);
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", settlement_id);
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
