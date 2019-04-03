using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Automation.Lib.Models
{
    public class Enums
    {

        public enum ResponseStatus
        {
             Success=0,
             Failed=1,
             SystemError=2
        }

        public enum CommandResponseStatus
        {
            Success = 0,
            Failed = 1,
        }


        public enum AutoTestBrowser
        {
            chrome=0,
            ie = 1,
            ff = 2,
        }

        public enum CommandType
        {
            Click = 0,
            TypeText = 1,
            ClickAnchorTagHref = 2,
            ClickFromList = 3,
            LoadURL = 4,
            AssertTextExists=5,
            ExecuteJS=6,
            SelectDropDown=7,
            WaitSecond=8
        }
    }
}
