using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlitzWare.Properties;
using Microsoft.Win32;

namespace BlitzWare
{
    public partial class Main : Form
    {
        private bool dragging = false;
        private Point startPoint = new Point(0, 0);

        public Main()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            welcome.Text = "Welcome back, " + Program.BlitzWareAuth.userData.Username + "!";
            userid.Text = "ID: " + Program.BlitzWareAuth.userData.Id;
            username.Text = "Username: " + Program.BlitzWareAuth.userData.Username;
            expiry.Text = "Subscription Expiry: " + Program.BlitzWareAuth.userData.ExpiryDate;
            email.Text = "Email: " + Program.BlitzWareAuth.userData.Email;
            hwid.Text = "HWID: " + Program.BlitzWareAuth.userData.HWID;
            ip.Text = "IP: " + Program.BlitzWareAuth.userData.LastIP;
            lastlogin.Text = "Last Login: " + Program.BlitzWareAuth.userData.LastLogin;
        }

        private void MainR6S_Load(object sender, EventArgs e)
        {
            //picturebox.Load(User.ProfilePicture);
            welcome.Text = "Welcome back, " + Program.BlitzWareAuth.userData.Username + "!";
            userid.Text = "ID: " + Program.BlitzWareAuth.userData.Id;
            username.Text = "Username: " + Program.BlitzWareAuth.userData.Username;
            expiry.Text = "Subscription Expiry: " + Program.BlitzWareAuth.userData.ExpiryDate;
            email.Text = "Email: " + Program.BlitzWareAuth.userData.Email;
            hwid.Text = "HWID: " + Program.BlitzWareAuth.userData.HWID;
            ip.Text = "IP: " + Program.BlitzWareAuth.userData.LastIP;
            lastlogin.Text = "Last Login: " + Program.BlitzWareAuth.userData.LastLogin;
        }

        private void welcome_Click(object sender, EventArgs e)
        {

        }

        private void userid_Click(object sender, EventArgs e)
        {

        }

        private void username_Click(object sender, EventArgs e)
        {

        }

        private void expiry_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);
            }
        }

        private void label3_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void label3_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void label3_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);
            }
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void label1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);
            }
        }

        private void Main_Load_1(object sender, EventArgs e)
        {

        }

        private void Main_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void Main_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void Main_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);
            }
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this.startPoint.X, p.Y - this.startPoint.Y);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not implemented!", Program.BlitzWareAuth.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
            {
                MessageBox.Show("Please provide a code to enable 2FA!", Program.BlitzWareAuth.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Not implemented!", Program.BlitzWareAuth.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (textBox2.Text.Length == 0)
            {
                MessageBox.Show("Please provide a code to disable 2FA!", Program.BlitzWareAuth.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Not implemented!", Program.BlitzWareAuth.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
