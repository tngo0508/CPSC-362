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
    public partial class Vendors : Form
    {
        private XElement m_Servers;
        private DataTable m_DataTable;

        public Vendors()
        {
            InitializeComponent();
        }

        private void Config_Load(object sender, EventArgs e)
        {
            CommonFunctions.SetFormInfo(this);

            BuildTable();
        }

        public void BuildTable()
        {
            m_Servers = DataAccessLayer.DataAccessLayer.GetServers();

            m_DataTable = new DataTable();

            m_DataTable.Columns.Add("ID", typeof(string));
            m_DataTable.Columns.Add("Name", typeof(string));
            m_DataTable.Columns.Add("Assembly Name", typeof(string));
            m_DataTable.Columns.Add("Class Name", typeof(string));
            m_DataTable.Columns.Add("Proxy Assembly Name", typeof(string));
            m_DataTable.Columns.Add("Proxy Class Name", typeof(string));

            IEnumerable<XElement> serverLst = m_Servers.Elements("server");

            foreach (XElement server in serverLst)
            {
                DataRow dataRow = m_DataTable.NewRow();

                dataRow[0] = server.GetAttribute("id");
                dataRow[1] = server.GetAttribute("name");
                dataRow[2] = server.GetAttribute("assemblyname");
                dataRow[3] = server.GetAttribute("classname");
                dataRow[4] = server.GetAttribute("proxyassemblyname");
                dataRow[5] = server.GetAttribute("proxyclassname");

                m_DataTable.Rows.Add(dataRow);
            }

            gridControl1.DataSource = m_DataTable;

            gridView1.Columns[0].OptionsColumn.AllowEdit = false;
        }

        private void btnNewRow_Click(object sender, EventArgs e)
        {
            if (m_DataTable != null)
            {
                DataRow dataRow = m_DataTable.NewRow();
                dataRow[0] = Guid.NewGuid().ToString();
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
                XElement server = m_Servers.Elements("server").FirstOrDefault(pred => pred.GetAttribute("id").Trim().ToUpper() == dataRow[0].ToString().Trim().ToUpper());

                if (string.IsNullOrEmpty(dataRow[1].ToString()))
                {
                    MessageBox.Show("Data unfullfilled " + dataRow[0].ToString() + ".", CommonFunctions.GetCaption(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    continue;
                }

                if (server == null)
                {
                    server = new XElement("server");

                    m_Servers.Add(server);
                }

                server.SetAttributeValue("id", dataRow[0].ToString());
                server.SetAttributeValue("name", dataRow[1].ToString());
                server.SetAttributeValue("assemblyname", dataRow[2].ToString());
                server.SetAttributeValue("classname", dataRow[3].ToString());
                server.SetAttributeValue("proxyassemblyname", dataRow[4].ToString());
                server.SetAttributeValue("proxyclassname", dataRow[5].ToString());
            }

            DataAccessLayer.DataAccessLayer.SaveServers();

            MessageBox.Show("Save Successfull.", CommonFunctions.GetCaption(), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
