using System;

namespace WinFormsApp1
{
    public class TSPLPrintResult
    {
        public byte[] ImageData { get; set; }
        public int StartX { get; set; }        // в точках
        public int StartY { get; set; }        // в точках
        public int WidthBytes { get; set; }    // ширина в байтах
        public int HeightPoints { get; set; }  // высота в точках
        public int GraphicsMode { get; set; }  // 0-2
    }

    public class TSPLImageProcessor
    {

    }
}