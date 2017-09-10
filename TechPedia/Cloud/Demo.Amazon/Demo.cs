using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using System.IO;
using App.TechPedia.Common;
namespace Demo.Amazon
{
    public class Demo
    {
        private string GetWorkingFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public virtual XElement GetInventory()
        {
            string folderName = GetWorkingFolder();
            string fileName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) + ".xml";
            fileName = Path.Combine(folderName, fileName);

            XElement returnItem = XElement.Load(fileName);
            return returnItem;
        }

        public virtual IEnumerable<XElement> SearchItem(XElement parameters)
        {
            XElement inventory = GetInventory();
            IEnumerable<XElement> returnList;

            string search1 = parameters.GetAttribute("search1");                            // e.g Electronic
            if (string.IsNullOrEmpty(search1))
            {
                returnList = inventory.Descendants("item");
            }
            else
            {
                XElement element1 = inventory.Elements().FirstOrDefault(pred => pred.Name.LocalName.Trim().ToUpper() == search1.Trim().ToUpper());
                string search2 = parameters.GetAttribute("search2");                        //  e.g. TV

                if (string.IsNullOrEmpty(search2))
                {
                    returnList = element1.Descendants("item");
                }
                else
                {
                    XElement element2 = element1.Elements().FirstOrDefault(pred => pred.Name.LocalName.Trim().ToUpper() == search2.Trim().ToUpper());
                    string search3 = parameters.GetAttribute("search3");                    //  e.g Samsung

                    if(string.IsNullOrEmpty(search3))
                    {
                        returnList = element2.Descendants("item");
                    }
                    else
                    {
                        XElement element3 = element2.Elements().FirstOrDefault(pred => pred.Name.LocalName.Trim().ToUpper() == search3.Trim().ToUpper());
                        returnList = element3.Descendants("item");
                    }
                }
            }

            return returnList;
        }

        public virtual XElement GetItem(string itemID)
        {
            XElement inventory = GetInventory();

            return inventory.Descendants("item").FirstOrDefault(pred => pred.GetAttribute("itemid").Trim().ToUpper() == itemID.Trim().ToUpper());
        }

        public virtual void PlaceOrder(XElement order)
        {
            string orderID = order.GetAttribute("orderid");

            string fileName = Path.Combine(GetWorkingFolder(), orderID + ".xml");

            order.Save(fileName);
        }

        public virtual string QueryOrderStatus(string orderID, string itemID)
        {
            string orderFile = Path.Combine(GetWorkingFolder(), orderID + ".xml");
            string status = "Order not yet received";

            if (File.Exists(orderFile))
            {
                status = "Order Received";

                XElement order = XElement.Load(orderFile);

                XElement item = order.Elements("item").FirstOrDefault(pred => pred.GetAttribute("itemid").Trim().ToUpper() == itemID.Trim().ToUpper());

                if (item != null)
                {
                    if(!string.IsNullOrEmpty(item.GetAttribute("status")))
                        status = item.GetAttribute("status");
                }
            }

            return status;
        }

        public virtual XElement GetOrderList()
        {
            string workingFolder = GetWorkingFolder();
            XElement returnItem = new XElement("orderlist");

            string[] files = Directory.GetFiles(workingFolder, "o_*.xml", SearchOption.TopDirectoryOnly);

            foreach (string file in files)
            {
                XElement item = new XElement("item", new XAttribute("order", Path.GetFileNameWithoutExtension(file)));
                returnItem.Add(item);
            }

            return returnItem;
        }

        public virtual XElement GetOrder(string orderID)
        {
            string workingFolder = GetWorkingFolder();
            string fileName = Path.Combine(workingFolder, orderID + ".xml");

            XElement order = XElement.Load(fileName);

            return order;
        }
    }
}
