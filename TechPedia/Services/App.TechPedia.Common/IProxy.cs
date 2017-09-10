using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
namespace App.TechPedia.Common
{
    public interface IProxy
    {
        void SetConfiguration(XElement webServiceConfiguration);
        IEnumerable<XElement> SearchItem(XElement parameters);
        XElement GetItem(string itemID);
        XElement PlaceOrder(XElement parameters);
        string QueryOrderStatus(string orderid, string itemID);
        XElement GetOrderList();
        XElement GetOrder(string orderID);
    }
}
