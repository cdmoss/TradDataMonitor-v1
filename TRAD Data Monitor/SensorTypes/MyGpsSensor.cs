using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
// Added
using Phidget22;

namespace TRADDataMonitor.SensorTypes
{
    public class MyGpsSensor
    {
        Timer _GPSAlerts;
        GPS device;
        private double initialLatitude = -1, initialLongitude = -1, lastLatitude = -1, lastLongitude = -1, distanceFromInitialLocation = -1, distanceThreshold = -1;
        private DateTime initialTimeStamp;

        // Delegate for email alert
        public delegate void EmailAlertHandler(double distanceThreshold, string sensor, double lat, double lng, double val);
        public EmailAlertHandler thresholdBroken;

        public MyGpsSensor(int hubPort, string type, double distanceThreshold)
        {
            device = new GPS();
            device.HubPort = hubPort;
            device.IsHubPortDevice = false;
            device.PositionChange += Device_PositionChange;

            // create a VOC alert timer for instance of sensor
            _GPSAlerts = new Timer(100000);
            _GPSAlerts.AutoReset = true;
            _GPSAlerts.Elapsed += _GPSAlerts_Elapsed;

            this.distanceThreshold = distanceThreshold;

            //Open the connection
            try
            {
                device.Open(4000);
            }
            catch (Exception ex)
            {
                throw new Exception($"There was an error with the {type} sensor. Check connections and try again. \n \n System Error Message: \n" + ex.Message);
            }
        }        

        private void Device_PositionChange(object sender, Phidget22.Events.GPSPositionChangeEventArgs e)
        {    
            if(initialLatitude != -1 || initialLongitude != -1)
            {
                initialLatitude = e.Latitude;
                initialLongitude = e.Longitude;
                initialTimeStamp = DateTime.Now;
            }

            lastLatitude = e.Latitude;
            lastLongitude = e.Longitude;

            distanceFromInitialLocation = Math.Sqrt(Math.Pow(initialLatitude - lastLatitude, 2) + Math.Pow(initialLongitude - lastLongitude, 2));

            if (distanceFromInitialLocation > distanceThreshold)
            {
                thresholdBroken?.Invoke(distanceThreshold, "GPS", lastLatitude, lastLongitude, distanceFromInitialLocation);
                _GPSAlerts.Enabled = true;
            }         
        }

        private void _GPSAlerts_Elapsed(object sender, ElapsedEventArgs e)
        {
            thresholdBroken?.Invoke(distanceThreshold, "GPS", lastLatitude, lastLongitude, distanceFromInitialLocation);

            if (!(distanceFromInitialLocation > distanceThreshold))
            {  
                _GPSAlerts.Enabled = false;
            }
        }
        public string[] ProduceData()
        {
            string[] ret = new string[3];
            ret[0] = initialTimeStamp.ToString();
            ret[1] = "GPS Data (Trailer Location)";
            ret[2] = Math.Round(initialLatitude, 6).ToString() + " °, " + Math.Round(initialLongitude, 6).ToString() + " °";
            return ret;
        }
    }
}
