
using Infinity.Automation.Lib.Engine;
using Infinity.Automation.Lib.Helpers;
using Infinity.Automation.Lib.Models;
using mrpFS.AutoTest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Infinity.Automation.Lib.Models.Enums;

namespace infinity.AutoTest
{
    static class Program
    {

        internal static class NativeMethods
        {
            [DllImport("kernel32.dll")]
            internal static extern Boolean AllocConsole();
        }

        const Int32 SW_MINIMIZE = 6;

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow([In] IntPtr hWnd, [In] Int32 nCmdShow);


       private static IEmailHelper _emailHelper;
       private static ICommandManager _commandManager;
       private static IScreenRecorderHelper screenRecorderHelper;
        private static IImpersonateUser _impersonateUser;
        private static bool _isDebugInfo = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] arguments)
        {
           // arguments = new string[1] { @"C:\Dev\mrpAutomationTests" };
            
            if (arguments.Count() > 0)
            {
                NativeMethods.AllocConsole();

                if(arguments.Count() > 1)
                {
                    _isDebugInfo = arguments[0].ToString().ToLower()=="true" || arguments[0].ToString().ToLower() == "1" ? true : false;
                }
                    
                int port = System.Configuration.ConfigurationManager.AppSettings["EmailPortNumber"].ToString() == "" ? 0 : Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["EmailPortNumber"]);
                string smtp = System.Configuration.ConfigurationManager.AppSettings["EmailSMTP"];
                _emailHelper = new EmailHelper(smtp, port);

                _impersonateUser = new ImpersonateUser();

                string folderPath = arguments[0];
                Console.WriteLine("******************Infinity.Automation Tester v1.0.0*******************");
                Console.WriteLine("Started :" + DateTime.Now);
                Console.WriteLine("Checking tests in directory :" + folderPath + "...please wait...");
                Console.WriteLine("");

                MinimizeConsoleWindow();


                string path = folderPath;
                string previousDirPath = "";
                var directories = Directory.GetDirectories(path);
                var responses = new List<TestResponseDTO>();
                var dirToUse = new List<string>();
                foreach (var dir in directories)
                {
                    if (new DirectoryInfo(dir).Name.ToLower().StartsWith("results_"))
                    {
                        continue;
                    }
                    dirToUse.Add(dir);
                }

                if (dirToUse.Count == 0)
                {
                    dirToUse = new List<string>();
                    dirToUse.Add(path);
                }

                IPortableDataStore portableDataStore = new PortableDataStore();
                var portableDBPath = System.Configuration.ConfigurationManager.AppSettings["PortableDB"].ToString();
                if (portableDBPath.StartsWith("\\"))
                {
                    portableDBPath = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName + portableDBPath;
                }
                portableDataStore.DBFullPath = portableDBPath;
                portableDataStore.EnablePortablDB = System.Configuration.ConfigurationManager.AppSettings["EnablePortableDB"].ToString().ToLower()=="true";

                foreach (var dir in dirToUse)
                {
                    string dirPath = dir;
                   
                    if (previousDirPath != dirPath)
                    {
                        previousDirPath = dirPath;
                        DirectoryInfo di = new DirectoryInfo(dirPath);
                        Console.WriteLine("Running tests in sub-directory :" + di.Name + "...please wait...");
                        
                        //screenRecorderHelper = new ScreenRecorderHelper();
                        screenRecorderHelper = new ScreenRecorderHelperNew();
                        string showMessagePrefix = System.Configuration.ConfigurationManager.AppSettings["ShowMessageTestRunningPrefix"].ToString();
                        _commandManager = new CommandManager(dirPath, true, _emailHelper, _impersonateUser, screenRecorderHelper, _onCommandManagerInitComplete,"*.tst", _isDebugInfo, showMessagePrefix, portableDataStore);

                        responses =(_commandManager.ExecuteCommands(_commandManager.TestObjectDTO, _onTestRunComplete, _onTestCommandComplete, _onAllTestRunComplete, _onTestRunStarted));
                        if (!_isDebugInfo)
                        {
                            Console.WriteLine("Test completed :" + DateTime.Now);
                        }
                        _commandManager.CreateFinalOutputOfResults(responses);
                        _onAllTestRunComplete(responses);
                        
                    }

                }

               

                Console.ReadLine();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());

            }
                
        }

        private static void MinimizeConsoleWindow()
        {
            IntPtr hWndConsole = GetConsoleWindow();
            ShowWindow(hWndConsole, SW_MINIMIZE);
        }

        private static void _onCommandManagerInitComplete(ResponseStatus responseStatus, string message)
        {
            if (responseStatus == ResponseStatus.Failed)
            {
                Console.ForegroundColor = ConsoleColor.Red;

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.WriteLine("Initialize test command manager:" + responseStatus.ToString() + ":" + message);
        }

        private static void _onTestRunStarted(TestDetailDTO testDetail)
        {

            Console.WriteLine("Test Started:" + testDetail.Name + "...."+DateTime.Now);
        }
        private static void _onTestRunComplete(TestResponseDTO testResponseDTO)
        {


            Console.WriteLine("Test completed:" + testResponseDTO.TestDetailDTO.Name + " : run number :" + testResponseDTO.TestRunNumber + "." + testResponseDTO.ResponseStatus.ToString() + "...." + DateTime.Now);
        }

        private static void _onTestCommandComplete(CommandDTO commandDTO)
        {
            if (_isDebugInfo)
            {
                if (commandDTO.CommandStatus == CommandResponseStatus.Failed)
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                Console.WriteLine("Command completed:" + commandDTO.CommandType.ToString() + "- ID: " + commandDTO.IDToClick.ToString() + "- Class: " + commandDTO.ClassNameToClick.ToString() + "- Value: " + commandDTO.Value + ": " + commandDTO.CommandStatus.ToString());
            }
            
        }

        private static void _onAllTestRunComplete(List<TestResponseDTO> testResponseDTO)
        {
            try
            {
                foreach(var item in testResponseDTO)
                {
                    if (item.CommandsExecuted.Any(i => i.CommandStatus == CommandResponseStatus.Failed) || item.ResponseStatus != ResponseStatus.Success)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Tests completed: FAILED : " + item.TestDetailDTO.Name + " : run number :" + item.TestRunNumber + "." + item.ResponseStatus.ToString() + ":" + item.ResponseMessage);
                        Console.WriteLine("ErrorStack:" + item.ResponseDetail);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Tests completed: SUCCESS : " + item.TestDetailDTO.Name + " : run number :" + item.TestRunNumber + "." + item.ResponseStatus.ToString() + ":" + item.ResponseMessage);
                    }
                }
                
                


            }
            catch (Exception ex)
            {
                Console.WriteLine("All Tests completed system error:" + ex.Message);
            }


            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Finished :" + DateTime.Now);
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("Press any key to exit");

            //end
            Console.ReadLine();
        }
    }
}
