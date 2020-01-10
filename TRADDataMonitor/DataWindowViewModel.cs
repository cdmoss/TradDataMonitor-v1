using Avalonia.Media.Imaging;
using RDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace TRADDataMonitor
{
    public class DataWindowViewModel : INotifyPropertyChanged
    {
        DataAccessor _data;

        string _selectedSensor;
        Bitmap _graph;

        public string[] SensorTypes { get; set; } = { "Moisture", "Humidity", "Air Temperature", "Soil Temperature", "Light", "Oxygen", "None"};

        public string SelectedSensor
        {
            get { return _selectedSensor; }
            set 
            {
                _selectedSensor = value;
                OnPropertyChanged();
            }
        }

        public Bitmap Graph
        {
            get { return _graph; }
            set 
            {
                _graph = value;
                OnPropertyChanged();
            }
        }

        public DataWindowViewModel()
        {
            _data = new DataAccessor();
            SelectedSensor = "None";
        }

        void CreateGraph(string sensorTypes)
        {
            try
            {
                DataTableToCSV(_data.GetSensorData(SelectedSensor));

                using (Process proc = new Process())
		{
			proc.StartInfo.FileName = "sh";
			proc.StartInfo.Arguments = "/home/pi/Trad-Data-Monitor/TradPackage/chart.sh";
	                proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.RedirectStandardOutput = true
        	        proc.Start();
                	proc.WaitForExit();
		}
                

                Graph = new Bitmap("/home/pu/Trad-Data-Monitor/TradPackage/graph.png");
            }
            catch (Exception ex)
            {
                throw;
            }
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

            File.WriteAllText("C:\\Users\\cheze\\Desktop\\TradPackage\\data.csv", sb.ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
