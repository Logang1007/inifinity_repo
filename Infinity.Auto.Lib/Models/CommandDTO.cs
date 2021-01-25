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
        public string TestId { get; set; }
        public string TestUniqueCode { get; set; }
        public CommandType CommandType { get; set; }
        public string Value = "";
        public string IDToClick = "";
        public string ClassNameToClick = "";
        public string XPath = "";
        public AttributeToClick AttributeToClick { get; set; }
        public int IndexToClick { get; set; } = -1;
        public CommandResponseStatus CommandStatus { get; set; }
        public ScreenShot ScreenShot { get; set; }
        public string Message = "";
        public AppendRandomToValue AppendRandomToValue { get; set; }
        public bool Execute { get; set; }
        public int ExcelColIndexValue = -1;
        public DateNowValue DateNowValue { get; set; }
        public bool OverrideErrorOnNotFound { get; set; }
        public SQLObj SQLObj { get; set; }
        public List<LookUp> VariablesToSet { get; set; }
        public int ElementIndexToFind { get; set; } = -1;
        public int FindElementMaxRetries { get; set; } = 20;
        public List<ElementObject> TextToTypeMany { get; set; }
        public List<ElementObject> ClickMany { get; set; }
        public TimeTakenDTO TimeTaken { get; set; }
        public Switch SwitchClick { get; set; }
        public AssertElementsOrderByAttribute AssertElementsOrderByAttribute { get; set; }
        public string CssSelector { get; set; }
        public AssertElementValue AssertElementValueEquals { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime DurationStartTime { get; set; }
        public DateTime DurationEndTime { get; set; }
        public bool IsFailed { get; set; }
        public string ResponseMessage { get; set; }
        public List<ClickDropDown> ClickDropDown { get; set; }
        public AssertElementCount AssertElementCount { get; set; }
        public bool MoveToElement { get; set; } = true;

        public CommandDTO()
        {
            ScreenShot = new ScreenShot();
            AttributeToClick = new AttributeToClick();
            AppendRandomToValue = new AppendRandomToValue();
            Execute = true;
            DateNowValue = new DateNowValue();
            OverrideErrorOnNotFound = false;
            SQLObj = new SQLObj();
            VariablesToSet = new List<LookUp>();
            TextToTypeMany = new List<ElementObject>();
            TimeTaken = new TimeTakenDTO();
            SwitchClick = new Switch();
            AssertElementsOrderByAttribute = new AssertElementsOrderByAttribute() { OrderValue = new List<string>()};
            AssertElementValueEquals = new AssertElementValue();
            ClickDropDown = new List<ClickDropDown>();
            AssertElementCount = new AssertElementCount();
        }

    }

    public class AttributeToClick
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string TextToFind { get; set; }
        public int IndexToClick { get; set; } = -1;
        public IndexToClickRandom IndexToClickRandom { get; set; } = new IndexToClickRandom();
    }

    public class IndexToClickRandom
    {
        public int Min { get; set; } = -1;
        public int Max { get; set; } = -1;
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

    public class SQLObj
    {
        public string SqlScriptFullPath { get; set; }
        public string DBConnectionString { get; set; }
        public List<LookUp> ReplaceVariables { get; set; }
        public string VariableContainerName { get; set; }
    }

    public class LookUp
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class ElementObject
    {
        public string NameOrId { get; set; }
        public string ClassName { get; set; }

        public string XPath { get; set; }

        public AttributeToClick AttributeToClick { get; set; }
        public string TextToType { get; set; }
        public string CssSelector { get; set; }

        public int IndexToFind { get; set; }
        public int WaitAfterSeconds { get; set; } = -1;
    }

    public class Switch
    {
        public string If { get; set; }
        public string Then { get; set; }
        public string Else { get; set; }
    }

    public class AssertElementsOrderByAttribute
    {
        public string CssSelector { get; set; }
        public List<string> OrderValue { get; set; }
        public string AttributeName { get; set; }
        public bool CheckInnerText { get; set; }
    }

    public class AssertElementValue
    {
        public AttributeToClick ElementToFindByAttribute { get; set; }
        public string IDToClick { get; set; }
        public string ClassNameToClick { get; set; }
        public string CssSelector { get; set; }
        public string ExpectedValue { get; set; }
        public ValueSelector ValueSelector { get; set; }
    }

    public class ClickDropDown
    {
        public AttributeToClick AttributeFindBy { get; set; }
        public string ClassName { get; set; }
        public string NameOrId { get; set; }
        public string XPath { get; set; }
        public int OptionIndexToClick { get; set; } = -1;
        public string CssSelector { get; set; }
        public int WaitAfterSeconds { get; set; } = -1;
    }

    public class AssertElementCount
    {
        public AttributeToClick AttributeFindBy { get; set; }
        public int ExpectedCount { get; set; }
        public string ClassNameToClick { get; set; }
        public string IDToClick { get; set; }
        public string XPath { get; set; }
        public string CssSelector { get; set; }
    }


    public enum ValueSelector
    {
        Text =1,
        InputValue=2
    }

}
