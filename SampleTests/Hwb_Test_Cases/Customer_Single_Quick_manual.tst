{
  "TestDetail": {
  "TestUniqueCode":"002",
    "ExcelColIndexValue": -1,
    "Description": "Customer able to take a single manual pick",
    "TestIncludeToRunFirst": ".\\vars.tstinc",
    "Name": "Customer able to take a single manual pick",
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
  },
  "Commands": [
    {
      "Value": "VarContainer{newmoburl}",
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
	{
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
	  "ClickMany":[{AttributeToClick:{Name:"class",Value:"checkmark",IndexToClick:0}}],
	  "CommandType": 12,
	  "FindElementMaxRetries":5,
      "OverrideErrorOnNotFound": false,
    },
	{
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
	  "ClickMany":[{AttributeToClick:{Name:"class",Value:"btn btn-success btn-login",IndexToClick:0}}],
	  "CommandType": 12,
	  "FindElementMaxRetries":5,
      "OverrideErrorOnNotFound": false,
    },
	{
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
	  "ClickMany":[
	  {AttributeToClick:{Name:"alt",Value:"Lucky Numbers",IndexToClick:0}},
	  {AttributeToClick:{Name:"class",Value:"flag-icon flag-icon-za",IndexToClick:0}},
		{AttributeToClick:{Name:"class",Value:"lotto-date-and-time",IndexToClick:0}},
	  
	  ],
	  "CommandType": 12,
	  "FindElementMaxRetries":8,
      "OverrideErrorOnNotFound": false,
    },
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
	{
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
	  "ClickMany":[{AttributeToClick:{Name:"class",Value:"btn-bet btn btn-success",IndexToClick:0},WaitAfterSeconds:3}],
	  "CommandType": 12,
	  "FindElementMaxRetries":5,
      "OverrideErrorOnNotFound": false,
	   "ScreenShot": {
        "Take": true,
        "Name": "",
        "Img": null
      },
    },
	{
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
	  "TextToTypeMany":[{AttributeToClick:{Name:"class",Value:"stake-input-box",IndexToClick:0},TextToType:"2"}],
	  "ElementIndexToFind":1,
	  "FindElementMaxRetries":5,
      "XPath": "",
      "Message": "",
      "ExcelColIndexValue": -1,
      "CommandType": 11,
      "CommandStatus": 0,
      "ScreenShot": {
        "Take": false,
        "Name": "",
        "Img": null
      },
	  "Execute": true,
      "OverrideErrorOnNotFound": false,
    },
	{
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
	  "ClickMany":[{AttributeToClick:{Name:"class",Value:"btn-success",TextToFind:"Submit"},WaitAfterSeconds:4}],
	  "CommandType": 12,
	  "FindElementMaxRetries":8,
      "OverrideErrorOnNotFound": false,
	   "ScreenShot": {
        "Take": true,
        "Name": "",
        "Img": null
      },
    },
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
    },
  ]
}