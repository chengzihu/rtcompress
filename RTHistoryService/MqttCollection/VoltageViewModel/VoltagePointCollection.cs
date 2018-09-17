using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.DynamicDataDisplay.Common;

namespace MqttClient.VoltageViewModel
{
    public class VoltagePointCollection : RingArray <VoltagePoint>
    {
        private const int TOTAL_POINTS =300;

        public VoltagePointCollection()
            : base(TOTAL_POINTS) // here i set how much values to show 
        {    
        }
    }

    public class VoltagePoint
    {        
        public DateTime Date { get; set; }
        
        public double Voltage { get; set; }

        public int Interval { get; set; }

        public VoltagePoint(double voltage, DateTime date,int interval)
        {
            this.Date = date;
            this.Voltage = voltage;
            this.Interval = interval;
        }
    }
}
