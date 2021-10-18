using System;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;
using System.Threading;
namespace LightBoxLedController
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            SinRed();
        }


        static void SendUdp(int srcPort, string dstIp, int dstPort, byte[] data)
        {
            using (UdpClient c = new UdpClient(srcPort))
            {
                c.Send(data, data.Length, dstIp, dstPort);
            }
        }

        static void SinRed()
        {

            double x = 0d;
            double y = 0d;
            double speed = 0.1d;
            Color mainColor = new Color(255, 255, 255);
            Color c = new Color(255, 255, 255);

            int interval = 1;

            while(true)
            {
                y = Math.Sin(x);

                c.r = (int)(((y + 1d) / 2d) * mainColor.r);
                c.g = (int)(((y + 1d) / 2d) * mainColor.g);
                c.b = (int)(((y + 1d) / 2d) * mainColor.b);
                c.UpdateHex();
                string s = "#" + c.hex + c.hex + c.hex + c.hex;

                SendUdp(4211, "192.168.86.133", 4211, Encoding.ASCII.GetBytes(s));

                x +=  speed;             

                Thread.Sleep(1);
            }
        }
    }
}

public class Color
{
    public int r { set; get; }
    public int g { set; get; }
    public int b { set; get; }
    public string hex { private set; get; }

    public Color(int r, int g, int b)
    {
        if (r < 0) this.r = 0; else if (r > 255) this.r = 255;
        if (g < 0) this.g = 0; else if (g > 255) this.g = 255;
        if (b < 0) this.b = 0; else if (b > 255) this.b = 255;

        this.r = r;
        this.g = g;
        this.b = b;

        UpdateHex();
    }

    public Color()
    {
        r = 255;
        g = 255;
        b = 255;
        UpdateHex();
    }

    public void SetHSV(double h, double S, double V)
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

            this.r = Clamp((int)(R * 255.0));
            this.g = Clamp((int)(G * 255.0));
            this.b = Clamp((int)(B * 255.0));
            UpdateHex();
        }
    }

    private int Clamp(int i)
    {
        if (i < 0) return 0;
        if (i > 255) return 255;
        return i;
    }

    public void UpdateHex()
    {
        this.hex = this.r.ToString("X2") + this.g.ToString("X2") + this.b.ToString("X2");
    }
}