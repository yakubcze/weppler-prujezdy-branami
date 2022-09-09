using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;

namespace PrujezdyBranami_Service
{
    public partial class PrujezdyBranami_Service : ServiceBase
    {
        Timer timer = new Timer();
        public PrujezdyBranami_Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string filter = "*.jpg"; //filter pro FileSystemWatcher - "*.jpg" = vsechny soubory s priponou .jpg
            string pathC = ConfigurationManager.AppSettings["pathToFolderBranaC"]; //ziskani cest ke slozkam a log souboru z configu
            string pathE = ConfigurationManager.AppSettings["pathToFolderBranaE"];
            string log = ConfigurationManager.AppSettings["pathToFileLog"];

            WriteLog("Sluzba spustena v: " + DateTime.Now);
            WriteLog("Hlidam slozky: \nBrana C: " + pathC + "\nBrana E: " + pathE);
            WriteLog("Cesta k Logu: " + log);

            FileSystemWatcher watcherC = new FileSystemWatcher();
            FileSystemWatcher watcherE = new FileSystemWatcher();

            watcherC.Path = pathC;
            watcherE.Path = pathE;

            watcherC.Filter = filter;
            watcherE.Filter = filter;

            watcherC.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcherE.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcherC.Created += new FileSystemEventHandler(OnChange);
            watcherE.Created += new FileSystemEventHandler(OnChange);

            watcherC.EnableRaisingEvents = true;
            watcherE.EnableRaisingEvents = true;
        }

        protected override void OnStop()
        {
            WriteLog("Sluzba zastavena v: " + DateTime.Now);
        }

        private static void OnChange(object source, FileSystemEventArgs e)
        {
            string[] prujezdFullPath = e.FullPath.Split('\\'); //rozdeleni nazvu fotky vcetne cesty, ulozeni do pole; je potreba kvuli rozliseni brany E a C
            string[] prujezdName = e.Name.Split('_'); //rozdeleni nazvu fotky, ulozeni do pole

            DateTime parsedDate = DateTime.ParseExact(prujezdName[0], "yyyyMMddHHmmssfff", null); //parsovani data z nazvu fotky (...,fff=milisekunda)
            string sqlFormattedDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss.fff"); //prevod data do formatu vhodny pro SQL

            if (e.Name.Contains("BACKGROUND")) //zpracovava fotky pouze BACKGROUND (nikoliv PLATE)
            {
                //brana C
                if (prujezdFullPath.Contains("Brana C"))
                {
                    if (e.Name.Contains("unknown"))
                    {
                        string query = String.Format("INSERT INTO spz_prujezd VALUES ('{0}', '{1}', '{2}')", sqlFormattedDate, "Neznámá RZ", "C");
                        QuerySQL(query);
                        //WriteLog(query); //pro testovaci ucely - pro pouziti odkomentuj a zakomentuj QuerySQL(query)
                    }
                    else
                    {
                        string query = String.Format("INSERT INTO spz_prujezd VALUES ('{0}', '{1}', '{2}')", sqlFormattedDate, prujezdName[1], "C");
                        QuerySQL(query);
                        //WriteLog(query);
                    }
                }
                //brana E
                else if (prujezdFullPath.Contains("Brana E"))
                {
                    if (e.Name.Contains("unknown"))
                    {
                        string query = String.Format("INSERT INTO spz_prujezd VALUES ('{0}', '{1}', '{2}')", sqlFormattedDate, "Neznámá RZ", "E");
                        QuerySQL(query);
                        //WriteLog(query);
                    }
                    else
                    {
                        string query = String.Format("INSERT INTO spz_prujezd VALUES ('{0}', '{1}', '{2}')", sqlFormattedDate, prujezdName[1], "E");
                        QuerySQL(query);
                        //WriteLog(query);
                    }
                }
            }

        }

        public static void WriteLog(string message)
        {
            string pathToLog = ConfigurationManager.AppSettings["pathToFileLog"];

            if (!File.Exists(pathToLog))
            {
                using (StreamWriter sw = File.CreateText(pathToLog))
                {
                    sw.WriteLine(message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(pathToLog))
                {
                    sw.WriteLine(message);
                }
            }
        }

        public static void QuerySQL(string query)
        {
            string connectionString = ConfigurationManager.AppSettings["connectionString"]; //ziskani connectionstringu z configu
            SqlConnection cnn;
            cnn = new SqlConnection(connectionString);

            SqlCommand command = new SqlCommand(query, cnn);
            try
            {
                cnn.Open();
                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                WriteLog(ex.ToString());
            }
            finally
            {
                cnn.Close();
            }
        }
    }
}
