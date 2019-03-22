using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Settlement.Classes.Other;
using Settlement.Classes.Helper;

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

        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        //Close connection
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

        //Insert statement
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

        //Update statement
        public void Update(string query_cmd)
        {
            // for example
            string query = query_cmd;
            //Open connection
            if (this.OpenConnection() == true)
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;

                //Execute query
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        //Delete statement
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

        // fetch deduct data result within period datetime
        public List<string> FetchDeductResult(string start_dt, string end_dt)
        {
            string query = "SELECT * FROM deduct_card_results WHERE is_processed=0 AND transaction_dt BETWEEN '" + start_dt + "' AND '" + end_dt + "'";
            List<string> list = new List<string>();

            //Open connection
            if (this.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    list.Add(dataReader["result"] + "");
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return list;
            }
            else
            {
                return list;
            }
        }

        //Count statement
        public int Count()
        {
            // for example
            string query = "SELECT Count(*) FROM settlements";
            int Count = -1;

            //Open Connection
            if (this.OpenConnection() == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar() + "");

                //close Connection
                this.CloseConnection();

                return Count;
            }
            else
            {
                return Count;
            }
        }

        //Backup
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

                //Save file to C:\ with the current date as a filename
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
            catch (IOException ex)
            {
                Console.WriteLine("Error , unable to backup!");
            }
        }

        //Restore
        public void Restore()
        {
            try
            {
                //Read file from C:\
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
            catch (IOException ex)
            {
                Console.WriteLine("Error , unable to Restore!");
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
            catch (MySqlException ex)
            {
                successful = false;
            }
            return successful;
        }

        public JObject FetchAllTransactionWithinPeriod(string settlement_id, string start_dt, string end_dt, string bank)
        {
            // detail transaction process
            string query = "select * from deduct_card_results where is_processed=1 and settlement_id = '" + settlement_id + "' and transaction_dt between '" + start_dt + "' and '" + end_dt + "'";
            JObject data_transaction = new JObject();
            JArray detail = new JArray();
            if (this.OpenConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    string transaction_dt = dataReader["transaction_dt"].ToString();
                    JObject detail_transaction = new JObject();
                    detail_transaction["amount"] = Convert.ToInt32(dataReader["amount"].ToString());
                    detail_transaction["transaction_dt"] = tk.ConvertDatetimeToDefaultFormatMySQL(transaction_dt);
                    detail_transaction["ipv4"] = dataReader["ipv4"].ToString();
                    detail_transaction["operator"] = dataReader["operator"].ToString();
                    detail_transaction["ID_reader"] = dataReader["ID_reader"].ToString();

                    detail.Add(detail_transaction);
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
                    data_transaction["created_settlement_dt"] = tk.ConvertDatetimeToDefaultFormatMySQL(created);
                    data_transaction["bank"] = bank;
                    data_transaction["details"] = detail;
                }
                dataReader.Close();
                this.CloseConnection();
            }
            return data_transaction;
        }
    }
}
