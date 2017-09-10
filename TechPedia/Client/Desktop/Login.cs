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
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            XElement user = DataAccessLayer.DataAccessLayer.GetUser(txtUserID.Text);

            if(user == null)
            {
                txtUserID.Focus();
                MessageBox.Show("Invalid User ID.", CommonFunctions.GetCaption(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(user.GetAttribute("password").Trim().ToUpper() != txtPassword.Text.Trim().ToUpper())
            {
                txtPassword.Focus();
                MessageBox.Show("Invalid Password.", CommonFunctions.GetCaption(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CommonFunctions.CurrentLoggedInUser = user;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            UserProfile userProfile = new UserProfile(null);
            userProfile.ShowDialog();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            CommonFunctions.SetFormInfo(this);
        }
    }
}
