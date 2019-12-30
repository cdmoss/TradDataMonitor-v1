using System;
using System.Collections.Generic;
using System.Text;
// Added
using Phidget22;

namespace TRADDataMonitor.SensorTypes
{
    public class MyGpsSensor : PhidgetSensor
    {
        GPS device;
        private double initialLatitude = -1, initialLongitude = -1, lastLatitude = -1, lastLongitude = -1, distanceFromInitialLocation = -1;
        private DateTime initialTimeStamp;

        public MyGpsSensor(int hubPort, string type, double maxThreshold) : base(hubPort, type, maxThreshold)
        {
            device = new GPS();
            device.HubPort = hubPort;
            device.IsHubPortDevice = false;
            device.PositionChange += Device_PositionChange;

            maxThreshold = this.maxThreshold;

            //Open the connection
            try
            {
                device.Open(4000);
            }
            catch (Exception ex)
            {
                throw new Exception($"There was an error with hub port {hubPort}. Check connections and try again. \n \n System Error Message: \n" + ex.Message);
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

            if (distanceFromInitialLocation > maxThreshold)
            {
                // Send an email alert that the threshold has exceeded the max value
                //thresholdBroken?.Invoke(0, maxThreshold, hubPort);
            }         
        }

        public override String[] ProduceData()
        {
            string[] ret = new string[3];
            ret[0] = initialTimeStamp.ToString();
            ret[1] = "GPS Data (Trailer Location)";
            ret[2] = Math.Round(initialLatitude, 6).ToString() + " °, " + Math.Round(initialLongitude, 6).ToString() + " °";
            return ret;
        }
    }
}
