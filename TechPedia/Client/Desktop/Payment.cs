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
namespace App.TechPedia
{
    public partial class Payment : Form
    {
        private bool m_NewFlag = false;

        public Payment(bool newFlag)
        {
            InitializeComponent();

            m_NewFlag = newFlag;
        }

        private void Payment_Load(object sender, EventArgs e)
        {
            CommonFunctions.SetFormInfo(this);

            FillList();

            if (m_NewFlag)
            {
                btnNew_Click(btnNew, null);
            }
            else
            {
                if (lstPaymentList.Items.Count > 0)
                    lstPaymentList.SelectedIndex = 0;
            }
        }

        private void lstPaymentList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstPaymentList.SelectedItem == null)
                return;

            string paymentID = lstPaymentList.SelectedItem.ToString();

            XElement paymentInfo = CommonFunctions.CurrentLoggedInUser.Elements("paymentinfo").FirstOrDefault(pred => pred.GetAttribute("id").Trim().ToUpper() == paymentID.Trim().ToUpper());

            if (paymentInfo == null)
                return;

            txtCardName.Text = paymentInfo.GetAttribute("id");
            txtName.Text = paymentInfo.GetAttribute("name");
            txtCardNumber.Text = paymentInfo.GetAttribute("number");
            txtAddress1.Text = paymentInfo.GetAttribute("address1");
            txtAddress2.Text = paymentInfo.GetAttribute("address2");
            txtCity.Text = paymentInfo.GetAttribute("city");
            txtZip.Text = paymentInfo.GetAttribute("zip");
            txtState.Text = paymentInfo.GetAttribute("state");

            string[] expdate = paymentInfo.GetAttribute("expdate").Split('/');
            txtMonth.Value = int.Parse(expdate[0]);
            txtYear.Value = int.Parse(expdate[1]);

            string type = paymentInfo.GetAttribute("type").Trim().ToUpper();
            if (type == "VISA")
                radVisa.Checked = true;
            else if (type == "MASTER CARD")
                radMaster.Checked = true;
            else if (type == "DISCOVER")
                radDiscover.Checked = true;
            else if (type == "AMERICAN EXPRESS")
                radAmericanExpress.Checked = true;

            btnNew.Text = "&New";
            m_NewFlag = false;
        }

        private void FillList()
        {
            IEnumerable<XElement> paymentInfos = CommonFunctions.CurrentLoggedInUser.Elements("paymentinfo");

            lstPaymentList.Items.Clear();
            foreach (XElement paymentInfo in paymentInfos)
            {
                lstPaymentList.Items.Add(paymentInfo.GetAttribute("id"));
            }
        }

        private void Clear()
        {
            txtCardName.Clear();
            txtName.Clear();
            txtCardNumber.Clear();
            txtAddress1.Clear();
            txtAddress2.Clear();
            txtCity.Clear();
            txtState.Clear();
            txtZip.Clear();

            radVisa.Checked = true;
            txtMonth.ResetText();
            txtYear.ResetText();
        }

        private void Save()
        {
            XElement paymentInfo = CommonFunctions.CurrentLoggedInUser.Elements("paymentinfo").FirstOrDefault(pred => pred.GetAttribute("id").Trim().ToUpper() == txtCardName.Text.Trim().ToUpper());

            string type = string.Empty;
            string expDate = string.Empty;

            if (radVisa.Checked)
                type = "Visa";
            else if (radMaster.Checked)
                type = "Master Card";
            else if (radAmericanExpress.Checked)
                type = "American Express";
            else if (radDiscover.Checked)
                type = "Discover";

            expDate = txtMonth.Value + "/" + txtYear.Value;

            if (m_NewFlag)
            {
                if (paymentInfo != null)
                {
                    MessageBox.Show("Card name already exists.", CommonFunctions.GetCaption(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                paymentInfo = new XElement("paymentinfo");

                CommonFunctions.CurrentLoggedInUser.Add(paymentInfo);
            }
            else
            {
                if (paymentInfo == null)
                {
                    MessageBox.Show(txtCardName.Text + " not found.");
                    return;
                }
            }


            paymentInfo.SetAttributeValue("id", txtCardName.Text);
            paymentInfo.SetAttributeValue("name", txtName.Text);
            paymentInfo.SetAttributeValue("number", txtCardNumber.Text);
            paymentInfo.SetAttributeValue("address1", txtAddress1.Text);
            paymentInfo.SetAttributeValue("address2", txtAddress2.Text);
            paymentInfo.SetAttributeValue("city", txtCity.Text);
            paymentInfo.SetAttributeValue("state", txtState.Text);
            paymentInfo.SetAttributeValue("zip", txtZip.Text);
            paymentInfo.SetAttributeValue("type", type);
            paymentInfo.SetAttributeValue("expdate", expDate);

            DataAccessLayer.DataAccessLayer.SaveUsers();
            MessageBox.Show("Saved Successful.", CommonFunctions.GetCaption(), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            XElement paymentInfo = CommonFunctions.CurrentLoggedInUser.Elements("paymentinfo").FirstOrDefault(pred => pred.GetAttribute("id").Trim().ToUpper() == txtCardName.Text.Trim().ToUpper());
            paymentInfo.Remove();

            DataAccessLayer.DataAccessLayer.SaveUsers();

            FillList();

            Clear();

            if (lstPaymentList.Items.Count > 0)
                lstPaymentList.SelectedIndex = 0;

            MessageBox.Show("Delete Successful.", CommonFunctions.GetCaption(), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            Clear();

            m_NewFlag = true;
            btnNew.Text = "&Cancel";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            int selectindex = lstPaymentList.SelectedIndex;

            Save();

            btnNew.Text = "&New";
            m_NewFlag = false;

            FillList();

            if (lstPaymentList.Items.Count > 0 && selectindex > -1)
                lstPaymentList.SelectedIndex = selectindex;
        }
    }
}
