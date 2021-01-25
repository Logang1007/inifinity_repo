using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Automation.Lib.Helpers
{
    public static class HelperExtensions
    {
        public static void Highlight(this IWebElement context,string color="yellow")
        {
            var rc = (RemoteWebElement)context;
            if (rc != null)
            {
                var jsDriver = (IJavaScriptExecutor)rc.WrappedDriver;
                var element = rc;// some element you find;
                string highlightJavascript = @"arguments[0].style.cssText = 'border-width: 2px; border-style: solid; border-color: " + color + "';";
                jsDriver.ExecuteScript(highlightJavascript, new object[] { element });
            }
           
        }

    }

    public class WebDriverHelpers
    {
        public static void CreateDivOnTopOfPage(RemoteWebDriver driver, string text, string bgColor = "yellow")
        {
            var jsDriver = (IJavaScriptExecutor)driver;
            StringBuilder jsScript = new StringBuilder();
            jsScript.AppendLine("var targt = window;var div = document.createElement('div');");
            jsScript.AppendLine("div.innerHTML =\"<span> "+ text + "</span>\";");
            jsScript.AppendLine("div.id='selDivMessageContainer';");
            jsScript.AppendLine("if(!document.getElementById(\"selDivMessageContainer\")){targt.document.body.append(div)};");
            jsScript.AppendLine("document.getElementById(\"selDivMessageContainer\").style.backgroundColor='"+ bgColor + "';");
            jsScript.AppendLine("document.getElementById(\"selDivMessageContainer\").style.position='fixed';");
            jsScript.AppendLine("document.getElementById(\"selDivMessageContainer\").style.top=0;");
            jsScript.AppendLine("document.getElementById(\"selDivMessageContainer\").style.width='100%';");
            jsScript.AppendLine("document.getElementById(\"selDivMessageContainer\").style.zIndex='100000000000';");
            jsScript.AppendLine("document.getElementById(\"selDivMessageContainer\").style.height='50px';");
            jsScript.AppendLine("document.getElementById(\"selDivMessageContainer\").style.textAlign='center';");
            jsDriver.ExecuteScript(jsScript.ToString());
        }

        public static void MoveToElement(RemoteWebDriver driver, IWebElement elem)
        {
            var jsDriver = (IJavaScriptExecutor)driver;
            var js = String.Format("window.scrollTo({0}, {1})", elem.Location.X, elem.Location.Y);
            jsDriver.ExecuteScript(js);
        }
    }
}
