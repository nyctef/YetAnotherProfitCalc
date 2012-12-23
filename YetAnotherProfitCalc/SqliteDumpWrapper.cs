using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Diagnostics;

namespace YetAnotherProfitCalc
{
    /// <summary>
    /// based on http://www.dreamincode.net/forums/topic/157830-using-sqlite-with-c%23/
    /// 
    /// TODO: need to figure out how to handle connections - maybe just one static one
    /// </summary>
    public class SQLiteDumpWrapper
    {
        public string dbConnection;

        public SQLiteDumpWrapper() : this(Path.Combine(Environment.CurrentDirectory, "Resources/esc10-sqlite3-v1.db")) { }

        public SQLiteDumpWrapper(String inputFile)
        {
            dbConnection = String.Format("Data Source={0}", inputFile);
            Console.Error.WriteLine("Looking for data dump at: {"+dbConnection+"}");
		}

        public SQLiteDumpWrapper(Dictionary<String, String> connectionOpts)
        {
            String str = "";
            foreach (KeyValuePair<String, String> row in connectionOpts)
            {
                str += String.Format("{0}={1}; ", row.Key, row.Value);
            }
            str = str.Trim().Substring(0, str.Length - 1);
            dbConnection = str;
        }

        public SQLiteDataReader RunSQLTableQuery(string sql)
        {
            using (SQLiteConnection cnn = new SQLiteConnection(dbConnection))
            {
                cnn.Open();
                return RunSQLTableQuery(sql, cnn);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public SQLiteDataReader RunSQLTableQuery(string sql, SQLiteConnection cnn)
        {
            var dt = new DataTable();

            using (SQLiteCommand mycommand = new SQLiteCommand(cnn))
            {
                mycommand.CommandText = sql;
				//Console.WriteLine("Running SQL:");
				//Console.WriteLine(sql);
                return mycommand.ExecuteReader();
            }
        }

        public string RunSQLStringQuery(string sql)
        {
            using (SQLiteConnection cnn = new SQLiteConnection(dbConnection))
            {
                cnn.Open();
                return RunSQLStringQuery(sql, cnn);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public string RunSQLStringQuery(string sql, SQLiteConnection cnn)
        {
            using (SQLiteCommand mycommand = new SQLiteCommand(cnn))
            {
				//Console.WriteLine("Running SQL:");
				//Console.WriteLine(sql);
                mycommand.CommandText = sql;
                object value = mycommand.ExecuteScalar();
                if (value != null && !(value is DBNull))
                {
                    return value.ToString();
                }
                return "";
            }
        }
    }
}