using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml.Linq;
using System.IO;
using App.TechPedia.Common;
namespace Proxy.Base
{
    public class ProxyBase : IProxy
    {
        public object ServerComponent;
        public XElement WebServiceConfiguration;

        public virtual void SetConfiguration(XElement webServiceConfiguration)
        {
            WebServiceConfiguration = webServiceConfiguration;
            CreateServerComponent();
        }

        public virtual IEnumerable<XElement> SearchItem(XElement parameters)
        {
            if (ServerComponent == null)
                return null;

            object item = ServerComponent.GetType().InvokeMember("SearchItem", BindingFlags.InvokeMethod, null, ServerComponent, new object[] { parameters });

            return item as IEnumerable<XElement>;
        }

        public virtual XElement GetItem(string itemID)
        {
            if (ServerComponent == null)
                return null;

            object item = ServerComponent.GetType().InvokeMember("GetItem", BindingFlags.InvokeMethod, null, ServerComponent, new object[] { itemID });

            return item as XElement;
        }

        public virtual XElement GetOrderList()
        {
            if (ServerComponent == null)
                return null;

            object item = ServerComponent.GetType().InvokeMember("GetOrderList", BindingFlags.InvokeMethod, null, ServerComponent, null);

            return item as XElement;
        }

        public virtual XElement GetOrder(string orderID)
        {
            if (ServerComponent == null)
                return null;

            object item = ServerComponent.GetType().InvokeMember("GetOrder", BindingFlags.InvokeMethod, null, ServerComponent, new object[] { orderID });

            return item as XElement;    
        }

        public virtual XElement PlaceOrder(XElement parameters)
        {
            if (ServerComponent == null)
                return null;

            object item = ServerComponent.GetType().InvokeMember("PlaceOrder", BindingFlags.InvokeMethod, null, ServerComponent, new object[] { parameters });

            return item as XElement;
        }

        public virtual string QueryOrderStatus(string orderID, string itemID)
        {
            if (ServerComponent == null)
                return null;

            object item = ServerComponent.GetType().InvokeMember("QueryOrderStatus", BindingFlags.InvokeMethod, null, ServerComponent, new object[] { orderID, itemID });

            return item.ToString();
        }

        protected virtual void CreateServerComponent()
        {
            XAttribute temp = WebServiceConfiguration.Attribute("assemblyname");
            string temp2 = Path.GetFileNameWithoutExtension(temp.Value);
            string temp1 = Path.Combine(GetAppPath(), "Servers");
            temp1 = Path.Combine(temp1, temp2);
            string assemblyName = Path.Combine(temp1, temp.Value);

            temp = WebServiceConfiguration.Attribute("classname");
            string className = temp.Value;

            Assembly assembly = Assembly.LoadFile(assemblyName);

            Type classType = assembly.GetTypes().FirstOrDefault(pred => pred.Name.Trim().ToUpper() == className.Trim().ToUpper());

            ServerComponent = Activator.CreateInstance(classType);
        }

        protected virtual string GetAppPath()
        {
            return System.Windows.Forms.Application.StartupPath;
        }
    }
}
