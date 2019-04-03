using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Automation.Lib.Helpers
{
    public interface IImpersonateUser
    {
        void Impersonate(string userName, string password);
        void UndoImpersonation();
    }
}
