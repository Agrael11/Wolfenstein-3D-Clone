using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3DTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            pictureBox1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            Program.game = new Game();
            Program.game.Init();

            Thread thr = new Thread(Draw);
            thr.Start();

        }

        bool released = false;

        public void Draw()
        {


            while (!this.Visible) ;
            while (this.Visible)
            {
                Bitmap display = new Bitmap(320, 200);
                Program.game.Graphics = Graphics.FromImage(display);
                Program.game.Update();
                Program.game.Draw();
                pictureBox1.Image = display;
            }
        }

        public delegate void _setPictureBox(Bitmap bmp);
        public void SetPictureBox(Bitmap bmp)
        {
            if (pictureBox1.InvokeRequired)
            {
                try
                {
                    pictureBox1.Invoke(new _setPictureBox(SetPictureBox), bmp);
                }
                catch
                {

                }
            }
            else pictureBox1.Image = bmp;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!Program.game.Pressedkeys.Contains(e.KeyCode))
                Program.game.Pressedkeys.Add(e.KeyCode);
            if (e.Modifiers == Keys.Alt)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SwitchFullScreen();
                }
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (Program.game.Pressedkeys.Contains(e.KeyCode))
                Program.game.Pressedkeys.Remove(e.KeyCode);
        }

        bool fullscreen = false;

        public void SwitchFullScreen()
        {
            fullscreen = !fullscreen;
            if (fullscreen)
            {
                originalBounds = this.Bounds;
                this.FormBorderStyle = FormBorderStyle.None;
                this.Focus();
                this.BringToFront();
                this.TopMost = true;
                this.Bounds = Screen.PrimaryScreen.Bounds;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Normal;
                this.TopMost = false;
                this.Bounds = originalBounds;
            }
        }

        public Rectangle originalBounds;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0112)
            {
                if (m.WParam == new IntPtr(0xF030))
                {
                    if (!fullscreen) SwitchFullScreen();
                }
            }
            base.WndProc(ref m);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Audio song = new Audio("Sounds/Level1.wav", true);
            song.Play();
        }
    }
}