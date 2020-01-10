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
        Timer _AirQualityData, _VOCAlerts, _CO2Alerts, _VOCAlertCooldown, _CO2AlertCooldown;
        int _lastVOC, _lastCO2;
        double _minVOC, _maxVOC, _minCO2, _maxCO2;
        DateTime _lastTimestamp, lastVOCThresholdBrokenDate, lastCO2ThresholdBrokenDate;
        private string _liveData = "";
        private bool VOCEmailTimerOnCooldown = false, CO2EmailTimerOnCooldown = false;

        // Delegate for email alert
        public delegate void EmailAlertHandler(double minThresh, double maxThresh, string sensor, double val, string alertType);
        public EmailAlertHandler thresholdBroken;

        // Delegate for email reply
        public delegate string EmailCheckReplies(DateTime alertSent, string alertSubject);
        public EmailCheckReplies checkReplies;

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
            
            // assign thresholds
            _minVOC = minV;
            _maxVOC = maxV;
            _minCO2 = minC;
            _maxCO2 = maxC;

            // create a VOC alert timer for instance of sensor
            _VOCAlerts = new Timer(1800000);
            _VOCAlerts.AutoReset = true;
            _VOCAlerts.Elapsed += _VOCAlerts_Elapsed;

            _VOCAlertCooldown = new Timer(3600000);
            _VOCAlertCooldown.AutoReset = true;
            _VOCAlertCooldown.Elapsed += _VOCAlertCooldown_Elapsed;

            // create a CO2 alert timer for instance of sensor
            _CO2Alerts = new Timer(1800000);
            _CO2Alerts.AutoReset = true;
            _CO2Alerts.Elapsed += _CO2Alerts_Elapsed;

            _CO2AlertCooldown = new Timer(3600000);
            _CO2AlertCooldown.AutoReset = true;
            _CO2AlertCooldown.Elapsed += _CO2Alerts_Elapsed;

            // create data timer for instance of sensor
            _AirQualityData = new Timer(1000);
            _AirQualityData.AutoReset = true;
            _AirQualityData.Elapsed += new ElapsedEventHandler(GetWebData);
        }

        public void OpenConnection()
        {
            _AirQualityData.Start();
        }

        public void CloseConnection()
        {
            _AirQualityData.Stop();
        }

        // gets data from nodemcu webserver
        void GetWebData(Object source, ElapsedEventArgs e)
        {
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument document = web.Load("http://10.0.0.71");
                _lastVOC = Convert.ToInt32(document.DocumentNode.SelectSingleNode("/div/span[1]").InnerText);
                _lastCO2 = Convert.ToInt32(document.DocumentNode.SelectSingleNode("/div/span[2]").InnerText);
                _lastTimestamp = DateTime.Now;

                LiveData = "VOC: " + _lastVOC + " ppb,  CO2: " + _lastCO2 + " ppm";

                // Alerts for VOC
                if ((_lastVOC < _minVOC || _lastVOC < _maxVOC) && !_VOCAlerts.Enabled && !VOCEmailTimerOnCooldown)
                {
                    lastVOCThresholdBrokenDate = DateTime.Now;
                    thresholdBroken?.Invoke(_minVOC, _maxVOC, "VOC", _lastVOC, "broken");
                    _VOCAlerts.Enabled = true;
                }

                // Alerts for CO2
                if ((_lastCO2 < _minCO2 || _lastCO2 < _maxCO2) && !_CO2Alerts.Enabled && !CO2EmailTimerOnCooldown)
                {
                    lastCO2ThresholdBrokenDate = DateTime.Now;
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
            string replyMessage = checkReplies?.Invoke(lastVOCThresholdBrokenDate, $"VOC THRESHOLD BROKEN");

            if (replyMessage.Contains("OK") || replyMessage.Contains("Ok") || replyMessage.Contains("ok"))
            {
                _VOCAlertCooldown.Enabled = true;
                VOCEmailTimerOnCooldown = true;
                _VOCAlerts.Enabled = false;

            }
            else
            {
                thresholdBroken?.Invoke(_minVOC, _maxVOC, "VOC", _lastVOC, "broken");
            }

            //if (!(lastVoltage < minThreshold || lastVoltage > maxThreshold))
            //{
            //    _emailTimer.Enabled = false;
            //    thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastVoltage, "fixed");
            //}

        }

        private void _CO2Alerts_Elapsed(object sender, ElapsedEventArgs e)
        {
            string replyMessage = checkReplies?.Invoke(lastCO2ThresholdBrokenDate, $"CO2 THRESHOLD BROKEN");

            if (replyMessage.Contains("OK") || replyMessage.Contains("Ok") || replyMessage.Contains("ok"))
            {
                _CO2AlertCooldown.Enabled = true;
                CO2EmailTimerOnCooldown = true;
                _CO2Alerts.Enabled = false;

            }
            else
            {
                thresholdBroken?.Invoke(_minCO2, _maxCO2, "CO2", _lastCO2, "broken");
            }

            //if (!(lastVoltage < minThreshold || lastVoltage > maxThreshold))
            //{
            //    _emailTimer.Enabled = false;
            //    thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastVoltage, "fixed");
            //}

        }
        public void _VOCAlertCooldown_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // send a reply email

            _VOCAlertCooldown.Enabled = false;
            VOCEmailTimerOnCooldown = false;
        }
        public void _CO2AlertCooldown_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // send a reply email

            _CO2AlertCooldown.Enabled = false;
            CO2EmailTimerOnCooldown = false;
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
