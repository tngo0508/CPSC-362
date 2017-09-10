using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows.Forms;
using App.TechPedia.Common;
using App.TechPedia.DataAccessLayer;
using DevExpress.XtraEditors.Repository;
namespace App.TechPedia
{
    public partial class Checkout : Form
    {
        public Checkout()
        {
            InitializeComponent();
        }

        private double m_Total = 0;
        private RepositoryItemHyperLinkEdit m_Edit;
        private RepositoryItemMemoEdit m_EditQty;
        private DataTable m_DataTable = null;

        private void Checkout_Load(object sender, EventArgs e)
        {
            CommonFunctions.SetFormInfo(this);

            LoadCartList();
        }

        private void LoadCartList()
        {
            m_DataTable = new DataTable();
            m_DataTable.Columns.Add("VendorID");
            m_DataTable.Columns.Add("Vendor");
            m_DataTable.Columns.Add("Item ID");
            m_DataTable.Columns.Add("Name");
            m_DataTable.Columns.Add("Price");
            m_DataTable.Columns.Add("Description");
            m_DataTable.Columns.Add("Qty", typeof(double));
            m_DataTable.Columns.Add("RemoveCart");

            IEnumerable<XElement> cartList = DataAccessLayer.DataAccessLayer.GetCarts().Elements("item");
            IEnumerable<XElement> servers = DataAccessLayer.DataAccessLayer.GetServers().Elements("server");

            foreach (XElement cart in cartList)
            {
                DataRow dataRow = m_DataTable.NewRow();

                XElement server = servers.FirstOrDefault(pred => pred.GetAttribute("id").Trim().ToUpper() == cart.GetAttribute("vendorid").Trim().ToUpper());
                if (server != null)
                {
                    IProxy proxy = CommonFunctions.GetProxy(server);

                    if (proxy != null)
                    {
                        XElement item = proxy.GetItem(cart.GetAttribute("itemid"));

                        if (item != null)
                        {
                            dataRow[0] = server.GetAttribute("id");
                            dataRow[1] = server.GetAttribute("name");
                            dataRow[2] = cart.GetAttribute("itemid");
                            dataRow[3] = item.GetAttribute("name");
                            dataRow[4] = item.GetAttribute("price");
                            dataRow[5] = item.GetAttribute("description");
                            dataRow[6] = cart.GetAttribute("qty");
                            dataRow[7] = "Remove from Cart";

                            m_Total += double.Parse(item.GetAttribute("price"));
                        }
                        m_DataTable.Rows.Add(dataRow);
                    }
                }
            }

            gridControl1.DataSource = m_DataTable;

            gridView1.Columns[0].OptionsColumn.AllowEdit = false;
            gridView1.Columns[0].Visible = false;
            gridView1.Columns[1].OptionsColumn.AllowEdit = false;
            gridView1.Columns[2].OptionsColumn.AllowEdit = false;
            gridView1.Columns[3].OptionsColumn.AllowEdit = false;
            gridView1.Columns[4].OptionsColumn.AllowEdit = false;
            gridView1.Columns[5].OptionsColumn.AllowEdit = false;


            m_Edit = new RepositoryItemHyperLinkEdit();
            m_Edit.Click += EditXML_Click;
            gridView1.Columns[7].ColumnEdit = m_Edit;

            m_EditQty = new RepositoryItemMemoEdit();
            m_EditQty.Click += EditQty_Click;
            gridView1.Columns[6].ColumnEdit = m_EditQty;

            gridControl1.Refresh();

            FillPaymentInfo();

            ShowTotal();
        }

        private void EditQty_Click(object sender, EventArgs e)
        {
            DataRow dataRow = gridView1.GetFocusedDataRow();
            if (dataRow == null)
                return;

            string vendorid = dataRow[0].ToString();
            string itemid = dataRow[2].ToString();
            double qty = 0;
            if (double.TryParse(dataRow[6].ToString(), out qty))
            {
                DataAccessLayer.DataAccessLayer.UpdateCart(vendorid, itemid, qty);
                ShowTotal();
            }
        }

        private void EditXML_Click(object sender, EventArgs e)
        {
            DataRow dataRow = gridView1.GetFocusedDataRow();
            if (dataRow == null)
                return;

            string vendorid = dataRow[0].ToString();
            string itemid = dataRow[2].ToString();

            DataAccessLayer.DataAccessLayer.RemoveFromCart(vendorid, itemid);

            (gridControl1.DataSource as DataTable).Rows.Remove(dataRow);

            ShowTotal();
        }

        private void ShowTotal()
        {
            double price = 0;
            double qty = 0;
            m_Total = 0;
            foreach ( DataRow row in m_DataTable.Rows )
            {
                if(double.TryParse (row[4].ToString(), out price))
                {
                    if(double.TryParse(row[6].ToString(), out qty))
                    {
                        m_Total += price * qty;
                    }
                }
            }

            double tax = 0;
            double shipping = 0;
            tax = (m_Total * 12 / 100);

            if (radFreeShippings.Checked)
                shipping = 0;
            else if (rad15.Checked)
                shipping = 15;
            else if (rad7.Checked)
                shipping = 7;

            Total.Text = (Math.Round(m_Total, 2)).ToString("0.00");
            Tax.Text = (Math.Round(tax, 2)).ToString("0.00");
            Shipping.Text = (Math.Round(shipping, 2)).ToString("0.00");
            GrandTotal.Text = Math.Round((m_Total + tax + shipping), 2).ToString("0.00");

            Total.TextAlign = ContentAlignment.TopRight;
            Tax.TextAlign = ContentAlignment.TopRight;
            Shipping.TextAlign = ContentAlignment.TopRight;
            GrandTotal.TextAlign = ContentAlignment.TopRight;
        }

        private void FillPaymentInfo()
        {
            IEnumerable<XElement> paymentInfos = CommonFunctions.CurrentLoggedInUser.Elements("paymentinfo");
            cmbPayment.Items.Clear();
            foreach (XElement paymentInfo in paymentInfos)
            {
                cmbPayment.Items.Add(paymentInfo.GetAttribute("id"));
            }
        }

        private void btnAddPaymentMethod_Click(object sender, EventArgs e)
        {
            Payment payment = new Payment(true);
            payment.ShowDialog();

            FillPaymentInfo();
        }

        private void rad15_CheckedChanged(object sender, EventArgs e)
        {
            ShowTotal();
        }

        private void rad7_CheckedChanged(object sender, EventArgs e)
        {
            ShowTotal();
        }

        private void radFreeShippings_CheckedChanged(object sender, EventArgs e)
        {
            ShowTotal();
        }

        private void btnPlaceOrder_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cmbPayment.Text))
            {
                MessageBox.Show("Select payment method.", CommonFunctions.GetCaption(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            IEnumerable<XElement> cartList = DataAccessLayer.DataAccessLayer.GetCarts().Elements("item");
            IEnumerable<XElement> servers = DataAccessLayer.DataAccessLayer.GetServers().Elements("server");
            Dictionary<string, List<XElement>> orderList = new Dictionary<string, List<XElement>>();

            orderList.Add("ALL", new List<XElement>());

            foreach (XElement cart in cartList)
            {
                XElement server = servers.FirstOrDefault(pred => pred.GetAttribute("id").Trim().ToUpper() == cart.GetAttribute("vendorid").Trim().ToUpper());
                if (server != null)
                {
                    IProxy proxy = CommonFunctions.GetProxy(server);

                    if (proxy != null)
                    {
                        XElement item = proxy.GetItem(cart.GetAttribute("itemid"));

                        if (item != null)
                        {
                            string vendorid = cart.GetAttribute("vendorid");
                            XElement order = new XElement("item", new XAttribute("vendorid", vendorid),
                                                                  new XAttribute("itemid", cart.GetAttribute("itemid")),
                                                                  new XAttribute("price", item.GetAttribute("price")),
                                                                  new XAttribute("qty", cart.GetAttribute("qty"))
                                                         );

                            if (!orderList.ContainsKey(vendorid.Trim().ToUpper()))
                                orderList.Add(vendorid.Trim().ToUpper(), new List<XElement>());

                            orderList[vendorid.ToUpper().Trim()].Add(order);
                            orderList["ALL"].Add(order);
                        }
                    }
                }
            }

            string orderID = "O_" + DateTime.Now.ToOADate().ToString();

            foreach (KeyValuePair<string, List<XElement>> item in orderList)
            {
                XElement order = new XElement("order");

                order.SetAttributeValue("orderid", orderID);

                foreach (XElement element in item.Value)
                {
                    order.Add(element);
                }

                order.Add(GetPaymentAndShippingInfo());

                if(item.Key.Trim().ToUpper() == "ALL")
                {
                    DataAccessLayer.DataAccessLayer.PlaceOrder(order);
                }
                else
                {
                    XElement server = servers.FirstOrDefault(pred => pred.GetAttribute("id").Trim().ToUpper() == item.Key.Trim().ToUpper());

                    IProxy proxy = CommonFunctions.GetProxy(server);

                    if(proxy != null)
                    {
                        proxy.PlaceOrder(order);
                    }
                }
            }

            List<XElement> allOrders = orderList["ALL"];
            foreach ( XElement order in allOrders )
            {
                DataAccessLayer.DataAccessLayer.RemoveFromCart(order.GetAttribute("vendorid"), order.GetAttribute("itemid"));
            }
            LoadCartList();
            ShowTotal();

            cmbPayment.Text = string.Empty;
            txtAddress.Text = string.Empty;
            txtCity.Text = string.Empty;
            txtZipCode.Text = string.Empty;
            txtState.Text = string.Empty;

            MessageBox.Show("Order placed to vendors.", CommonFunctions.GetCaption(), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private XElement GetPaymentAndShippingInfo()
        {
            XElement otherInfo = new XElement("otherinfo");

            XElement paymentInfo = CommonFunctions.CurrentLoggedInUser.Elements("paymentinfo").FirstOrDefault(pred => pred.GetAttribute("id").Trim().ToUpper() == cmbPayment.Text.Trim().ToUpper());

            otherInfo.Add(paymentInfo);

            int shiproute = 0;
            if (rad15.Checked)
                shiproute = 1;
            else if (rad7.Checked)
                shiproute = 2;
            else
                shiproute = 3;

            otherInfo.SetAttributeValue("shippingformat", shiproute);

            otherInfo.SetAttributeValue("address", txtAddress.Text);
            otherInfo.SetAttributeValue("city", txtCity.Text);
            otherInfo.SetAttributeValue("state", txtState.Text);
            otherInfo.SetAttributeValue("zipcode", txtZipCode.Text);

            return otherInfo;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnUseSameAsBillingAddress_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cmbPayment.Text))
            {
                MessageBox.Show("Select payment method.", CommonFunctions.GetCaption(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            XElement paymentInfo = CommonFunctions.CurrentLoggedInUser.Elements("paymentinfo").FirstOrDefault(pred => pred.GetAttribute("id").Trim().ToUpper() == cmbPayment.Text.Trim().ToUpper());
            if(paymentInfo != null)
            {
                txtAddress.Text = paymentInfo.GetAttribute("address1");
                txtCity.Text = paymentInfo.GetAttribute("city");
                txtState.Text = paymentInfo.GetAttribute("state");
                txtZipCode.Text = paymentInfo.GetAttribute("zip");
            }
        }
    }
}
