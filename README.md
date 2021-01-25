# inifinity_repo
A web automation framework that allows regression testing of web apps

0. Test detail object
"TestDetail": {
  "TestUniqueCode":"001",
    "ExcelColIndexValue": -1,
    "Description": "Customer able to take a single quick pick",
    "TestIncludeToRunFirst": ".\\vars.tstinc",
    "Name": "Customer able to take a single quick pick",
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
    "NumberOfIterations": 1,
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
      "OutPutFullPath": "",
	  "Quality":20
    },
    "ImpersonateUser": {
      "Apply": false,
      "UserName": "",
      "Password": ""
    },
	"SimulateNetworkCondition":{Enabled:true,DownloadSpeed:4,UploadSpeed:4,LatencySeconds:0,IsOffline:false},
	"ShowTestRunningMessage":false,
	"RunTest":false,
  }

1. A test start file will be executed first before any test files(Only one test start file is allowed per directory).
  - A test start file has the .tststart extension.
  - Sample test file:
  {
	"Name": "Login determine",
    "Author": "Logan Govender",
	"Description": "Decide which test to run",
	"Browser":0,
	"ShowTestRunningMessage":true,
	"LoadURL": "https://demo6.hollywoodbets.net/",
	"SwitchTest":{If:"!FindElement('class','img-fluid',1222)",Then:"RunTest('.\\login_new_mob.tst')",Else:"RunTest('.\\login_new_mob2.tst')"},
}

2. A test file supports the following commands(below is a list of commonly used commands):
  - Click
    {
      "Value": "",
      "IDToClick": "someId",
      "ClassNameToClick": "someClass",
      "XPath":"//someXpath",
      "AttributeToClick":{Name:"some attribute name",Value:"some attribute value",IndexToClick:0},
      "CssSelector":"some css selector. you can use chrome dev tool bar to test your selector. eg. document.querySelectorAll('.classname')"
	  "CommandType": 0,
	  "FindElementMaxRetries":5, // number of seconds to retry checking for an element
      "OverrideErrorOnNotFound": false,
    }
    
    - ClickMany
    {
      "Value": "",
      "IDToClick": "someId",
      "ClassNameToClick": "someClass",
      "XPath":"//someXpath",
      "ClickMany":[
	  {AttributeToClick:{Name:"alt",Value:"Lucky Numbers",IndexToClick:0,WaitAfterSeconds:4}},
	  {AttributeToClick:{Name:"class",Value:"flag-icon flag-icon-za",IndexToClick:0,WaitAfterSeconds:4}},
		{AttributeToClick:{Name:"class",Value:"lotto-date-and-time",IndexToClick:0,WaitAfterSeconds:4}},
	  
	  ],
	  "CommandType": 12,
	  "FindElementMaxRetries":5, // number of seconds to retry checking for an element
      "OverrideErrorOnNotFound": false,
    }
    
    or 
    
    	{
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
	  "ClickMany":[
		{AttributeToClick:{Name:"class",Value:"number-circle",IndexToClickRandom:{Min:0,Max:10}}},
		{AttributeToClick:{Name:"class",Value:"number-circle",IndexToClickRandom:{Min:11,Max:20}}},
		{AttributeToClick:{Name:"class",Value:"number-circle",IndexToClickRandom:{Min:21,Max:30}}},
		{AttributeToClick:{Name:"class",Value:"number-circle",IndexToClickRandom:{Min:31,Max:40}}},
		{AttributeToClick:{Name:"class",Value:"number-circle",IndexToClickRandom:{Min:41,Max:50}}}
	  
	  ],
	  "CommandType": 12,
	  "FindElementMaxRetries":8,
      "OverrideErrorOnNotFound": false,
	  "MoveToElement": false,
    },
    
    -TextToTypeMany
   {
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
	  "TextToTypeMany":[{AttributeToClick:{Name:"aria-label",Value:"Username"},TextToType:"VarContainer{default_username}"},{AttributeToClick:{Name:"aria-label",Value:"Password"},TextToType:"VarContainer{default_userpassword}"}],
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
    
    - ClickDropDown
    {
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
	  "ClickDropDown":[
		{AttributeFindBy:{Name:"class",Value:"lotto-odds-container",IndexToClick:0},OptionIndexToClick:4}],
	  "CommandType": 16,
	  "FindElementMaxRetries":5,
      "OverrideErrorOnNotFound": false,
    },
    
    -VariablesToSet
     {
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
      "XPath": "",
      "Message": "",
      "ExcelColIndexValue": -1,
      "CommandType": 10,
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
	  "VariablesToSet":[{"name":"db_conn_string","value":"data source=;initial catalog=;MultipleActiveResultSets=True;App=EntityFramework;persist security info=True; Integrated Security=SSPI;"}
   }
   
   -SQL to execute
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
		"VariableContainerName":"RecordSet!"
	  },
      "OverrideErrorOnNotFound": false
    },
    
    - If then command
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
    
    - Assert Count of elements
     {
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
	  "AssertElementCount":{CssSelector:".selected-odds > ul > li [class*='number-circle']",ExpectedCount:5},
	  "CommandType": 17,
	  "FindElementMaxRetries":5,
      "OverrideErrorOnNotFound": false,
    },
    
    - Assert value exists(inner text or input value: Text, InputValue)
    {
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
	  "AssertElementValueEquals":{"ElementToFindByAttribute": {
        "Name": 'class',
        "Value": 'betStrikeStatus',
		"IndexToClick":0
      },IDToClick:'',ClassNameToClick:'',CssSelector:'', ValueSelector:'Text' ,ExpectedValue:'Success'},
	  "CommandType": 15,
	  "FindElementMaxRetries":5,
      "OverrideErrorOnNotFound": false,
	  "ScreenShot": {
        "Take": true,
        "Name": "Bet_Success",
        "Img": null
      },
    
    
