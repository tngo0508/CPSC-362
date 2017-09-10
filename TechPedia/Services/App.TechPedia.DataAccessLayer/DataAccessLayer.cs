using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using App.TechPedia.Common;

namespace App.TechPedia.DataAccessLayer
{
    public static class DataAccessLayer
    {
        private static XElement Servers;
        private static XElement Users;


        public static XElement GetServers()
        {
            if (Servers == null)
            {
                string serverFileName = GetServerFile();

                Servers = XElement.Load(serverFileName);
            }

            return Servers;
        }

        public static XElement GetUsers()
        {
            if (Users == null)
            {
                string userFileName = GetUserFile();

                Users = XElement.Load(userFileName);
            }

            return Users;
        }

        public static void SaveServers()
        {
            string fileName = GetServerFile();

            GetServers().Save(fileName);
        }

        public static void SaveUsers()
        {
            string fileName = GetUserFile();

            GetUsers().Save(fileName);
        }

        public static XElement GetUser(string userID)
        {
            XElement user = GetUsers().Elements("user").FirstOrDefault(pred => pred.GetAttribute("id").Trim().ToUpper() == userID.Trim().ToUpper());

            return user;
        }


        public static void RemoveFromCart(string vendorid, string itemID)
        {
            XElement cartElement = GetCarts();

            XElement item = cartElement.Elements("item").FirstOrDefault(pred => pred.GetAttribute("vendorid").Trim().ToUpper() == vendorid.Trim().ToUpper() && pred.GetAttribute("itemid").Trim().ToUpper() == itemID.Trim().ToUpper());

            if (item != null)
                item.Remove();

            cartElement.Save(GetCartFile());
        }

        public static void UpdateCart(string vendorid, string itemID, double qty)
        {
            XElement cartElement = GetCarts();

            XElement item = cartElement.Elements("item").FirstOrDefault(pred => pred.GetAttribute("vendorid").Trim().ToUpper() == vendorid.Trim().ToUpper() && pred.GetAttribute("itemid").Trim().ToUpper() == itemID.Trim().ToUpper());

            if (item != null)
                item.SetAttributeValue("qty", qty.ToString());

            cartElement.Save(GetCartFile());
        }

        public static void AddToCart(string vendorid, string itemID, int qty)
        {
            XElement cartElement = GetCarts();

            XElement cartItem = new XElement("item", new XAttribute("vendorid", vendorid),
                                                     new XAttribute("itemid", itemID),
                                                     new XAttribute("qty", qty)
                                            );

            cartElement.Add(cartItem);

            cartElement.Save(GetCartFile());
        }

        public static XElement GetCarts()
        {
            string cartFile = GetCartFile();
            XElement cartElement = null;

            if (File.Exists(cartFile))
            {
                cartElement = XElement.Load(cartFile);
            }
            else
            {
                cartElement = new XElement("cart");
                cartElement.Save(cartFile);
            }

            return cartElement;
        }

        public static void PlaceOrder(XElement element)
        {
            string orderID = element.GetAttribute("orderid");

            string orderFileName = GetOrderFileName(orderID);

            element.Save(orderFileName);
        }

        public static XElement GetOrderList()
        {
            string workingFolder = GetWorkingFolder();

            if (!Directory.Exists(workingFolder))
                Directory.CreateDirectory(workingFolder);

            XElement returnItem = new XElement("orderlist");

            string[] files = Directory.GetFiles(workingFolder, "o_*.xml", SearchOption.TopDirectoryOnly);

            foreach (string file in files)
            {
                XElement item = new XElement("item", new XAttribute("order", Path.GetFileNameWithoutExtension(file)));
                returnItem.Add(item);
            }

            return returnItem;
        }

        public static XElement GetOrder(string orderID)
        {
            string workingFolder = GetWorkingFolder();
            string fileName = Path.Combine(workingFolder, orderID + ".xml");

            XElement order = XElement.Load(fileName);

            return order;
        }

        private static string GetUserFile()
        {
            string fileName = Path.Combine(CommonFunctions.GetAppPath(), "Users.xml");

            if (!File.Exists(fileName))
            {
                XElement element = new XElement("users");
                XElement adminUser = new XElement("user", new XAttribute("id", "admin"),
                                                          new XAttribute("password", "admin"),
                                                          new XAttribute("firstname", "admin"),
                                                          new XAttribute("lastname", "admin")
                                                 );
                element.Add(adminUser);
                element.Save(fileName);
            }

            return fileName;
        }
        
        private static string GetServerFile()
        {
            string fileName = Path.Combine(CommonFunctions.GetAppPath(), "Servers.xml");

            if (!File.Exists(fileName))
            {
                XElement element = new XElement("servers");
                element.Save(fileName);
            }

            return fileName;
        }

        private static string GetCartFile()
        {
            string workingFolder = GetWorkingFolder();

            if (!Directory.Exists(workingFolder))
                Directory.CreateDirectory(workingFolder);

            string fileName = Path.Combine(workingFolder, "Carts.xml");

            if (!File.Exists(fileName))
            {
                XElement element = new XElement("carts");
                element.Save(fileName);
            }

            return fileName;
        }

        private static string GetOrderFileName(string orderID)
        {
            string workingFolder = GetWorkingFolder();

            if (!Directory.Exists(workingFolder))
                Directory.CreateDirectory(workingFolder);

            string fileName = Path.Combine(workingFolder, orderID + ".xml");

            if (!File.Exists(fileName))
            {
                XElement element = new XElement("orders");
                element.Save(fileName);
            }

            return fileName;
        }

        private static string GetWorkingFolder()
        {
            string loggedInUserID = CommonFunctions.CurrentLoggedInUser.GetAttribute("id");

            string folder = Path.Combine(CommonFunctions.GetAppPath(), loggedInUserID);

            return folder;
        }
    }
}
