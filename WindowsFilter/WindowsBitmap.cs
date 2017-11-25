using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFilter
{
    /// <summary>
    /// WindowsBitmap contains necessary static method to grab image from a given windows handle and performs some graphic processing.
    /// </summary>
    class WindowsBitmap
    {
      
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);
        /*
         * ratio is used when Users set scale for all program in windows display. For my case, I set scale as 1.5 and thus one need to set ratio_x or ratio_y to that value to correctly grab full screen image.
         */
        public static double ratio_y0 = 1.5;
        public static double ratio_x0 = 1.5;
        public static double ratio_y = ratio_x0;
        public static double ratio_x = ratio_y0;
        private static unsafe void setColor(byte*color, uint c)
        {
            uint* color_ = (uint*)color;
            color_[0] = c;
        }
        private static unsafe void setColor(byte* color, byte r,byte g,byte b)
        {
            color[1] = r;
            color[2] = g;
            color[3] = b;
        }
        public static unsafe Bitmap unsafeExpand(Bitmap b, int thred = 170, bool IsInvert = false, int dx = 2, int dy=2)
        {
           

            BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, b.PixelFormat);

            byte bitsPerPixel = 32;
            byte bytesPerPixel = (byte) (bitsPerPixel / 8);
            uint front_color = 0xFF000000;
            uint background_color = 0xFFFFFFFF;
            if(IsInvert)
            {
                front_color = 0xFFFFFFFF;
                background_color = 0xFF000000;
            }
            /*This time we convert the IntPtr to a ptr*/
            byte* scan0 = (byte*)bData.Scan0.ToPointer();

            for (int i = dx; i < bData.Height; ++i)
            {
                for (int j = dy; j < bData.Width; ++j)
                {
                    byte* data = scan0 + i * bData.Stride + j *bytesPerPixel;
                   
                    if (data[1]+data[2]+data[3]<thred*3)
                  
                    {
                        setColor(data,front_color);
                        for(int x=0;x<dx;x++)
                            for (int y=0; y<dy; y++)
                            {
                                byte* data_nearby = scan0 + (i-x) * bData.Stride + (j-y) * bytesPerPixel;
                                setColor(data_nearby, front_color);
                            }
                    }
                    else
                    {
                        setColor(data, background_color);
                    }
                    //data is a pointer to the first byte of the 3-byte color data
                }
            }

            b.UnlockBits(bData);

            return b;
        }
        public static Bitmap grabWindowBitmap(IntPtr hwnd)
    {
            try
            {
                RECT rc;
                GetWindowRect(hwnd, out rc);

                Bitmap bmp = new Bitmap((int)Math.Round(rc.Width * ratio_x), (int)Math.Round(rc.Height * ratio_y), PixelFormat.Format32bppArgb);
                Graphics gfxBmp = Graphics.FromImage(bmp);
                IntPtr hdcBitmap = gfxBmp.GetHdc();

                PrintWindow(hwnd, hdcBitmap, 0);

                gfxBmp.ReleaseHdc(hdcBitmap);
                gfxBmp.Dispose();
                //bmp = getBinaryImage(bmp);
                return bmp;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                Bitmap bmp = new Bitmap(100, 100, PixelFormat.Format32bppArgb);
                return bmp;
            }
    }

    

}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    private int _Left;
    private int _Top;
    private int _Right;
    private int _Bottom;

    public RECT(RECT Rectangle) : this(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom)
    {
    }
    public RECT(int Left, int Top, int Right, int Bottom)
    {
        _Left = Left;
        _Top = Top;
        _Right = Right;
        _Bottom = Bottom;
    }

    public int X
    {
        get { return _Left; }
        set { _Left = value; }
    }
    public int Y
    {
        get { return _Top; }
        set { _Top = value; }
    }
    public int Left
    {
        get { return _Left; }
        set { _Left = value; }
    }
    public int Top
    {
        get { return _Top; }
        set { _Top = value; }
    }
    public int Right
    {
        get { return _Right; }
        set { _Right = value; }
    }
    public int Bottom
    {
        get { return _Bottom; }
        set { _Bottom = value; }
    }
    public int Height
    {
        get { return _Bottom - _Top; }
        set { _Bottom = value + _Top; }
    }
    public int Width
    {
        get { return _Right - _Left; }
        set { _Right = value + _Left; }
    }
    public Point Location
    {
        get { return new Point(Left, Top); }
        set
        {
            _Left = value.X;
            _Top = value.Y;
        }
    }
    public Size Size
    {
        get { return new Size(Width, Height); }
        set
        {
            _Right = value.Width + _Left;
            _Bottom = value.Height + _Top;
        }
    }

    public static implicit operator Rectangle(RECT Rectangle)
    {
        return new Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height);
    }
    public static implicit operator RECT(Rectangle Rectangle)
    {
        return new RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
    }
    public static bool operator ==(RECT Rectangle1, RECT Rectangle2)
    {
        return Rectangle1.Equals(Rectangle2);
    }
    public static bool operator !=(RECT Rectangle1, RECT Rectangle2)
    {
        return !Rectangle1.Equals(Rectangle2);
    }

    public override string ToString()
    {
        return "{Left: " + _Left + "; " + "Top: " + _Top + "; Right: " + _Right + "; Bottom: " + _Bottom + "}";
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public bool Equals(RECT Rectangle)
    {
        return Rectangle.Left == _Left && Rectangle.Top == _Top && Rectangle.Right == _Right && Rectangle.Bottom == _Bottom;
    }

    public override bool Equals(object Object)
    {
        if (Object is RECT)
        {
            return Equals((RECT)Object);
        }
        else if (Object is Rectangle)
        {
            return Equals(new RECT((Rectangle)Object));
        }

        return false;
    }
}
}
