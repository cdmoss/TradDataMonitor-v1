using Avalonia.Media.Imaging;
using Phidget22;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using TRADDataMonitor.SensorTypes;

namespace TRADDataMonitor
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region variables

        // data access
        DataAccessor _data;

        // email handler
        EmailHandler _email;

        // config variables

        // port assignment for current vint hub
        string _hubPort0, _hubPort1, _hubPort2, _hubPort3, _hubPort4, _hubPort5;

        double _minSoilTemperature, _minAirTemperature, _minHumidity, _minMoisture, _minOxygen, _minVOC, _minCO2;
        double _maxSoilTemperature, _maxAirTemperature, _maxHumidity, _maxMoisture, _maxOxygen, _maxVOC, _maxCO2, _maxGpsDistance;
        string _recipientEmailAddress, _senderEmailAddress, _senderEmailPassword, _senderEmailSmtpAddress, _senderEmailSmtpPort, _dataCollectionIntervalTime;
        bool _gpsEnabled = false;
        ItemsChangeObservableCollection<VintHub> _unsavedVintHubs;
        VintHub _selectedConfigHub;

        // data collection variables
        MyGpsSensor _gps;
        AirQualitySensor _aqs;
        Timer _dataCollectionTimer;
        ItemsChangeObservableCollection<VintHub> _savedVintHubs;
        VintHub _selectedSessionHub;
        string _dataCollectionStatus = "Disabled: No data is being collected";
        bool _gpsInitialDataStored, _isCollectingData = false;

        // for message boxes
        MainWindow _mainWindow;

        // reference picture 
        Bitmap _phidgetImage { get; set; } = new Bitmap("phidget_hub_6_ports.png");

        // sensor list for combo boxes
        string[] _sensorTypes { get; set; } = { "Moisture", "Humidity/Air Temperature", "Soil Temperature", "Light", "Oxygen", "None" };
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
                SelectedConfigHub.Sensor0 = _data.CreateSensor(value, 0, SelectedConfigHub.HubName, SelectedConfigHub.Wireless);
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
                    SelectedConfigHub.Sensor1 = _data.CreateSensor(value, 1, SelectedConfigHub.HubName, SelectedConfigHub.Wireless);
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
                    SelectedConfigHub.Sensor2 = _data.CreateSensor(value, 2, SelectedConfigHub.HubName, SelectedConfigHub.Wireless);
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
                    SelectedConfigHub.Sensor3 = _data.CreateSensor(value, 3, SelectedConfigHub.HubName, SelectedConfigHub.Wireless);
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
                    SelectedConfigHub.Sensor4 = _data.CreateSensor(value, 4, SelectedConfigHub.HubName, SelectedConfigHub.Wireless);
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
                    SelectedConfigHub.Sensor5 = _data.CreateSensor(value, 5, SelectedConfigHub.HubName, SelectedConfigHub.Wireless);
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
                UnsavedVintHubs.Refresh();
                OnPropertyChanged();
            }
        }

        public VintHub SelectedConfigHub
        {
            get { return _selectedConfigHub; }
            set 
            {    
                // prevent a weird error where this gets set to null when trying to load config after saving
                if (value != null)
                    _selectedConfigHub = value;

                if (_selectedConfigHub != null)
                {
                    HubPort0 = _selectedConfigHub.Sensor0.SensorType;
                    HubPort1 = _selectedConfigHub.Sensor1.SensorType;
                    HubPort2 = _selectedConfigHub.Sensor2.SensorType;
                    HubPort3 = _selectedConfigHub.Sensor3.SensorType;
                    HubPort4 = _selectedConfigHub.Sensor4.SensorType;
                    HubPort5 = _selectedConfigHub.Sensor5.SensorType;
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

        public double MaxCO2
        {
            get { return _maxVOC; }
            set
            {
                _maxCO2 = value;
                OnPropertyChanged();
            }
        }

        public double MinCO2
        {
            get { return _minCO2; }
            set
            {
                _minCO2 = value;
                OnPropertyChanged();
            }
        }

        #endregion
        public string RecipientEmailAddress
        {
            get { return _recipientEmailAddress; }
            set
            {
                if (value != _recipientEmailAddress)
                {
                    _recipientEmailAddress = value;
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
                    MessageBox.Show(_mainWindow, "Only integer values are allowed for this field.", "Invalid Input", MessageBox.MessageBoxButtons.Ok);
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
                    MessageBox.Show(_mainWindow, "Only integer values are allowed for this field.", "Invalid Input", MessageBox.MessageBoxButtons.Ok);
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

        public double MaxGpsDistance
        {
            get { return _maxGpsDistance; }
            set
            {
                _maxGpsDistance = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #endregion
        #region current session info

        public ItemsChangeObservableCollection<VintHub> SavedVintHubs
        {
            get { return _savedVintHubs; }
            set 
            {
                _savedVintHubs = value;
                _savedVintHubs.Refresh();
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

        public AirQualitySensor AQS
        {
            get { return _aqs; }
            set
            {
                if (value != _aqs)
                {
                    _aqs = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #endregion

        #region methods
        public MainWindowViewModel()
        {
            LoadConfiguration();
        }

        void SaveConfiguration()
        {
            if (!_isCollectingData)
            {
                #region input validation
                int smtpPort;
                int dataInterval;

                // Checks if the recepient email is valid before saving
                if (_email.IsValidEmail(RecipientEmailAddress))
                {
                    _data.RecipientEmailAddress = RecipientEmailAddress;
                }
                else
                {
                    MessageBox.Show(_mainWindow, "The recipient email address is invalid.", "Invalid Input", MessageBox.MessageBoxButtons.Ok);
                    return;
                }

                // Checks if the sender email is valid before saving
                if (_email.IsValidEmail(SenderEmailAddress))
                {
                    SenderEmailAddress = SenderEmailAddress;
                }
                else
                {
                    MessageBox.Show(_mainWindow, "The sender email address is invalid.", "Invalid Input", MessageBox.MessageBoxButtons.Ok);
                    return;
                }

                // Sets the sender email password
                _data.SenderEmailPassword = SenderEmailPassword;

                // Tries to convert the textbox string to an integer
                try
                {
                    smtpPort = Int32.Parse(SenderEmailSmtpPort);

                    // Checks if the SMTP address and port are valid
                    if (_email.IsValidSmtp(SenderEmailSmtpAddress, smtpPort))
                    {
                        _data.SenderEmailSmtpAddress = SenderEmailSmtpAddress;
                        _data.SenderEmailSmtpPort = smtpPort;
                    }
                    else
                    {
                        MessageBox.Show(_mainWindow, "The sender SMTP address and/or port are invalid.", "Invalid Input", MessageBox.MessageBoxButtons.Ok);
                        return;
                    }
                }
                catch (Exception smtpPortError)
                {
                    MessageBox.Show(_mainWindow, smtpPortError.Message, "Invalid Input", MessageBox.MessageBoxButtons.Ok);
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
                    MessageBox.Show(_mainWindow, dataIntervalError.Message, "Invalid Input", MessageBox.MessageBoxButtons.Ok);
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
                _data.RecipientEmailAddress = _recipientEmailAddress;
                _data.SenderEmailAddress = _senderEmailAddress;
                _data.SenderEmailPassword = _senderEmailPassword;
                _data.SenderEmailSmtpAddress = _senderEmailSmtpAddress;
                _data.SenderEmailSmtpPort = Convert.ToInt32(_senderEmailSmtpPort);
                _data.DataCollectionIntervalTime = Convert.ToInt32(_dataCollectionIntervalTime);
                _data.GpsEnabled = _gpsEnabled;

                _email = new EmailHandler(_recipientEmailAddress, _senderEmailAddress, _senderEmailPassword, _senderEmailSmtpAddress, _senderEmailSmtpPort);

                if (_data.SaveConfiguration() == "good" && _data.LoadConfiguration() == "good")
                {
                    LoadConfiguration();

                    MessageBox.Show(_mainWindow, "Configuration was successfully saved and is now the current configuration.", "Configuration Save Result", MessageBox.MessageBoxButtons.Ok);
                }
                else
                    MessageBox.Show(_mainWindow, $"Configuration was not saved properly. Please ensure all fields contain valid input and try again", "Configuration Save Result", MessageBox.MessageBoxButtons.Ok);
            }
            else
            {
                MessageBox.Show(_mainWindow, $"Cannot save a configuration during a data collection session. Turn off data collection and retry.", "Configuration Save Result", MessageBox.MessageBoxButtons.Ok);
            }
        }

        void LoadConfiguration()
        { 
            _data = new DataAccessor();
            _email = new EmailHandler(_recipientEmailAddress, _senderEmailAddress, _senderEmailPassword, _senderEmailSmtpAddress, _senderEmailSmtpPort);

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
                MinCO2 = _data.MinCO2;
                MaxCO2 = _data.MaxCO2;

                // load saved vint hubs
                SavedVintHubs = _data.VintHubs;

                UnsavedVintHubs = new ItemsChangeObservableCollection<VintHub>();

                foreach (VintHub hub in SavedVintHubs)
                {
                    UnsavedVintHubs.Add(hub);
                }

                SelectedConfigHub = _unsavedVintHubs[0];
                HubPort0 = SelectedConfigHub.Sensor0.SensorType;
                HubPort1 = SelectedConfigHub.Sensor1.SensorType;
                HubPort2 = SelectedConfigHub.Sensor2.SensorType;
                HubPort3 = SelectedConfigHub.Sensor3.SensorType;
                HubPort4 = SelectedConfigHub.Sensor4.SensorType;
                HubPort5 = SelectedConfigHub.Sensor5.SensorType;

                // Instantiates the data collection timer, sets the timer interval
                _dataCollectionTimer = new Timer();
                _dataCollectionTimer.Interval = Convert.ToInt32(DataCollectionIntervalTime);
                _dataCollectionTimer.Elapsed += Tmr_Elapsed;

                // Build GPS and VOC Sensors
                if (GpsEnabled)
                {
                    _gps = new MyGpsSensor(-1, "GPS", 5000);
                    _gps.thresholdBroken += _email.SendEmailAlert;
                    _gps.checkReplies += _email.RetrieveEmailReply;
                }

                AQS = new AirQualitySensor(_minVOC, _maxVOC, _minCO2, _minCO2);
                AQS.thresholdBroken += _email.SendEmailAlert;
                AQS.checkReplies += _email.RetrieveEmailReply;
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
                HubPort0 = _sensorTypes[5];
                HubPort1 = _sensorTypes[5];
                HubPort2 = _sensorTypes[5];
                HubPort3 = _sensorTypes[5];
                HubPort4 = _sensorTypes[5];
                HubPort5 = _sensorTypes[5];

                MessageBox.Show(_mainWindow, "No valid configuration profile was detected. Default values will be displayed in this form", "No Configuration Found", MessageBox.MessageBoxButtons.Ok);
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
            SelectedConfigHub = _unsavedVintHubs[0];
        }
        
        public void SendTestEmailAlert()
        {
            _email.SendEmailAlert(-1, -1, "testHub", "testSensor", -1, -1, "test");
        }

        public void StartDataCollection()
        {
            if (!_isCollectingData)
            {
                try
                {
                    // open connections for all connected sensors

                    if (_gps != null)
                    {
                    }

                    AQS.OpenConnection();

                    foreach (var hub in _savedVintHubs)
                    {
                        if (hub.Wireless)
                            Net.EnableServerDiscovery(Phidget22.ServerType.DeviceRemote);
                        foreach (var sensor in hub.AllSensors)
                        {
                            sensor.OpenConnection();
                            sensor.thresholdBroken += _email.SendEmailAlert;
                            sensor.checkReplies += _email.RetrieveEmailReply;
                        }
                    }

                    SelectedSessionHub = _savedVintHubs[0];


                    _dataCollectionTimer.Start();
                    DataCollectionStatus = "Enabled: Data is being collected";
                    _isCollectingData = true;
                    MessageBox.Show(_mainWindow, "Data Collection successfully initiated", "Data Collection Initiation Result", MessageBox.MessageBoxButtons.Ok);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(_mainWindow, "An error has occurred: \n" + ex.Message, "Data Collection Initiation Result", MessageBox.MessageBoxButtons.Ok);
                }
            }
            else
            {
                MessageBox.Show(_mainWindow, "There is an already active data collection session", "Data Collection Initiation Result", MessageBox.MessageBoxButtons.Ok);
            }
        }

        public void StopDataCollection()
        {
            if (_isCollectingData)
            {
                try
                {
                    // close connections for all connected sensors
                    _aqs.CloseConnection();
                    foreach (var hub in _savedVintHubs)
                    {
                        if (hub.Wireless)
                            Net.DisableServerDiscovery(Phidget22.ServerType.DeviceRemote);
                        foreach (var sensor in hub.AllSensors)
                        {
                            sensor.CloseConnection();
                        }
                    }

                    _dataCollectionTimer.Stop();
                    SelectedSessionHub = null;
                    DataCollectionStatus = "Disabled: No data is being collected";
                    _isCollectingData = false;
                    MessageBox.Show(_mainWindow, "Data Collection successfully halted.", "Data Collection Stop Result", MessageBox.MessageBoxButtons.Ok);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(_mainWindow, "An error has occurred: \n" + ex.Message, "Data Collection Stop Result", MessageBox.MessageBoxButtons.Ok);
                }
            }
            else
            {
                MessageBox.Show(_mainWindow, "No ongoing data collection to stop.", "Data Collection Stop Result", MessageBox.MessageBoxButtons.Ok);
            }
        }

        public void ViewData()
        {
            DataWindow dw = new DataWindow();
            dw.Show();
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

            // store data from VOC
            string[] voc = _aqs.ProduceVOCData();
            string[] co2 = _aqs.ProduceCO2Data();
            _data.InsertData(voc[0], voc[1], voc[2]);
            _data.InsertData(co2[0], co2[1], co2[2]);

            // store data from all sensors
            foreach (VintHub vintHub in _savedVintHubs)
            {
                foreach (PhidgetSensor sensor in vintHub.AllSensors)
                {
                    if (sensor.SensorType != "None")
                    {
                        if (sensor.SensorType == "Humidity/Air Temperature")
                        {
                            MyHumidityAirTemperatureSensor humiditySensor = (MyHumidityAirTemperatureSensor)sensor;
                            string[] humidity = humiditySensor.ProduceHumidityData();
                            string[] temp = humiditySensor.ProduceAirTemperatureData();

                            _data.InsertData(humidity[0], humidity[1], humidity[2]);
                            _data.InsertData(temp[0], temp[1], temp[2]);
                        }
                        else
                        {
                            string[] tmp = sensor.ProduceData();
                            _data.InsertData(tmp[0], tmp[1], tmp[2]);
                        }
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
