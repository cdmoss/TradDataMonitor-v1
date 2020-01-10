using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace TRADDataMonitor
{
    public class VintHub : INotifyPropertyChanged
    {
        PhidgetSensor _sensor0, _sensor1, _sensor2, _sensor3, _sensor4, _sensor5;
        ItemsChangeObservableCollection<PhidgetSensor> _allSensors;
        bool _wireless;
        string _hubName;
        string _wirelessString; //, _idString;
        #region sensor properties
        public PhidgetSensor Sensor0
        {
            get { return _sensor0; }
            set
            {
                _sensor0 = value;
                OnPropertyChanged();
            }
        }

        public PhidgetSensor Sensor1
        {
            get { return _sensor1; }
            set
            {
                _sensor1 = value;
                OnPropertyChanged();
            }
        }

        public PhidgetSensor Sensor2
        {
            get { return _sensor2; }
            set
            {
                _sensor2 = value;
                OnPropertyChanged();
            }
        }

        public PhidgetSensor Sensor3
        {
            get { return _sensor3; }
            set
            {
                _sensor3 = value;
                OnPropertyChanged();
            }
        }

        public PhidgetSensor Sensor4
        {
            get { return _sensor4; }
            set
            {
                _sensor4 = value;
                OnPropertyChanged();
            }
        }

        public PhidgetSensor Sensor5
        {
            get { return _sensor5; }
            set
            {
                _sensor5 = value;
                OnPropertyChanged();
            }
        }

        public ItemsChangeObservableCollection<PhidgetSensor> AllSensors
        {
            get { return _allSensors; }
            set
            {
                if (value != _allSensors)
                {
                    _allSensors = value;
                    _allSensors.Refresh();
                }
            }
        }
        #endregion

        public bool Wireless
        {
            get { return _wireless; }
            set
            {
                if (value)
                    WirelessString = "Enabled";
                else
                    WirelessString = "Disabled";

                _wireless = value;  
                OnPropertyChanged();
            }
        }

        public string WirelessString  
        { 
            get { return _wirelessString; }
            set 
            {
                _wirelessString = value;
                OnPropertyChanged();
            }
        }

        public string HubName
        {
            get { return _hubName; }
            set
            {
                _hubName = value;
                OnPropertyChanged();
            }
        }

        public VintHub(PhidgetSensor[] sensors, bool wireless, string hubName)
        {
            try
            {
                AllSensors = new ItemsChangeObservableCollection<PhidgetSensor>();
                if (sensors.Length == 6)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Type hubType = typeof(VintHub);
                        PropertyInfo hubInfo = hubType.GetProperty($"Sensor{i}");
                        hubInfo.SetValue(this, sensors[i]);
                        PhidgetSensor sensor = (PhidgetSensor)hubInfo.GetValue(this, null);
                        AllSensors.Add(sensor);
                    }

                    Wireless = wireless;

                    this.HubName = hubName;
                }
                else
                {
                    throw new ArgumentException("Exactly 6 sensors are required to initiate a VINT Hub.");
                }
            }
            catch
            {
                return;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
