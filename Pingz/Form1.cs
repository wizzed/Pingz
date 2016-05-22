using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Media;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using Pingz.Properties;
using Microsoft.VisualBasic;
using System.Configuration;

namespace Pingzz
{
    public partial class Pingz : Form
    {
        public Pingz()
        {
            InitializeComponent();
        }
        Ping p = new Ping();
        String[] pings;
        string str = "";
        int count = 0;
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private void Form1_Load(object sender, EventArgs e)
        {
            pings = new String[Settings.Default.PingzSize];
            this.Size = new Size(25, (int)((13.05f * (float)pings.Length)+0.5));
            this.Location = Settings.Default.PingzLocation;
            timer1.Interval = Settings.Default.PingzDelay;

            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("By WizzeD");
            trayMenu.MenuItems.Add("Change Size", changeSize);
            trayMenu.MenuItems.Add("Change Delay", changeDelay);
            trayMenu.MenuItems.Add("Change Ping Address", changePing);
            trayMenu.MenuItems.Add("Exit", OnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Pingz";
            trayIcon.Icon = new Icon(this.Icon, 40, 40);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            trayIcon.MouseClick += new MouseEventHandler(trayIcon_MouseClick);

            this.MouseWheel += new MouseEventHandler(MouseWheel_opacity);
            label1.MouseWheel += new MouseEventHandler(MouseWheel_opacity);
            doPing();
        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            doPing();
               
        }
        public void doPing()
        {
            try
            {
                PingReply reply = p.Send(Settings.Default.PingzURL, 3000);
                if (reply.Status == IPStatus.Success)
                {
                    if (count > pings.Length - 1)
                    {
                        for (int i = 0; i < pings.Length - 1; i++)
                        {
                            pings[i] = pings[i + 1];
                        }
                        pings[pings.Length - 1] = reply.RoundtripTime.ToString();
                    }
                    else
                    {
                        pings[count] = reply.RoundtripTime.ToString();
                    }
                    count++;
                    if (count % 5 == 0)
                    {
                        this.TopMost = false;
                        this.TopMost = true;
                    }
                    for (int ii = 0; ii < pings.Length; ii++)
                    {
                        str += pings[ii] + "\n";
                    }
                    label1.Text = str;
                    str = null;
                    str = "";
                }
            }
            catch { }
        }
        #region drag to move window
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }

        }
        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        #endregion

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                Settings.Default.PingzLocation = new System.Drawing.Point(this.Location.X, this.Location.Y);
                Settings.Default.Save();
                Settings.Default.Upgrade();
                trayIcon.Visible = false;
                this.Close();
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            Settings.Default.PingzLocation = new System.Drawing.Point(this.Location.X, this.Location.Y);
            Settings.Default.Save();
            trayIcon.Visible = false;
            this.Close();
        }

        private void changeSize(object sender, EventArgs e)
        {
            string value = Settings.Default.PingzSize.ToString();
            int parsedValue = Settings.Default.PingzSize;
            if (InputBox.Show("", "&Enter a new size:", ref value) == DialogResult.OK)
            {
                if (int.TryParse(value, out parsedValue) == true)
                {
                    if (parsedValue < 1)
                        parsedValue = 1;
                }
                else
                {
                    parsedValue = Settings.Default.PingzSize;
                }

                Settings.Default.PingzSize = parsedValue;
                pings = new String[Settings.Default.PingzSize];
                count = 0;
                label1.Text = "";
                this.Size = new Size(25, (int)(13.25 * (float)pings.Length + 0.5));
            }
            Settings.Default.Save();
        }

        private void changeDelay(object sender, EventArgs e)
        {
            string value = Settings.Default.PingzDelay.ToString();
            int parsedValue = Settings.Default.PingzDelay;
            if (InputBox.Show("", "&Enter a new delay(ms):", ref value) == DialogResult.OK)
            {
                if (int.TryParse(value, out parsedValue) == true)
                {
                    if (parsedValue < 500)
                        parsedValue = 500;
                }
                else
                {
                    parsedValue = Settings.Default.PingzDelay;
                }

                Settings.Default.PingzDelay = parsedValue;
                timer1.Interval = parsedValue;
            }
            Settings.Default.Save();
        }

        private void changePing(object sender, EventArgs e)
        {
            string value = Settings.Default.PingzURL.ToString();
            if (InputBox.Show("", "&Enter a IP to Ping:( Current: "+ value +")", ref value) == DialogResult.OK)
            {
                if (value.Trim() == "")
                {
                    value = "www.google.com";
                }

                Settings.Default.PingzURL = value;
            }
            Settings.Default.Save();
        }

        private void MouseWheel_opacity(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                if (e.Delta < 0)
                {
                    if (this.Opacity > 0.3)
                    {
                        this.Opacity += (float)e.Delta / 120 / 5;
                    }
                }
                else
                {
                    this.Opacity += (float)e.Delta / 120 / 5;
                }
            }

            this.Opacity = Math.Max(this.Opacity, 0.3);
        }

        private void trayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            this.TopMost = false;
            this.TopMost = true;
        }    
    }
}
