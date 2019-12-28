using System;
using System.Collections.Generic;
using System.Text;
// Added
using Phidget22;

namespace TRADDataMonitor.SensorTypes
{
    public class MyMoistureSensor : PhidgetSensor
    {
        VoltageInput device;
        public DateTime lastTimestamp;
        private double lastVoltage = -1;

        public MyMoistureSensor(int hubPort, string type, double minThreshold, double maxThreshold, bool wireless) : base(hubPort, type, minThreshold, maxThreshold, wireless)
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

            if(lastVoltage < minThreshold)
            {

            }
            else if(lastVoltage > maxThreshold)
            {

            }
        }

        public override string[] ProduceData()
        {
            string[] ret = new string[3];
            ret[0] = lastTimestamp.ToString();
            ret[1] = "Moisture Voltage";
            ret[2] = LiveData;
            return ret;
        }
    }
}
