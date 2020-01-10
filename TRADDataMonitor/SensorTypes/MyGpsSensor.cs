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
        Timer _GPSAlerts, _GPSAlertCooldown;
        GPS device;
        private double initialLatitude = -1, initialLongitude = -1, lastLatitude = -1, lastLongitude = -1, distanceFromInitialLocation = -1, distanceThreshold = -1;
        private DateTime initialTimeStamp, lastThresholdBrokenDate;
        private bool GPSAlertOnCooldown = false;

        // Delegate for email alert
        public delegate void EmailAlertHandler(double distanceThreshold, string sensor, double lat, double lng, double val);
        public EmailAlertHandler thresholdBroken;

        // Delegate for email reply
        public delegate string EmailCheckReplies(DateTime alertSent, string alertSubject);
        public EmailCheckReplies checkReplies;

        public MyGpsSensor(int hubPort, string type, double distanceThreshold)
        {
            device = new GPS();
            device.HubPort = hubPort;
            device.IsHubPortDevice = false;
            device.PositionChange += Device_PositionChange;

            // create a GPS alert timer for instance of sensor
            _GPSAlerts = new Timer(1800000);
            _GPSAlerts.AutoReset = true;
            _GPSAlerts.Elapsed += _GPSAlerts_Elapsed;

            _GPSAlertCooldown = new Timer(3600000);
            _GPSAlertCooldown.AutoReset = true;
            _GPSAlertCooldown.Elapsed += _GPSAlerts_Elapsed;

            this.distanceThreshold = (distanceThreshold / Math.PI / 6378 * 360);

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

            if (distanceFromInitialLocation > distanceThreshold && !_GPSAlerts.Enabled && !GPSAlertOnCooldown)
            {
                thresholdBroken?.Invoke(distanceThreshold, "GPS", lastLatitude, lastLongitude, distanceFromInitialLocation);
                _GPSAlerts.Enabled = true;
            }         
        }

        private void _GPSAlerts_Elapsed(object sender, ElapsedEventArgs e)
        {
            string replyMessage = checkReplies?.Invoke(lastThresholdBrokenDate, $"GPS THRESHOLD BROKEN");

            if (replyMessage.Contains("OK") || replyMessage.Contains("Ok") || replyMessage.Contains("ok"))
            {
                _GPSAlertCooldown.Enabled = true;
                GPSAlertOnCooldown = true;
                _GPSAlerts.Enabled = false;

            }
            else
            {
                thresholdBroken?.Invoke(distanceThreshold, "GPS", lastLatitude, lastLongitude, distanceFromInitialLocation);
            }

            //if (!(lastVoltage < minThreshold || lastVoltage > maxThreshold))
            //{
            //    _emailTimer.Enabled = false;
            //    thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastVoltage, "fixed");
            //}
   
        }
        public void _GPSAlertCooldown_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // send a reply email

            _GPSAlertCooldown.Enabled = false;
            GPSAlertOnCooldown = false;
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
