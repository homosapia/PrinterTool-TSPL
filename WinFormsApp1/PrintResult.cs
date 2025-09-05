using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    public class PrintResult
    {
        public byte[] ImageData { get; set; }
        public int PrintStartX { get; set; }
        public int PrintStartY { get; set; }
        public int ImageWidthBytes { get; set; }
        public int ImageHeightPoints { get; set; }
        public int GraphicsMode { get; set; }
    }
}
