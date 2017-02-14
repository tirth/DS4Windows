﻿using System;
using System.Drawing;
using System.Windows.Forms;


namespace DS4Windows
{
    public partial class DupBox : Form
    {
        public string oldfilename;
        DS4Form yes;
        public DupBox(string name, DS4Form mainwindow)
        {
            InitializeComponent();
            oldfilename = name;
            yes = mainwindow;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (tBProfile.Text != null && tBProfile.Text != "" && !tBProfile.Text.Contains("\\") && !tBProfile.Text.Contains("/") && !tBProfile.Text.Contains(":") && !tBProfile.Text.Contains("*") && !tBProfile.Text.Contains("?") && !tBProfile.Text.Contains("\"") && !tBProfile.Text.Contains("<") && !tBProfile.Text.Contains(">") && !tBProfile.Text.Contains("|"))
            {
                System.IO.File.Copy(Global.appdatapath + "\\Profiles\\" + oldfilename + ".xml", Global.appdatapath + "\\Profiles\\" + tBProfile.Text + ".xml", true);
                yes.RefreshProfiles();
                Close();
            }
            else
                MessageBox.Show(Properties.Resources.ValidName, Properties.Resources.NotValid, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);            
        }

        private void tBProfile_TextChanged(object sender, EventArgs e)
        {
            if (tBProfile.Text != null && tBProfile.Text != "" && !tBProfile.Text.Contains("\\") && !tBProfile.Text.Contains("/") && !tBProfile.Text.Contains(":") && !tBProfile.Text.Contains("*") && !tBProfile.Text.Contains("?") && !tBProfile.Text.Contains("\"") && !tBProfile.Text.Contains("<") && !tBProfile.Text.Contains(">") && !tBProfile.Text.Contains("|"))
                tBProfile.ForeColor = SystemColors.WindowText;
            else
                tBProfile.ForeColor = SystemColors.GrayText;
        }

        private void tBProfile_Enter(object sender, EventArgs e)
        {
            if (tBProfile.Text == "<" + Properties.Resources.TypeNewName + ">")
                tBProfile.Text = "";
        }

        private void tBProfile_Leave(object sender, EventArgs e)
        {
            if (tBProfile.Text == "")
                tBProfile.Text = "<" + Properties.Resources.TypeNewName + ">";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
