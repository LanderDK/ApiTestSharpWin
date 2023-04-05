using BlitzWare;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlitzWare
{
    public partial class Login : Form
    {
        private bool dragging = false;
        private Point startPoint = new Point(0, 0);

        public Login()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void username_TextChanged(object sender, EventArgs e)
        {

        }

        private void password_TextChanged(object sender, EventArgs e)
        {

        }

        private void login_Click(object sender, EventArgs e)
        {
            if (rememberMeCheckBox.Checked)
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + API.OnProgramStart.Name, "Username", username.Text);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + API.OnProgramStart.Name, "Password", password.Text);
            }
            if (!rememberMeCheckBox.Checked)
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + API.OnProgramStart.Name, "Username", "");
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + API.OnProgramStart.Name, "Password", "");
            }
            if (API.Login(username.Text, password.Text))
            {
                //Put code here of what you want to do after successful login
                MessageBox.Show("Welcome back, " + API.User.Username + "!" + "\n\nYou have successfully logged in!", API.OnProgramStart.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Main main = new Main();
                main.Show();
                this.Hide();
            }
        }

        private void register_Click(object sender, EventArgs e)
        {
            this.Hide();
            Register register = new Register();
            register.Show();
        }

        private void extendSub_Click(object sender, EventArgs e)
        {
            this.Hide();
            ExtendSub extend = new ExtendSub();
            extend.Show();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void Login_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void Login_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void Login_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X-this.startPoint.X, p.Y-this.startPoint.Y);
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
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

        private void Login_Load(object sender, EventArgs e)
        {
            string keyName = @"HKEY_CURRENT_USER\Software\BlitzWare\" + API.OnProgramStart.Name;
            string valueName = "Username";
            if (Registry.GetValue(keyName, valueName, null) == null)
            {
                //code if key Not Exist
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + API.OnProgramStart.Name, "Username", "");
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + API.OnProgramStart.Name, "Password", "");
            }
            else
            {
                //code if key Exist
            }

            var Username = Registry.GetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + API.OnProgramStart.Name, "Username", null);
            var Password = Registry.GetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + API.OnProgramStart.Name, "Password", null);
            if ((Username.Equals("") || Username == "") && (Password.Equals("") || Password == ""))
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + API.OnProgramStart.Name, "Username", "");
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + API.OnProgramStart.Name, "Password", "");
            }
            if ((!Username.Equals("") || Username != "") && (!Password.Equals("") || Password != ""))
            {
                username.Text = (string)Username;
                password.Text = (string)Password;
            }
            if (!API.ApplicationSettings.freeMode)
            {
                button4.Visible = true;
            }
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
    }
}
