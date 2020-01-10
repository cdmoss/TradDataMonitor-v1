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
        Timer _temperatureEmailTimer, _temperatureAlertCooldown;
        HumiditySensor humidityDevice;
        TemperatureSensor temperatureDevice;
        private double lastHumidity = -1, lastAirTemperature = -1;
        // private double minAirThreshold, maxAirThreshold, minHumThreshold, maxHumThreshold;
        public DateTime lastTimestamp, lastHumidityThresholdBrokenDate, lastTemperatureThresholdBrokenDate;
        private bool humidityEmailTimerOnCooldown = false, temperatureEmailTimerOnCooldown = false;

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

            _temperatureEmailTimer = new Timer(180000);
            _temperatureEmailTimer.AutoReset = true;
            _temperatureEmailTimer.Elapsed += _temperatureAlerts_Elapsed;

            _temperatureAlertCooldown = new Timer(3600000);
            _temperatureAlertCooldown.AutoReset = true;
            _temperatureAlertCooldown.Elapsed += _temperatureAlertCooldown_Elapsed;
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

            if ((lastHumidity < minThreshold || lastHumidity > maxThreshold) && !_emailTimer.Enabled && !humidityEmailTimerOnCooldown) 
            {
                lastHumidityThresholdBrokenDate = DateTime.Now;
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, "Humidity", hubPort, lastHumidity, "broken");
                _emailTimer.Enabled = true;
            }     
        }
        private void TemperatureDevice_TemperatureChange(object sender, Phidget22.Events.TemperatureSensorTemperatureChangeEventArgs e)
        {
            lastAirTemperature = e.Temperature;
            lastTimestamp = DateTime.Now;
            LiveData = lastHumidity.ToString() + " %, " + lastAirTemperature.ToString() + " °C";

            if ((lastAirTemperature < secondMinThreshold || lastAirTemperature > secondMaxThreshold) && !_temperatureEmailTimer.Enabled && !temperatureEmailTimerOnCooldown)
            {
                lastTemperatureThresholdBrokenDate = DateTime.Now;
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, "Air Temperature", hubPort, lastAirTemperature, "broken");
                _temperatureEmailTimer.Enabled = true;
            }
        }

        public override void _emailTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            string replyMessage = checkReplies?.Invoke(lastHumidityThresholdBrokenDate, $"{SensorType} THRESHOLD BROKEN");

            if (replyMessage.Contains("OK") || replyMessage.Contains("Ok") || replyMessage.Contains("ok"))
            {
                _emailAlertCooldown.Enabled = true;
                humidityEmailTimerOnCooldown = true;
                _emailTimer.Enabled = false;

            }
            else
            {
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastHumidity, "broken");
            }

            //if (!(lastVoltage < minThreshold || lastVoltage > maxThreshold))
            //{
            //    _emailTimer.Enabled = false;
            //    thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastVoltage, "fixed");
            //}
        }

        private void _temperatureAlerts_Elapsed(object sender, ElapsedEventArgs e)
        {
            string replyMessage = checkReplies?.Invoke(lastTemperatureThresholdBrokenDate, $"{SensorType} THRESHOLD BROKEN");

            if (replyMessage.Contains("OK") || replyMessage.Contains("Ok") || replyMessage.Contains("ok"))
            {
                _temperatureAlertCooldown.Enabled = true;
                temperatureEmailTimerOnCooldown = true;
                _temperatureEmailTimer.Enabled = false;

            }
            else
            {
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastAirTemperature, "broken");
            }

            //if (!(lastVoltage < minThreshold || lastVoltage > maxThreshold))
            //{
            //    _emailTimer.Enabled = false;
            //    thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastVoltage, "fixed");
            //}
        }
        public override void _emailAlertCooldown_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // send a reply email

            _emailAlertCooldown.Enabled = false;
            humidityEmailTimerOnCooldown = false;
        }
        public void _temperatureAlertCooldown_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // send a reply email

            _temperatureAlertCooldown.Enabled = false;
            temperatureEmailTimerOnCooldown = false;
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
