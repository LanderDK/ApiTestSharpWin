using Microsoft.Win32;
using System;
using System.Drawing;
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
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + Program.BlitzWareAuth.AppName, "Username", username.Text);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + Program.BlitzWareAuth.AppName, "Password", password.Text);
            }
            if (!rememberMeCheckBox.Checked)
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + Program.BlitzWareAuth.AppName, "Username", "");
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + Program.BlitzWareAuth.AppName, "Password", "");
            }
            if (Program.BlitzWareAuth.Login(username.Text, password.Text, twoFactor.Text))
            {
                MessageBox.Show("Welcome back, " + Program.BlitzWareAuth.userData.Username + "!" + "\n\nYou have successfully logged in!", Program.BlitzWareAuth.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Program.BlitzWareAuth.Log(Program.BlitzWareAuth.userData.Username, "User logged in");
                // do code you want
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
            string keyName = @"HKEY_CURRENT_USER\Software\BlitzWare\" + Program.BlitzWareAuth.AppName;
            string valueName = "Username";
            if (Registry.GetValue(keyName, valueName, null) == null)
            {
                //code if key Not Exist
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + Program.BlitzWareAuth.AppName, "Username", "");
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + Program.BlitzWareAuth.AppName, "Password", "");
            }
            else
            {
                //code if key Exist
            }

            var Username = Registry.GetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + Program.BlitzWareAuth.AppName, "Username", null);
            var Password = Registry.GetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + Program.BlitzWareAuth.AppName, "Password", null);
            if ((Username.Equals("") || Username == "") && (Password.Equals("") || Password == ""))
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + Program.BlitzWareAuth.AppName, "Username", "");
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\BlitzWare\" + Program.BlitzWareAuth.AppName, "Password", "");
            }
            if ((!Username.Equals("") || Username != "") && (!Password.Equals("") || Password != ""))
            {
                username.Text = (string)Username;
                password.Text = (string)Password;
            }
            if (Program.BlitzWareAuth.appData.FreeMode == 0)
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
