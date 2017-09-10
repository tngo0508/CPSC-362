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
using DevExpress.XtraEditors.Repository;
using App.TechPedia.Common;
using App.TechPedia.DataAccessLayer;
namespace CloudTest
{
    public partial class Order : Form
    {
        public Order()
        {
            InitializeComponent();
        }

        private IProxy m_proxy = null;
        private XElement m_OrderItem = null;
        private RepositoryItemComboBox m_Edit = null;

        private void Order_Load(object sender, EventArgs e)
        {
            IEnumerable<XElement> serverList = DataAccessLayer.GetServers().Elements("server");

            cmbChooseServer.Items.Clear();
            foreach (XElement server in serverList)
            {
                cmbChooseServer.Items.Add(server.GetAttribute("name"));
            }

            if (cmbChooseServer.Items.Count > 0)
                cmbChooseServer.SelectedIndex = 0;
        }

        private void cmbChooseServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_proxy = null;

            XElement server = DataAccessLayer.GetServers().Elements("server").FirstOrDefault(pred => pred.GetAttribute("name").Trim().ToUpper()
                                                                                                        == cmbChooseServer.SelectedItem.ToString().Trim().ToUpper());

            if (server != null)
                m_proxy = CommonFunctions.GetProxy(server);

            LoadOrders();
        }

        private void LoadOrders()
        {
            if (m_proxy != null)
            {
                XElement orderItem = m_proxy.GetOrderList();

                IEnumerable<XElement> items = orderItem.Elements("item");

                lstOrder.Items.Clear();
                foreach (XElement order in items)
                {
                    string orderID = order.GetAttribute("order");
                    orderID = order.GetAttribute("order").ToLower().Replace("o_", string.Empty);
                    DateTime dateTime = DateTime.FromOADate(double.Parse(orderID));
                    string listItem = dateTime.ToString() + ", " + order.GetAttribute("order");
                    lstOrder.Items.Add(listItem);
                }

                if (lstOrder.Items.Count > 0)
                    lstOrder.SelectedIndex = 0;
            }

        }

        private void lstOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_proxy == null)
                return;

            string[] temp = lstOrder.SelectedItem.ToString().Split(',');
            m_OrderItem = m_proxy.GetOrder(temp[1].Trim());

            IEnumerable<XElement> orderItemList = m_OrderItem.Elements("item");
            IEnumerable<XElement> serverList = DataAccessLayer.GetServers().Elements("server");

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("VendorID");
            dataTable.Columns.Add("Vendor");
            dataTable.Columns.Add("Item ID");
            dataTable.Columns.Add("Name");
            dataTable.Columns.Add("Price");
            dataTable.Columns.Add("Description");
            dataTable.Columns.Add("Qty");
            dataTable.Columns.Add("Status");
            dataTable.Columns.Add("element", typeof(XElement));

            foreach (XElement orderItem in orderItemList)
            {
                DataRow dataRow = dataTable.NewRow();

                XElement server = serverList.FirstOrDefault(pred => pred.GetAttribute("id").Trim().ToUpper() == orderItem.GetAttribute("vendorid").Trim().ToUpper());

                if (server != null)
                {
                    XElement productItem = m_proxy.GetItem(orderItem.GetAttribute("itemid"));

                    dataRow[0] = orderItem.GetAttribute("vendorid");
                    dataRow[1] = server.GetAttribute("name");
                    dataRow[2] = orderItem.GetAttribute("itemid");
                    dataRow[3] = productItem.GetAttribute("name");
                    dataRow[4] = orderItem.GetAttribute("price");
                    dataRow[5] = productItem.GetAttribute("description");
                    dataRow[6] = orderItem.GetAttribute("qty");
                    dataRow[7] = string.IsNullOrEmpty(orderItem.GetAttribute("status")) ? "Order Received" : orderItem.GetAttribute("status");
                    dataRow[8] = orderItem;

                    dataTable.Rows.Add(dataRow);
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
            gridView1.Columns[7].OptionsColumn.AllowEdit = true;
            gridView1.Columns[8].OptionsColumn.AllowEdit = false;
            gridView1.Columns[8].Visible = false;

            m_Edit = new RepositoryItemComboBox();
            m_Edit.Items.Add("Order Received");
            m_Edit.Items.Add("Processing");
            m_Edit.Items.Add("Shipped");
            m_Edit.Items.Add("Delivered");
            m_Edit.SelectedValueChanged += M_Edit_SelectedValueChanged;
            gridView1.Columns[7].ColumnEdit = m_Edit;

            gridControl1.Refresh();
        }

        private void M_Edit_SelectedValueChanged(object sender, EventArgs e)
        {
            DataRow dataRow = gridView1.GetFocusedDataRow();

            XElement orderElement = dataRow[8] as XElement;

            orderElement.SetAttributeValue("status", ((DevExpress.XtraEditors.TextEdit)sender).Text);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (m_OrderItem == null || m_proxy == null)
                return;

            m_proxy.PlaceOrder(m_OrderItem);

            MessageBox.Show("Order Saved.", CommonFunctions.GetCaption(), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
