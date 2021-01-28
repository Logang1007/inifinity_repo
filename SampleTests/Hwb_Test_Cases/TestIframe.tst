{
  "TestDetail": {
  "TestUniqueCode":"003",
    "ExcelColIndexValue": -1,
    "Description": "Test iframe feature",
    "TestIncludeToRunFirst": ".\\vars.tstinc",
    "Name": "TestIFrame",
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
      "Value": "VarContainer{uaturl1}",
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
	  "ClickMany":[{AttributeToClick:{Name:"sportid",Value:"21"}, WaitAfterSeconds:1},
	  {CssSelector:".eventBrowserTournamentHeadingSubTable td", IndexToFind:0, WaitAfterSeconds:2},
	  {CssSelector:".tournamentTab td", IndexToFind:2, WaitAfterSeconds:2},
	  {CssSelector:".eventBrowserEventsTable td", IndexToFind:0, WaitAfterSeconds:1}
	  ],
	  "CommandType": 12,
	  "FindElementMaxRetries":5,
      "OverrideErrorOnNotFound": false,
	  "Execute": true,
    },
	{
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
	  "ClickMany":[{CssSelector:'.bettingGridTableDataRow img',IndexToFind:20}],
	  "CommandType": 12,
	  "FindElementMaxRetries":5,
      "OverrideErrorOnNotFound": false,
	  "IFrame":{IDToClick:"ifBetEvents"},
	  "Execute": true,
    },
	
  ]
}