using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LedGui
{
    public static class Player
    {
        public static bool play = false;
        private static float speed = 1f;
        public static float minspeed = 0f;
        public static float maxspeed = 5f;
        public static object playlock = new object();
        private static string Ip = "192.168.86.136";
        private static float t = 0;

        public static void PlayLoop(List<BezierPanel> panels)
        {
            while (true)
            {
                float interval = 0.0025f * speed;
                if (play)
                {
                    List<string> colors = new List<string>();
                    for (int i = 0; i < panels.Count; i += 3)
                    {
                        float h = (1 - panels[i].bcurve.Evaluate(t).Y);
                        float s = (1 - panels[i + 1].bcurve.Evaluate(t).Y);
                        float v = (1 - panels[i + 2].bcurve.Evaluate(t).Y);

                        int r, g, b;

                        HSVtoRGB(h, s, v, out r, out g, out b);

                        colors.Add((r).ToString("X2") + (g).ToString("X2") + (b).ToString("X2"));
                    }
                    string S = "#" + colors[0] + colors[1] + colors[2] + colors[3];
                    SendUdp(4211, Ip, 4211, Encoding.ASCII.GetBytes(S));
                    //Debug.WriteLine("t is {0}", t);
                    t += interval;
                    if (t >= 1) t = 0f;
                    Thread.Sleep(1);
                }
            }
        }

        public static void Play()
        {
            lock (playlock)
            {
                play = true;
            }
        }

        public static void Pause()
        {
            lock (playlock)
            {
                play = false;
            }
        }

        public static void SetSpeed(float spd)
        {
            lock (playlock)
            {
                speed = spd.Map(0f, 100f, minspeed, maxspeed);
            }
        }

        public static void SetIp(string ip)
        {
            Ip = ip;
        }

        public static void Reset()
        {
            lock (playlock)
            {
                t = 0f;
            }
        }

        private static void SendUdp(int srcPort, string dstIp, int dstPort, byte[] data)
        {
            using (UdpClient c = new UdpClient(srcPort))
            {
                c.Send(data, data.Length, dstIp, dstPort);
            }
        }

        private static int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }

        private static void HSVtoRGB(double h, double S, double V, out int r, out int g, out int b)
        {
            // hsv should be 0 to 1
            // rgb out is 0 to 255

            double H = h * 360;

            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };

            double R, G, B;

            if (V <= 0)
            {
                R = G = B = 0;
            }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {
                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;
                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;
                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;
                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;
                    default:
                        R = G = B = V;
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
        }
    }
}
