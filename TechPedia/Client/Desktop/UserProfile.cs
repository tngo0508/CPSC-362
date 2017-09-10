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
using App.TechPedia.Common;
using App.TechPedia.DataAccessLayer;
namespace App.TechPedia
{
    public partial class UserProfile : Form
    {
        private XElement m_User;

        public UserProfile(XElement element)
        {
            InitializeComponent();
            m_User = element;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            IEnumerable<XElement> userList = DataAccessLayer.DataAccessLayer.GetUsers().Elements("user");

            if (m_User == null)
            {
                XElement user = null;
                user = userList.FirstOrDefault(pred => pred.GetAttribute("id").Trim().ToUpper() == txtUserID.Text.Trim().ToUpper());

                if (user != null)
                {
                    MessageBox.Show("User already exists.", CommonFunctions.GetCaption(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                m_User = new XElement("user");
                DataAccessLayer.DataAccessLayer.GetUsers().Add(m_User);
            }

            m_User.SetAttributeValue("id", txtUserID.Text);
            m_User.SetAttributeValue("password", txtPassword.Text);
            m_User.SetAttributeValue("firstname", txtFirstName.Text);
            m_User.SetAttributeValue("lastname", txtLastName.Text);
            m_User.SetAttributeValue("address1", txtAddress1.Text);
            m_User.SetAttributeValue("address2", txtAddress2.Text);
            m_User.SetAttributeValue("city", txtCity.Text);
            m_User.SetAttributeValue("zip", txtZip.Text);

            DataAccessLayer.DataAccessLayer.SaveUsers();

            MessageBox.Show("User saved.", CommonFunctions.GetCaption(), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Profile_Load(object sender, EventArgs e)
        {
            CommonFunctions.SetFormInfo(this);

            if (m_User != null)
            {
                txtUserID.Text = m_User.GetAttribute("id");
                txtPassword.Text = m_User.GetAttribute("password");
                txtFirstName.Text = m_User.GetAttribute("firstname");
                txtLastName.Text = m_User.GetAttribute("lastname");
                txtAddress1.Text = m_User.GetAttribute("address1");
                txtAddress2.Text = m_User.GetAttribute("address2");
                txtCity.Text = m_User.GetAttribute("city");
                txtZip.Text = m_User.GetAttribute("zip");
                txtState.Text = m_User.GetAttribute("state");
            }
        }
    }
}
