using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
// Added
using Phidget22;

namespace TRADDataMonitor.SensorTypes
{
    public class MyHumidityAirTemperatureSensor : PhidgetSensor
    {
        Timer _temperatureAlerts;
        HumiditySensor humidityDevice;
        TemperatureSensor temperatureDevice;
        private double lastHumidity = -1, lastAirTemperature = -1;
        private double minAirThreshold, maxAirThreshold, minHumThreshold, maxHumThreshold;
        public DateTime lastTimestamp;

        public MyHumidityAirTemperatureSensor(int hubPort, string type, string hubName, double minHumThreshold, double maxHumThreshold, double minAirThreshold, double maxAirThreshold, bool wireless) : base(hubPort, type, hubName, minHumThreshold, maxHumThreshold, minAirThreshold, maxAirThreshold, wireless)
        {
            humidityDevice = new HumiditySensor();
            humidityDevice.HubPort = hubPort;
            humidityDevice.IsHubPortDevice = false;
            humidityDevice.HumidityChange += Device_HumidityChange;

            temperatureDevice = new TemperatureSensor();
            temperatureDevice.HubPort = hubPort;       

            temperatureDevice.IsHubPortDevice = false;          
            temperatureDevice.TemperatureChange += TemperatureDevice_TemperatureChange;

            _temperatureAlerts = new Timer(600000);
            _temperatureAlerts.AutoReset = true;
            _temperatureAlerts.Elapsed += _temperatureAlerts_Elapsed;
        }

        public override void OpenConnection()
        {
            try
            {
                //Open the connection
                humidityDevice.Open(4000);
                temperatureDevice.Open(4000);
            }
            catch (Exception ex)
            {
                throw new Exception($"There was an error with the {_sensorType} sensor connected to port {hubPort} on hub: {hubName}. Check connections and try again. \n \n System Error Message: \n" + ex.Message);
            }
        }

        public override void CloseConnection()
        {
            try
            {
                humidityDevice.Close();
                temperatureDevice.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"There was an error with the {_sensorType} sensor connected to port {hubPort} on hub: {hubName}. Check connections and try again. \n \n System Error Message: \n" + ex.Message);
            }
        }

        private void Device_HumidityChange(object sender, Phidget22.Events.HumiditySensorHumidityChangeEventArgs e)
        {
            lastHumidity = e.Humidity;
            lastTimestamp = DateTime.Now;
            LiveData = lastHumidity.ToString() + " %, " + lastAirTemperature.ToString() + " °C";

            if ((lastHumidity < minThreshold || lastHumidity > maxThreshold) && !_emailTimer.Enabled) 
            {
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, "Humidity", hubPort, lastHumidity, "broken");
                _emailTimer.Enabled = true;
            }     
        }
        private void TemperatureDevice_TemperatureChange(object sender, Phidget22.Events.TemperatureSensorTemperatureChangeEventArgs e)
        {
            lastAirTemperature = e.Temperature;
            lastTimestamp = DateTime.Now;
            LiveData = lastHumidity.ToString() + " %, " + lastAirTemperature.ToString() + " °C";

            if ((lastAirTemperature < secondMinThreshold || lastAirTemperature > secondMaxThreshold) && !_temperatureAlerts.Enabled)
            {
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, "Air Temperature", hubPort, lastAirTemperature, "broken");
                _temperatureAlerts.Enabled = true;
            }
        }

        public override void _emailTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, "Humidity", hubPort, lastHumidity, "broken");

            if (!(lastHumidity < minThreshold || lastHumidity > maxThreshold))
            {
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, "Humidity", hubPort, lastHumidity, "fixed");
                _emailTimer.Enabled = false;
            }
        }

        private void _temperatureAlerts_Elapsed(object sender, ElapsedEventArgs e)
        {
            thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, "Air Temperature", hubPort, lastAirTemperature, "broken");

            if (!(lastAirTemperature < secondMinThreshold || lastAirTemperature > secondMaxThreshold))
            {
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, "Humidity", hubPort, lastHumidity, "fixed");
                _temperatureAlerts.Enabled = false;
            }
        }
        public string[] ProduceHumidityData()
        {
            string[] ret = new string[3];
            ret[0] = lastTimestamp.ToString();
            ret[1] = "Humidity (%)";
            ret[2] = lastHumidity.ToString();
            return ret;
        }

        public string[] ProduceAirTemperatureData()
        {
            string[] ret = new string[3];
            ret[0] = lastTimestamp.ToString();
            ret[1] = "Air Temperature (°C)";
            ret[2] = lastAirTemperature.ToString();
            return ret;
        }
    }
}
