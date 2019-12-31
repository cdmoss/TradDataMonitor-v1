using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
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
                device.Open(4000);
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
                device.Close();
            }
            catch (Exception ex)
            {
                throw new Exception($"There was an error with the {_sensorType} sensor connected to port {hubPort} on hub: {hubName}. Check connections and try again. \n \n System Error Message: \n" + ex.Message);
            }
        }

        private void Device_TemperatureChange(object sender, Phidget22.Events.TemperatureSensorTemperatureChangeEventArgs e)
        {
            lastSoilTemperature = e.Temperature;
            lastTimestamp = DateTime.Now;
            LiveData = lastSoilTemperature.ToString() + " °C";

            if ((lastSoilTemperature < minThreshold || lastSoilTemperature > maxThreshold) && !_emailTimer.Enabled)
            {
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastSoilTemperature, "broken");
                _emailTimer.Enabled = true;
            }
        }

        public override void _emailTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastSoilTemperature, "broken");

            if (!(lastSoilTemperature < minThreshold || lastSoilTemperature > maxThreshold) && _emailTimer.Enabled)
            {
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastSoilTemperature, "fixed");
                _emailTimer.Enabled = false;
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
