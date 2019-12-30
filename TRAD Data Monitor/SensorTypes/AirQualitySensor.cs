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
        Timer _VOCTimer;
        int _lastVOC, _lastCO2;
        DateTime _lastTimestamp;
        private string _liveData;

        public string LiveData
        {
            get { return _liveData; }
            set 
            {
                _liveData = value;
                OnPropertyChanged();
            }
        }


        public AirQualitySensor(int interval)
        {
            _VOCTimer = new Timer(interval);
            _VOCTimer.AutoReset = true;
            _VOCTimer.Elapsed += new ElapsedEventHandler(GetWebData);
            _VOCTimer.Start();
        }

        // gets data from nodemcu webserver
        void GetWebData(Object source, ElapsedEventArgs e)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = web.Load("http://192.168.100.163");
            _lastVOC = Convert.ToInt32(document.DocumentNode.SelectSingleNode("/div/span[1]").InnerText);
            _lastCO2 = Convert.ToInt32(document.DocumentNode.SelectSingleNode("/div/span[2]").InnerText);
            _lastTimestamp = DateTime.Now;

            LiveData = "VOC: " + _lastVOC + " ppb,  CO2: " + _lastCO2 + " ppm";
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
