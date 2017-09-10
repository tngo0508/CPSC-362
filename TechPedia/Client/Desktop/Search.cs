using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using App.TechPedia.Common;
using App.TechPedia.DataAccessLayer;
using Proxy.Base;
using DevExpress.XtraEditors.Repository;
namespace App.TechPedia
{
    public partial class Search : Form
    {
        private RepositoryItemHyperLinkEdit m_EditXML = null;
        private bool m_blnSearchPressed = false;
        private XElement m_searchTaxonomy;

        public Search()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadSearchList();
            m_blnSearchPressed = true;
        }

        private void LoadCategory()
        {
            IEnumerable<XElement> items = m_searchTaxonomy.Elements();

            cmbCategory.Items.Clear();
            foreach ( XElement item in items )
            {
                cmbCategory.Items.Add(item.Name.LocalName);
            }

            cmbCategory.Items.Add("");
        }

        private void LoadSearchList()
        {
            XElement parameters = new XElement("parameter",
                                                    new XAttribute("search1", cmbCategory.Text),
                                                    new XAttribute("search2", cmbItem.Text),
                                                    new XAttribute("search3", cmbBrand.Text)
                                              );


            XElement servers = DataAccessLayer.DataAccessLayer.GetServers();
            IEnumerable<XElement> serverList = servers.Elements("server");
            XElement cartList = DataAccessLayer.DataAccessLayer.GetCarts();

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("VendorID");
            dataTable.Columns.Add("Vendor");
            dataTable.Columns.Add("Item ID");
            dataTable.Columns.Add("Name");
            dataTable.Columns.Add("Description");
            dataTable.Columns.Add("Price", typeof(double));
            dataTable.Columns.Add("Cart");

            foreach (XElement server in serverList)
            {
                IProxy proxy = CommonFunctions.GetProxy(server);
                proxy.SetConfiguration(server);

                IEnumerable<XElement> searchList = proxy.SearchItem(parameters);

                foreach (XElement searchItem in searchList)
                {
                    DataRow dataRow = dataTable.NewRow();

                    dataRow[0] = server.GetAttribute("id");
                    dataRow[1] = server.GetAttribute("name");
                    dataRow[2] = searchItem.GetAttribute("itemid");
                    dataRow[3] = searchItem.GetAttribute("name");
                    dataRow[4] = searchItem.GetAttribute("description");
                    dataRow[5] = searchItem.GetAttribute("price");
                    dataRow[6] = "Add to Cart";

                    if (cartList != null)
                    {
                        XElement cartItem = cartList.Descendants("item").FirstOrDefault(pred => pred.GetAttribute("itemid").Trim().ToUpper() == searchItem.GetAttribute("itemid").Trim().ToUpper()
                                                                                                        && pred.GetAttribute("vendorid").Trim().ToUpper() == server.GetAttribute("id").Trim().ToUpper());
                        if (cartItem != null)
                            dataRow[6] = "In Cart";
                    }

                    dataTable.Rows.Add(dataRow);
                }
            }

            gridControl1.DataSource = dataTable;

            gridView1.Columns[0].OptionsColumn.AllowEdit = false;
            gridView1.Columns[0].Visible = false;
            gridView1.Columns[1].OptionsColumn.AllowEdit = false;
            gridView1.Columns[2].OptionsColumn.AllowEdit = false;
            gridView1.Columns[3].OptionsColumn.AllowEdit = false;
            gridView1.Columns[4].OptionsColumn.AllowEdit = false;
            gridView1.Columns[5].OptionsColumn.AllowEdit = false;

            gridControl1.Refresh();

            m_EditXML = new RepositoryItemHyperLinkEdit();
            m_EditXML.Click += EditXML_Click;
            gridView1.Columns[6].ColumnEdit = m_EditXML;
        }

        private void EditXML_Click(object sender, EventArgs e)
        {
            DataRow dataRow = gridView1.GetFocusedDataRow();
            if (dataRow == null)
                return;

            DataAccessLayer.DataAccessLayer.AddToCart(dataRow[0].ToString(), dataRow[2].ToString(), 1);

            btnSearch_Click(btnSearch, null);
        }

        private void btnProceedToCheckout_Click(object sender, EventArgs e)
        {
            Checkout checkOut = new Checkout();
            checkOut.ShowDialog();

            if (m_blnSearchPressed)
                LoadSearchList();
        }

        private void btnMyProfile_Click(object sender, EventArgs e)
        {
            UserProfile profile = new UserProfile(CommonFunctions.CurrentLoggedInUser);
            profile.ShowDialog();
        }

        private void btnMyOrders_Click(object sender, EventArgs e)
        {
            OrderStatus order = new OrderStatus();
            order.ShowDialog();
        }

        private void btnPaymentInfo_Click(object sender, EventArgs e)
        {
            Payment paymentInfo = new Payment(false);
            paymentInfo.ShowDialog();
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            Users users = new Users();
            users.ShowDialog();
        }

        private void btnVendors_Click(object sender, EventArgs e)
        {
            Vendors vendors = new Vendors();
            vendors.ShowDialog();
        }

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbItem.Items.Clear();

            XElement items = m_searchTaxonomy.Elements().FirstOrDefault(pred => pred.Name.LocalName.Trim().ToUpper() == cmbCategory.Text.Trim().ToUpper());

            if (items != null)
            {
                foreach (XElement item in items.Elements())
                {
                    cmbItem.Items.Add(item.Name.LocalName);
                }

                cmbItem.Items.Add("");
            }
        }

        private void cmbItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbBrand.Items.Clear();

            XElement items = m_searchTaxonomy.Elements().FirstOrDefault(pred => pred.Name.LocalName.Trim().ToUpper() == cmbCategory.Text.Trim().ToUpper());

            if (items != null)
            {
                XElement items1 = items.Elements().FirstOrDefault(pred => pred.Name.LocalName.Trim().ToUpper() == cmbItem.Text.Trim().ToUpper());

                if (items1 != null)
                {
                    foreach (XElement item in items1.Elements())
                    {
                        cmbBrand.Items.Add(item.Name.LocalName);
                    }

                    cmbBrand.Items.Add("");
                }
            }
        }

        private void Search_Load(object sender, EventArgs e)
        {
            CommonFunctions.SetFormInfo(this);

            string fileName = Path.Combine(CommonFunctions.GetAppPath(), "SearchTaxonomy.xml");
            m_searchTaxonomy = XElement.Load(fileName);

            LoadCategory();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
