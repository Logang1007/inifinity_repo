{
  "TestDetail": {
    "ExcelColIndexValue": -1,
    "Description": "Bid or buy navigate",
    "TestIncludeToRunFirst": "",
    "Name": "Search_For_Watch",
    "Author": "Logan Govender",
    "FileName": null,
    "FilePath": null,
    "DirectoryPath": null,
    "OutputFullFilePath": null,
    "OutPutFile": {
      "AppendTimeToFolderName": false,
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
      "Record": false,
      "ScreenNumber": 1,
      "OutPutFullPath": ""
    },
    "ImpersonateUser": {
      "Apply": false,
      "UserName": "",
      "Password": ""
    }
  },
  "Commands": [
    {
      "Value": "https://www.bidorbuy.co.za",
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
	    "DBConnectionString":"data source=UAT-SYX-SQL3;initial catalog=BetGames;MultipleActiveResultSets=True;App=EntityFramework;persist security info=True; Integrated Security=SSPI;",
		"SqlScriptFullPath":"C:\\dev_code\\infinity\\inifinity_repo\\SampleTests\\BidOrBuy_RegressionPack1\\SampleSqlScript.sql",
		"ReplaceVariables":[{"name":"TicketNumber","value":"99908300000347"}],
		"VariableContainerName":"BetGame"
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
	  "VariablesToSet":[{"name":"searchBoxName","value":"1234"},{"name":"searchBoxName2","value":"12344444"}]
    },
    {
      "Value": "watch",
      "IDToClick": "VarContainer{searchBoxName}VarContainerDB{BetGame,BetID}",
      "ClassNameToClick": "",
      "XPath": "",
      "Message": "",
      "ExcelColIndexValue": -1,
      "CommandType": 1,
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
      "OverrideErrorOnNotFound": false
    },
    {
      "Value": "",
      "IDToClick": "",
      "ClassNameToClick": "",
      "XPath": "//button[@class='bob-btn bob-btn-blue bob-btn-sm']",
      "Message": "",
      "ExcelColIndexValue": -1,
      "CommandType": 0,
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
      "OverrideErrorOnNotFound": false
    }
  ]
}