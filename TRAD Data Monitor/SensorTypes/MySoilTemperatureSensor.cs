using System;
using System.Collections.Generic;
using System.Text;
// Added
using Phidget22;

namespace TRADDataMonitor.SensorTypes
{
    public class MySoilTemperatureSensor : PhidgetSensor
    {
        TemperatureSensor device;
        DateTime lastTimestamp;
        double lastSoilTemperature;
        public bool insertRecord = false;


        public MySoilTemperatureSensor(int hubPort, string type, string hubName, double minThreshold, double maxThreshold, bool wireless) : base(hubPort, type, hubName, minThreshold, maxThreshold, wireless)
        {
            device = new TemperatureSensor();
            device.HubPort = hubPort;
            device.IsHubPortDevice = false;
            device.TemperatureChange += Device_TemperatureChange; ;
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

        private void Device_TemperatureChange(object sender, Phidget22.Events.TemperatureSensorTemperatureChangeEventArgs e)
        {
            lastSoilTemperature = e.Temperature;
            lastTimestamp = DateTime.Now;
            LiveData = lastSoilTemperature.ToString();

            if (lastSoilTemperature < minThreshold)
            {
                // Send an email alert that the threshold has exceeded the min value
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastSoilTemperature);
            }
            else if (lastSoilTemperature > maxThreshold)
            {
                // Send an email alert that the threshold has exceeded the max value
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastSoilTemperature);
            }
        }

        public override string[] ProduceData()
        {
            string[] ret = new string[3];
            ret[0] = lastTimestamp.ToString();
            ret[1] = "Temperature (°C)";
            ret[2] = LiveData;
            return ret;
        }
    }
}
