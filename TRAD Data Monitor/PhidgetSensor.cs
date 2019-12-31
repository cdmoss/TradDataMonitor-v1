using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;

namespace TRADDataMonitor
{
    public class PhidgetSensor : INotifyPropertyChanged
    {
        protected Timer _emailTimer = new Timer(5000);
        protected int hubPort = -1;
        protected double minThreshold = -1, maxThreshold = -1, secondMinThreshold = -1, secondMaxThreshold = -1;
        protected bool hubPortDevice = true, thresholdEnabled = false, wirelessEnabled;
        protected string hubName;
        protected string _sensorType, _liveData, _portString;

        public string SensorType
        {
            get { return _sensorType; }
            set 
            { 
                _sensorType = value;
                OnPropertyChanged();
            }
        }

        public string LiveData
        {
            get { return _liveData; }
            set 
            { 
                _liveData = value;
                OnPropertyChanged();
            }
        }

        public string HubPort
        {
            get { return _portString.ToString(); }
            set
            {
                _portString = value;
                OnPropertyChanged();
            }
        }

        // Delegate for email alert
        public delegate void EmailAlertHandler(double minThresh, double maxThresh, string hubName, string sensor, int portID, double val, string alertType);
        public EmailAlertHandler thresholdBroken;

        // Constructor for both a minimum threshold value and a maximimum threshold value
        public PhidgetSensor(int hubPort, string type, string hubName, double minThreshold, double maxThreshold, bool wireless)
        {
            //Assign the channel from phidget port
            this.hubPort = hubPort;
            this._sensorType = type;
            this.minThreshold = minThreshold;
            this.maxThreshold = maxThreshold;
            this.wirelessEnabled = wireless;
            this.hubName = hubName;
            this.HubPort = hubPort.ToString();

            _emailTimer.Elapsed += _emailTimer_Elapsed;
            _emailTimer.Enabled = false;
        }

        public virtual void _emailTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            
        }

        // Constructor for 2 minimum threshold values and 2 maximimum threshold values
        public PhidgetSensor(int hubPort, string type, string hubName, double firstMinThreshold, double firstMaxThreshold, double secondMinThreshold, double secondMaxThreshold, bool wireless)
        {
            //Assign the channel from phidget port
            this.hubPort = hubPort;
            this._sensorType = type;
            this.minThreshold = firstMinThreshold;
            this.maxThreshold = firstMaxThreshold;
            this.secondMinThreshold = secondMinThreshold;
            this.secondMaxThreshold = secondMaxThreshold;
            this.wirelessEnabled = wireless;
            this.HubPort = hubPort.ToString();
        }

        // Constructor for only a single threshold value
        public PhidgetSensor(int hubPort, string type, double singleThreshold)
        {
            //Assign the channel from phidget port
            this.hubPort = hubPort;
            this._sensorType = type;
        }

        public virtual string[] ProduceData()
        {
            return null;
        }

        public virtual void OpenConnection()
        {

        }

        public virtual void CloseConnection()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
