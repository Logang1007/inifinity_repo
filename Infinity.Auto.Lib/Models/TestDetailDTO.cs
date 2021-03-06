﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Infinity.Automation.Lib.Models.Enums;

namespace Infinity.Automation.Lib.Models
{
    public class TestDetailDTO
    {
        public string TestUniqueCode { get; set; }
        public string TestId { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }
        public string DirectoryPath { get; set; }
        public string OutputFullFilePath { get; set; }

        public OutPutFile OutPutFile { get; set; }
        public AutoTestBrowser Browser { get; set; }
        public int NumberOfIterations { get; set; }
        public ExcelDocument ExcelDocument { get; set; }
        public EmailResults EmailResults { get; set; }
        public int ExcelColIndexValue = -1;
        public string Description = "";
        public string TestIncludeToRunFirst = "";
        public BrowserOptions BrowserOptions { get; set; }
        public RecordVideo RecordVideo { get; set; }
        public ExImpersonateUser ImpersonateUser { get; set; }
        public string ResultsFolder { get; set; }
        public bool DoesContainAngular { get; set; }
        public string ResponseMessage { get; set; }
        public TimeTakenDTO TimeTaken { get; set; }
        public SimulateNetworkCondition SimulateNetworkCondition { get; set; }

        public bool ShowTestRunningMessage { get; set; }
        public bool RunTest { get; set; } = true;
        
        public TestDetailDTO()
        {
            ExcelDocument = new ExcelDocument() { Use = false };
            EmailResults = new EmailResults() { SendEmail = false,FromEmail="",Subject="" };
            BrowserOptions = new BrowserOptions() { Maximized = false, ShowBrowser = true };
            RecordVideo = new RecordVideo() { Record = false, ScreenNumber = 1,OutPutFullPath="" };
            ImpersonateUser = new ExImpersonateUser() { Apply = false, Password = "", UserName = "" };
            TimeTaken = new TimeTakenDTO();
            SimulateNetworkCondition = new SimulateNetworkCondition() { Enabled = false, 
                DownloadSpeed = DownloadSpeed.Default,
                UploadSpeed = UploadSpeed.Default,
                LatencySeconds = 1,
                IsOffline=false
            };
        }
    }

    public class OutPutFile
    {
        public string FolderPath { get; set; }
        public bool CreateOutPut { get; set; }
        public bool AppendTimeToFolderName = false;
    }

    public class ExcelDocument
    {
        public string ExcelDocumentPath { get; set; }
        public string WorkSheetName { get; set; }
        public Dictionary<int,List<ExcelColData>> Data { get; set; }
        public bool Use = false;
    }

    public class ExcelColData {
        public int ColIndex { get; set; }
        public string Data { get; set; }
    }

    public class EmailResults
    {
        public bool SendEmail = false;
        public string FromEmail = "";
        public string Subject = "";
        public List<string> EmailTo { get; set; }
    }

    public class BrowserOptions{
        public bool ShowBrowser = true;
        public bool Maximized = false;
    }

    public class RecordVideo
    {
        public bool Record = true;
        public int ScreenNumber=1;
        public string OutPutFullPath = "";
        public int AdditionalWidth { get; set; } = 0;
        public int AdditionalHeight { get; set; } = 0;
        public int Quality { get; set; } = 15;
    }

    public class ExImpersonateUser
    {
        public bool Apply = false;
        public string UserName = "";
        public string Password = "";
    }

    public class SimulateNetworkCondition
    {
        public bool Enabled { get; set; }
        public DownloadSpeed DownloadSpeed { get; set; }
        public UploadSpeed UploadSpeed { get; set; }
        public int LatencySeconds { get; set; }
        public bool IsOffline { get; set; }
    }

    public enum DownloadSpeed
    {
        Default = 0,
        VerySlow =1,
        Slow=2,
        Fast=3,
        VeryFast=4
    }

    public enum UploadSpeed
    {
        Default = 0,
        VerySlow = 1,
        Slow = 2,
        Fast = 3,
        VeryFast = 4
    }
}
