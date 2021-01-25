using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Infinity.Automation.Lib.Models.Enums;

namespace Infinity.Automation.Lib.Models
{
    public class TestStartDetail
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public Switch SwitchTest { get; set; }
        public string LoadURL { get; set; } = "";
        public AutoTestBrowser Browser { get; set; } = AutoTestBrowser.chrome;
        public bool ShowTestRunningMessage { get; set; }
        public TestStartDetail()
        {
            SwitchTest = new Switch();
        }
    }
}
