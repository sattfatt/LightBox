using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace LedGui
{
    public class Bezier
    {
        private readonly object handlesLock = new object();

        public List<Handle> Handles;
        public Pen handlesPen;
        public Pen selectedPen;
        public Pen linePen;
        public float resolution;
        public float pointSize;
        private Panel _panel;
        
        private Handle SelectedHandle;
        private HandleType SelectedControl;
        private int SelectedIndex;

        public float SelectionPadding;

        public Bezier(Panel panel)
        {
            Handles = new List<Handle>();

            Handle h = new Handle(new Vector2(0f, 0.5f), new Vector2(0.05f, 0.5f), new Vector2(-0.05f, 0.5f));
            Handle h1 = new Handle(new Vector2(1f, 0.5f), new Vector2(1.05f, 0.5f), new Vector2(0.95f, 0.5f));
            Handles.Add(h);
            Handles.Add(h1);

            handlesPen = new Pen(Color.Orange, 1f);
            selectedPen = new Pen(Color.Cyan, 1f);
            linePen = new Pen(Color.WhiteSmoke, 1f);

            handlesPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            selectedPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            linePen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            pointSize = 5f;
            SelectionPadding = 5f;
            _panel = panel;

            SelectedHandle = h;
            SelectedControl = HandleType.MainPoint;
            SelectedIndex = 0;
            resolution = 100f;
        }

        public Vector2 Evaluate(float t)
        {
            lock(handlesLock)
            {
                for (int i = 0; i < Handles.Count - 1; i++)
                {
                    float alpha1 = 1f / (Handles.Count - 1) * i;
                    float alpha2 = 1f / (Handles.Count - 1) * (i + 1);
                    if(alpha1 <= t && t <= alpha2)
                    {
                        // calculate the new parameter for this chunk
                        float p = t.Map(alpha1, alpha2, 0f, 1f);
                        // here evaluate bezier using handle i and handle i + 1, then break out.
                        return DeCasteljaus(p, Handles[i], Handles[i+1]);
                    }
                }
                return Vector2.Zero;
            }
            
        }

        private Vector2 DeCasteljaus(float t, Handle h1, Handle h2)
        {
            Vector2 A = h1.MainPoint;
            Vector2 B = h1.RightHandle;
            Vector2 C = h2.LeftHandle;
            Vector2 D = h2.MainPoint;

            float oneMinust = 1f - t;

            Vector2 Q = oneMinust * A + t * B;
            Vector2 R = oneMinust * B + t * C;
            Vector2 S = oneMinust * C + t * D;
            Vector2 P = oneMinust * Q + t * R;
            Vector2 T = oneMinust * R + t * S;
            Vector2 U = oneMinust * P + t * T;

            return U;
        }

        public void DrawCurve(Graphics g)
        {
            
            DrawHandles(g);

            float interval = 1f / resolution;

            float t = 0f;

            Vector2 s = new Vector2(_panel.Width, _panel.Height);

            for (int i = 0; i < resolution - 1; i++)
            {
                g.DrawLine(linePen, (Evaluate(t) * s).toPointF(), (Evaluate(t + interval) * s).toPointF());
                t += interval;
            }
        }

        private void DrawHandles(Graphics g)
        {
            Vector2 s = new Vector2(_panel.Width, _panel.Height);
            for (int i = 0; i < Handles.Count; i++)
            {
                g.DrawLine(handlesPen, (Handles[i].MainPoint * s).toPointF(), (Handles[i].LeftHandle * s).toPointF());
                g.DrawLine(handlesPen, (Handles[i].MainPoint * s).toPointF(), (Handles[i].RightHandle * s).toPointF());

                Pen P;
                P = SelectedHandle == Handles[i] ? selectedPen : handlesPen;

                g.DrawEllipse(P,_panel.Width * Handles[i].LeftHandle.X - 0.5f * pointSize,
                                _panel.Height * Handles[i].LeftHandle.Y - 0.5f * pointSize, pointSize, pointSize);
                g.DrawEllipse(P, _panel.Width * Handles[i].MainPoint.X - 0.5f * pointSize,
                                _panel.Height * Handles[i].MainPoint.Y - 0.5f * pointSize, pointSize, pointSize);
                g.DrawEllipse(P, _panel.Width * Handles[i].RightHandle.X - 0.5f * pointSize,
                                _panel.Height * Handles[i].RightHandle.Y - 0.5f * pointSize, pointSize, pointSize);
            }

        }

        public void AddPointOrSelect(float x, float y)
        {
            if(!CheckSelected(x, y))
            {
                Handles.Add(new Handle(new Vector2(x / _panel.Width, y / _panel.Height)));
                Handles = Handles.OrderBy(x => x.MainPoint.X).ToList();
                CheckSelected(x, y);
            }
        }

        public void UpdateSelected(float x, float y)
        {
            lock (handlesLock)
            { 
                float Xx = x / _panel.Width;
                float Yy = y / _panel.Height;

                Vector2 rightVect = SelectedHandle.RightHandle - SelectedHandle.MainPoint;
                Vector2 leftVect = SelectedHandle.LeftHandle - SelectedHandle.MainPoint;

                if(SelectedIndex != 0 && SelectedIndex != Handles.Count-1)
                {
                    Vector2 rightpointC = Handles[SelectedIndex + 1].LeftHandle;
                    Vector2 leftpointC = Handles[SelectedIndex - 1].RightHandle;
                    switch (SelectedControl)
                    {
                        case HandleType.LeftHandle:
                            SelectedHandle.LeftHandle = Vector2.Clamp(new Vector2(Xx, Yy), new Vector2(leftpointC.X, 0), new Vector2(SelectedHandle.MainPoint.X, 1));
                            //if(SelectedHandle.LeftHandle.X > SelectedHandle.MainPoint.X)
                            //{
                            //    SelectedHandle.LeftHandle = new Vector2(SelectedHandle.MainPoint.X, SelectedHandle.LeftHandle.Y);
                            //}
                            break;
                        case HandleType.MainPoint:
                            SelectedHandle.MainPoint = Vector2.Clamp(new Vector2(Xx, Yy), Vector2.Zero, Vector2.One);
                            SelectedHandle.LeftHandle = Vector2.Clamp(SelectedHandle.MainPoint + leftVect, new Vector2(leftpointC.X, 0), new Vector2(SelectedHandle.MainPoint.X, 1));
                            SelectedHandle.RightHandle = Vector2.Clamp(SelectedHandle.MainPoint + rightVect, new Vector2(SelectedHandle.MainPoint.X, 0), new Vector2(rightpointC.X, 1));
                            if (Handles[SelectedIndex - 1].MainPoint.X >= Xx)
                            {
                                Handles.Reverse(SelectedIndex - 1, 2);
                                SelectedIndex--;
                            }
                            else if(Handles[SelectedIndex + 1].MainPoint.X < Xx)
                            {
                                Handles.Reverse(SelectedIndex, 2);
                                SelectedIndex++;
                            }
                            break;
                        case HandleType.RightHandle:
                            SelectedHandle.RightHandle = Vector2.Clamp(new Vector2(Xx, Yy), new Vector2(SelectedHandle.MainPoint.X, 0), new Vector2(rightpointC.X, 1));
                            break;
                        default:
                            break;
                    }
                }
                else if(SelectedIndex == 0 || SelectedIndex == Handles.Count - 1)
                {
                
                    switch (SelectedControl)
                    {
                        case HandleType.LeftHandle:
                            if(SelectedIndex == Handles.Count - 1)
                            {
                                SelectedHandle.LeftHandle = Vector2.Clamp(new Vector2(Xx, Yy), Vector2.Zero, Vector2.One);
                            }
                            break;
                        case HandleType.MainPoint:
                            SelectedHandle.MainPoint = Vector2.Clamp(new Vector2(Xx, Yy), new Vector2(SelectedHandle.MainPoint.X, 0), new Vector2(SelectedHandle.MainPoint.X, 1));
                            SelectedHandle.LeftHandle = SelectedHandle.MainPoint + leftVect;
                            SelectedHandle.RightHandle = SelectedHandle.MainPoint + rightVect;
                            break;
                        case HandleType.RightHandle:
                            if (SelectedIndex == 0)
                            {
                                SelectedHandle.RightHandle = Vector2.Clamp(new Vector2(Xx, Yy), Vector2.Zero, Vector2.One);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public bool CheckSelected(float x, float y)
        {
            Handle h;
            HandleType ht;
            for (int i = 0; i < Handles.Count; i++)
            {
                h = DeNormalize(Handles[i]);
                float dist;
                ht = h.getClosestHandle(new Vector2(x, y), out dist);

                if(dist < pointSize + SelectionPadding)
                {
                    SelectedControl = ht;
                    SelectedHandle = Handles[i];
                    SelectedIndex = i;
                    return true;
                }
            }
            return false;
        }

        private Handle DeNormalize(Handle h)
        {
            Handle dh = new Handle(new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
            Vector2 S = new Vector2(_panel.Width, _panel.Height);

            dh.LeftHandle += h.LeftHandle * S;
            dh.MainPoint += h.MainPoint * S;
            dh.RightHandle += h.RightHandle * S;

            return dh;
        }

        public void DeleteSelected()
        {
            if (SelectedIndex != 0 && SelectedIndex != Handles.Count - 1)
            {

                SelectedIndex--;
                SelectedHandle = Handles[SelectedIndex];
                SelectedControl = HandleType.MainPoint;

                Handles.Remove(Handles[SelectedIndex + 1]);
                _panel.Refresh();
            }
        }
    }

    public enum HandleType
    {
        LeftHandle,
        MainPoint,
        RightHandle
    }

    public class Handle
    {
        public Vector2 MainPoint;
        public Vector2 RightHandle;
        public Vector2 LeftHandle;

        public Handle(Vector2 point, Vector2 righthandle, Vector2 lefthandle)
        {
            MainPoint = point;
            RightHandle = righthandle;
            LeftHandle = lefthandle;
        }
        public Handle(Vector2 point)
        {
            MainPoint = point;
            RightHandle = new Vector2(MainPoint.X + 0.05f, MainPoint.Y);
            LeftHandle = new Vector2(MainPoint.X - 0.05f, MainPoint.Y);
        }

        public HandleType getClosestHandle(Vector2 point, out float distance)
        {
            float d1, d2, d3;

            d1 = Vector2.Distance(point, MainPoint);
            d2 = Vector2.Distance(point, RightHandle);
            d3 = Vector2.Distance(point, LeftHandle);

            if (d1 < d2 && d1 < d3)
            {
                distance = d1;
                return HandleType.MainPoint;
            }
            else if (d2 < d1 && d2 < d3)
            {
                distance = d2;
                return HandleType.RightHandle;
            }
            else
            {
                distance = d3;
                return HandleType.LeftHandle;
            }
        }


    }

    public class CurveEditor
    {
        private Panel _panel;
        private List<Vector2> _points;
        public Pen standardPen;
        public Pen selectedPen;
        public Pen linePen;
        public float pointSize = 2;
        public int selected;
        private float selectionPadding = 10f;
        public CurveEditor(Panel panel)
        {
            _panel = panel;
            _points = new List<Vector2>();
            _points.Add(new Vector2(0f, 0.5f));
            _points.Add(new Vector2(1f, 0.5f));
        }

        public void DrawPoints(Graphics g)
        {
            if (_points.Count > 1)
                g.DrawCurve(linePen, UnNormalizedPoints(_points).toPointF());

            for (int i = 0; i < _points.Count; i++)
            {
                Pen p;

                p = i == selected ? selectedPen : standardPen;

                g.DrawEllipse(p, (int)(_points[i].X * _panel.Width - 0.5 * pointSize), (int)(_points[i].Y * _panel.Height - 0.5 * pointSize), pointSize, pointSize);
            }
        }

        private Vector2[] UnNormalizedPoints(List<Vector2> pts)
        {
            Vector2[] output = new Vector2[pts.Count];

            for (int i = 0; i < pts.Count; i++)
            {
                output[i].X = pts[i].X * _panel.Width;
                output[i].Y = pts[i].Y * _panel.Height;
            }

            return output;
        }

        public void AddPointorSelect(float x, float y)
        {
            if (!CheckifSelected(x, y))
            {
                _points.Add(new Vector2(x / _panel.Width, y / _panel.Height));
                _points = _points.OrderBy(x => x.X).ToList();
                CheckifSelected(x, y);
            }
        }

        bool CheckifSelected(float x, float y)
        {
            Vector2[] unp = UnNormalizedPoints(_points);

            for (int i = 0; i < _points.Count; i++)
            {
                float dist = (float)Math.Sqrt((x - unp[i].X) * (x - unp[i].X) + (y - unp[i].Y) * (y - unp[i].Y));

                if (dist < pointSize + selectionPadding)
                {
                    selected = i;
                    return true;
                }

            }
            selected = -1;
            return false;
        }

        public void UpdateSelected(float x, float y)
        {
            if (selected != -1 && selected != 0 && selected != _points.Count - 1)
            {
                _points[selected] = Vector2.Clamp(Vector2.Divide(new Vector2(x, y), new Vector2(_panel.Width, _panel.Height)), Vector2.Zero, Vector2.One);


                if (_points[selected - 1].X >= x / _panel.Width)
                {
                    _points.Reverse(selected - 1, 2);
                    selected--;
                }
                else if (_points[selected + 1].X < x / _panel.Width)
                {
                    _points.Reverse(selected, 2);
                    selected++;
                }
            }
            else if (selected == 0 || selected == _points.Count - 1)
            {
                Vector2 p = new Vector2(_points[selected].X, Math.Clamp(y / _panel.Height, 0f, 1f));
                _points[selected] = p;
            }
        }
    }

    public static class Extensions
    {
        public static PointF[] toPointF(this Vector2[] a)
        {
            PointF[] b = new PointF[a.Length];

            for (int i = 0; i < a.Length; i++)
            {
                b[i] = new PointF(a[i].X, a[i].Y);
            }

            return b;
        }

        public static PointF toPointF(this Vector2 a)
        {
            PointF b = new PointF(a.X, a.Y);
            return b;
        }

        public static float Map(this float value, float from1, float from2, float to1, float to2)
        {
            return to1 + (value - from1) * (to2 - to1) / (from2 - from1);
        }
    }
}
