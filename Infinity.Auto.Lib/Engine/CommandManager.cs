using Infinity.Automation.Lib.Helpers;
using Infinity.Automation.Lib.Models;
using LinqToExcel;
using Microsoft.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static Infinity.Automation.Lib.Models.Enums;
using CommandType = Infinity.Automation.Lib.Models.Enums.CommandType;

namespace Infinity.Automation.Lib.Engine
{
    public class CommandManager : ICommandManager
    {



        public List<TestObjectDTO> TestObjectDTO { get; set; }
        public RemoteWebDriver WebDriver;
        private IEmailHelper _emailHelper;
        private IScreenRecorderHelper _screenRecorderHelper;
        private bool _isTestIncludeRunning = false;
        private string _lastTestIncludeRan = "";
        private IImpersonateUser _impersonateUser;
        private Dictionary<string, Dictionary<string, string>> _variableContainer = new Dictionary<string, Dictionary<string, string>>();
        private string _systemVariableName = "Sys_Variables";
        private string _testDirectory = "";
        private int _findElementRetryCount = 0;
        private bool _logDebugInfo = false;
        private string _testStartFileFullPath = "";
        private string _showMessageTestRunningPrefix = "";
        private IPortableDataStore _portableDataStore;
        public CommandManager(string path, bool isDirectory, IEmailHelper emailHelper, IImpersonateUser impersonateUser, IScreenRecorderHelper screenRecorderHelper, OnCommandManagerInitComplete onCommandManagerInitComplete, string testExtFilter = "*.tst", bool logDebugInfo = true,string showMessageTestRunningPrefix="",
            IPortableDataStore portableDataStore=null)
        {
            _emailHelper = emailHelper;
            _impersonateUser = impersonateUser;
            _screenRecorderHelper = screenRecorderHelper;
            TestObjectDTO = new List<TestObjectDTO>();
            _testDirectory = path;
            _logDebugInfo = logDebugInfo;
            _testStartFileFullPath = "";
            _showMessageTestRunningPrefix = showMessageTestRunningPrefix;
            _portableDataStore = portableDataStore;

            if (isDirectory)
            {
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, testExtFilter);
                    if (files.Length == 0)
                    {
                        onCommandManagerInitComplete(ResponseStatus.Failed, "The directory must contain sub-directories with tests inside!");
                    }
                    else
                    {
                        onCommandManagerInitComplete(ResponseStatus.Success, "Success");
                        foreach (var item in files)
                        {
                            var fi = new FileInfo(item);
                            //add first start test file
                            if (fi.Extension.ToLower().Replace(".", "") == "tststart")
                            {
                                _testStartFileFullPath = item;
                                break;
                            }

                        }
                        if (string.IsNullOrEmpty(_testStartFileFullPath))
                        {
                            foreach (var item in files)
                            {
                                var fi = new FileInfo(item);
                                //ignore inc tests
                                if (fi.Extension.ToLower().Replace(".", "") == "tst")
                                {
                                    var testObjectDTO = _parseTestFile(item);
                                    if(testObjectDTO.TestDetail.RunTest == true)
                                    {
                                        TestObjectDTO.Add(testObjectDTO);
                                    }
                                   
                                }

                            }
                        }
                     

                      
                    }

                }
                else
                {
                    throw new Exception(path + " folder doesnot exist!"); ;
                }
            }
            else
            {
                if (File.Exists(path))
                {
                    var fi = new FileInfo(path);
                    if (fi.Extension.ToLower() == testExtFilter.Replace("*", ""))
                    {
                        //ignore inc tests
                        if (fi.Extension.ToLower().Replace(".", "") != "tstinc")
                        {
                            onCommandManagerInitComplete(ResponseStatus.Success, "Success");

                            var testObjectDTO = _parseTestFile(path);
                            if (testObjectDTO.TestDetail.RunTest == true)
                            {
                                TestObjectDTO.Add(testObjectDTO);
                            }
                              
                        }
                     
                    }
                    else
                    {
                        throw new Exception(path + " is not a valid test file, it must have a .tst extension!");
                    }

                }
                else
                {
                    throw new Exception(path + " file doesnot exist!");
                }


            }

        }

        private string _getFilePath(string filePath)
        {
            bool isRelativeFilePath = filePath.StartsWith(".");
            if (isRelativeFilePath)
            {
                var relArray = filePath.Split('\\')[0].ToString().ToCharArray();
                if (relArray.Length == 1)
                {
                    var dir = new DirectoryInfo(_testDirectory).FullName;
                    filePath = dir + filePath.Remove(0, 1);
                }
                else
                {
                    var parentDirPath = new DirectoryInfo(_testDirectory).Parent.FullName;
                    var newParentDir = parentDirPath;
                    foreach (var item in relArray)
                    {
                        newParentDir = new DirectoryInfo(newParentDir).Parent.FullName;
                    }
                    filePath = newParentDir + filePath.Remove(0, 1);
                }
                
            }
            return filePath;
        }

        private TestObjectDTO _parseTestFile(string fulltestFilePath)
        {
            fulltestFilePath = _getFilePath(fulltestFilePath);
           
            var fi = new FileInfo(fulltestFilePath);
            
            var fileText = File.ReadAllText(fulltestFilePath);
            if (!string.IsNullOrEmpty(fileText))
            {   
                var testObjectDTO = JsonConvert.DeserializeObject<TestObjectDTO>(fileText);

                testObjectDTO.TestDetail.FileName = fi.Name;
                testObjectDTO.TestDetail.FilePath = fulltestFilePath;
                testObjectDTO.TestDetail.DirectoryPath = fi.Directory.FullName;
                string output = testObjectDTO.TestDetail.OutPutFile.FolderPath;
                string format = "dd_MMM_yyy";
                if (testObjectDTO.TestDetail.OutPutFile.AppendTimeToFolderName)
                {
                    format = "dd_MMM_yyy_HH_mm_ss";
                }
                if (string.IsNullOrEmpty(testObjectDTO.TestDetail.OutPutFile.FolderPath))
                {
                    output = fi.Directory.FullName + "\\Results_" + Path.GetFileNameWithoutExtension(fi.Name) + "_" + DateTime.Now.ToString(format);

                }
                else
                {
                    output = output + "\\" + Path.GetFileNameWithoutExtension(fi.Name) + "_" + DateTime.Now.ToString(format);
                }
                if (!Directory.Exists(output))
                {
                    Directory.CreateDirectory(output);
                }

                testObjectDTO.TestDetail.OutputFullFilePath = output;
                testObjectDTO.TestDetail.ExcelDocument = _parseExcelData(testObjectDTO.TestDetail.ExcelDocument);
                testObjectDTO.TestDetail.ResultsFolder = output;
                return testObjectDTO;
            }
            else
            {
                return new TestObjectDTO();
            }



        }

        private string _getShowMessageRunning(string message)
        {
            return _showMessageTestRunningPrefix + "" + message;
        }
       

        public List<TestResponseDTO> ExecuteCommands(List<TestObjectDTO> testObjectDTO, OnTestRunComplete onTestRunComplete,
            OnTestCommandComplete onTestCommandComplete, OnAllTestRunComplete onAllTestRunComplete, OnTestRunStarted onTestRunStarted)
        {
            List<TestResponseDTO> responses = new List<TestResponseDTO>();

            TestResponseDTO returnValue = new TestResponseDTO();
            returnValue.CommandsExecuted = new List<CommandDTO>();
            int currentTestRunNumber = 1;
            bool isMoreThan1Test = testObjectDTO.Count > 1;

            if (!string.IsNullOrEmpty(_testStartFileFullPath))
            {
                string testStartFileContent = File.ReadAllText(_testStartFileFullPath);
                var testStartDetail = JsonConvert.DeserializeObject<TestStartDetail>(testStartFileContent);
                _setMinimalWebDriver(testStartDetail.Browser);
                WebDriver.Navigate().GoToUrl(testStartDetail.LoadURL);
                if (testStartDetail.ShowTestRunningMessage)
                {
                    WebDriverHelpers.CreateDivOnTopOfPage(WebDriver, _getShowMessageRunning("Running Test start file :" + testStartDetail.Name), "green");
                }
                var cmd = new CommandDTO();
                cmd.SwitchClick = testStartDetail.SwitchTest;
                var testToRun =_executeSwitchTest(cmd, ref returnValue);
                if(testToRun != null)
                {
                    testObjectDTO = testToRun;
                }
            }
       
                //init the browser object
                foreach (var testObj in testObjectDTO)
                {
                   string testId = Guid.NewGuid().ToString();
                   testObj.TestDetail.TestId = testId;
                   var stopWatch = Stopwatch.StartNew();
                    try
                    {
                        returnValue.TestDetailDTO = new TestDetailDTO();
                        returnValue.TestDetailDTO = testObj.TestDetail;
                    
                        returnValue.TestDetailDTO.TimeTaken = new TimeTakenDTO();
                        returnValue.TestDetailDTO.TimeTaken.StartTime = DateTime.Now;
                    try
                    {
                        //impersonate windows user
                        if (returnValue.TestDetailDTO.ImpersonateUser.Apply == true)
                        {
                            _impersonateUser.Impersonate(returnValue.TestDetailDTO.ImpersonateUser.UserName, returnValue.TestDetailDTO.ImpersonateUser.Password);
                        }

                        if (testObj.TestDetail.TestIncludeToRunFirst != _lastTestIncludeRan)
                        {

                            if (!string.IsNullOrEmpty(testObj.TestDetail.TestIncludeToRunFirst))
                            {
                                if (File.Exists(_getFilePath(testObj.TestDetail.TestIncludeToRunFirst)))
                                {
                                    _lastTestIncludeRan = testObj.TestDetail.TestIncludeToRunFirst;
                                    var testInclObjectDTO = _parseTestFile(testObj.TestDetail.TestIncludeToRunFirst);
                                    List<TestObjectDTO> testInclObjectDTOList = new List<TestObjectDTO>();
                                    testInclObjectDTOList.Add(testInclObjectDTO);
                                    _isTestIncludeRunning = true;
                                    ExecuteCommands(testInclObjectDTOList, onTestRunComplete, onTestCommandComplete, onAllTestRunComplete, onTestRunStarted);
                                }
                                else
                                {
                                    string errorMessage = "INCLUDETESTDOESNOTEXIST:Include test file:" + testObj.TestDetail.TestIncludeToRunFirst + " doesnot exist!";
                                    testObj.TestDetail.ResponseMessage = errorMessage;
                                    _createOutput(testObj, currentTestRunNumber, new List<CommandDTO>(),true);
                                    throw new Exception(errorMessage);
                                }
                            }

                        }



                        int totalIterations = testObj.TestDetail.NumberOfIterations;
                        if (testObj.TestDetail.ExcelDocument.Data.Count > 0)
                        {
                            totalIterations = testObj.TestDetail.ExcelDocument.Data.Count;
                        }

                        if (totalIterations <= 0)
                        {
                            testObj.TestDetail.NumberOfIterations = 1;
                        }

                        testObj.TestDetail.NumberOfIterations = totalIterations;
                        returnValue.TotalIterations = testObj.TestDetail.NumberOfIterations;
                        int rowIndex = 0;
                        onTestRunStarted(testObj.TestDetail);
                        for (int i = 1; i <= testObj.TestDetail.NumberOfIterations; i++)
                        {
                            List<CommandDTO> listCurrentCommands = new List<CommandDTO>();
                            currentTestRunNumber = i;

                            if (!_isTestIncludeRunning)
                            {
                                if (WebDriver == null)
                                {
                                    _setWebDriver(testObj.TestDetail);
                                }
                            }
                            else
                            {

                                if (WebDriver == null)
                                {
                                    _setWebDriver(testObj.TestDetail);
                                }
                            }

                           

                            //start video recording
                            testObj.TestDetail.RecordVideo = _recordVideo(testObj.TestDetail, currentTestRunNumber);
                            

                            //now execute the command for each test object
                            CommandDTO currentCommandExecuted = new CommandDTO();
                            foreach (var cmd in testObj.Commands.Where(c => c.Execute == true).ToList())
                            {
                                cmd.TimeTaken = new TimeTakenDTO();
                                cmd.TimeTaken.StartTime = DateTime.Now;
                                //add div on page to show what test is running
                                if (testObj.TestDetail.ShowTestRunningMessage)
                                {
                                    WebDriverHelpers.CreateDivOnTopOfPage(WebDriver, _getShowMessageRunning("Running test : " + testObj.TestDetail.Name+", cmd:"+ cmd.CommandType.ToString()));
                                }
                               
                                cmd.Value = _applyRandomToValue(cmd.Value, cmd.AppendRandomToValue);
                                if (cmd.ExcelColIndexValue > -1)
                                {
                                    cmd.Value = _getExcelColValue(cmd.Value, rowIndex, cmd.ExcelColIndexValue, testObj.TestDetail.ExcelDocument);
                                }
                                cmd.Value = _getDateNowValue(cmd.Value, cmd.DateNowValue);
                                cmd.Value = _getVariable(cmd.Value);
                                cmd.Value = _getDBVariable(cmd.Value);
                                cmd.IDToClick = _getVariable(cmd.IDToClick);
                                cmd.IDToClick = _getDBVariable(cmd.IDToClick);
                                cmd.ClassNameToClick = _getVariable(cmd.ClassNameToClick);
                                cmd.ClassNameToClick = _getDBVariable(cmd.ClassNameToClick);
                                cmd.XPath = _getVariable(cmd.XPath);
                                cmd.XPath = _getDBVariable(cmd.XPath);
                                if (cmd.SQLObj != null && !string.IsNullOrEmpty(cmd.SQLObj.DBConnectionString))
                                {
                                    cmd.SQLObj.DBConnectionString = _getVariable(cmd.SQLObj.DBConnectionString);
                                    cmd.SQLObj.SqlScriptFullPath = _getVariable(cmd.SQLObj.SqlScriptFullPath);
                                    cmd.SQLObj.VariableContainerName = _getVariable(cmd.SQLObj.VariableContainerName);
                                }

                                var stopWatchCmd = Stopwatch.StartNew();
                                returnValue.TimeTaken = new TimeTakenDTO();
                                returnValue.TimeTaken.StartTime = DateTime.Now;
                                switch (cmd.CommandType)
                                {
                                    case CommandType.Click:
                                        currentCommandExecuted = _executeClick(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;

                                        break;
                                    case CommandType.ClickAnchorTagHref:
                                        currentCommandExecuted = _executeClickAnchorTagHref(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.ClickFromList:
                                        currentCommandExecuted = _executeClickFromList(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.LoadURL:
                                        currentCommandExecuted = _executeLoadURL(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.TypeText:
                                        currentCommandExecuted = _executeTypeText(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.AssertTextExists:
                                        currentCommandExecuted = _executeAssertTextExists(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.ExecuteJS:
                                        currentCommandExecuted = _executeExecuteJS(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.WaitSecond:
                                        currentCommandExecuted = _executeExecuteWaitSecond(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.SelectDropDown:
                                        currentCommandExecuted = _executeExecuteSelectDropDown(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.ExecuteSQL:
                                        currentCommandExecuted = _executeSQL(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.SetVariable:
                                        currentCommandExecuted = _setVariable(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.TypeTextMany:
                                        currentCommandExecuted = _executeTypeTextMany(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.ClickMany:
                                        currentCommandExecuted = _executeClickMany(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.SwitchClick:
                                        currentCommandExecuted = _executeSwitchClick(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.AssertElementsOrder:
                                        currentCommandExecuted = _executeAssertElementsOrderByClass(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.AssertElementValueEquals:
                                        currentCommandExecuted = _executeAssertElementValueEquals(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.ClickDropDown:
                                        currentCommandExecuted = _executeClickDropDown(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                    case CommandType.AssertElementCount:
                                        currentCommandExecuted = _executeAssertElementCount(cmd, ref returnValue);
                                        returnValue.TestRunNumber = currentTestRunNumber;
                                        break;
                                        

                                }
                                cmd.TimeTaken.EndTime = DateTime.Now;
                                returnValue.TimeTaken.EndTime = DateTime.Now;
                                var endMS = stopWatchCmd.ElapsedMilliseconds;
                                returnValue.TimeTaken.MillisecondsTaken = endMS;
                                onTestCommandComplete(currentCommandExecuted);
                                listCurrentCommands.Add(currentCommandExecuted);

                                if (currentCommandExecuted.CommandStatus != CommandResponseStatus.Success)
                                {
                                    if (testObj.TestDetail.RecordVideo.Record == true)
                                    {
                                        _screenRecorderHelper.Dispose();
                                    }

                                    returnValue.ResponseMessage = currentCommandExecuted.Message;
                                    _createOutput(testObj, currentTestRunNumber, listCurrentCommands);
                                    throw new Exception(returnValue.ResponseMessage);

                                }


                            }

                            //stop video recording
                            if (testObj.TestDetail.RecordVideo.Record == true)
                            {
                                _screenRecorderHelper.Dispose();
                            }


                            onTestRunComplete(returnValue);
                            _createOutput(testObj, currentTestRunNumber, listCurrentCommands);
                            rowIndex++;

                            if (!_isTestIncludeRunning)
                            {
                                CleanUp();
                                _variableContainer = new Dictionary<string, Dictionary<string, string>>();
                            }


                        }


                    }
                    catch (Exception ex)
                    {
                        returnValue.ResponseDetail = ex.StackTrace;
                        returnValue.ResponseMessage =  ex.Message +"."+(ex.InnerException?.Message);
                        returnValue.ResponseStatus = ResponseStatus.SystemError;
                        returnValue.TestRunNumber = currentTestRunNumber;
                    }

                    CleanUp();

                    if (returnValue.ResponseStatus != ResponseStatus.Success)
                    {
                        var data = returnValue.CommandsExecuted.FirstOrDefault(i => i.CommandStatus == CommandResponseStatus.Failed);
                        if (data != null)
                        {
                            returnValue.ResponseDetail = "Cmd failed:" + data.CommandType.ToString() + "(" + data.IDToClick + ")" + "." + returnValue.ResponseDetail;
                        }

                    }
                    returnValue.TestDetailDTO.TimeTaken.EndTime = DateTime.Now;
                    returnValue.TestDetailDTO.TimeTaken.MillisecondsTaken = stopWatch.ElapsedMilliseconds;
                    responses.Add(returnValue);
                    returnValue = new TestResponseDTO();
                    returnValue.CommandsExecuted = new List<CommandDTO>();


                    if (_isTestIncludeRunning)
                    {
                        _isTestIncludeRunning = false;
                        /*
                            bool hasAnyFailed = responses.Any(xx => xx.ResponseStatus != ResponseStatus.Success);
                            if (!hasAnyFailed)
                            {

                                ExecuteCommands(this.TestObjectDTO, onTestRunComplete,
                                    onTestCommandComplete, onAllTestRunComplete);
                            }

                         */

                    }
                }
                catch(Exception ex)
                {
                    returnValue.ResponseDetail = ex.StackTrace;
                    returnValue.ResponseMessage = ex.Message;
                    returnValue.ResponseStatus = ResponseStatus.SystemError;
                    returnValue.TestRunNumber = currentTestRunNumber;
                    returnValue.TestDetailDTO.TimeTaken.EndTime = DateTime.Now;
                    returnValue.TestDetailDTO.TimeTaken.MillisecondsTaken = stopWatch.ElapsedMilliseconds;
                    responses.Add(returnValue);
                    returnValue = new TestResponseDTO();
                    returnValue.CommandsExecuted = new List<CommandDTO>();
                }

               

            }


           // _variableContainer = new Dictionary<string, Dictionary<string, string>>();


            return responses;

        }

        private string _getTestIncludeFileName(string testIncludeToRunFirst)
        {
            if (File.Exists(testIncludeToRunFirst))
            {
                return new FileInfo(testIncludeToRunFirst).Name;
            }
            var testIncludeNameArray = testIncludeToRunFirst.Split('\\');
            return testIncludeNameArray.Last();
        }

        private bool _isTestFailed(TestResponseDTO item)
        {
            bool isFailed = item.ResponseStatus != ResponseStatus.Success || item.CommandsExecuted.Count(x => x.CommandStatus != CommandResponseStatus.Success) > 0;
            return isFailed;
        }

        public void CreateFinalOutputOfResults(List<TestResponseDTO> testResponseDTO)
        {
            foreach(var item in testResponseDTO)
            {
                item.IsTestFailed = _isTestFailed(item);
               
                _portableDataStore.AddTestRunResults(item);
            }

            var chartJsFullPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Extlibs\\chartv2.js";
            var testHeaderData =  _portableDataStore.GetChartDataForToday();
            var mainTestDir = Directory.GetParent(_testDirectory).FullName;
            StringBuilder emailBody = new StringBuilder();
            emailBody.AppendLine("<html>");
            emailBody.AppendLine("<head/>");
            emailBody.AppendLine("<style>");
            emailBody.AppendLine(".small{font-family:tahoma;font-size:8pt;color:black}");
            emailBody.AppendLine(".med{font-family:tahoma;font-size:12pt;color:black}");
            emailBody.AppendLine(".lrg{font-family:tahoma;font-size:16pt;color:black}");
            emailBody.AppendLine(".success{font-family:tahoma;color:green;font-weight:bold}");
            emailBody.AppendLine(".failed{font-family:tahoma;color:red;font-weight:bold}");
            emailBody.AppendLine(".table-results{border:solid 1px silver;width:100%}");
            emailBody.AppendLine("</style>");
            emailBody.AppendLine("<script src=\"chartv2.js\"></script>");
            emailBody.AppendLine("<script>");
            emailBody.AppendLine("async function drawCharts(){" + Environment.NewLine);
            emailBody.AppendLine(" await drawChartToday();" + Environment.NewLine);
            emailBody.AppendLine("}" + Environment.NewLine);
            emailBody.AppendLine("async function drawChartToday(){" + Environment.NewLine);
            emailBody.AppendLine(" var ctxToday = document.getElementById('chartToday').getContext('2d');" + Environment.NewLine);
            emailBody.AppendLine("  var barChartData = "+JsonConvert.SerializeObject(testHeaderData) +"; " + Environment.NewLine);
            emailBody.AppendLine("  var chart = new Chart(ctxToday, {" + Environment.NewLine);
            emailBody.AppendLine("    type: 'bar'," + Environment.NewLine);
            emailBody.AppendLine("    data: barChartData," + Environment.NewLine);
            emailBody.AppendLine("   options: {" + Environment.NewLine);
            emailBody.AppendLine("   title: {" + Environment.NewLine);
            emailBody.AppendLine("   display: true," + Environment.NewLine);
            emailBody.AppendLine("   text: 'Tests status for today'" + Environment.NewLine);
            emailBody.AppendLine("  }," + Environment.NewLine);
            emailBody.AppendLine("  tooltips: {" + Environment.NewLine);
            emailBody.AppendLine("  mode: 'index',intersect: false}," + Environment.NewLine);
            emailBody.AppendLine("  responsive: true," + Environment.NewLine);
            emailBody.AppendLine("  scales: {" + Environment.NewLine);
            emailBody.AppendLine("  xAxes: [{" + Environment.NewLine);
            emailBody.AppendLine("    stacked: true}]," + Environment.NewLine);
            emailBody.AppendLine("  yAxes: [{" + Environment.NewLine);
            emailBody.AppendLine("   stacked: true}]," + Environment.NewLine);
            emailBody.AppendLine("   }" + Environment.NewLine);
            emailBody.AppendLine("  }" + Environment.NewLine);
            emailBody.AppendLine("  });" + Environment.NewLine);
            emailBody.AppendLine("}" + Environment.NewLine);
            emailBody.AppendLine("</script>");
            emailBody.AppendLine("</head>");
         
            emailBody.AppendLine("<body onload=\"drawCharts();\">");

            emailBody.AppendLine("<div  style=\"width:75%\"><canvas id=\"chartToday\"></canvas></div>");

            emailBody.AppendLine("<div class='med'>Test results:</div><br/><br/>");
            emailBody.AppendLine("<div class='med'>Date:" + DateTime.Now.ToString("dd MMM yyyy HH:mm:ss") + "</div><br/><br/>");

            emailBody.AppendLine("<table class='table-results'>");
            emailBody.AppendLine("<tr>");
            emailBody.AppendLine("<td class='med'>Test name</td>");
            emailBody.AppendLine("<td class='med'>Status</td>");
            emailBody.AppendLine("<td class='med'>Duration(Secs)</td>");
            emailBody.AppendLine("<td class='med'>Network Sim</td>");
            emailBody.AppendLine("<td class='med'>Results</td>");
            emailBody.AppendLine("</tr>");
            foreach (var item in testResponseDTO)
            {
                bool isFailed = _isTestFailed(item);
                item.IsTestFailed = isFailed;
                string headerClass = (isFailed ? "lrg failed" : "lrg success");
                string failedOrPassed = (isFailed ? "FAILED" : "PASSED");
                string folderLink =  item.TestDetailDTO.ResultsFolder;
                string testName = item.TestDetailDTO.Name;
                bool isTestIncludeError = false;
                if(isFailed && item.ResponseMessage.Contains("INCLUDETESTDOESNOTEXIST"))
                {
                    testName = _getTestIncludeFileName(item.TestDetailDTO.TestIncludeToRunFirst);
                    isTestIncludeError = true;
                }
                emailBody.AppendLine("<tr>");
                emailBody.AppendLine("<td class='med'>" + testName + "</td>");
                emailBody.AppendLine("<td class='med'><span class='" + headerClass + "'>" + failedOrPassed + "</span></td>");
                emailBody.AppendLine("<td class='med'>" + TimeSpan.FromMilliseconds(item.TestDetailDTO.TimeTaken.MillisecondsTaken).TotalSeconds + "</td>");
                emailBody.AppendLine("<td class='med'>Enabled:" + item.TestDetailDTO.SimulateNetworkCondition.Enabled +", DL:"+ item.TestDetailDTO.SimulateNetworkCondition.DownloadSpeed.ToString()+ ", UL:"+ item.TestDetailDTO.SimulateNetworkCondition.IsOffline.ToString()+ ", Latency:"+ item.TestDetailDTO.SimulateNetworkCondition.LatencySeconds.ToString()+ ", Offline:"+ item.TestDetailDTO.SimulateNetworkCondition.DownloadSpeed.ToString()+"</td>");
                emailBody.AppendLine("<td class='med'><a href='file://" + folderLink + "'>results</a></td>");
                emailBody.AppendLine("</tr>");
            }
            emailBody.AppendLine("</table>");

            emailBody.AppendLine("<br/>");
            emailBody.AppendLine("<br/>");
            emailBody.AppendLine("<div class='med'>Thank you</div>");
            emailBody.AppendLine("<br/>");
            emailBody.AppendLine("</body>");
            emailBody.AppendLine("</html>");
            string body = emailBody.ToString();
            string newFolderTestResultsFile = mainTestDir + "\\"+ new DirectoryInfo(_testDirectory).Name + "_Test_Results_" + DateTime.Now.ToString("dd_MMM_yyyy_HH_mm_ss")+".html";
            string chartJsNewFullPath = mainTestDir + "\\chartv2.js";
            if (!File.Exists(chartJsNewFullPath))
            {
                File.Copy(chartJsFullPath, chartJsNewFullPath);
            }
            File.WriteAllText(newFolderTestResultsFile, body);
                
        }

        public void CleanUp()
        {
            try
            {
                _impersonateUser.UndoImpersonation();

                if (WebDriver != null)
                {
                    WebDriver.Quit();
                    WebDriver = null;
                }
            }
            catch
            {

            }

        }

        

        private RecordVideo _recordVideo(TestDetailDTO testObj, int currentTestRunNumber)
        {
            var returnValue = new RecordVideo();
            returnValue = testObj.RecordVideo;
            if (testObj.RecordVideo.Record == true)
            {
                string videoDirectory = testObj.OutputFullFilePath + "\\Video";
                if (!Directory.Exists(videoDirectory))
                {
                    Directory.CreateDirectory(videoDirectory);
                }

                string videoFullFilePath = videoDirectory + "\\No" + currentTestRunNumber + "_" + testObj.Name + "_" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + ".avi";
                testObj.RecordVideo.OutPutFullPath = videoFullFilePath;
                returnValue = testObj.RecordVideo;

                _screenRecorderHelper.RecordScreen(new RecorderParams(videoFullFilePath, 10, SharpAvi.KnownFourCCs.Codecs.Xvid, testObj.RecordVideo.Quality, testObj.RecordVideo.ScreenNumber));

            }

            return returnValue;
        }

        private void _setMinimalWebDriver(AutoTestBrowser browser)
        {
            switch (browser)
            {
                case AutoTestBrowser.chrome:
                    ChromeOptions chromeOptions = new ChromeOptions();

                    var chromeDriverPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + System.Configuration.ConfigurationManager.AppSettings["ChromeDriver"];
                    var chromeDriver = new ChromeDriver(chromeDriverPath, chromeOptions);
                    WebDriver = chromeDriver;


                    break;
                case AutoTestBrowser.ie:

                    InternetExplorerOptions ieOptions = new InternetExplorerOptions();
                    ieOptions.IntroduceInstabilityByIgnoringProtectedModeSettings = true;
                    ieOptions.IgnoreZoomLevel = true;
                    WebDriver = new InternetExplorerDriver(ieOptions);
                    break;
            }
        }

        private void _setWebDriver(TestDetailDTO testDetailDTO)
        {
            //string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //FileInfo fi = new FileInfo(appPath);
            //string dir = fi.Directory.FullName;

            switch (testDetailDTO.Browser)
            {
                case AutoTestBrowser.chrome:
                    ChromeOptions chromeOptions = new ChromeOptions();
                    
                    if (!testDetailDTO.BrowserOptions.ShowBrowser)
                    {
                        chromeOptions.AddArgument("headless");
                    }
                   // ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                   
                    if (!_logDebugInfo)
                    {
                        chromeOptions.AddArgument("--disable-logging");
                        chromeOptions.AddArgument("--log-level=3");
                    }

                    

                     var chromeDriverPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+ "\\" +System.Configuration.ConfigurationManager.AppSettings["ChromeDriver"];
                    
                    var chromeDriver = new ChromeDriver(chromeDriverPath, chromeOptions);
                    if (testDetailDTO.SimulateNetworkCondition.Enabled && testDetailDTO.SimulateNetworkCondition.DownloadSpeed !=DownloadSpeed.Default)
                    {
                        ChromeNetworkConditions chromeNetworkConditions = new ChromeNetworkConditions();
                        chromeNetworkConditions.DownloadThroughput = _setDownloadSpeed(testDetailDTO.SimulateNetworkCondition.DownloadSpeed);
                        chromeNetworkConditions.UploadThroughput = _setUploadSpeed(testDetailDTO.SimulateNetworkCondition.UploadSpeed);
                      
                        var latency = new TimeSpan(0, 0, testDetailDTO.SimulateNetworkCondition.LatencySeconds == 0 ? 1 : testDetailDTO.SimulateNetworkCondition.LatencySeconds);
                        chromeNetworkConditions.Latency = latency;
                        chromeNetworkConditions.IsOffline= testDetailDTO.SimulateNetworkCondition.IsOffline;
                        chromeDriver.NetworkConditions = chromeNetworkConditions;
                        
                    }
                   
                    WebDriver = chromeDriver;


                    break;
                case AutoTestBrowser.ie:

                    InternetExplorerOptions ieOptions = new InternetExplorerOptions();
                    ieOptions.IntroduceInstabilityByIgnoringProtectedModeSettings = true;
                    ieOptions.IgnoreZoomLevel = true;
                    WebDriver = new InternetExplorerDriver(ieOptions);
                    break;
            }

            if (testDetailDTO.BrowserOptions.Maximized)
            {
                WebDriver.Manage().Window.Maximize();
            }
        }

        private long _setDownloadSpeed(DownloadSpeed downloadSpeed)
        {
            switch (downloadSpeed)
            {
                case DownloadSpeed.Default:
                    return 10000 * 1024;
                case DownloadSpeed.VerySlow:
                    return 128 * 1024;
                case DownloadSpeed.Slow:
                    return 256 * 1024;
                case DownloadSpeed.Fast:
                    return 1024 * 1024;
                case DownloadSpeed.VeryFast:
                    return 5000 * 1024;
            }
            return 10000 * 1024;
        }

        private long _setUploadSpeed(UploadSpeed uploadSpeed)
        {
            
            switch (uploadSpeed)
            {
                case UploadSpeed.Default:
                    return 10000 * 1024;
                case UploadSpeed.VerySlow:
                    return 128 * 1024;
                case UploadSpeed.Slow:
                    return 256 * 1024;
                case UploadSpeed.Fast:
                    return 1024 * 1024;
                case UploadSpeed.VeryFast:
                    return 5000 * 1024;
            }
            return 10000 * 1024;
        }

        private ExcelDocument _parseExcelData(ExcelDocument excelDocument)
        {
            var returnValue = new ExcelDocument();
            returnValue = excelDocument;
            returnValue.Data = new Dictionary<int, List<ExcelColData>>();

            //check if excel document and had data into object
            if (!string.IsNullOrEmpty(excelDocument.ExcelDocumentPath) && excelDocument.Use == true)
            {

                string pathToExcelFile = _getFilePath(excelDocument.ExcelDocumentPath);
                var excel = new ExcelQueryFactory(pathToExcelFile);
                var excelData = (from c in excel.Worksheet(excelDocument.WorkSheetName)
                                 select c);
                excelDocument.Data = new Dictionary<int, List<ExcelColData>>();

                int rowIndex = 0;

                foreach (var data in excelData)
                {
                    List<ExcelColData> listExcelColData = new List<ExcelColData>();
                    int colIndex = 0;
                    foreach (var itemData in data)
                    {
                        ExcelColData excelColData = new ExcelColData();
                        excelColData.ColIndex = colIndex;
                        excelColData.Data = itemData.ToString();
                        listExcelColData.Add(excelColData);
                        colIndex++;
                    }

                    returnValue.Data.Add(rowIndex, listExcelColData);

                    rowIndex++;
                }

            }



            return returnValue;
        }

        private string _getDateNowValue(string value, DateNowValue dateNowValue)
        {
            string returnValue = value;
            if (dateNowValue.Apply == true)
            {
                var dateNow = DateTime.Now;
                if (dateNowValue.DayAdd > 0)
                {
                    dateNow = dateNow.AddDays(dateNowValue.DayAdd);
                }
                string dateNowFormatted = dateNow.ToString();
                if (!string.IsNullOrEmpty(dateNowValue.Format))
                {
                    dateNowFormatted = dateNow.ToString(dateNowValue.Format);
                }
                if (returnValue.Contains("#DateNowValue"))
                {
                    returnValue = returnValue.Replace("#DateNowValue", dateNowFormatted);
                }
                else
                {
                    returnValue = dateNowFormatted;
                }

            }

            return returnValue;
        }
        private string _getExcelColValue(string value, int rowIndex, int colIndex, ExcelDocument excelDocument)
        {
            string returnValue = value;
            if (excelDocument.Data.Count > 0)
            {
                returnValue = excelDocument.Data[rowIndex][colIndex].Data;
            }

            return returnValue;
        }

        private int _getRandomValue(int min,int max)
        {
            Random random = new Random();
            int rnd = random.Next(min, max);
            return rnd;
        }

        private string _applyRandomToValue(string value, AppendRandomToValue appendRandomToValue)
        {
            if (appendRandomToValue.Apply == true)
            {
                int rnd = _getRandomValue(appendRandomToValue.Min, appendRandomToValue.Max);
                return value + "" + rnd;
            }

            return value;
        }

        private void _createOutput(TestObjectDTO testObj, int currentTestRunNumber, List<CommandDTO> listCurrentCommands, bool isIncludeTestError=false)
        {
           

            if (testObj.TestDetail.OutPutFile.CreateOutPut == true)
            {
                string testName = testObj.TestDetail.Name;
                string testDescription = testObj.TestDetail.Description;
                string newFileName = "No" + currentTestRunNumber + "_" + testObj.TestDetail.Name + "_" + DateTime.Now.ToString("dd_MMM_yyyy_HH_mm_ss");
                if (isIncludeTestError)
                {
                    testName = _getTestIncludeFileName(testObj.TestDetail.TestIncludeToRunFirst);
                    testDescription = testName;
                    newFileName = "No" + currentTestRunNumber + "_" + testName + "_" + DateTime.Now.ToString("dd_MMM_yyyy_HH_mm_ss");
                }
                string fullFilePath = testObj.TestDetail.OutputFullFilePath + "\\" + newFileName;
                if (File.Exists(newFileName))
                {
                    File.Delete(newFileName);
                }

                string fullImageFilePath = testObj.TestDetail.OutputFullFilePath + "\\Screenshots";


                StringBuilder sbContent = new StringBuilder();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("*********Test Results**************");
                sb.AppendLine("Date: " + DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"));
                sb.AppendLine("TestName: " + testName);
                sb.AppendLine("Author: " + testObj.TestDetail.Author);
                sb.AppendLine("Iterations: " + testObj.TestDetail.NumberOfIterations);
                sb.AppendLine("Description: " + testDescription);
                string outcome = "SUCCESS";
                bool isSuccess = true;
                if (listCurrentCommands.Any(i => i.CommandStatus != CommandResponseStatus.Success) || isIncludeTestError)
                {
                    outcome = "FAILED";
                    isSuccess = false;
                }
                sb.AppendLine("----------Outcome:" + outcome + "-------------------");
                sb.AppendLine("");
                sb.AppendLine("");
              
                if(listCurrentCommands.Count == 0)
                {
                    sb.AppendLine(testObj.TestDetail.ResponseMessage);
                    sb.AppendLine("");
                }
                else
                {
                    sb.AppendLine("----------Commands-------------------");
                }
                foreach (var cmd in listCurrentCommands)
                {
                    if (cmd.AttributeToClick == null)
                    {
                        cmd.AttributeToClick = new AttributeToClick();
                    }
                    if (cmd.IDToClick == null)
                    {
                        cmd.IDToClick = "";
                    }
                    if (cmd.ClassNameToClick == null)
                    {
                        cmd.ClassNameToClick = "";
                    }
                    if (cmd.Value == null)
                    {
                        cmd.Value = "";
                    }
                    if (cmd.ScreenShot == null)
                    {
                        cmd.ScreenShot = new ScreenShot();
                    }
                    if (cmd.XPath == null)
                    {
                        cmd.XPath = "";
                    }

                    string lineText = cmd.CommandType.ToString() + " [" + cmd.CommandStatus.ToString() + ":" + cmd.Message + "]-> Id:" + cmd.IDToClick + ",ClassName:" + cmd.ClassNameToClick + ",Value:" + cmd.Value + ",AttributeToClick: Name=" + cmd.AttributeToClick.Name + "&Value=" + cmd.AttributeToClick.Value + ",ScreenShot: Take=" + cmd.ScreenShot.Take + "&Value=" + cmd.ScreenShot.Name + ",IndexToClick:" + cmd.IndexToClick + ",ExcelColIndexValue:" + cmd.ExcelColIndexValue + ",DateNowValue: Apply=" + cmd.DateNowValue.Apply + "&DayAdd=" + cmd.DateNowValue.DayAdd + "&Format=" + cmd.DateNowValue.Format+ ",XPath:"+cmd.XPath;
                    lineText = lineText + ", SQLObj.DBConnectionString:" + cmd.SQLObj.DBConnectionString + ", SQLObj.SqlScriptFullPath:" + cmd.SQLObj.SqlScriptFullPath + ", SQLObj.VariableContainerName:" + cmd.SQLObj.VariableContainerName + ", SQLObj.ReplaceVariables:" + (cmd.SQLObj.ReplaceVariables!=null ? String.Concat(cmd.SQLObj.ReplaceVariables.Select(o => o.name + " " + o.value)) : "");
                    lineText = lineText + ", VariablesToSet:" + (cmd.VariablesToSet != null ? String.Concat(cmd.VariablesToSet.Select(o => o.name + " " + o.value)) : "");
                    lineText = lineText + ", FindElementMaxRetries:" + cmd.FindElementMaxRetries;
                    lineText = lineText + ", TextToTypeMany:" + (cmd.TextToTypeMany != null ? String.Concat(cmd.TextToTypeMany.Select(o => "Attr.Name:"+o.AttributeToClick?.Name + " "+ "Attr.Value:" + o.AttributeToClick?.Value+ ",ClassName:" + o.ClassName+ ",NameOrId:" + o.NameOrId+ ",TextToType:" + o.TextToType+ ",Value:" + o.AttributeToClick?.Value + ",Index:" + o.AttributeToClick?.IndexToClick)) : "");
                    lineText = lineText + ", ClickMany:" + (cmd.ClickMany != null ? String.Concat(cmd.ClickMany.Select(o => "Attr.Name:" + o.AttributeToClick?.Name + " " + "Attr.Value:" + o.AttributeToClick?.Value + ",ClassName:" + o.ClassName + ",NameOrId:" + o.NameOrId + ",TextToType:" + o.TextToType + ",Value:" + o.AttributeToClick?.Value + ",Index:" + o.AttributeToClick?.IndexToClick)) : "");
                    lineText = lineText + ", Time Taken:" + TimeSpan.FromMilliseconds(cmd.TimeTaken.MillisecondsTaken).TotalSeconds;
                    sb.AppendLine(lineText);
                    sbContent.AppendLine(lineText);
                    //now save any images
                    if (cmd.ScreenShot.Take == true && cmd.ScreenShot.Img != null)
                    {
                        if (!Directory.Exists(fullImageFilePath))
                        {
                            Directory.CreateDirectory(fullImageFilePath);
                        }

                        string imageFullName = fullImageFilePath + "\\No" + currentTestRunNumber + "_" + cmd.ScreenShot.Name + "_" + DateTime.Now.ToString("dd_MMM_yyyy_HH_mm_ss") + ".png";
                        MemoryStream ms = new MemoryStream(cmd.ScreenShot.Img, 0, cmd.ScreenShot.Img.Length);

                        // Convert byte[] to Image
                        ms.Write(cmd.ScreenShot.Img, 0, cmd.ScreenShot.Img.Length);
                        Image image = Image.FromStream(ms, true);
                        image.Save(imageFullName, System.Drawing.Imaging.ImageFormat.Png);
                        ms = null;
                        image = null;
                    }



                }


                sb.AppendLine("----------End-----------------------");


                string fileText = sb.ToString();
                File.WriteAllText(fullFilePath, fileText);

                //send email
                if (testObj.TestDetail.EmailResults.SendEmail == true)
                {

                    string subject = testObj.TestDetail.EmailResults.Subject;
                    if (string.IsNullOrEmpty(testObj.TestDetail.EmailResults.Subject))
                    {
                        subject = System.Configuration.ConfigurationManager.AppSettings["EmailSubject"];
                    }

                    subject = subject.Replace("#TestName", testName);
                    subject = subject.Replace("#DateNow", DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"));
                    subject = subject.Replace("#TestStatus", outcome);

                    string emailFrom = testObj.TestDetail.EmailResults.FromEmail;
                    if (string.IsNullOrEmpty(testObj.TestDetail.EmailResults.FromEmail))
                    {
                        emailFrom = System.Configuration.ConfigurationManager.AppSettings["EmailFrom"];
                    }

                    StringBuilder emailBody = new StringBuilder();
                    emailBody.AppendLine("<html>");
                    emailBody.AppendLine("<head/>");
                    emailBody.AppendLine("<style>");
                    emailBody.AppendLine(".small{font-size:'8pt',color:black}");
                    emailBody.AppendLine(".med{font-size:'12pt',color:black}");
                    emailBody.AppendLine(".lrg{font-size:'16pt',color:black}");
                    emailBody.AppendLine(".success{color:'green',font-weight:'bold'}");
                    emailBody.AppendLine(".failed{color:'red',font-weight:'bold'}");
                    emailBody.AppendLine("</style>");
                    emailBody.AppendLine("</head>");
                    emailBody.AppendLine("<body>");
                    string headerClass = "lrg success";
                    if (!isSuccess)
                    {
                        headerClass = "lrg failed";
                    }
                    string folderLink = testObj.TestDetail.OutputFullFilePath.Replace(@"\", "/");
                    emailBody.AppendLine("<div class='med'>Date:" + DateTime.Now.ToString("dd MMM yyyy HH:mm:ss") + "</div>");
                    emailBody.AppendLine("<div class='med'>Test Name:" + testObj.TestDetail.Name + "</div>");
                    emailBody.AppendLine("<div class='med'>Test Author:" + testObj.TestDetail.Author + "</div>");
                    emailBody.AppendLine("<div class='med'>Test Description:" + testObj.TestDetail.Description + "</div>");
                    emailBody.AppendLine("<div class='med'>Test Results:can be found <a href='file://" + folderLink + "'>here</a></div>");
                    emailBody.AppendLine("<br/>");
                    emailBody.AppendLine("<div class='" + headerClass + "'>Test run " + outcome + "</div>");
                    emailBody.AppendLine("<br/>");
                    emailBody.AppendLine("<br/>");
                    emailBody.AppendLine("<div class='med'>Commands</div>");
                    emailBody.AppendLine("<br/>");
                    emailBody.AppendLine("<br/>");
                    emailBody.AppendLine(sbContent.ToString());
                    emailBody.AppendLine("<br/>");
                    emailBody.AppendLine("<br/>");
                    emailBody.AppendLine("<div class='med'>Thank you</div>");
                    emailBody.AppendLine("<br/>");
                    emailBody.AppendLine("</body>");
                    emailBody.AppendLine("</html>");
                    string body = emailBody.ToString();

                    _emailHelper.Send(emailFrom, testObj.TestDetail.EmailResults.EmailTo, subject, body);

                }

            }
        }

        private IWebElement _waitForElementExist(RemoteWebDriver driver,By findBy,int secondsTimeOut, int secondsPoll)
        {
           
            WebDriverWait wait = new WebDriverWait(new SystemClock(), driver, new TimeSpan(0, 0, secondsTimeOut), new TimeSpan(0, 0, secondsPoll));

            var elem = wait.Until(ExpectedConditions.ElementExists(findBy));

            return elem;
            
        }
        private ReadOnlyCollection<IWebElement> _waitForElementsExist(RemoteWebDriver driver, By findBy, int secondsTimeOut, int secondsPoll)
        {

            WebDriverWait wait = new WebDriverWait(new SystemClock(), driver, new TimeSpan(0, 0, secondsTimeOut), new TimeSpan(0, 0, secondsPoll));

            var elems = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(findBy)).ToList().AsReadOnly();

            return elems;

        }

        private void _toggleIframe(CommandDTO cmd)
        {
            if (cmd.IFrame != null)
            {
                if (!string.IsNullOrEmpty(cmd.IFrame.IDToClick))
                {
                    WebDriver.SwitchTo().Frame(WebDriver.FindElement(By.Id(cmd.IFrame.IDToClick)));
                }
                if (!string.IsNullOrEmpty(cmd.IFrame.ClassNameToClick))
                {
                    var by = By.CssSelector("[class*='" + cmd.IFrame.ClassNameToClick + "']");
                    WebDriver.SwitchTo().Frame(WebDriver.FindElement(by));
                }
                if (!string.IsNullOrEmpty(cmd.IFrame.CssSelector))
                {
                    var by = By.CssSelector(cmd.IFrame.CssSelector);
                    if(cmd.IFrame.IndexToFind > -1)
                    {
                        var elems = WebDriver.FindElements(by);
                        WebDriver.SwitchTo().Frame(elems[cmd.IFrame.IndexToFind]);
                    }
                    else
                    {
                        WebDriver.SwitchTo().Frame(WebDriver.FindElement(by));
                    }
                    
                }
                if (!string.IsNullOrEmpty(cmd.IFrame.XPath))
                {
                    var by = By.XPath(cmd.IFrame.XPath);
                    if (cmd.IFrame.IndexToFind > -1)
                    {
                        var elems = WebDriver.FindElements(by);
                        WebDriver.SwitchTo().Frame(elems[cmd.IFrame.IndexToFind]);
                    }
                    else
                    {
                        WebDriver.SwitchTo().Frame(WebDriver.FindElement(by));
                    }

                }
                if (cmd.IFrame.AttributeFindBy!=null && !string.IsNullOrEmpty(cmd.IFrame.AttributeFindBy.Name))
                {
                    var byAttr = By.XPath(String.Format("//*[contains(@{0},'{1}')]",
                             cmd.IFrame.AttributeFindBy.Name,
                             cmd.IFrame.AttributeFindBy.Value));
                    if (cmd.IFrame.AttributeFindBy.IndexToClick > -1)
                    {
                        var elems = WebDriver.FindElements(byAttr);
                        WebDriver.SwitchTo().Frame(elems[cmd.IFrame.AttributeFindBy.IndexToClick]);
                    }
                    else
                    {
                        WebDriver.SwitchTo().Frame(WebDriver.FindElement(byAttr));
                    }

                }
            }
            else
            {
                WebDriver.SwitchTo().DefaultContent();
            }
        }

        private IWebElement _findElement(CommandDTO cmd, bool throwError=true)
        {
            int secondsTimeOut = cmd.FindElementMaxRetries;
            int secondsPoll = 2;
            _toggleIframe(cmd);

            if (!string.IsNullOrEmpty(cmd.IDToClick))
            {
                var by = By.Id(cmd.IDToClick);
              
                var elem =  WebDriver.FindElement(By.Id(cmd.IDToClick));
                if (elem == null)
                {
                    elem = _waitForElementExist(WebDriver,by, secondsTimeOut, secondsPoll);
                    if (elem == null)
                    {
                        if (throwError)
                        {
                            throw new Exception("Cannot find element by id :" + cmd.IDToClick);
                        }
                    }
                   
                   
                }
                _moveToElement(elem, cmd);
                return elem;
            }

            if (!string.IsNullOrEmpty(cmd.ClassNameToClick))
            {
                var by = By.CssSelector("[class*='" + cmd.ClassNameToClick + "']");
                if (cmd.ElementIndexToFind > -1)
                {
                   
                    var elemByClassName = WebDriver.FindElements(by);
                    if(elemByClassName.Count == 0)
                    {
                        elemByClassName = _waitForElementsExist(WebDriver, by, secondsTimeOut, secondsPoll);
                    }
                   
                    var elem = elemByClassName.Count == 0 ? null : elemByClassName[cmd.ElementIndexToFind];
                    if (elem == null)
                    {
                        if (throwError)
                        {
                            throw new Exception("Cannot find element by class name :" + cmd.ClassNameToClick);
                        }

                    }
                    _moveToElement(elem, cmd);
                    return elem;
                }
                else
                {
                    var elem = WebDriver.FindElement(by);
                    if (elem == null) {
                        elem = _waitForElementExist(WebDriver, by, secondsTimeOut, secondsPoll);
                    }
                    if (elem == null) {
                        if (throwError)
                        {
                            throw new Exception("Cannot find element by class name :" + cmd.ClassNameToClick);
                        }
                    }
                    _moveToElement(elem, cmd);
                    return elem;
                }
               

                
            }

            if (!string.IsNullOrEmpty(cmd.XPath))
            {
                var by = By.XPath(cmd.XPath);
                var elem = WebDriver.FindElement(by);
                if(elem == null)
                {
                    elem = _waitForElementExist(WebDriver, by, secondsTimeOut, secondsPoll);
                }
                if(elem == null)
                {
                    if (throwError)
                    {
                        throw new Exception("Cannot find element by xpath :" + cmd.XPath);
                    }
                }
                _moveToElement(elem, cmd);
                return elem;
            }

            if (!string.IsNullOrEmpty(cmd.AttributeToClick.Name))
            {
                IWebElement elem = null;
                string errorMessage = "";
                var byAttr = By.XPath(String.Format("//*[contains(@{0},'{1}')]",
                                 cmd.AttributeToClick.Name,
                                 cmd.AttributeToClick.Value));
                try
                {
                    if(cmd.AttributeToClick.IndexToClick > -1)
                    {
                        var elems = _waitForElementsExist(WebDriver, byAttr, secondsTimeOut, secondsPoll);
                        elem = elems[cmd.AttributeToClick.IndexToClick];

                    }
                    else
                    {
                       
                        if (cmd.AttributeToClick.IndexToClickRandom != null && cmd.AttributeToClick.IndexToClickRandom.Min > -1)
                        {
                            var elems = _waitForElementsExist(WebDriver, byAttr, secondsTimeOut, secondsPoll);
                            int rnd = _getRandomValue(cmd.AttributeToClick.IndexToClickRandom.Min, cmd.AttributeToClick.IndexToClickRandom.Max);
                            elem = elems[rnd];
                        }
                        else
                        {
                            elem = _waitForElementExist(WebDriver, byAttr, secondsTimeOut, secondsPoll);
                        }
                    }

                   
                }
                catch(Exception ex)
                {
                    errorMessage = ex.Message;
                    //try like
                    if (throwError)
                    {
                        throw new Exception("Cannot find element by attribute name :" + cmd.AttributeToClick.Name + " : " + cmd.AttributeToClick.Value + ".Error:" + errorMessage);
                    }
                }


               
                if (elem == null)
                {

                    if (throwError)
                    {
                        throw new Exception("Cannot find element by attribute name :" + cmd.AttributeToClick.Name + " : " + cmd.AttributeToClick.Value + ".Error:" + errorMessage);
                    }
                }
                _moveToElement(elem, cmd);

                return elem;
            }

            if (!string.IsNullOrEmpty(cmd.CssSelector))
            {
                var by = By.CssSelector(cmd.CssSelector);
                if (cmd.IndexToClick > -1)
                {
                    IWebElement elem = null;
                   
                    var elems = WebDriver.FindElements(by);
                    if (elems == null || elems.Count == 0)
                    {
                        elems = _waitForElementsExist(WebDriver, by, secondsTimeOut, secondsPoll);
                    }

                    if (elems == null || elems.Count == 0)
                    {
                        if (throwError)
                        {
                            throw new Exception("Cannot find element list to be used by css selector :" + cmd.CssSelector);
                        }
                           
                    }
                    elem = elems[cmd.IndexToClick];
                    _moveToElement(elem, cmd);
                    return elem;
                }
                else
                {
                    var elem = WebDriver.FindElement(by);
                    if (elem == null)
                    {
                        elem = _waitForElementExist(WebDriver, by, secondsTimeOut, secondsPoll);
                    }
                    if (elem == null)
                    {
                        if (throwError)
                        {
                            throw new Exception("Cannot find element by css selector :" + cmd.CssSelector);
                        }
                    }
                    _moveToElement(elem, cmd);
                    return elem;
                }
                
            }

            return null;

        }

        private ReadOnlyCollection<IWebElement> _findElements(CommandDTO cmd)
        {
            int secondsTimeOut = cmd.FindElementMaxRetries;
            int secondsPoll = 2;
            if (!string.IsNullOrEmpty(cmd.IDToClick))
            {
                var by = By.Id(cmd.IDToClick);
                var elem = WebDriver.FindElements(by);
               
                if (elem == null)
                {
                    elem = _waitForElementsExist(WebDriver, by, secondsTimeOut, secondsPoll);
                }
                if (elem == null)
                {
                    throw new Exception("Cannot find element list by id :" + cmd.IDToClick);

                }
                return elem;
            }

            if (!string.IsNullOrEmpty(cmd.ClassNameToClick))
            {
                var by = By.ClassName(cmd.ClassNameToClick);
                var elem = WebDriver.FindElements(by);
                if (elem == null)
                {
                    elem = _waitForElementsExist(WebDriver, by, secondsTimeOut, secondsPoll);
                }

                if (elem == null)
                {
                    throw new Exception("Cannot find element list by class name :" + cmd.ClassNameToClick);

                }


              
                return elem;
            }

            if (!string.IsNullOrEmpty(cmd.CssSelector))
            {
                var by = By.CssSelector(cmd.CssSelector);
                var elems = WebDriver.FindElements(by);
                if (elems == null || elems.Count == 0)
                {
                    elems = _waitForElementsExist(WebDriver, by, secondsTimeOut, secondsPoll);
                }
                if (elems == null || elems.Count == 0)
                {
                    throw new Exception("Cannot find element by css selector :" + cmd.CssSelector);
                }

                return elems;
            }

            if (!string.IsNullOrEmpty(cmd.AssertElementsOrderByAttribute.CssSelector))
            {
                var by = By.CssSelector(cmd.AssertElementsOrderByAttribute.CssSelector);
                var elems = WebDriver.FindElements(by);
                if (elems == null || elems.Count == 0)
                {
                    elems = _waitForElementsExist(WebDriver, by, secondsTimeOut, secondsPoll);
                }
                if (elems == null || elems.Count == 0)
                {
                    throw new Exception("Cannot find element by css selector :" + cmd.AssertElementsOrderByAttribute.CssSelector);
                }
                return elems;
            }

            if (!string.IsNullOrEmpty(cmd.AttributeToClick.Name))
            {

                var byAttr = By.XPath(String.Format("//*[contains(@{0},'{1}')]",
                             cmd.AttributeToClick.Name,
                             cmd.AttributeToClick.Value));
                var elems = WebDriver.FindElements(byAttr);
                if (elems == null || elems.Count == 0)
                {
                    elems = _waitForElementsExist(WebDriver, byAttr, secondsTimeOut, secondsPoll);
                }
                if (elems == null || elems.Count == 0)
                {
                    throw new Exception("Cannot find element by attribute to click :" + cmd.AttributeToClick.Name+", value:"+cmd.AttributeToClick.Value);
                }
                return elems;
            }

            return null;

        }

        private ScreenShot _takeScreenShot(CommandDTO cmdExec)
        {
            var returnValue = new ScreenShot();
            byte[] screenshotAsByteArray = null;
            if (cmdExec.ScreenShot.Take == true)
            {
                Screenshot ss = ((ITakesScreenshot)WebDriver).GetScreenshot();
                screenshotAsByteArray = ss.AsByteArray;


            }
            returnValue.Name = cmdExec.ScreenShot.Name;
            returnValue.Img = screenshotAsByteArray;
            returnValue.Take = cmdExec.ScreenShot.Take;
            return returnValue;
        }

        private CommandDTO _getErrorCmd(CommandDTO cmdExec, Exception ex)
        {
            var returnValue = cmdExec;
            if (cmdExec.OverrideErrorOnNotFound == true)
            {
                returnValue.CommandStatus = CommandResponseStatus.Success;
                returnValue.Message = "Exception overridden";
                return returnValue;
            }
            else
            {
                returnValue.CommandStatus = CommandResponseStatus.Failed;
                returnValue.Message = ex.Message;
                return cmdExec;
            }
        }

        private CommandDTO _executeClick(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {
                var elem = _findElement(cmd);
                cmdExec.ScreenShot = _takeScreenShot(cmdExec);
                elem.Click();
                testResponseDTO.CommandsExecuted.Add(cmdExec);
            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);
               
            }
            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private CommandDTO _executeLoadURL(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {
                cmdExec.ScreenShot = _takeScreenShot(cmdExec);
                WebDriver.Navigate().GoToUrl(cmd.Value);

            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);

            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private CommandDTO _executeTypeText(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {
                var elem = _findElement(cmd);
                elem.Clear();
                elem.Highlight();
                elem.SendKeys(cmd.Value);
                cmdExec.ScreenShot = _takeScreenShot(cmdExec);
            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);

            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private CommandDTO _executeClickAnchorTagHref(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {
                var elem = _findElement(cmd);
                if (elem.GetAttribute("href") != null)
                {
                    var linkHref = elem.GetAttribute("href");
                    cmdExec.ScreenShot = _takeScreenShot(cmdExec);
                    WebDriver.Navigate().GoToUrl(linkHref);

                }
            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);

            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private CommandDTO _executeClickFromList(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {
                var elemList = _findElements(cmd);
                cmdExec.ScreenShot = _takeScreenShot(cmdExec);
                elemList[cmd.IndexToClick].Click();

            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);
            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private CommandDTO _executeAssertTextExists(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {
                cmdExec.ScreenShot = _takeScreenShot(cmdExec);
                IWebElement body = WebDriver.FindElement(By.TagName("body"));
                if (!body.Text.Contains(cmd.Value))
                {
                    cmdExec.CommandStatus = CommandResponseStatus.Failed;
                    cmdExec.Message = "Text(" + cmd.Value + ") does not exist on the page!";
                }



            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);
            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private CommandDTO _executeExecuteJS(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {

                IJavaScriptExecutor js = (IJavaScriptExecutor)WebDriver;
                js.ExecuteScript(cmd.Value);
                cmdExec.ScreenShot = _takeScreenShot(cmdExec);

            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);
            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private CommandDTO _executeExecuteSelectDropDown(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {

                var elem = _findElement(cmd);

                var selectElement = new SelectElement(elem);
                selectElement.SelectByValue(cmd.Value);

                cmdExec.ScreenShot = _takeScreenShot(cmdExec);

            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);
            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private CommandDTO _executeExecuteWaitSecond(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {

                //WebDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Convert.ToInt32(cmd.Value));
                Thread.Sleep(new TimeSpan(0, 0, Convert.ToInt32(cmd.Value)));
                cmdExec.ScreenShot = _takeScreenShot(cmdExec);

            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);
            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private CommandDTO _executeSQL(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {
                string sqlScriptFile = _getFilePath(cmd.SQLObj.SqlScriptFullPath);
                bool isSqlFile = File.Exists(sqlScriptFile);
                string sqlScript = sqlScriptFile;
                if (isSqlFile)
                {
                    sqlScript = File.ReadAllText(sqlScriptFile).Replace("GO", Environment.NewLine);
                }

                foreach (var item in cmd.SQLObj.ReplaceVariables)
                {
                    sqlScript = sqlScript.Replace("@" + item.name, item.value);
                }

                using (SqlConnection connection =
            new SqlConnection(cmd.SQLObj.DBConnectionString))
                {
                    // Create the Command and Parameter objects.
                    SqlCommand command = new SqlCommand(sqlScript, connection);

                    // Open the connection in a try/catch block.
                    // Create and execute the DataReader, writing the result
                    // set to the console window.
                    try
                    {
                        connection.Open();
                       
                        int i = 0;
                        if (_variableContainer.ContainsKey(cmd.SQLObj.VariableContainerName))
                        {
                            throw new Exception("VariableContainerName " + cmd.SQLObj.VariableContainerName + " already exists in the variable container");
                        }
                        else
                        {
                            _variableContainer.Add(cmd.SQLObj.VariableContainerName, new Dictionary<string, string>());
                        }
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            foreach (DataRow row in dt.Rows)
                            {
                                foreach (DataColumn column in dt.Columns)
                                {
                                    var colName = column.ColumnName;
                                    if (!_variableContainer[cmd.SQLObj.VariableContainerName].ContainsKey(colName))
                                    {
                                        _variableContainer[cmd.SQLObj.VariableContainerName].Add(colName, row[column].ToString());
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        cmdExec = _getErrorCmd(cmdExec, ex);
                    }
               
                }


            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);
            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private CommandDTO _setVariable(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {

                if (!_variableContainer.ContainsKey(_systemVariableName))
                {
                    _variableContainer.Add(_systemVariableName, new Dictionary<string, string>());
                }

                foreach(var item in cmd.VariablesToSet)
                {
                    if (_variableContainer[_systemVariableName].ContainsKey(item.name))
                    {
                        _variableContainer[_systemVariableName][item.name] = item.value;
                    }
                    else
                    {
                        _variableContainer[_systemVariableName].Add(item.name, item.value);
                    }

                }

                

            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);
            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }


        private string _getVariable(string variableString)
        {
            string returnValue = variableString;
            string text = variableString;
            if (string.IsNullOrEmpty(text))
            {
                text = "";
            }
            if (!text.ToLower().Contains("varcontainer{"))
            {
                return text;
            }

            MatchCollection matchedValues = _getVariableMatch(text);
            foreach(var item in matchedValues)
            {
                var key = item.ToString().Replace("{","").Replace("}", "");
                var value = _variableContainer[_systemVariableName].FirstOrDefault(x => x.Key == key);
                returnValue = returnValue.Replace("VarContainer{"+value.Key+"}", value.Value);
            }

            return returnValue;
        }

        private string _getDBVariable(string variableString)
        {
            string returnValue = variableString;
            string text = variableString;
            if (string.IsNullOrEmpty(text))
            {
                text="";
            }
            if(!text.ToLower().Contains("varcontainerdb{"))
            {
                return text;
            }

            MatchCollection matchedValues = _getVariableMatch(text);
            foreach (var item in matchedValues)
            {
                var keyArrary = item.ToString().Replace("{", "").Replace("}", "").Split(',');
                var value = _variableContainer[keyArrary[0]].FirstOrDefault(x => x.Key == keyArrary[1]);
                returnValue = returnValue.Replace("VarContainerDB{" + keyArrary[0] + ","+ keyArrary[1] + "}", value.Value);
            }

            return returnValue;
        }

        private MatchCollection _getVariableMatch(string variableString)
        {
            string text = variableString;
            string pattern = @"{(.*?)}";

            Regex rg = new Regex(pattern);
            MatchCollection matchedValues = rg.Matches(text);

            return matchedValues;
        }

        private void _moveToElement(IWebElement elem, CommandDTO cmd)
        {
            if (cmd.MoveToElement)
            {
                WebDriverHelpers.MoveToElement(WebDriver, elem);
            }
           
        }

        private CommandDTO _executeTypeTextMany(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {
                
                foreach(var item in cmd.TextToTypeMany)
                {
                    var cmdTmp = new CommandDTO();
                    cmdTmp.ClassNameToClick = item.ClassName;
                    cmdTmp.AttributeToClick = new AttributeToClick();
                    cmdTmp.AttributeToClick.Name = item.AttributeToClick.Name;
                    cmdTmp.AttributeToClick.Value = item.AttributeToClick.Value;
                    cmdTmp.AttributeToClick.IndexToClick = item.AttributeToClick.IndexToClick;
                    cmdTmp.IDToClick = item.NameOrId;
                    cmdTmp.XPath = item.XPath;
                    cmdTmp.CssSelector = item.CssSelector;
                    var textToType = _getDBVariable(item.TextToType);
                    textToType = _getVariable(textToType);
                    var elem = _findElement(cmdTmp);
                    elem.Highlight();
                    elem.Clear();
                    elem.SendKeys(textToType);
                    if (item.WaitAfterSeconds > 0)
                    {
                        Thread.Sleep(item.WaitAfterSeconds * 1000);
                    }
                }
               
                cmdExec.ScreenShot = _takeScreenShot(cmdExec);
            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);

            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private CommandDTO _executeClickMany(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {

                foreach (var item in cmd.ClickMany)
                {
                    var cmdTmp = new CommandDTO();
                    cmdTmp.ClassNameToClick = item.ClassName;
                    cmdTmp.AttributeToClick = new AttributeToClick();
                    if (item.AttributeToClick == null || string.IsNullOrEmpty(item.AttributeToClick.Name))
                    {
                        cmdTmp.CssSelector = item.CssSelector;
                        cmdTmp.IndexToClick = item.IndexToFind;
                    }
                    else
                    {
                        cmdTmp.AttributeToClick.Name = item.AttributeToClick.Name;
                        cmdTmp.AttributeToClick.Value = item.AttributeToClick.Value;
                        cmdTmp.AttributeToClick.IndexToClick = item.AttributeToClick.IndexToClick;
                        cmdTmp.IDToClick = item.NameOrId;
                        cmdTmp.XPath = item.XPath;
                        cmdTmp.AttributeToClick.IndexToClickRandom = item.AttributeToClick.IndexToClickRandom;
                    }
                    cmdTmp.MoveToElement = cmdExec.MoveToElement;
                    cmdTmp.IFrame = cmdExec.IFrame;
                    cmdTmp.FindElementMaxRetries = cmdExec.FindElementMaxRetries;
                    if (item.AttributeToClick == null) { item.AttributeToClick = new AttributeToClick(); }
                    if (string.IsNullOrEmpty(item.AttributeToClick.TextToFind))
                    {
                        var elem = _findElement(cmdTmp);
                        elem.Highlight();
                        elem.Click();
                    }
                    else
                    {
                        var elems = _findElements(cmdTmp);
                        var elemData = elems.FirstOrDefault(x => x.Text.Trim() == item.AttributeToClick.TextToFind);
                        if (elemData == null)
                        {
                            throw new Exception("Element to find by text '"+ item.AttributeToClick.TextToFind + "' was not found");
                        }
                        else
                        {
                            elemData.Highlight();
                            cmdExec.ScreenShot = _takeScreenShot(cmdExec);
                            elemData.Click();
                        }
                    }
                  
                    if(item.WaitAfterSeconds > 0)
                    {
                        Thread.Sleep(item.WaitAfterSeconds * 1000);
                    }
                }

                
            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);

            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }
      

        private AttributeToClick _getSwitchIfAttribute(string ifCode,ref string method,ref bool isNot)
        {
            ifCode = _getDBVariable(ifCode);
            ifCode = _getVariable(ifCode);
            isNot = ifCode.StartsWith("!");
            var ifCodeArray = ifCode.Replace("!", "").Replace("FindElement", "").Replace("(", "").Replace(")", "").Split(',');
            var attr = new AttributeToClick();
            attr.Name = ifCodeArray[0].Replace("'", "");
            attr.Value = ifCodeArray[1].Replace("'", "");
            attr.IndexToClick = Convert.ToInt32(ifCodeArray[2]);
            if (ifCodeArray.Contains("FindElement"))
            {
                method = "FindElement";
            }
            return attr;
        }

        private AttributeToClick _getSwitchThenOrElseAttribute(string thenCode, ref string method, ref bool isNot,ref List<string> testFiles)
        {
            thenCode = _getDBVariable(thenCode);
            thenCode = _getVariable(thenCode);
            isNot = thenCode.StartsWith("!");
            if (thenCode.Contains("FindElement"))
            {
                method = "FindElement";
            }
            if (thenCode.Contains("RunTest"))
            {
                method = "RunTest";
            }
            var thenCodeArray = thenCode.Replace("!", "").Replace("FindElement", "").Replace("RunTest", "").Replace("(", "").Replace(")", "").Split(',');
            var attr = new AttributeToClick();

            if (method == "RunTest")
            {
                var testFilesArrary = thenCodeArray[0].Replace("'", "").Split(',');
                foreach(var file in testFilesArrary)
                {
                    testFiles.Add(file);
                }
                
               
            }
            else
            {
                attr.Name = thenCodeArray[0].Replace("'", "");
                attr.Value = thenCodeArray[1].Replace("'", "");
                attr.IndexToClick = Convert.ToInt32(thenCodeArray[2]);
            }
            
           
           
            return attr;
        }

        private CommandDTO _executeSwitchClick(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {
                string ifMethod = "";
                bool ifNot = false;
                var ifAttr = _getSwitchIfAttribute(cmd.SwitchClick.If, ref ifMethod, ref ifNot);
                var cmdTmp = new CommandDTO();
                cmdTmp.AttributeToClick = ifAttr;
                var ifElem = _findElement(cmdTmp, false);
                if(ifNot && ifElem == null)
                {
                    string thenMethod = "";
                    bool thenNot = false;
                    List<string> testFiles = new List<string>();
                    var thenAttr = _getSwitchThenOrElseAttribute(cmd.SwitchClick.Then, ref thenMethod, ref thenNot,ref testFiles);
                    cmdTmp = new CommandDTO();
                    cmdTmp.AttributeToClick = thenAttr;
                    var thenElem = _findElement(cmdTmp);
                    var elem = thenElem;
                    elem.Highlight();
                    elem.Click();
                }
                else
                {
                    var elem = ifElem;
                    elem.Highlight();
                    elem.Click();
                }

               

                cmdExec.ScreenShot = _takeScreenShot(cmdExec);

            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);
            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private List<TestObjectDTO> _executeSwitchTest(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {
                string ifMethod = "";
                bool ifNot = false;
                var ifAttr = _getSwitchIfAttribute(cmd.SwitchClick.If, ref ifMethod, ref ifNot);
                var cmdTmp = new CommandDTO();
                cmdTmp.AttributeToClick = ifAttr;
                var ifElem = _findElement(cmdTmp, false);
                if (ifNot && ifElem == null)
                {
                    string thenMethod = "";
                    bool thenNot = false;
                    List<string> testFiles = new List<string>();
                    
                    var thenAttr = _getSwitchThenOrElseAttribute(cmd.SwitchClick.Then, ref thenMethod, ref thenNot,ref testFiles);
                    if(testFiles.Count > 0)
                    {
                        foreach (var file in testFiles)
                        {
                            var testDto = _parseTestFile(file);
                            TestObjectDTO.Add(testDto);
                        }
                        _testStartFileFullPath = "";
                        return TestObjectDTO;
                    }

                    return null;
                   
                }
                else
                {
                    string elseMethod = "";
                    bool elseNot = false;
                    List<string> testFiles = new List<string>();

                    var thenAttr = _getSwitchThenOrElseAttribute(cmd.SwitchClick.Else, ref elseMethod, ref elseNot, ref testFiles);
                    if (testFiles.Count > 0)
                    {
                        foreach (var file in testFiles)
                        {
                            var testDto = _parseTestFile(file);
                            TestObjectDTO.Add(testDto);
                        }

                        return TestObjectDTO;
                    }
                }

            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);
            }

            return null;
        }

        private bool _hasIndex(List<string> listArray,int index)
        {
            try
            {
                var val = listArray[index];
                return true;
            }
            catch
            {
                return false;
            }
        }

        private CommandDTO _executeAssertElementsOrderByClass(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {

                var elems = _findElements(cmd);
                if (cmd.AssertElementsOrderByAttribute.CheckInnerText)
                {
                    var orderCount = 0;
                    foreach (var item in elems)
                    {
                        string innerText = item.Text;
                        if (_hasIndex(cmd.AssertElementsOrderByAttribute.OrderValue, orderCount))
                        {
                            string orderValue = cmd.AssertElementsOrderByAttribute.OrderValue[orderCount];
                            if(orderValue != innerText)
                            {
                                throw new Exception(orderValue+" is in the incorrect order. It should be :"+ innerText);
                            }
                        }
                       
                        orderCount++;
                    }
                }
                else
                {
                    var orderCount = 0;
                    foreach (var item in elems)
                    {
                        string attrValue = item.GetAttribute(cmd.AssertElementsOrderByAttribute.AttributeName);
                        if (_hasIndex(cmd.AssertElementsOrderByAttribute.OrderValue, orderCount))
                        {
                            string orderValue = cmd.AssertElementsOrderByAttribute.OrderValue[orderCount];
                            if (orderValue != attrValue)
                            {
                                throw new Exception(orderValue + " is in the incorrect order. It should be :" + attrValue);
                            }
                        }

                        orderCount++;
                    }
                }

                cmdExec.ScreenShot = _takeScreenShot(cmdExec);

            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);
            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private CommandDTO _executeAssertElementValueEquals(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {

                var cmdTmp = cmd;
                cmdTmp.AttributeToClick.IndexToClick = cmd.AssertElementValueEquals.ElementToFindByAttribute.IndexToClick;
                cmdTmp.AttributeToClick.Name = cmd.AssertElementValueEquals.ElementToFindByAttribute.Name;
                cmdTmp.AttributeToClick.Value = cmd.AssertElementValueEquals.ElementToFindByAttribute.Value;
                cmdTmp.IDToClick = cmd.AssertElementValueEquals.IDToClick;
                cmdTmp.ClassNameToClick = cmd.AssertElementValueEquals.ClassNameToClick;
                cmdTmp.CssSelector = cmd.AssertElementValueEquals.CssSelector;
                var elem = _findElement(cmdTmp);
                string elementValue= "";
                switch (cmd.AssertElementValueEquals.ValueSelector)
                {
                    case ValueSelector.Text:
                        elementValue = elem.Text;
                        break;
                    case ValueSelector.InputValue:
                        elementValue = elem.GetAttribute("value");
                        break;
                }

                cmdExec.ScreenShot = _takeScreenShot(cmdExec);
                elementValue = elementValue.TrimStart().TrimEnd();
                if (elementValue != cmd.AssertElementValueEquals.ExpectedValue)
                {
                    throw new Exception("Failed. Expected value :"+ cmd.AssertElementValueEquals.ExpectedValue + " but actual value was :"+ elementValue+ ". Ensure that your ExpectedValue doesnot have any trailing spaces(begin and end)");
                }


            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);
            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private CommandDTO _executeClickDropDown(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {

                foreach (var item in cmd.ClickDropDown)
                {
                    var cmdTmp = new CommandDTO();
                    cmdTmp.ClassNameToClick = item.ClassName;
                    cmdTmp.AttributeToClick = new AttributeToClick();
                    var index = item.OptionIndexToClick;
                    if (item.AttributeFindBy != null)
                    {
                        if (!string.IsNullOrEmpty(item.AttributeFindBy.Name))
                        {
                            cmdTmp.AttributeToClick.Name = item.AttributeFindBy.Name;
                            cmdTmp.AttributeToClick.Value = item.AttributeFindBy.Value;
                            cmdTmp.AttributeToClick.IndexToClick = item.AttributeFindBy.IndexToClick;
                        }
                    }
                   
                    cmdTmp.CssSelector = item.CssSelector;
                    cmdTmp.IDToClick = item.NameOrId;
                    cmdTmp.XPath = item.XPath;

                    var elem = _findElement(cmdTmp);
                    elem.Highlight();

                    var elemByClassName = elem.FindElements(By.CssSelector("option"));
 
                    elem = elemByClassName[index];
                    elem.Click();
                    if (item.WaitAfterSeconds > 0)
                    {
                        Thread.Sleep(item.WaitAfterSeconds * 1000);
                    }
                }

                cmdExec.ScreenShot = _takeScreenShot(cmdExec);
            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);

            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

        private CommandDTO _executeAssertElementCount(CommandDTO cmd, ref TestResponseDTO testResponseDTO)
        {
            var cmdExec = cmd;
            cmdExec.CommandStatus = CommandResponseStatus.Success;
            cmdExec.Message = "Success";
            try
            {

                var cmdTmp = new CommandDTO();
                cmdTmp.ClassNameToClick = cmdExec.AssertElementCount.ClassNameToClick;
                cmdTmp.AttributeToClick = new AttributeToClick();
                if (cmdExec.AssertElementCount.AttributeFindBy!= null && !string.IsNullOrEmpty(cmdExec.AssertElementCount.AttributeFindBy.Name))
                {
                    cmdTmp.AttributeToClick.Name = cmdExec.AssertElementCount.AttributeFindBy.Name;
                    cmdTmp.AttributeToClick.Value = cmdExec.AssertElementCount.AttributeFindBy.Value;
                    cmdTmp.AttributeToClick.IndexToClick = cmdExec.AssertElementCount.AttributeFindBy.IndexToClick;
                }
                cmdTmp.CssSelector = cmdExec.AssertElementCount.CssSelector;
                cmdTmp.IDToClick = cmdExec.AssertElementCount.IDToClick;
                cmdTmp.XPath = cmdExec.AssertElementCount.XPath;
                cmdExec.ScreenShot = _takeScreenShot(cmdExec);
                var elem = _findElements(cmdTmp);
                if(elem.Count != cmdExec.AssertElementCount.ExpectedCount)
                {
                    throw new Exception("Element count failed, Expected "+ cmdExec.AssertElementCount.ExpectedCount+" but actual was "+ elem.Count);
                }
                
            }
            catch (Exception ex)
            {
                cmdExec = _getErrorCmd(cmdExec, ex);

            }

            testResponseDTO.CommandsExecuted.Add(cmdExec);
            return cmdExec;
        }

       


    }

   
}
