using System;
using System.Collections.Generic;
using System.Text;
// Added
using Phidget22;

namespace TRADDataMonitor.SensorTypes
{
    public class MyHumiditySensor : PhidgetSensor
    {
        HumiditySensor humidityDevice;
        TemperatureSensor temperatureDevice;
        private double lastHumidity = -1, lastAirTemperature = -1;
        public DateTime lastTimestamp;

        public MyHumiditySensor(int hubPort, string type, double minThreshold, double maxThreshold, bool wireless) : base(hubPort, type, minThreshold, maxThreshold, wireless)
        {
            humidityDevice = new HumiditySensor();
            humidityDevice.HubPort = hubPort;
            humidityDevice.IsHubPortDevice = false;
            humidityDevice.HumidityChange += Device_HumidityChange;

            temperatureDevice = new TemperatureSensor();
            temperatureDevice.HubPort = hubPort;       
            temperatureDevice.IsHubPortDevice = false;          
            temperatureDevice.TemperatureChange += TemperatureDevice_TemperatureChange;           
        }

       

        public override void OpenConnection()
        {
            try
            {
                //Open the connection
                if (wirelessEnabled)
                    Net.EnableServerDiscovery(Phidget22.ServerType.DeviceRemote);
                humidityDevice.Open(4000);
            }
            catch (Exception ex)
            {
                throw new Exception($"There was an error with hub port {hubPort}. Check connections and try again. \n \n System Error Message: \n" + ex.Message);
            }
        }

        private void Device_HumidityChange(object sender, Phidget22.Events.HumiditySensorHumidityChangeEventArgs e)
        {
            lastHumidity = e.Humidity;
            lastTimestamp = DateTime.Now;
            LiveData = lastHumidity.ToString() + ", " + lastAirTemperature.ToString();

            if (lastHumidity < minThreshold) 
            {
                // Send an email alert that the threshold has exceeded the min value
                thresholdBroken?.Invoke();
            }     
            else if(lastHumidity > maxThreshold)
            {
                // Send an email alert that the threshold has exceeded the max value
                thresholdBroken?.Invoke();
            }
        }
        private void TemperatureDevice_TemperatureChange(object sender, Phidget22.Events.TemperatureSensorTemperatureChangeEventArgs e)
        {
            lastAirTemperature = e.Temperature;
            lastTimestamp = DateTime.Now;
            LiveData = lastHumidity.ToString() + " %, " + lastAirTemperature.ToString() + " °C";

            if (lastAirTemperature < minThreshold)
            {
                // Send an email alert that the threshold has exceeded the min value
                thresholdBroken?.Invoke();
            }
            else if (lastAirTemperature > maxThreshold)
            {
                // Send an email alert that the threshold has exceeded the max value
                thresholdBroken?.Invoke();
            }
        }
        public override String[] ProduceData()
        {
            string[] ret = new string[3];
            ret[0] = lastTimestamp.ToString();
            ret[1] = "Humidity and Air Temperature";
            ret[2] = LiveData;
            return ret;
        }
    }
}
