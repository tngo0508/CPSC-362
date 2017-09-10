using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using App.TechPedia.DataAccessLayer;
using App.TechPedia.Common;
namespace App.TechPedia
{
    public partial class OrderStatus : Form
    {
        public OrderStatus()
        {
            InitializeComponent();
        }

        private void OrderStatus_Load(object sender, EventArgs e)
        {
            CommonFunctions.SetFormInfo(this);

            FillOrderList();
        }

        private void FillOrderList()
        {
            XElement orderList = DataAccessLayer.DataAccessLayer.GetOrderList();
            IEnumerable<XElement> items = orderList.Elements("item");

            lstOrders.Items.Clear();
            foreach (XElement order in items)
            {
                string orderID = order.GetAttribute("order");
                orderID = order.GetAttribute("order").ToLower().Replace("o_", string.Empty);
                DateTime dateTime = DateTime.FromOADate(double.Parse(orderID));
                string listItem = dateTime.ToString() + ", " + order.GetAttribute("order");
                lstOrders.Items.Add(listItem);
            }

            if (lstOrders.Items.Count > 0)
                lstOrders.SelectedIndex = 0;
        }

        private void lstOrders_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] temp = lstOrders.SelectedItem.ToString().Split(',');
            XElement orderElement = DataAccessLayer.DataAccessLayer.GetOrder(temp[1].Trim());

            IEnumerable<XElement> orderItemList = orderElement.Elements("item");
            IEnumerable<XElement> serverList = DataAccessLayer.DataAccessLayer.GetServers().Elements("server");

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("VendorID");
            dataTable.Columns.Add("Vendor");
            dataTable.Columns.Add("Item ID");
            dataTable.Columns.Add("Name");
            dataTable.Columns.Add("Price");
            dataTable.Columns.Add("Description");
            dataTable.Columns.Add("Qty");
            dataTable.Columns.Add("Status");

            foreach (XElement orderItem in orderItemList)
            {
                DataRow dataRow = dataTable.NewRow();

                XElement server = serverList.FirstOrDefault(pred => pred.GetAttribute("id").Trim().ToUpper() == orderItem.GetAttribute("vendorid").Trim().ToUpper());

                if (server != null)
                {
                    IProxy proxy = CommonFunctions.GetProxy(server);

                    if (proxy != null)
                    {
                        XElement productItem = proxy.GetItem(orderItem.GetAttribute("itemid"));

                        dataRow[0] = orderItem.GetAttribute("vendorid");
                        dataRow[1] = server.GetAttribute("name");
                        dataRow[2] = orderItem.GetAttribute("itemid");
                        dataRow[3] = productItem.GetAttribute("name");
                        dataRow[4] = orderItem.GetAttribute("price");
                        dataRow[5] = productItem.GetAttribute("description");
                        dataRow[6] = orderItem.GetAttribute("qty");
                        dataRow[7] = proxy.QueryOrderStatus(temp[1].Trim(), orderItem.GetAttribute("itemid"));

                        dataTable.Rows.Add(dataRow);
                    }
                }
            }

            gridControl1.DataSource = dataTable;

            gridView1.Columns[0].Visible = false;
            gridView1.Columns[1].OptionsColumn.AllowEdit = false;
            gridView1.Columns[2].OptionsColumn.AllowEdit = false;
            gridView1.Columns[3].OptionsColumn.AllowEdit = false;
            gridView1.Columns[4].OptionsColumn.AllowEdit = false;
            gridView1.Columns[5].OptionsColumn.AllowEdit = false;
            gridView1.Columns[6].OptionsColumn.AllowEdit = false;
            gridView1.Columns[7].OptionsColumn.AllowEdit = false;

            gridControl1.Refresh();

            XElement otherInfo = orderElement.Element("otherinfo");
            string shippingFormat = otherInfo.GetAttribute("shippingformat");
            if (shippingFormat == "1")
            {
                shippingFormat = "$15 (5-7 days)";
            }
            else if (shippingFormat == "2")
            {
                shippingFormat = "$7.00 (3-5 days)";
            }
            else
            {
                shippingFormat = "Free Shipping";
            }

            StringBuilder sB = new StringBuilder();
            sB.Append("Shipping format : " + shippingFormat + Environment.NewLine);
            sB.Append("Shipping Address : " + otherInfo.GetAttribute("address") + Environment.NewLine);
            sB.Append("Shipping City : " + otherInfo.GetAttribute("city") + Environment.NewLine);
            sB.Append("Shipping State : " + otherInfo.GetAttribute("state") + Environment.NewLine);
            sB.Append("Shipping ZipCode : " + otherInfo.GetAttribute("zipcode") + Environment.NewLine);
            sB.Append(Environment.NewLine + Environment.NewLine);
            sB.Append("Payment Card Type : " + otherInfo.Element("paymentinfo").GetAttribute("type") + Environment.NewLine);
            sB.Append("Payment Card Expiry Date : " + otherInfo.Element("paymentinfo").GetAttribute("expdate") + Environment.NewLine);
            sB.Append("Payment Card Name : " + otherInfo.Element("paymentinfo").GetAttribute("id") + Environment.NewLine);
            sB.Append("Payment Card Holder Name : " + otherInfo.Element("paymentinfo").GetAttribute("name") + Environment.NewLine);
            sB.Append("Payment Card Number : " + otherInfo.Element("paymentinfo").GetAttribute("number") + Environment.NewLine);
            sB.Append("Payment Card Billing Address 1 : " + otherInfo.Element("paymentinfo").GetAttribute("address1") + Environment.NewLine);
            sB.Append("Payment Card Billing Address 2 : " + otherInfo.Element("paymentinfo").GetAttribute("address2") + Environment.NewLine);
            sB.Append("Payment Card City : " + otherInfo.Element("paymentinfo").GetAttribute("city") + Environment.NewLine);
            sB.Append("Payment Card State : " + otherInfo.Element("paymentinfo").GetAttribute("state") + Environment.NewLine);
            sB.Append("Payment Card ZipCode : " + otherInfo.Element("paymentinfo").GetAttribute("zip") + Environment.NewLine);

            txtDescription.Text = sB.ToString();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
