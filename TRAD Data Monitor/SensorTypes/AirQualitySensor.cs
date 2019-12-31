using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;

namespace TRADDataMonitor.SensorTypes
{
    public class AirQualitySensor : INotifyPropertyChanged
    {
        Timer _AirQualityData, _VOCAlerts, _CO2Alerts;
        int _lastVOC, _lastCO2;
        double _minVOC, _maxVOC, _minCO2, _maxCO2;
        DateTime _lastTimestamp;
        private string _liveData = "";

        // Delegate for email alert
        public delegate void EmailAlertHandler(double minThresh, double maxThresh, string sensor, double val, string alertType);
        public EmailAlertHandler thresholdBroken;

        public string LiveData
        {
            get { return _liveData; }
            set 
            {
                _liveData = value;
                OnPropertyChanged();
            }
        }

        public AirQualitySensor(double minV, double maxV, double minC, double maxC)
        {
            // create data timer for instance of sensor
            _AirQualityData = new Timer(1000);
            _AirQualityData.AutoReset = true;
            _AirQualityData.Elapsed += new ElapsedEventHandler(GetWebData);
            _AirQualityData.Start();

            // create a VOC alert timer for instance of sensor
            _VOCAlerts = new Timer(600000);
            _VOCAlerts.AutoReset = true;
            _VOCAlerts.Elapsed += _VOCAlerts_Elapsed;

            // create a CO2 alert timer for instance of sensor
            _CO2Alerts = new Timer(600000);
            _CO2Alerts.AutoReset = true;
            _CO2Alerts.Elapsed += _CO2Alerts_Elapsed;

            // assign thresholds
            _minVOC = minV;
            _maxVOC = maxV;
            _minCO2 = minC;
            _maxCO2 = maxC;
        }

        // gets data from nodemcu webserver
        void GetWebData(Object source, ElapsedEventArgs e)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument document = web.Load("http://192.168.100.163");
                _lastVOC = Convert.ToInt32(document.DocumentNode.SelectSingleNode("/div/span[1]").InnerText);
                _lastCO2 = Convert.ToInt32(document.DocumentNode.SelectSingleNode("/div/span[2]").InnerText);
                _lastTimestamp = DateTime.Now;

                LiveData = "VOC: " + _lastVOC + " ppb,  CO2: " + _lastCO2 + " ppm";

                // Alerts for VOC
                if ((_lastVOC < _minVOC || _lastVOC < _maxVOC) && !_VOCAlerts.Enabled)
                {
                    thresholdBroken?.Invoke(_minVOC, _maxVOC, "VOC", _lastVOC, "broken");
                    _VOCAlerts.Enabled = true;
                }

                // Alerts for CO2
                if ((_lastCO2 < _minCO2 || _lastCO2 < _maxCO2) && !_CO2Alerts.Enabled)
                {
                    thresholdBroken?.Invoke(_minCO2, _maxCO2, "CO2", _lastCO2, "broken");
                    _CO2Alerts.Enabled = true;
                }
            }
            catch (Exception)
            {
                LiveData = "An error occured with the air quality sensor.";
            }
        }
        private void _VOCAlerts_Elapsed(object sender, ElapsedEventArgs e)
        {
            thresholdBroken?.Invoke(_minVOC, _maxVOC, "VOC", _lastVOC, "broken");

            if (!(_lastVOC < _minVOC || _lastVOC < _maxVOC))
            {
                _VOCAlerts.Enabled = false;
                thresholdBroken?.Invoke(_minVOC, _maxVOC, "VOC", _lastVOC, "fixed");
            }
        }

        private void _CO2Alerts_Elapsed(object sender, ElapsedEventArgs e)
        {
            thresholdBroken?.Invoke(_minCO2, _maxCO2, "CO2", _lastCO2, "broken");

            if (!(_lastCO2 < _minCO2 || _lastCO2 < _maxCO2))
            {
                _CO2Alerts.Enabled = false;
                thresholdBroken?.Invoke(_minCO2, _maxCO2, "VOC", _lastCO2, "fixed");
            }
        }

        public string[] ProduceVOCData()
        {
            string[] ret = new string[3];
            ret[0] = _lastTimestamp.ToString();
            ret[1] = "VOC (ppb)";
            ret[2] = _lastVOC.ToString();
            return ret;
        }

        public string[] ProduceCO2Data()
        {
            string[] ret = new string[3];
            ret[0] = _lastTimestamp.ToString();
            ret[1] = "CO2 (ppm)";
            ret[2] = _lastCO2.ToString();
            return ret;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
