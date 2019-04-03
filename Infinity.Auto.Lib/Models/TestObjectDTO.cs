using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Automation.Lib.Models
{
    public class TestObjectDTO
    {
        public TestDetailDTO TestDetail { get; set; }
        public List<CommandDTO> Commands { get; set; }
    }
}
