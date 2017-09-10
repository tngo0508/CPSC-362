using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
namespace App.TechPedia.Common
{
    public static class CommonFunctions
    {
        public static XElement CurrentLoggedInUser;

        public static string GetAppPath()
        {
            return System.Windows.Forms.Application.StartupPath;
        }

        public static IProxy GetProxy(XElement configuration)
        {
            string proxyAssemblyName = Path.Combine(CommonFunctions.GetAppPath(), configuration.GetAttribute("proxyassemblyname"));
            string proxyClassName = configuration.GetAttribute("proxyclassname");

            Assembly assembly = Assembly.LoadFile(proxyAssemblyName);

            Type classType = assembly.GetTypes().FirstOrDefault(pred => pred.Name.Trim().ToUpper() == proxyClassName.Trim().ToUpper());

            IProxy proxyBase = Activator.CreateInstance(classType) as IProxy;

            proxyBase.SetConfiguration(configuration);

            return proxyBase;
        }

        public static string GetCaption()
        {
            return "TechPedia";
        }

        public static void SetFormInfo(Form form)
        {
            Icon icon = Icon.ExtractAssociatedIcon(Path.Combine(GetAppPath(), "search.ico"));
            form.Icon = icon;

            if(CurrentLoggedInUser != null)
            {
                form.Text += " [" + CurrentLoggedInUser.GetAttribute("firstname") + ", " + CurrentLoggedInUser.GetAttribute("lastname") + "]";
            }
        }
    }

    public static class ExtensionMethods
    {
        public static string GetAttribute(this XElement element, string attributeName)
        {
            if (element == null || string.IsNullOrEmpty(attributeName))
                return string.Empty;

            XAttribute attribute = element.Attribute(attributeName);
            if (attribute != null)
                return attribute.Value;

            return string.Empty;
        }
    }
}
