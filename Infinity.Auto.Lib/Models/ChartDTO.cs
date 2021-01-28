using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Automation.Lib.Models
{
    public class ChartDTO
    {
        public List<string> labels { get; set; } = new List<string>();
        public List<ChartDataset> datasets { get; set; } = new List<ChartDataset>();

        
    }

    public class ChartDataset
    {
        public string label { get; set; }
        public string backgroundColor { get; set; }
        public int maxBarThickness { get; set; }
        public double[] data { get; set; }
    }
}
