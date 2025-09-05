using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    public class TsplBitmapData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int BytesPerRow { get; set; }
        public string HexData { get; set; }
        public string CompressionType { get; set; } = "0"; // 0 - без сжатия
    }

    public class PrinterSettings
    {
        public int Dpi { get; set; } = 203;
        public int PrintWidth { get; set; } // в точках (dots)
        public int PrintDensity { get; set; } = 8; // 0-15
        public string Compression { get; set; } = "0"; // 0-нет, 1-RLE
    }

    public class TsplBitmapConverter
    {
        public TsplBitmapData ConvertImageToTsplBitmap(byte[] imageBytes, PrinterSettings printerSettings)
        {
            using (var ms = new MemoryStream(imageBytes))
            using (var image = Image.FromStream(ms))
            {
                // Конвертируем изображение в 1bpp монохромное
                var monoBitmap = ConvertTo1Bpp(image);

                // Блокируем биты для работы с данными
                var bitmapData = monoBitmap.LockBits(
                    new Rectangle(0, 0, monoBitmap.Width, monoBitmap.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format1bppIndexed);

                try
                {
                    // Вычисляем размер данных
                    int bytesPerRow = (monoBitmap.Width + 7) / 8;
                    int totalBytes = bytesPerRow * monoBitmap.Height;

                    // Создаем массив для хранения данных
                    byte[] bitmapBytes = new byte[totalBytes];

                    // Копируем данные из bitmap
                    for (int y = 0; y < monoBitmap.Height; y++)
                    {
                        IntPtr rowPtr = bitmapData.Scan0 + (y * bitmapData.Stride);
                        Marshal.Copy(rowPtr, bitmapBytes, y * bytesPerRow, bytesPerRow);
                    }

                    // Конвертируем в HEX строку для TSPL
                    string hexData = BitConverter.ToString(bitmapBytes).Replace("-", "");

                    return new TsplBitmapData
                    {
                        Width = monoBitmap.Width,
                        Height = monoBitmap.Height,
                        BytesPerRow = bytesPerRow,
                        HexData = hexData,
                        CompressionType = printerSettings.Compression
                    };
                }
                finally
                {
                    monoBitmap.UnlockBits(bitmapData);
                    monoBitmap.Dispose();
                }
            }
        }

        private Bitmap ConvertTo1Bpp(Image image)
        {
            // Сначала создаем временное изображение в Format32bppArgb
            var tempBitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(tempBitmap))
            {
                // Настраиваем качество рендеринга
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // Рисуем исходное изображение на временном
                graphics.DrawImage(image, 0, 0, image.Width, image.Height);
            }

            // Теперь конвертируем в монохромное
            var monoBitmap = new Bitmap(tempBitmap.Width, tempBitmap.Height, PixelFormat.Format1bppIndexed);

            // Блокируем биты для конвертации
            var sourceData = tempBitmap.LockBits(
                new Rectangle(0, 0, tempBitmap.Width, tempBitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            var destData = monoBitmap.LockBits(
                new Rectangle(0, 0, monoBitmap.Width, monoBitmap.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format1bppIndexed);

            try
            {
                // Конвертируем пиксели
                Convert32bppTo1bpp(sourceData, destData, tempBitmap.Width, tempBitmap.Height);
            }
            finally
            {
                tempBitmap.UnlockBits(sourceData);
                monoBitmap.UnlockBits(destData);
                tempBitmap.Dispose();
            }

            return monoBitmap;
        }

        private void Convert32bppTo1bpp(BitmapData sourceData, BitmapData destData, int width, int height)
        {
            // Копируем данные из sourceData в destData с конвертацией
            // Это упрощенная реализация - на практике может потребоваться более сложная логика
            byte[] sourceBuffer = new byte[sourceData.Stride * height];
            byte[] destBuffer = new byte[destData.Stride * height];

            Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, sourceBuffer.Length);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int sourceIndex = y * sourceData.Stride + x * 4;
                    int destIndex = y * destData.Stride + x / 8;

                    // Получаем цвет пикселя (R, G, B)
                    byte r = sourceBuffer[sourceIndex + 2];
                    byte g = sourceBuffer[sourceIndex + 1];
                    byte b = sourceBuffer[sourceIndex];

                    // Преобразуем в оттенок серого
                    byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);

                    // Бинаризация (порог 128)
                    if (gray > 128)
                    {
                        // Устанавливаем бит (белый)
                        destBuffer[destIndex] |= (byte)(0x80 >> (x % 8));
                    }
                    // Иначе бит остается 0 (черный)
                }
            }

            Marshal.Copy(destBuffer, 0, destData.Scan0, destBuffer.Length);
        }

        public string GenerateTsplCommand(TsplBitmapData bitmapData, int x, int y)
        {
            // Генерируем TSPL команду BITMAP
            return $"BITMAP {x},{y},{bitmapData.BytesPerRow},{bitmapData.Height},{bitmapData.CompressionType},{bitmapData.HexData}";
        }
    }
}
