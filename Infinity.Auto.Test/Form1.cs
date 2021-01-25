

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mrpFS.AutoTest
{
    public partial class Form1 : Form
    {
        //public ICommandManager _commandManager;
        //private IEmailHelper _emailHelper;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.listBox1.Items.Clear();
            string path = @"C:\Dev\mrpAutomationTests";
            int port = System.Configuration.ConfigurationManager.AppSettings["EmailPortNumber"].ToString() =="" ? 0 :  Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["EmailPortNumber"]);
            string smtp = System.Configuration.ConfigurationManager.AppSettings["EmailSMTP"];
            //_emailHelper = new EmailHelper(smtp, port);
           // _commandManager = new CommandManager(path, true, _emailHelper, _onCommandManagerInitComplete);
           // _commandManager.ExecuteCommands(_onTestRunComplete, _onTestCommandComplete, _onAllTestRunComplete);


        }

        //private void _onCommandManagerInitComplete(ResponseStatus responseStatus, string message)
        //{

        //}
        //private void _onTestRunComplete(TestResponseDTO testResponseDTO)
        //{
        

        //    this.listBox1.Items.Add("Test completed:" + testResponseDTO.TestDetailDTO.Name + " : run number :" + testResponseDTO.TestRunNumber+"."+ testResponseDTO.ResponseStatus.ToString()+"");
        //}

        //private void _onTestCommandComplete(CommandDTO commandDTO)
        //{
        //    this.listBox1.Items.Add(commandDTO.CommandType.ToString() + " : " + commandDTO.Value + ": " + commandDTO.CommandStatus.ToString());
        //}

        //private void _onAllTestRunComplete(TestResponseDTO testResponseDTO)
        //{
        //    this.listBox1.Items.Add("All Tests completed:" + testResponseDTO.TestDetailDTO.Name + " : run number :" + testResponseDTO.TestRunNumber + "." + testResponseDTO.ResponseStatus.ToString() + ":"+ testResponseDTO.ResponseMessage);
        //}
    }
}
