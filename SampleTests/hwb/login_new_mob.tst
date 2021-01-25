{
  "TestDetail": {
  "TestUniqueCode":"001",
    "ExcelColIndexValue": -1,
    "Description": "Demo hollywood bets horse racing",
    "TestIncludeToRunFirst": ".\\vars.tstinc",
    "Name": "Horse racing",
    "Author": "Logan Govender",
    "FileName": null,
    "FilePath": null,
    "DirectoryPath": null,
    "OutputFullFilePath": null,
    "OutPutFile": {
      "AppendTimeToFolderName": true,
      "FolderPath": "",
      "CreateOutPut": true
    },
    "Browser": 0,
    "NumberOfIterations": 2,
    "ExcelDocument": {
      "Use": false,
      "ExcelDocumentPath": null,
      "WorkSheetName": null,
      "Data": null
    },
    "EmailResults": {
      "SendEmail": false,
      "FromEmail": "",
      "Subject": "",
      "EmailTo": [
        ""
      ]
    },
    "BrowserOptions": {
      "ShowBrowser": true,
      "Maximized": true
    },
    "RecordVideo": {
      "Record": true,
      "ScreenNumber": 1,
      "OutPutFullPath": ""
    },
    "ImpersonateUser": {
      "Apply": false,
      "UserName": "",
      "Password": ""
    },
	"SimulateNetworkCondition":{Enabled:true,DownloadSpeed:4,UploadSpeed:4,LatencySeconds:0,IsOffline:false},
	"ShowTestRunningMessage":true,
  },
  "Commands": [
  
  {
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
      "XPath": "",
      "Message": "",
      "ExcelColIndexValue": -1,
      "CommandType": 9,
      "AttributeToClick": {
        "Name": "",
        "Value": ""
      },
      "IndexToClick": 0,
      "CommandStatus": 0,
      "ScreenShot": {
        "Take": false,
        "Name": "",
        "Img": null
      },
      "AppendRandomToValue": {
        "Min": 0,
        "Max": 0,
        "Apply": false
      },
      "Execute": true,
      "DateNowValue": {
        "DayAdd": 0,
        "Format": null,
        "Apply": false
      },
	  "SQLObj":{
	    "DBConnectionString":"VarContainer{db_uat_syx_betgames_conn_string}",
		"SqlScriptFullPath":".\\SampleSqlScript.sql",
		"ReplaceVariables":[{"name":"TicketNumber","value":"99908300000347"}],
		"VariableContainerName":"BetGame"
	  },
      "OverrideErrorOnNotFound": false
    },
    {
      "Value": "https://demo6.hollywoodbets.net/",
      "IDToClick": "",
      "ClassNameToClick": "",
      "XPath": "",
      "Message": "",
      "ExcelColIndexValue": -1,
      "CommandType": 4,
      "AttributeToClick": {
        "Name": null,
        "Value": null
      },
      "IndexToClick": 0,
      "CommandStatus": 0,
      "ScreenShot": {
        "Take": false,
        "Name": null,
        "Img": null
      },
      "AppendRandomToValue": {
        "Min": 0,
        "Max": 0,
        "Apply": false
      },
      "Execute": true,
      "DateNowValue": {
        "DayAdd": 0,
        "Format": null,
        "Apply": false
      },
      "OverrideErrorOnNotFound": false
    },
	
	{
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
      "XPath": "",
      "Message": "",
      "ExcelColIndexValue": -1,
      "CommandType": 13,
	  "SwitchClick":{If:"!FindElement('class','img-fluid',1)",Then:"FindElement('class','img-fluid',1)"},
      "AttributeToClick": {
        "Name": null,
        "Value": null
      },
      "IndexToClick": 0,
      "CommandStatus": 0,
      "ScreenShot": {
        "Take": false,
        "Name": null,
        "Img": null
      },
      "AppendRandomToValue": {
        "Min": 0,
        "Max": 0,
        "Apply": false
      },
      "Execute": true,
      "DateNowValue": {
        "DayAdd": 0,
        "Format": null,
        "Apply": false
      },
      "OverrideErrorOnNotFound": false
    },
	{
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
	  "ClickMany":[{AttributeToClick:{Name:"class",Value:"img-fluid",IndexToClick:1}},{CssSelector:'country-selector.menu > li > span', IndexToFind:0}],
	  "FindElementMaxRetries":5,
      "XPath": "",
      "Message": "",
      "ExcelColIndexValue": -1,
      "CommandType": 12,
      "AttributeToClick": {
        "Name": "",
        "Value": ""
      },
      "IndexToClick": 0,
      "CommandStatus": 0,
      "ScreenShot": {
        "Take": false,
        "Name": "",
        "Img": null
      },
      "AppendRandomToValue": {
      },
      "Execute": true,
      "DateNowValue": {
        "DayAdd": 0,
        "Format": null,
        "Apply": false
      },
      "OverrideErrorOnNotFound": false,
    },
	{
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
      "XPath": "",
      "Message": "",
      "ExcelColIndexValue": -1,
      "CommandType": 15,
	  "AssertElementValueEquals":{"ElementToFindByAttribute": {
        "Name": 'class',
        "Value": 'tournament-date-and-time',
		"IndexToClick":0
      },IDToClick:'',ClassNameToClick:'',CssSelector:'', ValueSelector:'Text' ,ExpectedValue:'07 Jan 21 13:00'},
      "AttributeToClick": {
        "Name": null,
        "Value": null
      },
      "IndexToClick": 0,
      "CommandStatus": 0,
      "ScreenShot": {
        "Take": true,
        "Name": null,
        "Img": null
      },
      "AppendRandomToValue": {
        "Min": 0,
        "Max": 0,
        "Apply": false
      },
      "Execute": true,
      "DateNowValue": {
        "DayAdd": 0,
        "Format": null,
        "Apply": false
      },
      "OverrideErrorOnNotFound": false
    },
	{
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
      "XPath": "",
      "Message": "",
      "ExcelColIndexValue": -1,
      "CommandType": 14,
	  "AssertElementsOrderByAttribute":{CssSelector:'.menu.menu-svg-icons > li',CheckInnerText:true,AttributeName:'alt',OrderValue:['COVID-19 SA','Cape Met Antepost','Punters Challenge']},
      "AttributeToClick": {
        "Name": null,
        "Value": null
      },
      "IndexToClick": 0,
      "CommandStatus": 0,
      "ScreenShot": {
        "Take": false,
        "Name": null,
        "Img": null
      },
      "AppendRandomToValue": {
        "Min": 0,
        "Max": 0,
        "Apply": false
      },
      "Execute": true,
      "DateNowValue": {
        "DayAdd": 0,
        "Format": null,
        "Apply": false
      },
      "OverrideErrorOnNotFound": false
    },
	{
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
	  "TextToTypeMany":[{AttributeToClick:{Name:"aria-label",Value:"Username"},TextToType:"TestUsername"},{AttributeToClick:{Name:"aria-label",Value:"Password"},TextToType:"TestPassword"}],
	  "ElementIndexToFind":1,
	  "FindElementMaxRetries":5,
      "XPath": "",
      "Message": "",
      "ExcelColIndexValue": -1,
      "CommandType": 11,
      "AttributeToClick": {
        "Name": "",
        "Value": ""
      },
      "IndexToClick": 0,
      "CommandStatus": 0,
      "ScreenShot": {
        "Take": false,
        "Name": "",
        "Img": null
      },
      "AppendRandomToValue": {
        "Min": 50,
        "Max": 100,
        "Apply": false
      },
      "Execute": true,
      "DateNowValue": {
        "DayAdd": 0,
        "Format": null,
        "Apply": false
      },
      "OverrideErrorOnNotFound": false,
    },
	{
	  "FindElementMaxRetries":5,
      "CommandType": 0,
      "AttributeToClick": {
        "Name": "class",
        "Value": "tournament-date-and-time",
		"IndexToClick":0
      }
    }
  ]
}