using System;
using System.Collections.Generic;
using System.Text;
// Added
using Phidget22;

namespace TRADDataMonitor.SensorTypes
{
    public class MyLightSensor : PhidgetSensor
    {
        LightSensor device;
        public DateTime lastTimestamp;
        private double lastIlluminance = -1;

        public MyLightSensor(int hubPort, string type, double minThreshold, double maxThreshold, bool wireless) : base(hubPort, type, minThreshold, maxThreshold, wireless)
        {
            device = new LightSensor();
            device.HubPort = hubPort;
            device.IsHubPortDevice = false;
            device.IlluminanceChange += Device_IlluminanceChange;
        }

        public override void OpenConnection()
        {
            try
            {
                //Open the connection
                if (wirelessEnabled)
                    Net.EnableServerDiscovery(Phidget22.ServerType.DeviceRemote);
                device.Open(4000);
            }
            catch (Exception ex)
            {
                throw new Exception($"There was an error with hub port {hubPort}. Check connections and try again. \n \n System Error Message: \n" + ex.Message);
            }
        }

        private void Device_IlluminanceChange(object sender, Phidget22.Events.LightSensorIlluminanceChangeEventArgs e)
        {
            lastIlluminance = e.Illuminance;
            lastTimestamp = DateTime.Now;
            LiveData = lastIlluminance.ToString() + " lx";

            if (lastIlluminance < minThreshold)
            {
                // Send an email alert that the threshold has exceeded the min value
                thresholdBroken?.Invoke();
            }
            if (lastIlluminance > maxThreshold)
            {
                // Send an email alert that the threshold has exceeded the max value
                thresholdBroken?.Invoke();
            }
        }

        public override String[] ProduceData()
        {
            string[] ret = new string[3];
            ret[0] = lastTimestamp.ToString();
            ret[1] = "Light";
            ret[2] = LiveData;
            return ret;
        }
    }
}
