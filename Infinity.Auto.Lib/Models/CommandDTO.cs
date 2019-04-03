using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Infinity.Automation.Lib.Models.Enums;

namespace Infinity.Automation.Lib.Models
{
    public class CommandDTO
    {
        public CommandType CommandType { get; set; }
        public string Value = "";
        public string IDToClick = "";
        public string ClassNameToClick = "";
        public AttributeToClick AttributeToClick { get; set; }
        public int IndexToClick { get; set; }
        public CommandResponseStatus CommandStatus { get; set; }
        public ScreenShot ScreenShot { get; set; }
        public string Message = "";
        public AppendRandomToValue AppendRandomToValue { get; set; }
        public bool Execute { get; set; }
        public int ExcelColIndexValue = -1;
        public DateNowValue DateNowValue { get; set; }

        public CommandDTO()
        {
            ScreenShot = new ScreenShot();
            AttributeToClick = new AttributeToClick();
            AppendRandomToValue = new AppendRandomToValue();
            Execute = true;
            DateNowValue = new DateNowValue();
        }

    }

    public class AttributeToClick
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class ScreenShot
    {
        public bool Take { get; set; }
        public string Name { get; set; }

        public byte[] Img { get; set; }
    }

    public class AppendRandomToValue {
        public int Min { get; set; }
        public int Max { get; set; }

        public bool Apply { get; set; }
    }

    public class DateNowValue {

        public int DayAdd { get; set; }
        public string Format { get; set; }
        public bool Apply { get; set; }
    }


}
