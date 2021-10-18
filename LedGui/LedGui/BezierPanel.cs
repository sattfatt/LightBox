using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace LedGui
{
    public class BezierPanel
    {
        public Panel panel;
        public Bezier bcurve;
        public bool isDragging;
        public static BezierPanel CurrentFocusPanel;
        private static List<Handle> hclipboard;

        public BezierPanel(Panel p, string panel_name, int ylocation)
        {
            this.panel = p;
            this.panel.BackColor = System.Drawing.Color.DimGray;
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(3, ylocation);
            this.panel.Name = panel_name;
            this.panel.Size = new System.Drawing.Size(1078, 94);
            this.panel.TabIndex = 0;
            this.panel.SizeChanged += new System.EventHandler(this.panel_SizeChanged);
            this.panel.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_Paint);
            this.panel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel_MouseDown);
            this.panel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel_MouseMove);
            this.panel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel_MouseUp);
            this.panel.MouseEnter += Panel_MouseEnter;
            //this.panel.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.panel_PreviewKeyDown);

            bcurve = new Bezier(panel);
            hclipboard = new List<Handle>();
        }

        private void Panel_MouseEnter(object sender, EventArgs e)
        {
            CurrentFocusPanel = this;
        }

        public void panel_SizeChanged(object sender, EventArgs e)
        {
            var P = (Panel)sender;
            P.Refresh();
        }

        public void panel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            bcurve.DrawCurve(e.Graphics);
        }

        public void panel_MouseDown(object sender, MouseEventArgs e)
        {
            this.isDragging = true;
            bcurve.AddPointOrSelect(e.X, e.Y);
            var P = (Panel)sender;
            P.Refresh();
        }

        public void panel_MouseUp(object sender, MouseEventArgs e)
        {
            this.isDragging = false;
        }

        public void panel_MouseMove(object sender, MouseEventArgs e)
        {
            var P = (Panel)sender;
            if (this.isDragging)
            {
                bcurve.UpdateSelected(e.X, e.Y);
                P.Refresh();
            }
        }

        public static void CopyCurve()
        {
            List<Handle> h = CurrentFocusPanel.bcurve.Handles;
            hclipboard.Clear();
            for (int i = 0; i < h.Count; i++)
            {
                hclipboard.Add(new Handle(h[i].MainPoint, h[i].RightHandle, h[i].LeftHandle));
            }
        }

        public static void PasteCurve()
        {
            List<Handle> h = CurrentFocusPanel.bcurve.Handles;
            h.Clear();
            for (int i = 0; i < hclipboard.Count; i++)
            {
                h.Add(new Handle(hclipboard[i].MainPoint, hclipboard[i].RightHandle, hclipboard[i].LeftHandle));
            }
            CurrentFocusPanel.panel.Refresh();
        }
    }
}
