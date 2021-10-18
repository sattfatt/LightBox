using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;

namespace LedGui
{
    public partial class Form1 : Form
    {
        
        public Task t;
        public bool ctrl_pressed = false;
        public Form1()
        {
            InitializeComponent();
            InitializePanelsList();

            //b1 = new Bezier(panel1);
            t = new Task(() => Player.PlayLoop(this.panels));
            t.Start();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                if (Player.play == false)
                {
                    Player.Play();
                }
                else
                {
                    Player.Pause();
                }
            }
            if (e.KeyCode == Keys.Delete)
            {
                BezierPanel.CurrentFocusPanel.bcurve.DeleteSelected();
            }

            if(e.KeyCode == Keys.Control)
            {
                ctrl_pressed = true;
            }

            if(e.KeyCode == Keys.C && e.Control)
            {
                Debug.WriteLine("Copying.");
                BezierPanel.CopyCurve();
            }

            if (e.KeyCode == Keys.V && e.Control)
            {
                Debug.WriteLine("Pasting");
                BezierPanel.PasteCurve();
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Control)
            {
                ctrl_pressed = false;
            }
        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void play_button_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;

            if (Player.play == false)
            {
                Player.Play();
                b.Text = "Pause";
            }
            else
            {
                Player.Pause();
                b.Text = "Play";
            }
        }

        private void speedBar_Scroll(object sender, EventArgs e)
        {
            var bar = (TrackBar)sender;
            Player.SetSpeed(bar.Value);
            Debug.WriteLine(bar.Value);
        }

        private void connect_button_Click(object sender, EventArgs e)
        {
            Player.SetIp(maskedTextBox1.Text);
        }

        private void restart_button_Click(object sender, EventArgs e)
        {
            Player.Reset();
        }

        
    }
}
