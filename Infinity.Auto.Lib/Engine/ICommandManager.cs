
using Infinity.Automation.Lib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Infinity.Automation.Lib.Models.Enums;

namespace Infinity.Automation.Lib.Engine
{

    public delegate void OnCommandManagerInitComplete(ResponseStatus responseStatus,string message);
    public delegate void OnTestRunStarted(TestDetailDTO testDetail);
    public delegate void OnTestRunComplete(TestResponseDTO testResponseDTO);
    public delegate void OnTestCommandComplete(CommandDTO commandDTO);
    public delegate void OnAllTestRunComplete(List<TestResponseDTO> commandDTO);
    public interface ICommandManager
    {
        List<TestObjectDTO> TestObjectDTO { get; set; }
        List<TestResponseDTO> ExecuteCommands(List<TestObjectDTO> testObjectDTO, OnTestRunComplete onTestRunComplete, OnTestCommandComplete onTestCommandComplete, OnAllTestRunComplete onAllTestRunComplete, OnTestRunStarted onTestRunStarted);
        void CleanUp();
        void CreateFinalOutputOfResults(List<TestResponseDTO> testResponseDTO);
    }

  
}
