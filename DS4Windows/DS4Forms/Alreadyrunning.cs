﻿using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DS4Windows
{
    public partial class Alreadyrunning : Form
    {
        Stopwatch sw;

        public Alreadyrunning()
        {
            InitializeComponent();
            WindowState = FormWindowState.Minimized;
            Hide();
            var t = new Timer();
            t.Start();
            t.Tick += t_Tick;
            sw = new Stopwatch();
            sw.Start();        
        }

        void t_Tick(object sender, EventArgs e)
        {
            if (sw.ElapsedMilliseconds >= 10)
                Close();
        }
    }
}
