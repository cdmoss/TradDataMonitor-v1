using System;
using System.Collections.Generic;
using System.Text;
// Added
using System.Data.SQLite;
using System.IO;
using System.Net.Sockets;
using System.Data;
using System.ComponentModel;
using System.Net.Mail;
using System.Net;
using TRADDataMonitor.SensorTypes;
using System.Collections.ObjectModel;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading;
using System.Threading.Tasks;

namespace TRADDataMonitor
{
    public class DataAccessor
    {
        // Variables for the sensor connections and wireless connection

        // connection string and object
        private string _tradDBConnectionString = "Data Source=TradDB.sqlite;Version=3";
        private SQLiteConnection _tradDBConn;

        // Variables for sensors and hubs      
        

        // Timer for data collection
        

        // Properties
        public string  RecipientEmailAddress { get; set; }
        public string SenderEmailAddress { get; set; }
        public string SenderEmailPassword { get; set; }
        public string SenderEmailSmtpAddress { get; set; }
        public int SenderEmailSmtpPort { get; set; }
        public int DataCollectionIntervalTime { get; set; }
        public double MinSoilTemperature { get; set; }
        public double MinAirTemperature { get; set; }
        public double MinHumidity { get; set; }
        public double MinMoisture { get; set; }
        public double MinOxygen { get; set; }
        public double MinVOC { get; set; }
        public double MinCO2 { get; set; }
        public double MaxSoilTemperature { get; set; }
        public double MaxAirTemperature { get; set; }
        public double MaxHumidity { get; set; }
        public double MaxMoisture { get; set; }
        public double MaxOxygen { get; set; }
        public double MaxCO2 { get; set; }
        public double MaxVOC { get; set; }
        public bool GpsEnabled { get; set; }
        public ItemsChangeObservableCollection<VintHub> VintHubs { get; set; }

        // Constructor
        public DataAccessor()
        {
            _tradDBConn = new SQLiteConnection(_tradDBConnectionString);
        }

        // Input Validation Methods

        // Method to create a phidget class based on the configuration combo box and assign it to the correct hub port
        public PhidgetSensor CreateSensor(string sensorName, int hubPort, string hubName, bool wireless)
        {
            PhidgetSensor ret = null;
            // Using -1 as a parameter for debug purposes, come back and change it to the minThreshold and maxThreshold variable
            switch (sensorName)
            {
                case "Moisture":
                    ret = new MyMoistureSensor(hubPort, sensorName, hubName, MinMoisture, MaxMoisture, wireless);
                    break;
                case "Light":
                    ret = new MyLightSensor(hubPort, sensorName, hubName, -1, -1, wireless);
                    break;
                case "Humidity/Air Temperature":
                    ret = new MyHumidityAirTemperatureSensor(hubPort, sensorName, hubName, MinHumidity, MaxHumidity, MinAirTemperature, MaxAirTemperature, wireless);
                    break;
                case "Oxygen":
                    ret = new MyOxygenSensor(hubPort, sensorName, hubName, MinOxygen, MaxOxygen, wireless);
                    break;
                case "Soil Temperature":
                    ret = new MySoilTemperatureSensor(hubPort, sensorName, hubName, MinSoilTemperature, MaxSoilTemperature, wireless);
                    break;
                case "None":
                    ret = new PhidgetSensor(hubPort, "None", hubName, 0, 0, true);
                    break;
            }
            return ret;
        }

        // creates a new empty hub
        public VintHub CreateNewHub()
        { 
            PhidgetSensor[] sensors = {
                CreateSensor("None", 0, "New Hub", true),
                CreateSensor("None", 1, "New Hub", true),
                CreateSensor("None", 2, "New Hub", true),
                CreateSensor("None", 3, "New Hub", true),
                CreateSensor("None", 4, "New Hub", true),
                CreateSensor("None", 5, "New Hub", true) };

            VintHub ret = new VintHub(sensors, true, "New Hub");

            return ret;
        }

        // loads configuration from database
        public string LoadConfiguration()
        {
            string result = "";
            string getGeneralConfigQuery = $@"select * from GeneralConfig";
            string getVintConfig = "select * from VintHubConfig";
            string getExtensionConfig = "select * from ExtensionHubConfig";

            if (CheckForConfig())
            {
                try
                {
                    _tradDBConn.Open();

                    // Loads General Configuration Values
                    using (SQLiteCommand cmd = new SQLiteCommand(getGeneralConfigQuery, _tradDBConn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader[0].GetType().ToString() != "System.DBNull")
                                    RecipientEmailAddress = reader.GetString(0);

                                if (reader[1].GetType().ToString() != "System.DBNull")
                                    SenderEmailAddress = reader.GetString(1);

                                if (reader[2].GetType().ToString() != "System.DBNull")
                                    SenderEmailPassword = reader.GetString(2);

                                if (reader[3].GetType().ToString() != "System.DBNull")
                                    SenderEmailSmtpAddress = reader.GetString(3);

                                if (reader[4].GetType().ToString() != "System.DBNull")
                                    SenderEmailSmtpPort = reader.GetInt32(4);

                                if (reader[5].GetType().ToString() != "System.DBNull")
                                    DataCollectionIntervalTime = reader.GetInt32(5);

                                if (reader[6].GetType().ToString() != "System.DBNull")
                                    MinSoilTemperature = reader.GetDouble(6);

                                if (reader[7].GetType().ToString() != "System.DBNull")
                                    MinAirTemperature = reader.GetDouble(7);

                                if (reader[8].GetType().ToString() != "System.DBNull")
                                    MinHumidity = reader.GetDouble(8);

                                if (reader[9].GetType().ToString() != "System.DBNull")
                                    MinMoisture = reader.GetDouble(9);

                                if (reader[10].GetType().ToString() != "System.DBNull")
                                    MinOxygen = reader.GetDouble(10);

                                if (reader[11].GetType().ToString() != "System.DBNull")
                                    MinCO2 = reader.GetDouble(11);

                                if (reader[12].GetType().ToString() != "System.DBNull")
                                    MinVOC = reader.GetDouble(12);

                                if (reader[13].GetType().ToString() != "System.DBNull")
                                    MaxSoilTemperature = reader.GetDouble(13);

                                if (reader[14].GetType().ToString() != "System.DBNull")
                                    MaxAirTemperature = reader.GetDouble(14);

                                if (reader[15].GetType().ToString() != "System.DBNull")
                                    MaxHumidity = reader.GetDouble(15);

                                if (reader[16].GetType().ToString() != "System.DBNull")
                                    MaxMoisture = reader.GetDouble(16);

                                if (reader[17].GetType().ToString() != "System.DBNull")
                                    MaxOxygen = reader.GetDouble(17);

                                if (reader[18].GetType().ToString() != "System.DBNull")
                                    MaxVOC = reader.GetDouble(18);

                                if (reader[19].GetType().ToString() != "System.DBNull")
                                    MaxCO2 = reader.GetDouble(19);

                                if (reader[20].GetType().ToString() != "System.DBNull")
                                    GpsEnabled = reader.GetBoolean(20);
                            }
                        }
                    }
                    VintHubs = new ItemsChangeObservableCollection<VintHub>();

                    // loads all sets of saved VINT hub configs, 
                    using (SQLiteCommand cmd = new SQLiteCommand(getVintConfig, _tradDBConn))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                VintHubs.Add(CreateNewHub());                                
                            }
                            else
                            {
                                while (reader.Read())
                                {
                                    string hubName = reader.GetString(0);

                                    PhidgetSensor[] sensors = new PhidgetSensor[6];
                                    for (int i = 0; i < 6; i++)
                                    {
                                        PhidgetSensor sensor = CreateSensor(reader.GetString(i + 1), i, hubName, reader.GetBoolean(7));
                                        sensors[i] = sensor;
                                    }

                                    VintHub newVint = new VintHub(sensors, reader.GetBoolean(7), hubName);
                                    VintHubs.Add(newVint);
                                }
                            } 
                        }
                    }
                    _tradDBConn.Close();

                   

                    result = "good";
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                    _tradDBConn.Close();
                    throw;
                }
            }   
            return result;
        }

        // saves configuration to database
        public string SaveConfiguration()
        {
            string result = "";

            //query to create new GeneralConfig
            string createGeneralConfigQuery = $@"insert into GeneralConfig
                                                    values(
                                                        @RecipientEmail,
                                                        @SenderEmail, 
                                                        @SenderPassword, 
                                                        @SMTPAddress, 
                                                        @SMTPPort, 
                                                        @DataInterval, 
                                                        @MinSoilTemperature,
                                                        @MinAirTemperature,
                                                        @MinHumidity,
                                                        @MinMoisture,
                                                        @MinOxygen,
                                                        @MinCO2,
                                                        @MinVOC,                                                                                                                
                                                        @MaxSoilTemperature,
                                                        @MaxAirTemperature,
                                                        @MaxHumidity,
                                                        @MaxMoisture,
                                                        @MaxOxygen,
                                                        @MaxVOC,
                                                        @MaxVOC,    
                                                        @Gps
                                                        )";

            //query to update General config
            string updateGeneralConfigQuery = $@"update GeneralConfig
                                                    set 
                                                    RecipientEmail = @RecipientEmail,
                                                    SenderEmail = @SenderEmail,
                                                    SenderPassword = @SenderPassword,
                                                    SMTPAddress = @SMTPAddress,
                                                    SMTPPort = @SMTPPort,
                                                    DataInterval = @DataInterval,
                                                    MinSoilTemperature = @MinSoilTemperature,
                                                    MinAirTemperature = @MinAirTemperature,
                                                    MinHumidity = @MinHumidity,
                                                    MinMoisture = @MinMoisutre,
                                                    MinOxygen = @MinOxygen,
                                                    MinVOC = @MinVOC,
                                                    MinCO2 = @MinCO2,   
                                                    MaxSoilTemperature = @MaxSoilTemperature,
                                                    MaxAirTemperature = @MaxAirTemperature,
                                                    MaxHumidity = @MaxHumidity,
                                                    MaxMoisture = @MaxMoisture,
                                                    MaxOxygen = @MaxOxygen,
                                                    MaxVOC = @MaxVOC,
                                                    MaxCO2 = @MaxCO2,
                                                    Gps = @Gps";

            //query to create new hub config
            string createVintHubConfigQuery = $@"insert into VintHubConfig (HubName, Port0, Port1, Port2, Port3, Port4, Port5, Wireless) 
                                                    values(
                                                        @HubName,
                                                        @Port0,
                                                        @Port1, 
                                                        @Port2, 
                                                        @Port3, 
                                                        @Port4, 
                                                        @Port5, 
                                                        @Wireless);";

            try
            {
                SQLiteCommand generalConfigCmd;

                List<SQLiteCommand> vintConfigCmds = new List<SQLiteCommand>();

                // make sure tables are built
                BuildDataStores();

                //if (!CheckForConfig())
                //{

                 _tradDBConn.Open();

                // clear all hub configs
                SQLiteCommand deleteGeneralCmd = new SQLiteCommand("delete from GeneralConfig", _tradDBConn);
                deleteGeneralCmd.ExecuteNonQuery();

                generalConfigCmd = new SQLiteCommand(createGeneralConfigQuery, _tradDBConn);

                //}
                //else
                //{
                //    _tradDBConn.Open();

                //    generalConfigCmd = new SQLiteCommand(updateGeneralConfigQuery, _tradDBConn);
                //}

                using (generalConfigCmd)
                {
                    generalConfigCmd.Parameters.AddWithValue("@RecipientEmail", RecipientEmailAddress);
                    generalConfigCmd.Parameters.AddWithValue("@SenderEmail", SenderEmailAddress);
                    generalConfigCmd.Parameters.AddWithValue("@SenderPassword", SenderEmailPassword);
                    generalConfigCmd.Parameters.AddWithValue("@SMTPAddress", SenderEmailSmtpAddress);
                    generalConfigCmd.Parameters.AddWithValue("@SMTPPort", SenderEmailSmtpPort);
                    generalConfigCmd.Parameters.AddWithValue("@DataInterval", DataCollectionIntervalTime);
                    generalConfigCmd.Parameters.AddWithValue("@MinSoilTemperature", MinSoilTemperature);
                    generalConfigCmd.Parameters.AddWithValue("@MinAirTemperature", MinAirTemperature);
                    generalConfigCmd.Parameters.AddWithValue("@MinHumidity", MinHumidity);
                    generalConfigCmd.Parameters.AddWithValue("@MinMoisture", MinMoisture);
                    generalConfigCmd.Parameters.AddWithValue("@MinOxygen", MinOxygen);
                    generalConfigCmd.Parameters.AddWithValue("@MinVOC", MinVOC);
                    generalConfigCmd.Parameters.AddWithValue("@MinCO2", MinCO2);
                    generalConfigCmd.Parameters.AddWithValue("@MaxSoilTemperature", MaxSoilTemperature);
                    generalConfigCmd.Parameters.AddWithValue("@MaxAirTemperature", MaxAirTemperature);
                    generalConfigCmd.Parameters.AddWithValue("@MaxHumidity", MaxHumidity);
                    generalConfigCmd.Parameters.AddWithValue("@MaxMoisture", MaxMoisture);
                    generalConfigCmd.Parameters.AddWithValue("@MaxOxygen", MaxOxygen);
                    generalConfigCmd.Parameters.AddWithValue("@MaxVOC", MaxVOC);
                    generalConfigCmd.Parameters.AddWithValue("@MaxCO2", MaxCO2);
                    generalConfigCmd.Parameters.AddWithValue("@Gps", GpsEnabled);
                    
                    generalConfigCmd.ExecuteNonQuery();
                }

                // clear all hub configs
                SQLiteCommand deleteHubCmd = new SQLiteCommand("delete from VintHubConfig", _tradDBConn);
                deleteHubCmd.ExecuteNonQuery();

                // create a command for each hub detected
                foreach (VintHub hub in VintHubs)
                {
                    SQLiteCommand cmd = new SQLiteCommand(createVintHubConfigQuery, _tradDBConn);
                    cmd.Parameters.AddWithValue("@HubName", hub.HubName);
                    cmd.Parameters.AddWithValue("@Port0", hub.Sensor0.SensorType);
                    cmd.Parameters.AddWithValue("@Port1", hub.Sensor1.SensorType);
                    cmd.Parameters.AddWithValue("@Port2", hub.Sensor2.SensorType);
                    cmd.Parameters.AddWithValue("@Port3", hub.Sensor3.SensorType);
                    cmd.Parameters.AddWithValue("@Port4", hub.Sensor4.SensorType);
                    cmd.Parameters.AddWithValue("@Port5", hub.Sensor5.SensorType);
                    cmd.Parameters.AddWithValue("@Wireless", hub.Wireless);
                    cmd.ExecuteNonQuery();

                    //vintConfigCmds.Add(new SQLiteCommand(createVintHubConfigQuery, _tradDBConn));
                }

                //execute all hub commmands
                //foreach (SQLiteCommand cmd in vintConfigCmds)
                //{
                //    cmd.ExecuteNonQuery();
                //}

                _tradDBConn.Close();

                result = "good";
            }
            catch (Exception ex)
            {
                _tradDBConn.Close();
                result = ex.Message;
                throw;
            }
            return result;
        }

        public bool CheckForConfig()
        {
            bool configExists = false;
            DataTable test = new DataTable();

            //query to check if config exists
            string checkQuery = $@"select * from GeneralConfig";

            if (File.Exists("TradDB.sqlite"))
            {
                _tradDBConn.Open();
                using (SQLiteDataAdapter adp = new SQLiteDataAdapter(checkQuery, _tradDBConn))
                {
                    adp.Fill(test);
                    if (test.Rows.Count > 0)
                    {
                        configExists = true;
                    }
                }
                _tradDBConn.Close();
            }

            return configExists;
        }

        public bool BuildDataStores()
        {
            bool success;

            string createTables = $@"create table if not exists SensorData(
                                            SensorType text,
                                            Data real,
                                            DateTime text);

                                     create table if not exists VintHubConfig(
                                            HubName text primary key,
                                            Port0 text not null,
                                            Port1 text not null,
                                            Port2 text not null,
                                            Port3 text not null,
                                            Port4 text not null,
                                            Port5 text not null,
                                            Wireless bool not null
                                            );

                                    create table if not exists ExtensionHubConfig(
                                            Port0 text not null,
                                            Port1 text not null,
                                            Port2 text not null,
                                            Port3 text not null,
                                            Port4 text not null,
                                            Port5 text not null,
                                            Port6 text not null,
                                            Port7 text not null
                                            );

                                     create table if not exists GeneralConfig(
                                            RecipientEmail text not null,
                                            SenderEmail text not null,
                                            SenderPassword text not null,
                                            SMTPAddress text not null,
                                            SMTPPort int not null,
                                            DataInterval int not null,
                                            MinSoilTemperature real not null,
                                            MinAirTemperature real not null,
                                            MinHumidity real not null,
                                            MinMoisture real not null,
                                            MinOxygen real not null,
                                            MinCO2 real not null,
                                            MinVOC real not null,
                                            MaxSoilTemperature real not null,
                                            MaxAirTemperature real not null,
                                            MaxHumidity real not null,
                                            MaxMoisture real not null,
                                            MaxOxygen real not null,
                                            MaxVOC real not null,
                                            MaxCO2 real not null,
                                            Gps bool not null);";

            try
            {
                _tradDBConn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(createTables, _tradDBConn))
                {
                    cmd.ExecuteNonQuery();
                }
                _tradDBConn.Close();

                success = true;
            }
            catch (Exception ex)
            {
                _tradDBConn.Close();
                throw ex;
            }

            return success;
        }

        // inserts the sensor data into an SQLite DB
        public void InsertData(string collectionTime, string sensorType, string value)
        {
            try
            {
                string query = $@"insert into SensorData (SensorType, Data, DateTime)
                                values(
                                    '{sensorType}',
                                    '{value}',
                                    '{collectionTime}')";

                _tradDBConn.Open();

                using (SQLiteCommand cmd = new SQLiteCommand(query, _tradDBConn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                _tradDBConn.Close();
            }

        }
    }
}
