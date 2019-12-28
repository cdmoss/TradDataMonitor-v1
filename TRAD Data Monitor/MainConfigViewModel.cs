using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using System.IO;
using System.Data.SQLite;
using System.Net.Mail;
using System.Net;
using TRADDataMonitor.SensorTypes;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Timers;

namespace TRADDataMonitor
{
    public class MainConfigViewModel : INotifyPropertyChanged
    {
        #region variables

        // data access
        DataAccessor _data;

        // config variables

        // port assignment for current vint hub
        string _hubPort0, _hubPort1, _hubPort2, _hubPort3, _hubPort4, _hubPort5;

        string _selectedHubID, _selectedHubWireless; 
        double _minSoilTemperature, _minAirTemperature, _minHumidity, _minMoisture, _minOxygen, _minVOC;
        double _maxSoilTemperature, _maxAirTemperature, _maxHumidity, _maxMoisture, _maxOxygen, _maxVOC;
        string _recepientEmailAddress, _senderEmailAddress, _senderEmailPassword, _senderEmailSmtpAddress, _senderEmailSmtpPort, _dataCollectionIntervalTime;
        bool _gpsEnabled = false;
        ItemsChangeObservableCollection<VintHub> _unsavedVintHubs;
        VintHub _selectedConfigHub;

        // data collection variables
        MyGpsSensor _gps;
        Timer _dataCollectionTimer;
        ObservableCollection<VintHub> _savedVintHubs;
        VintHub _selectedSessionHub;
        string _dataCollectionStatus = "Disabled: No data is being collected";
        bool _gpsInitialDataStored;

        // for message boxes
        MainWindow _mainWindow;

        // reference picture 
        Bitmap _phidgetImage { get; set; } = new Bitmap("phidget_hub_6_ports.png");

        // sensor list for combo boxes
        string[] _sensorTypes { get; set; } = { "Moisture", "Humidity", "Temperature", "Light", "Oxygen", "None" };
        #endregion

        #region properties
        // probably don't need
        public DataAccessor Data
        {
            get { return _data; }
            set
            {
                _data = value;
            }
        }
        #region config
        #region sensor config

        public string HubPort0
        {
            get { return _hubPort0; }
            set
            {
                _hubPort0 = value;
                SelectedConfigHub.Sensor0 = _data.CreateSensor(value, 0, SelectedConfigHub.Wireless);
                OnPropertyChanged();
            }
        }

        public string HubPort1
        {
            get { return _hubPort1; }
            set
            {
                if (value != _hubPort1)
                {
                    _hubPort1 = value;
                    SelectedConfigHub.Sensor1 = _data.CreateSensor(value, 1, SelectedConfigHub.Wireless);
                    OnPropertyChanged();
                }
            }
        }

        public string HubPort2
        {
            get { return _hubPort2; }
            set
            {
                if (value != _hubPort2)
                {
                    _hubPort2 = value;
                    SelectedConfigHub.Sensor2 = _data.CreateSensor(value, 2, SelectedConfigHub.Wireless);
                    OnPropertyChanged();
                }
            }
        }

        public string HubPort3
        {
            get { return _hubPort3; }
            set
            {
                if (value != _hubPort3)
                {
                    _hubPort3 = value;
                    SelectedConfigHub.Sensor3 = _data.CreateSensor(value, 3, SelectedConfigHub.Wireless);
                    OnPropertyChanged();
                }
            }
        }

        public string HubPort4
        {
            get { return _hubPort4; }
            set
            {
                if (value != _hubPort4)
                {
                    _hubPort4 = value;
                    SelectedConfigHub.Sensor4 = _data.CreateSensor(value, 4, SelectedConfigHub.Wireless);
                    OnPropertyChanged();
                }
            }
        }
        public string HubPort5
        {
            get { return _hubPort5; }
            set
            {
                if (value != _hubPort5)
                {
                    _hubPort5 = value;
                    SelectedConfigHub.Sensor5 = _data.CreateSensor(value, 5, SelectedConfigHub.Wireless);
                    OnPropertyChanged();
                }
            }
        }

        public ItemsChangeObservableCollection<VintHub> UnsavedVintHubs
        {
            get { return _unsavedVintHubs; }
            set 
            { 
                _unsavedVintHubs = value;
                OnPropertyChanged();
            }
        }

        public VintHub SelectedConfigHub
        {
            get { return _selectedConfigHub; }
            set 
            {    
                _selectedConfigHub = value;
                if (_selectedConfigHub != null)
                {
                    HubPort0 = value.Sensor0.SensorType;
                    HubPort1 = value.Sensor1.SensorType;
                    HubPort2 = value.Sensor2.SensorType;
                    HubPort3 = value.Sensor3.SensorType;
                    HubPort4 = value.Sensor4.SensorType;
                    HubPort5 = value.Sensor5.SensorType;
                }
                
                OnPropertyChanged();
            }
        }

        #endregion
        #region general config
        #region threshold values
        public double MinSoilTemperature
        {
            get { return _minSoilTemperature; }
            set
            {
                _minSoilTemperature = value;
                OnPropertyChanged(nameof(MinSoilTemperature));
            }
        }

        public double MaxSoilTemperature
        {
            get { return _maxSoilTemperature; }
            set
            {
                _maxSoilTemperature = value;
                OnPropertyChanged(nameof(MaxSoilTemperature));
            }
        }

        public double MinAirTemperature
        {
            get { return _minAirTemperature; }
            set
            {
                _minAirTemperature = value;
                OnPropertyChanged(nameof(MinAirTemperature));
            }
        }

        public double MaxAirTemperature
        {
            get { return _maxAirTemperature; }
            set
            {
                _maxAirTemperature = value;
                OnPropertyChanged(nameof(MaxAirTemperature));
            }
        }

        public double MaxHumidity
        {
            get { return _maxHumidity; }
            set
            {
                _maxHumidity = value;
                OnPropertyChanged(nameof(MaxHumidity));
            }
        }

        public double MinHumidity
        {
            get { return _minHumidity; }
            set
            {
                _minHumidity = value;
                OnPropertyChanged(nameof(MinHumidity));
            }
        }

        public double MaxOxygen
        {
            get { return _maxOxygen; }
            set
            {
                _maxOxygen = value;
                OnPropertyChanged(nameof(MaxOxygen));
            }
        }

        public double MinOxygen
        {
            get { return _minOxygen; }
            set
            {
                _minOxygen = value;
                OnPropertyChanged(nameof(MinOxygen));
            }
        }

        public double MaxMoisture
        {
            get { return _maxMoisture; }
            set
            {
                _maxMoisture = value;
                OnPropertyChanged(nameof(MaxMoisture));
            }
        }

        public double MinMoisture
        {
            get { return _minMoisture; }
            set
            {
                _minMoisture = value;
                OnPropertyChanged(nameof(MinMoisture));
            }
        }

        public double MaxVOC
        {
            get { return _maxVOC; }
            set
            {
                _maxVOC = value;
                OnPropertyChanged(nameof(MaxVOC));
            }
        }

        public double MinVOC
        {
            get { return _minVOC; }
            set
            {
                _minVOC = value;
                OnPropertyChanged(nameof(MinVOC));
            }
        }

        #endregion
        public string RecipientEmailAddress
        {
            get { return _recepientEmailAddress; }
            set
            {
                if (value != _recepientEmailAddress)
                {
                    _recepientEmailAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SenderEmailAddress
        {
            get { return _senderEmailAddress; }
            set
            {
                if (value != _senderEmailAddress)
                {
                    _senderEmailAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SenderEmailSmtpPort
        {
            get { return _senderEmailSmtpPort; }
            set
            {
                if (int.TryParse(value, out int result))
                {
                    if (value != _senderEmailSmtpPort)
                    {
                        _senderEmailSmtpPort = value;
                        OnPropertyChanged();
                    }
                }
                else
                    MessageBox.Show(_mainWindow, "Only integer values are allowed for this field.", "Invalid Input", MessageBox.MessageBoxButtons.YesNoCancel);
            }
        }

        public string SenderEmailSmtpAddress
        {
            get { return _senderEmailSmtpAddress; }
            set
            {
                if (value != _senderEmailSmtpAddress)
                {
                    _senderEmailSmtpAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SenderEmailPassword
        {
            get { return _senderEmailPassword; }
            set
            {
                if (value != _senderEmailPassword)
                {
                    _senderEmailPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        // is string so it can be validated
        public string DataCollectionIntervalTime
        {
            get { return _dataCollectionIntervalTime; }
            set
            {
                if (int.TryParse(value, out int result))
                {
                    if (value != _senderEmailSmtpPort)
                    {
                        _dataCollectionIntervalTime = value;
                        OnPropertyChanged();
                    }
                }
                else
                    MessageBox.Show(_mainWindow, "Only integer values are allowed for this field.", "Invalid Input", MessageBox.MessageBoxButtons.YesNoCancel);
            }
        }

        public bool GpsEnabled
        {
            get { return _gpsEnabled; }
            set
            {
                _gpsEnabled = value;
                OnPropertyChanged();
            }
        }

        #endregion
        #endregion
        #region current session info

        public ObservableCollection<VintHub> SavedVintHubs
        {
            get { return _savedVintHubs; }
            set 
            {
                _savedVintHubs = value;
                OnPropertyChanged();
            }
        }


        public VintHub SelectedSessionHub
        {
            get { return _selectedSessionHub; }
            set 
            { 
                _selectedSessionHub = value;
                OnPropertyChanged();
            }
        }


        public string DataCollectionStatus
        {
            get { return _dataCollectionStatus; }
            set
            {
                if (value != _dataCollectionStatus)
                {
                    _dataCollectionStatus = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #endregion

        #region methods
        public MainConfigViewModel(MainWindow mw)
        {
            LoadConfiguration();
        }

        // Method to validate the email formatting (code from https://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address)
        public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Method to validate the SMTP address and port number(code from https://stackoverflow.com/questions/955431/how-to-validate-smtp-server)
        public bool IsValidSmtp(string hostAddress, int portNumber)
        {
            bool valid = false;
            try
            {
                TcpClient smtpTest = new TcpClient();
                smtpTest.Connect(hostAddress, portNumber);
                if (smtpTest.Connected)
                {
                    NetworkStream ns = smtpTest.GetStream();
                    StreamReader sr = new StreamReader(ns);
                    if (sr.ReadLine().Contains("220"))
                    {
                        valid = true;
                    }
                    smtpTest.Close();
                }
            }
            catch
            {

            }
            return valid;
        }

        void SaveConfiguration()
        {
            #region input validation
            int smtpPort;
            int dataInterval;

            // Checks if the recepient email is valid before saving
            if (IsValidEmail(RecipientEmailAddress))
            {
                _data.RecipientEmailAddress = RecipientEmailAddress;
            }
            else
            {
                MessageBox.Show(_mainWindow, "The recipient email address is invalid.", "Invalid Input", MessageBox.MessageBoxButtons.YesNoCancel);
                return;
            }

            // Checks if the sender email is valid before saving
            if (IsValidEmail(SenderEmailAddress))
            {
                SenderEmailAddress = SenderEmailAddress;
            }
            else
            {
                MessageBox.Show(_mainWindow, "The sender email address is invalid.", "Invalid Input", MessageBox.MessageBoxButtons.YesNoCancel);
                return;
            }

            // Sets the sender email password
            _data.SenderEmailPassword = SenderEmailPassword;

            // Tries to convert the textbox string to an integer
            try
            {
                smtpPort = Int32.Parse(SenderEmailSmtpPort);

                // Checks if the SMTP address and port are valid
                if (IsValidSmtp(SenderEmailSmtpAddress, smtpPort))
                {
                    _data.SenderEmailSmtpAddress = SenderEmailSmtpAddress;
                    _data.SenderEmailSmtpPort = smtpPort;
                }
                else
                {
                    MessageBox.Show(_mainWindow, "The sender SMTP address and/or port are invalid.", "Invalid Input", MessageBox.MessageBoxButtons.YesNoCancel);
                    return;
                }
            }
            catch (Exception smtpPortError)
            {
                MessageBox.Show(_mainWindow, smtpPortError.Message, "Invalid Input", MessageBox.MessageBoxButtons.YesNoCancel);
                return;
            }

            // Tries to convert the textbox string to an integer
            try
            {
                dataInterval = Int32.Parse(DataCollectionIntervalTime);

                // Sets the data collection timer interval
                _data.DataCollectionIntervalTime = dataInterval;
            }
            catch (Exception dataIntervalError)
            {
                MessageBox.Show(_mainWindow, dataIntervalError.Message, "Invalid Input", MessageBox.MessageBoxButtons.YesNoCancel);
                return;
            }
            #endregion

            // save vint hub configs
            SavedVintHubs = UnsavedVintHubs;
            _data.VintHubs = _savedVintHubs;

            // save general config values
            _data.MinSoilTemperature = _minSoilTemperature;
            _data.MaxSoilTemperature = _maxSoilTemperature;
            _data.MinAirTemperature = _minAirTemperature;
            _data.MaxAirTemperature = _maxAirTemperature;
            _data.MinHumidity = _minHumidity;
            _data.MaxHumidity = _maxHumidity;
            _data.MinMoisture = _minMoisture;
            _data.MaxMoisture = _maxMoisture;
            _data.MinOxygen = _minOxygen;
            _data.MaxOxygen = _maxOxygen;
            _data.MinVOC = _minVOC;
            _data.MaxVOC = _maxVOC;
            _data.RecipientEmailAddress = _recepientEmailAddress;
            _data.SenderEmailAddress = _senderEmailAddress;
            _data.SenderEmailPassword = _senderEmailPassword;
            _data.SenderEmailSmtpAddress = _senderEmailSmtpAddress;
            _data.SenderEmailSmtpPort = Convert.ToInt32(_senderEmailSmtpPort);
            _data.DataCollectionIntervalTime = Convert.ToInt32(_dataCollectionIntervalTime);
            _data.GpsEnabled = _gpsEnabled;

            if (_data.SaveConfiguration() == "good" && _data.LoadConfiguration() == "good")
            {
                //for(int i = 0; i < SavedVintHubs.Count; i++)
                //{
                //    SelectedSessionHub = SavedVintHubs[i];
                //}
                MessageBox.Show(_mainWindow, "Configuration was successfully saved and is now the current configuration.", "Configuration Save Result", MessageBox.MessageBoxButtons.YesNoCancel);
            }  
            else
                MessageBox.Show(_mainWindow, $"Configuration was not saved properly. Please ensure all fields contain valid input and try again", "Configuration Save Result", MessageBox.MessageBoxButtons.YesNoCancel);
        }

        void LoadConfiguration()
        { 
            _data = new DataAccessor();

            if (_data.LoadConfiguration() == "good")
            {
                // load general config values
                RecipientEmailAddress = _data.RecipientEmailAddress;
                SenderEmailAddress = _data.SenderEmailAddress;
                SenderEmailPassword = _data.SenderEmailPassword;
                SenderEmailSmtpAddress = _data.SenderEmailSmtpAddress;
                SenderEmailSmtpPort = _data.SenderEmailSmtpPort.ToString();
                DataCollectionIntervalTime = _data.DataCollectionIntervalTime.ToString();
                GpsEnabled = _data.GpsEnabled;
                MinSoilTemperature = _data.MinSoilTemperature;
                MaxSoilTemperature = _data.MaxSoilTemperature;
                MinAirTemperature = _data.MinAirTemperature;
                MaxAirTemperature = _data.MaxAirTemperature;
                MinHumidity = _data.MinHumidity;
                MaxHumidity = _data.MaxHumidity;
                MinMoisture = _data.MinMoisture;
                MaxMoisture = _data.MaxMoisture;
                MinOxygen = _data.MinOxygen;
                MaxOxygen = _data.MaxOxygen;
                MinVOC = _data.MinVOC;
                MaxVOC = _data.MaxVOC;


                // load saved vint hubs
                SavedVintHubs = _data.VintHubs;

                foreach (var sensor in _data.VintHubs)
                {
                    UnsavedVintHubs.Add(sensor);
                }

                SelectedConfigHub = UnsavedVintHubs[0];
                HubPort0 = SelectedConfigHub.Sensor0.SensorType;
                HubPort1 = SelectedConfigHub.Sensor1.SensorType;
                HubPort2 = SelectedConfigHub.Sensor2.SensorType;
                HubPort3 = SelectedConfigHub.Sensor3.SensorType;
                HubPort4 = SelectedConfigHub.Sensor4.SensorType;
                HubPort5 = SelectedConfigHub.Sensor5.SensorType;
            }
            else
            {
                // make sure database is built
                _data.BuildDataStores();

                // create new hub and add it to unsaved vint hubs
                UnsavedVintHubs = new ItemsChangeObservableCollection<VintHub>();

                VintHub newHub = _data.CreateNewHub();
                UnsavedVintHubs.Add(newHub);
                SelectedConfigHub = newHub;

                // set hubports to default
                HubPort0 = _sensorTypes[6];
                HubPort1 = _sensorTypes[6];
                HubPort2 = _sensorTypes[6];
                HubPort3 = _sensorTypes[6];
                HubPort4 = _sensorTypes[6];
                HubPort5 = _sensorTypes[6];

                

                MessageBox.Show(_mainWindow, "No valid configuration profile was detected. Default values will be displayed in this form", "No Configuration Found", MessageBox.MessageBoxButtons.YesNoCancel);
            }
        }   
        public void CreateNewVintHub()
        {
            VintHub vintHub = _data.CreateNewHub();
            UnsavedVintHubs.Add(vintHub);
        }
        public void RemoveVintHub()
        {
            UnsavedVintHubs.Remove(SelectedConfigHub);
            if (UnsavedVintHubs.Count < 1)
            {
                UnsavedVintHubs.Add(_data.CreateNewHub());
                DataCollectionIntervalTime = 1000.ToString();
            }
            
            SelectedConfigHub = UnsavedVintHubs[0];
        }
        // method that sends an email
        void SendEmailAlert()
        {
            SmtpClient smtp01 = new SmtpClient(SenderEmailSmtpAddress, Convert.ToInt32(SenderEmailSmtpPort));
            NetworkCredential netCred = new NetworkCredential(SenderEmailAddress, SenderEmailPassword);

            System.Net.Mime.ContentType mimeType = new System.Net.Mime.ContentType("text/html");
            smtp01.Credentials = netCred;
            smtp01.EnableSsl = true;
            MailMessage msg = new MailMessage(SenderEmailAddress, RecipientEmailAddress, "TEST Alert", "This is a test alert");
            smtp01.Send(msg);
        }

        

        public void StartDataCollection()
        {
            // Instantiates the data collection timer, sets the timer interval
            _dataCollectionTimer = new Timer();
            _dataCollectionTimer.Interval = Convert.ToInt32(DataCollectionIntervalTime);
            _dataCollectionTimer.Elapsed += Tmr_Elapsed;

            // builds gps sensor if needed
            // TODO: add threshold value
            if(GpsEnabled)
                _gps = new MyGpsSensor(-1, "GPS", -1);

            // open connections for all connected sensors
            foreach (var hub in _savedVintHubs)
            {
                foreach (var sensor in hub.AllSensors)
                {
                    sensor.OpenConnection();
                }
            }

            try
            {
                _dataCollectionTimer.Start();
                DataCollectionStatus = "Enabled: Data is being collected";
                MessageBox.Show(_mainWindow, "Data Collection successfully initiated", "Data Collection Initiation Result", MessageBox.MessageBoxButtons.Ok);
            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainWindow, "An error has occurred: \n" + ex.Message, "Data Collection Initiation Result", MessageBox.MessageBoxButtons.Ok);
            }
        }

        public void StopDataCollection()
        {
            try
            {
                _dataCollectionTimer.Stop();
                DataCollectionStatus = "Disabled: No data is being collected";
                MessageBox.Show(_mainWindow, "Data Collection successfully halted.", "Data Collection Stop Result", MessageBox.MessageBoxButtons.Ok);
            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainWindow, "An error has occurred: \n" + ex.Message, "Data Collection Stop Result", MessageBox.MessageBoxButtons.Ok);
            }
        }

        // Timer tick event
        private void Tmr_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // store gps location first time around
            if (!_gpsInitialDataStored && GpsEnabled)
            {
                string[] tmp = _gps.ProduceData();
                _data.InsertData(tmp[0], tmp[1], tmp[2]);
                _gpsInitialDataStored = true;
            }

            // store data from all sensors
            foreach (VintHub vintHub in _savedVintHubs)
            {
                foreach (PhidgetSensor sensor in vintHub.AllSensors)
                {
                    if (sensor.SensorType != "None")
                    {
                        string[] tmp = sensor.ProduceData();
                        _data.InsertData(tmp[0], tmp[1], tmp[2]);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
