using System;
using System.Collections.Generic;
using System.Text;
// Added
using Phidget22;

namespace TRADDataMonitor.SensorTypes
{
    public class MyOxygenSensor : PhidgetSensor
    {
        VoltageInput device;
        public DateTime lastTimestamp;
        private double lastVoltage;
        private bool insertRecord = true;

        public MyOxygenSensor(int hubPort, string type, string hubName, double minThreshold, double maxThreshold, bool wireless) : base(hubPort, type, hubName, minThreshold, maxThreshold, wireless)
        {
            device = new VoltageInput();
            device.HubPort = hubPort;
            device.IsHubPortDevice = true;
            device.VoltageChange += Device_VoltageChange;
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

        private void Device_VoltageChange(object sender, Phidget22.Events.VoltageInputVoltageChangeEventArgs e)
        {
            lastVoltage = e.Voltage;
            lastTimestamp = DateTime.Now;
            LiveData = lastVoltage.ToString();

            if (lastVoltage < minThreshold)
            {
                // Send an email alert that the threshold has exceeded the min value
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastVoltage);
            }
            if (lastVoltage > maxThreshold)
            {
                // Send an email alert that the threshold has exceeded the max value
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastVoltage);
            }
        }

        public override String[] ProduceData()
        {
            string[] ret = new string[3];
            ret[0] = lastTimestamp.ToString();
            ret[1] = "Oyxgen Voltage (V)";
            ret[2] = LiveData;
            return ret;
        }
    }
}
