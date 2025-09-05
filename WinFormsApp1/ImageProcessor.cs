using Hjg.Pngcs;

public class PrintImageResult
{
    public byte[] ImageData { get; set; }
    public int StartX { get; set; }
    public int StartY { get; set; }
    public int WidthInBytes { get; set; }
    public int HeightInPoints { get; set; }
    public int GraphicsMode { get; set; }
}

public class ImageProcessor
{
    private const int DPI = 203;
    private const double MM_TO_INCH = 25.4;

    public PrintImageResult ProcessImageForPrinting(byte[] pngData, int labelWidthMM, int labelHeightMM)
    {
        // Декодируем PNG и получаем размеры
        byte[] monochromeData = DecodePngToMonochrome(pngData, out int widthPixels, out int heightPixels);

        // Рассчитываем размеры в точках
        int widthDots = widthPixels;
        int heightDots = heightPixels;

        // Рассчитываем начальные координаты (центрируем изображение)
        int labelWidthDots = (int)(labelWidthMM * DPI / MM_TO_INCH);
        int labelHeightDots = (int)(labelHeightMM * DPI / MM_TO_INCH);

        int startX = Math.Max(0, (labelWidthDots - widthDots) / 2);
        int startY = Math.Max(0, (labelHeightDots - heightDots) / 2);

        // Рассчитываем ширину в байтах (округляем вверх до ближайшего байта)
        int widthInBytes = (widthDots + 7) / 8;

        // Преобразуем данные в формат TSPL (1 бит на пиксель)
        byte[] tsplData = ConvertToTSPLFormat(monochromeData, widthDots, heightDots, widthInBytes);

        return new PrintImageResult
        {
            ImageData = tsplData,
            StartX = startX,
            StartY = startY,
            WidthInBytes = widthInBytes,
            HeightInPoints = heightDots,
            GraphicsMode = 0 // ПЕРЕЗАПИСЬ
        };
    }

    private byte[] DecodePngToMonochrome(byte[] pngData, out int width, out int height)
    {
        using (var ms = new MemoryStream(pngData))
        {
            var reader = new PngReader(ms);
            width = reader.ImgInfo.Cols;
            height = reader.ImgInfo.Rows;

            byte[] monochromeData = new byte[width * height];

            for (int row = 0; row < height; row++)
            {
                ImageLine line = reader.ReadRowInt(row);

                for (int col = 0; col < width; col++)
                {
                    int index = row * width + col;

                    if (reader.ImgInfo.Channels == 1)
                    {
                        // Grayscale
                        monochromeData[index] = (byte)line.Scanline[col];
                    }
                    else if (reader.ImgInfo.Channels == 3)
                    {
                        // RGB -> Grayscale
                        byte r = (byte)line.Scanline[col * 3];
                        byte g = (byte)line.Scanline[col * 3 + 1];
                        byte b = (byte)line.Scanline[col * 3 + 2];
                        monochromeData[index] = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                    }
                    else if (reader.ImgInfo.Channels == 4)
                    {
                        // RGBA -> Grayscale с учетом альфа-канала
                        byte r = (byte)line.Scanline[col * 4];
                        byte g = (byte)line.Scanline[col * 4 + 1];
                        byte b = (byte)line.Scanline[col * 4 + 2];
                        byte a = (byte)line.Scanline[col * 4 + 3];

                        double alpha = a / 255.0;
                        double gray = (0.299 * r + 0.587 * g + 0.114 * b) * alpha;
                        monochromeData[index] = (byte)gray;
                    }
                    else
                    {
                        monochromeData[index] = 0;
                    }
                }
            }

            return monochromeData;
        }
    }

    private byte[] ConvertToTSPLFormat(byte[] monochromeData, int width, int height, int bytesPerRow)
    {
        byte[] result = new byte[bytesPerRow * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x += 8)
            {
                byte currentByte = 0;

                for (int bit = 0; bit < 8; bit++)
                {
                    int pixelX = x + bit;
                    if (pixelX < width)
                    {
                        int index = y * width + pixelX;
                        // Инвертируем: черный = 1, белый = 0 (для термопринтеров)
                        if (monochromeData[index] < 128) // Порог бинаризации
                        {
                            currentByte |= (byte)(0x80 >> bit);
                        }
                    }
                }

                result[y * bytesPerRow + x / 8] = currentByte;
            }
        }

        return result;
    }
}