using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Automation.Lib.Helpers
{
    public interface IEmailHelper
    {
        void Send(string fromEmailAddress, List<string> toEmailAddress, string subject, string mailMessage, List<EmailAttachment> emailAttachment = null);
        Task SendAsync(string fromEmailAddress, List<string> toEmailAddress, string subject, string mailMessage, List<EmailAttachment> emailAttachment = null);
    }

    public class EmailAttachment
    {
        public string FileName { get; set; }
        public byte[] File { get; set; }
    }
}
