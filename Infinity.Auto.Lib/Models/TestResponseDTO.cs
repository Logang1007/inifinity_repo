using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Infinity.Automation.Lib.Models.Enums;

namespace Infinity.Automation.Lib.Models
{
    public class TestResponseDTO
    {
        public ResponseStatus ResponseStatus { get; set; }

        public string ResponseMessage { get; set; }
        public string ResponseDetail { get; set; }

        public int TestRunNumber { get; set; }

        public TestDetailDTO TestDetailDTO { get; set; }

        public List<CommandDTO> CommandsExecuted { get; set; }

        public TestResponseDTO()
        {
            ResponseStatus = ResponseStatus.Success;
            ResponseMessage = "Success";
        }
    }

    


}
