using Infinity.Automation.Lib.Helpers;
using Infinity.Automation.Lib.Models;
using LinqToExcel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Infinity.Automation.Lib.Models.Enums;

namespace Infinity.Automation.Lib.Engine
{
    public class CommandManager : ICommandManager
    {



        public List<TestObjectDTO> TestObjectDTO { get; set; }
        public IWebDriver WebDriver;
        private IEmailHelper _emailHelper;
        private IScreenRecorderHelper _screenRecorderHelper;
        private bool _isTestIncludeRunning = false;
        private string _lastTestIncludeRan = "";
        private IImpersonateUser _impersonateUser;
        public CommandManager(string path, bool isDirectory, IEmailHelper emailHelper, IImpersonateUser impersonateUser, IScreenRecorderHelper screenRecorderHelper, OnCommandManagerInitComplete onCommandManagerInitComplete, string testExtFilter = "*.tst")
        {
            _emailHelper = emailHelper;
            _impersonateUser = impersonateUser;
            _screenRecorderHelper = screenRecorderHelper;
            TestObjectDTO = new List<TestObjectDTO>();

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
                            var testObjectDTO = _parseTestFile(item);
                            TestObjectDTO.Add(testObjectDTO);
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
                        onCommandManagerInitComplete(ResponseStatus.Success, "Success");

                        var testObjectDTO = _parseTestFile(path);
                        TestObjectDTO.Add(testObjectDTO);
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

        private TestObjectDTO _parseTestFile(string fulltestFilePath)
        {

            var fi = new FileInfo(fulltestFilePath);
            var fileText = File.ReadAllText(fulltestFilePath);
            if (!string.IsNullOrEmpty(fileText))
            {   
                var testObjectDTO = JsonConvert.DeserializeObject<TestObjectDTO>(fileText);
                //string testStr = JsonConvert.SerializeObject(testObjectDTO);
               // string jsonFormatted = JValue.Parse(testStr).ToString(Formatting.Indented);

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
                    output = fi.Directory.FullName + "\\" + Path.GetFileNameWithoutExtension(fi.Name) + "_" + DateTime.Now.ToString(format);

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
                return testObjectDTO;
            }
            else
            {
                return new TestObjectDTO();
            }



        }

       

        public TestResponseDTO ExecuteCommands(List<TestObjectDTO> testObjectDTO, OnTestRunComplete onTestRunComplete,
            OnTestCommandComplete onTestCommandComplete, OnAllTestRunComplete onAllTestRunComplete)
        {
            TestResponseDTO returnValue = new TestResponseDTO();
            returnValue.CommandsExecuted = new List<CommandDTO>();
            int currentTestRunNumber = 1;


            try
            {
                

                //init the browser object
                foreach (var testObj in testObjectDTO)
                {
                    returnValue.TestDetailDTO = new TestDetailDTO();
                    returnValue.TestDetailDTO = testObj.TestDetail;

                    //impersonate windows user
                    if (returnValue.TestDetailDTO.ImpersonateUser.Apply == true)
                    {
                        _impersonateUser.Impersonate(returnValue.TestDetailDTO.ImpersonateUser.UserName, returnValue.TestDetailDTO.ImpersonateUser.Password);
                    }

                    if (testObj.TestDetail.TestIncludeToRunFirst != _lastTestIncludeRan)
                    {

                        if (!string.IsNullOrEmpty(testObj.TestDetail.TestIncludeToRunFirst))
                        {
                            if (File.Exists(testObj.TestDetail.TestIncludeToRunFirst))
                            {
                                _lastTestIncludeRan = testObj.TestDetail.TestIncludeToRunFirst;
                                var testInclObjectDTO = _parseTestFile(testObj.TestDetail.TestIncludeToRunFirst);
                                List<TestObjectDTO> testInclObjectDTOList = new List<TestObjectDTO>();
                                testInclObjectDTOList.Add(testInclObjectDTO);
                                _isTestIncludeRunning = true;
                                ExecuteCommands(testInclObjectDTOList, onTestRunComplete, onTestCommandComplete, onAllTestRunComplete);
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
                    int rowIndex = 0;
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

                            cmd.Value = _applyRandomToValue(cmd.Value, cmd.AppendRandomToValue);
                            if (cmd.ExcelColIndexValue > -1)
                            {
                                cmd.Value = _getExcelColValue(cmd.Value, rowIndex, cmd.ExcelColIndexValue, testObj.TestDetail.ExcelDocument);
                            }
                            cmd.Value = _getDateNowValue(cmd.Value, cmd.DateNowValue);

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
                            }

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
                        }


                    }

                    

                }
            }
            catch (Exception ex)
            {
                
                returnValue.ResponseDetail = ex.StackTrace;
                returnValue.ResponseMessage = ex.Message;
                returnValue.ResponseStatus = ResponseStatus.SystemError;
                returnValue.TestRunNumber = currentTestRunNumber;
                CleanUp();
            }


            if (returnValue.ResponseStatus != ResponseStatus.Success)
            {
                var data = returnValue.CommandsExecuted.FirstOrDefault(i => i.CommandStatus == CommandResponseStatus.Failed);
                if (data != null)
                {
                    returnValue.ResponseDetail = "Cmd failed:" + data.CommandType.ToString() + "(" + data.IDToClick + ")" + "." + returnValue.ResponseDetail;
                }

            }



            if (_isTestIncludeRunning)
            {
                _isTestIncludeRunning = false;
                if (returnValue.ResponseStatus == ResponseStatus.Success)
                {
                    ExecuteCommands(this.TestObjectDTO, onTestRunComplete,
                        onTestCommandComplete, onAllTestRunComplete);
                }
            }
            else
            {
                onAllTestRunComplete(returnValue);
            }


            return returnValue;

        }

        public void CleanUp()
        {
            try
            {
                _impersonateUser.UndoImpersonation();

                if (WebDriver != null)
                {
                    WebDriver.Quit();
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

                _screenRecorderHelper.RecordScreen(new RecorderParams(videoFullFilePath, 10, SharpAvi.KnownFourCCs.Codecs.Xvid, 50, testObj.RecordVideo.ScreenNumber));

            }

            return returnValue;
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

                    WebDriver = new ChromeDriver(chromeOptions);
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

        private ExcelDocument _parseExcelData(ExcelDocument excelDocument)
        {
            var returnValue = new ExcelDocument();
            returnValue = excelDocument;
            returnValue.Data = new Dictionary<int, List<ExcelColData>>();

            //check if excel document and had data into object
            if (!string.IsNullOrEmpty(excelDocument.ExcelDocumentPath) && excelDocument.Use == true)
            {

                string pathToExcelFile = excelDocument.ExcelDocumentPath;
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

        private string _applyRandomToValue(string value, AppendRandomToValue appendRandomToValue)
        {
            if (appendRandomToValue.Apply == true)
            {
                Random random = new Random();
                int rnd = random.Next(appendRandomToValue.Min, appendRandomToValue.Max);
                return value + "" + rnd;
            }

            return value;
        }

        private void _createOutput(TestObjectDTO testObj, int currentTestRunNumber, List<CommandDTO> listCurrentCommands)
        {

            if (testObj.TestDetail.OutPutFile.CreateOutPut == true)
            {
                string newFileName = "No" + currentTestRunNumber + "_" + testObj.TestDetail + "_" + DateTime.Now.ToString("dd_MMM_yyyy_HH_mm_ss");
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
                sb.AppendLine("TestName: " + testObj.TestDetail.Name);
                sb.AppendLine("Author: " + testObj.TestDetail.Author);
                sb.AppendLine("Iterations: " + testObj.TestDetail.NumberOfIterations);
                sb.AppendLine("Description: " + testObj.TestDetail.Description);
                string outcome = "SUCCESS";
                bool isSuccess = true;
                if (listCurrentCommands.Any(i => i.CommandStatus != CommandResponseStatus.Success))
                {
                    outcome = "FAILED";
                    isSuccess = false;
                }
                sb.AppendLine("----------Outcome:" + outcome + "-------------------");
                sb.AppendLine("");
                sb.AppendLine("");
                sb.AppendLine("----------Commands-------------------");
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

                    subject = subject.Replace("#TestName", testObj.TestDetail.Name);
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

        private IWebElement _findElement(CommandDTO cmd)
        {

            if (!string.IsNullOrEmpty(cmd.IDToClick))
            {
                var elem = WebDriver.FindElement(By.Id(cmd.IDToClick));
                if (elem == null)
                {
                    throw new Exception("Cannot find element by id :" + cmd.IDToClick);
                }

                return elem;
            }

            if (!string.IsNullOrEmpty(cmd.ClassNameToClick))
            {
                var elem = WebDriver.FindElement(By.ClassName(cmd.ClassNameToClick));
                if (elem == null)
                {
                    throw new Exception("Cannot find element by class name :" + cmd.ClassNameToClick);
                }

                return elem;
            }

            if (!string.IsNullOrEmpty(cmd.XPath))
            {
                var elem = WebDriver.FindElement(By.XPath(cmd.XPath));
                if (elem == null)
                {
                    throw new Exception("Cannot find element by class name :" + cmd.ClassNameToClick);
                }

                return elem;
            }

            return null;

        }

        private ReadOnlyCollection<IWebElement> _findElements(CommandDTO cmd)
        {
            if (!string.IsNullOrEmpty(cmd.IDToClick))
            {
                var elem = WebDriver.FindElements(By.Id(cmd.IDToClick));
                if (elem == null)
                {
                    throw new Exception("Cannot find element list by id :" + cmd.IDToClick);
                }

                return elem;
            }

            if (!string.IsNullOrEmpty(cmd.ClassNameToClick))
            {
                var elem = WebDriver.FindElements(By.ClassName(cmd.ClassNameToClick));
                if (elem == null)
                {
                    throw new Exception("Cannot find element list by class name :" + cmd.ClassNameToClick);
                }

                return elem;
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

    }
}
