using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Automation.Lib.Helpers
{
    public interface IScreenRecorderHelper
    {
        void RecordScreen(RecorderParams recorderParams);
        void Dispose();
    }
}
