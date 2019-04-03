
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
    public delegate void OnTestRunComplete(TestResponseDTO testResponseDTO);
    public delegate void OnTestCommandComplete(CommandDTO commandDTO);
    public delegate void OnAllTestRunComplete(TestResponseDTO commandDTO);
    public interface ICommandManager
    {
        List<TestObjectDTO> TestObjectDTO { get; set; }
        TestResponseDTO ExecuteCommands(List<TestObjectDTO> testObjectDTO, OnTestRunComplete onTestRunComplete, OnTestCommandComplete onTestCommandComplete, OnAllTestRunComplete onAllTestRunComplete);
        void CleanUp();
    }
}
