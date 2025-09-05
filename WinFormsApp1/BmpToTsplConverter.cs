using System.Drawing.Imaging;
using System.Text;

public class BmpToTsplConverter
{
    public static (string hexData, int width, int height, int bytesPerRow) ConvertBmpToTsplHex(byte[] bmpData)
    {
        using var ms = new MemoryStream(bmpData);
        using var bitmap = new Bitmap(ms);

        return ConvertBitmapToTsplHex(bitmap);
    }

    public static (string hexData, int width, int height, int bytesPerRow) ConvertBitmapToTsplHex(Bitmap bitmap)
    {
        // Получаем HEX данные
        string hexData = GetMonochromeHexData(bitmap);

        int width = bitmap.Width;
        int height = bitmap.Height;
        int bytesPerRow = (int)Math.Ceiling(width / 8.0);

        return (hexData, width, height, bytesPerRow);
    }

    private static string GetMonochromeHexData(Bitmap bitmap)
    {
        var hexBuilder = new StringBuilder();

        // Блокируем битмап для доступа к пиксельным данным
        var bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format1bppIndexed
        );

        try
        {
            int stride = bitmapData.Stride;
            int height = bitmapData.Height;
            IntPtr scan0 = bitmapData.Scan0;

            // Читаем данные построчно
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < stride; x++)
                {
                    byte pixelData = System.Runtime.InteropServices.Marshal.ReadByte(scan0, y * stride + x);
                    hexBuilder.Append(pixelData.ToString("X2"));
                }
            }
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }

        return hexBuilder.ToString();
    }

    // Альтернативный метод, если LockBits не работает
    private static string GetMonochromeHexDataAlternative(Bitmap bitmap)
    {
        var hexBuilder = new StringBuilder();
        int width = bitmap.Width;
        int height = bitmap.Height;

        // Создаем временный массив для хранения байтов
        int bytesPerRow = (int)Math.Ceiling(width / 8.0);
        byte[] rowBuffer = new byte[bytesPerRow];

        for (int y = 0; y < height; y++)
        {
            Array.Clear(rowBuffer, 0, rowBuffer.Length);

            for (int x = 0; x < width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                bool isBlack = pixel.GetBrightness() < 0.5f;

                if (isBlack)
                {
                    int byteIndex = x / 8;
                    int bitPosition = 7 - (x % 8);
                    rowBuffer[byteIndex] |= (byte)(1 << bitPosition);
                }
            }

            // Добавляем байты строки в HEX
            foreach (byte b in rowBuffer)
            {
                hexBuilder.Append(b.ToString("X2"));
            }
        }

        return hexBuilder.ToString();
    }

    public static string GenerateTsplCommand(string hexData, int x, int y, int bytesPerRow, int height)
    {
        return $@"SIZE 41.7 mm, 25 mm
                    DIRECTION 0,0
                    REFERENCE 0,0
                    OFFSET 0 mm
                    SET PEEL OFF
                    SET CUTTER OFF
                    SET TEAR ON
                    CLS
                    BITMAP {x},{y},{bytesPerRow},{height},1,{hexData}
                    PRINT 1,1";
    }
}