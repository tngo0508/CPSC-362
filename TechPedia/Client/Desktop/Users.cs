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
using System.Xml;
using System.IO;
using App.TechPedia.Common;
using App.TechPedia.DataAccessLayer;
namespace App.TechPedia
{
    public partial class Users : Form
    {
        private DataTable m_DataTable;

        public Users()
        {
            InitializeComponent();
        }

        private XElement m_Users;

        private void Config_Load(object sender, EventArgs e)
        {
            CommonFunctions.SetFormInfo(this);

            m_Users = DataAccessLayer.DataAccessLayer.GetUsers();
            BuildTable();
        }

        public void BuildTable()
        {
            m_DataTable = new DataTable();

            m_DataTable.Columns.Add("ID", typeof(string));
            m_DataTable.Columns.Add("First Name", typeof(string));
            m_DataTable.Columns.Add("Last Name", typeof(string));
            m_DataTable.Columns.Add("Address 1", typeof(string));
            m_DataTable.Columns.Add("Address 2", typeof(string));
            m_DataTable.Columns.Add("City", typeof(string));
            m_DataTable.Columns.Add("State", typeof(string));
            m_DataTable.Columns.Add("Zip", typeof(string));

            IEnumerable<XElement> userLst = m_Users.Elements("user");

            foreach (XElement user in userLst)
            {
                DataRow dataRow = m_DataTable.NewRow();

                dataRow[0] = user.GetAttribute("id");
                dataRow[1] = user.GetAttribute("firstname");
                dataRow[2] = user.GetAttribute("lastname");
                dataRow[3] = user.GetAttribute("address1");
                dataRow[4] = user.GetAttribute("address2");
                dataRow[5] = user.GetAttribute("city");
                dataRow[6] = user.GetAttribute("state");
                dataRow[7] = user.GetAttribute("zip");

                m_DataTable.Rows.Add(dataRow);
            }

            gridControl1.DataSource = m_DataTable;

            gridView1.Columns[0].OptionsColumn.AllowEdit = false;
            gridView1.Columns[1].OptionsColumn.AllowEdit = false;
            gridView1.Columns[2].OptionsColumn.AllowEdit = false;
            gridView1.Columns[3].OptionsColumn.AllowEdit = false;
            gridView1.Columns[4].OptionsColumn.AllowEdit = false;
            gridView1.Columns[5].OptionsColumn.AllowEdit = false;
            gridView1.Columns[6].OptionsColumn.AllowEdit = false;
            gridView1.Columns[7].OptionsColumn.AllowEdit = false;
        }

        private void btnNewRow_Click(object sender, EventArgs e)
        {
            if (m_DataTable != null)
            {
                DataRow dataRow = m_DataTable.NewRow();
                m_DataTable.Rows.Add(dataRow);
            }

            gridControl1.RefreshDataSource();
            gridControl1.Refresh();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            foreach (DataRow dataRow in m_DataTable.Rows)
            {
                XElement userElement = m_Users.Elements("user").FirstOrDefault(pred => pred.GetAttribute("id").Trim().ToUpper() == dataRow[0].ToString().Trim().ToUpper());

                if (userElement == null)
                {
                    userElement = new XElement("user");

                    m_Users.Add(userElement);
                }

                userElement.SetAttributeValue("id", dataRow[0].ToString());
                userElement.SetAttributeValue("firstname", dataRow[1].ToString());
                userElement.SetAttributeValue("lastname", dataRow[2].ToString());
                userElement.SetAttributeValue("address1", dataRow[3].ToString());
                userElement.SetAttributeValue("address2", dataRow[4].ToString());
                userElement.SetAttributeValue("city", dataRow[5].ToString());
                userElement.SetAttributeValue("state", dataRow[6].ToString());
                userElement.SetAttributeValue("zip", dataRow[7].ToString());
            }

            DataAccessLayer.DataAccessLayer.SaveUsers();

            MessageBox.Show("Save Successfull.", CommonFunctions.GetCaption(), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
