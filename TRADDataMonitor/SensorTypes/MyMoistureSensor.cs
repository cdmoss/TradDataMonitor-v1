﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
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

        public MyMoistureSensor(int hubPort, string type, string hubName, double minThreshold, double maxThreshold, bool wireless) : base(hubPort, type, hubName, minThreshold, maxThreshold, wireless)
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

        private void Device_VoltageChange(object sender, Phidget22.Events.VoltageInputVoltageChangeEventArgs e)
        {
            lastVoltage = e.Voltage;
            lastTimestamp = DateTime.Now;
            LiveData = lastVoltage.ToString() + " V";

            if((lastVoltage < minThreshold || lastVoltage > maxThreshold ) && !_emailTimer.Enabled)
            {
                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastVoltage, "broken");
                _emailTimer.Enabled = true;
            }
        }

        public override void _emailTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastVoltage, "broken");

            if (!(lastVoltage < minThreshold || lastVoltage > maxThreshold))
            {
                _emailTimer.Enabled = false;

                thresholdBroken?.Invoke(minThreshold, maxThreshold, hubName, SensorType, hubPort, lastVoltage, "fixed");
            }
        }

        public override string[] ProduceData()
        {
            string[] ret = new string[3];
            ret[0] = lastTimestamp.ToString();
            ret[1] = "Moisture Voltage (V)";
            ret[2] = LiveData;
            return ret;
        }
    }
}