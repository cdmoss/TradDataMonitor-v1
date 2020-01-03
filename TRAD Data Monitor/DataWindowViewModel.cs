using Avalonia.Media.Imaging;
using RDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace TRADDataMonitor
{
    public class DataWindowViewModel : INotifyPropertyChanged
    {
        DataAccessor _data;

        bool _moisture, _humidity, _soilTemp, _airTemp, _oxygen, _voc, _co2;
        List<string> selectedSensors;

        public bool Moisture
        {
            get { return _moisture; }
            set 
            { 
                _moisture = value;
                if (value)
                    selectedSensors.Add("Moisture");
                OnPropertyChanged();
            }
        }

        public bool Humidity
        {
            get { return _moisture; }
            set
            {
                _moisture = value;
                if (value)
                    selectedSensors.Add("Humidity");
                OnPropertyChanged();
            }
        }

        public bool SoilTemperature
        {
            get { return _soilTemp; }
            set
            {
                _soilTemp = value;
                if (value)
                    selectedSensors.Add("Soil Temperature");
                else
                    selectedSensors.Remove("Soil Temperature");

                OnPropertyChanged();
            }
        }

        public bool AirTemperautre
        {
            get { return _airTemp; }
            set
            {
                _airTemp = value;
                if (value)
                    selectedSensors.Add("Air Temperature");
                else
                    selectedSensors.Remove("Air Temperature");

                OnPropertyChanged();
            }
        }

        public bool Oxygen
        {
            get { return _oxygen; }
            set
            {
                _oxygen = value;
                if (value)
                    selectedSensors.Add("Oxygen");
                else
                    selectedSensors.Remove("Oxygen");

                OnPropertyChanged();
            }
        }

        public bool VOC
        {
            get { return _voc; }
            set
            {
                _voc = value;
                if (value)
                    selectedSensors.Add("VOC");
                OnPropertyChanged();
            }
        }

        public bool CO2
        {
            get { return _co2; }
            set
            {
                _co2 = value;
                if (value)
                    selectedSensors.Add("CO2");
                OnPropertyChanged();
            }
        }

        public DataWindowViewModel()
        {
            _data = new DataAccessor();
            selectedSensors = new List<string>();

            selectedSensors.Add("Moisture");
            selectedSensors.Add("Humidity");

            DataTable dt = _data.GetSensorData(selectedSensors);

            DataTableToCSV(dt);
        }

        void CreateGraph(params string[] sensorTypes)
        {
            REngine.SetEnvironmentVariables();
            REngine engine = REngine.GetInstance();
            engine.Initialize();
            string fileName = "data.csv";

            CharacterVector fileNameVector = engine.CreateCharacterVector(new[] { fileName });
            engine.SetSymbol("fileName", fileNameVector);
        }

        // Taken From https://stackoverflow.com/questions/4959722/c-sharp-datatable-to-csv
        void DataTableToCSV(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>()
                .Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText("data.csv", sb.ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
