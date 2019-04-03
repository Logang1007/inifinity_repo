
using Infinity.Automation.Lib.Engine;
using Infinity.Automation.Lib.Helpers;
using Infinity.Automation.Lib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Infinity.Automation.Lib.Models.Enums;

namespace mrpFS.AutoTest
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

                int port = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["EmailPortNumber"]);
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

                foreach(var dir in directories)
                {
                    string dirPath = dir;
                    if (previousDirPath != dirPath)
                    {
                        previousDirPath = dirPath;
                        DirectoryInfo di = new DirectoryInfo(dirPath);
                        Console.WriteLine("Running tests in sub-directory :" + di.Name + "...please wait...");
                        
                        screenRecorderHelper = new ScreenRecorderHelper();
                        _commandManager = new CommandManager(dirPath, true, _emailHelper, _impersonateUser, screenRecorderHelper, _onCommandManagerInitComplete);
                        _commandManager.ExecuteCommands(_commandManager.TestObjectDTO, _onTestRunComplete, _onTestCommandComplete, _onAllTestRunComplete);
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
        private static void _onTestRunComplete(TestResponseDTO testResponseDTO)
        {


            Console.WriteLine("Test completed:" + testResponseDTO.TestDetailDTO.Name + " : run number :" + testResponseDTO.TestRunNumber + "." + testResponseDTO.ResponseStatus.ToString() + "");
        }

        private static void _onTestCommandComplete(CommandDTO commandDTO)
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

        private static void _onAllTestRunComplete(TestResponseDTO testResponseDTO)
        {
            try
            {
                if (testResponseDTO.CommandsExecuted.Any(i => i.CommandStatus == CommandResponseStatus.Failed) || testResponseDTO.ResponseStatus != ResponseStatus.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("All Tests completed: FAILED : " + testResponseDTO.TestDetailDTO.Name + " : run number :" + testResponseDTO.TestRunNumber + "." + testResponseDTO.ResponseStatus.ToString() + ":" + testResponseDTO.ResponseMessage);
                    Console.WriteLine("ErrorStack:" + testResponseDTO.ResponseDetail);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("All Tests completed: SUCCESS : " + testResponseDTO.TestDetailDTO.Name + " : run number :" + testResponseDTO.TestRunNumber + "." + testResponseDTO.ResponseStatus.ToString() + ":" + testResponseDTO.ResponseMessage);
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
