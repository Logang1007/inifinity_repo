using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Automation.Lib.Helpers
{
    public class ImpersonateUser: IImpersonateUser
    {
        private WindowsImpersonationContext _context=null;
        public void Impersonate(string userName,string password)
        {
            WindowsIdentity idnt = new WindowsIdentity(userName, password);
            _context = idnt.Impersonate();
        }

        public void UndoImpersonation()
        {
            if (_context != null)
            {
                _context.Undo();
            }
            
        }
    }
}
