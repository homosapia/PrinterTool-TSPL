using System.Text;

namespace WinFormsApp1
{
    public class TSPLFontGenerator
    {
        public List<string> GetSystemFonts()
        {
            return FontFamily.Families
                .Where(f => f.IsStyleAvailable(FontStyle.Regular))
                .Select(f => f.Name)
                .OrderBy(name => name)
                .ToList();
        }

        public string GenerateTSPLScriptWithRussianFont(string textToPrint, string fontName, int fontSize, FontStyle fontStyle)
        {
            var tsplScript = new StringBuilder();

            // Устанавливаем кодировку для русского языка
            tsplScript.AppendLine("CODEPAGE 1251");
            tsplScript.AppendLine("SIZE 40 mm, 25 mm");
            tsplScript.AppendLine("CLS");
            tsplScript.AppendLine();

            // Генерируем данные для русского шрифта
            var fontData = GenerateRussianFontBitmapData(fontName, fontSize, fontStyle);

            if (fontData != null)
            {
                // Загружаем шрифт в память принтера
                tsplScript.AppendLine($"DOWNLOAD \"RUSFONT\",{fontData.Size},{fontData.Type},{fontData.Height},{fontData.Width},\"{fontData.HexData}\"");
                tsplScript.AppendLine();

                // Используем загруженный шрифт - ВАЖНО: текст должен быть в кодировке 1251
                string encodedText = ConvertToWindows1251(textToPrint);
                tsplScript.AppendLine($"TEXT 50,50,\"RUSFONT\",0,1,1,\"{EscapeText(encodedText)}\"");
            }
            else
            {
                // Используем встроенный шрифт с кодировкой 1251
                string encodedText = ConvertToWindows1251(textToPrint);
                tsplScript.AppendLine($"TEXT 50,50,\"0\",0,1,1,\"{EscapeText(encodedText)}\"");
            }

            tsplScript.AppendLine("PRINT 1");

            return tsplScript.ToString();
        }

        private FontData GenerateRussianFontBitmapData(string fontName, int fontSize, FontStyle fontStyle)
        {
            try
            {
                using (var font = new Font(fontName, fontSize, fontStyle, GraphicsUnit.Point))
                using (var bmp = new Bitmap(1, 1))
                using (var g = Graphics.FromImage(bmp))
                {
                    // Измеряем размер русского текста для определения размеров символов
                    var textSize = g.MeasureString("АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ", font);
                    int width = (int)Math.Ceiling(textSize.Width / 33); // Средняя ширина символа
                    int height = (int)Math.Ceiling(textSize.Height);

                    // Создаем bitmap для рендеринга символов
                    using (var charBmp = new Bitmap(width * 2, height * 2)) // Запас по размерам
                    using (var charGraphics = Graphics.FromImage(charBmp))
                    {
                        charGraphics.Clear(Color.White);
                        charGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

                        var hexData = new StringBuilder();
                        int totalSize = 0;

                        // Генерируем данные для русских символов (кириллица)
                        string russianChars = GetRussianCharacters();

                        foreach (char c in russianChars)
                        {
                            charGraphics.Clear(Color.White);

                            // Рисуем символ по центру
                            var charSize = charGraphics.MeasureString(c.ToString(), font);
                            float x = (charBmp.Width - charSize.Width) / 2;
                            float y = (charBmp.Height - charSize.Height) / 2;

                            charGraphics.DrawString(c.ToString(), font, Brushes.Black, x, y);

                            // Обрезаем bitmap до реальных размеров символа
                            var croppedHex = CropAndConvertToHex(charBmp, (int)charSize.Width, (int)charSize.Height);
                            hexData.Append(croppedHex);
                            totalSize += croppedHex.Length / 2;
                        }

                        return new FontData
                        {
                            HexData = hexData.ToString(),
                            Size = (totalSize / 1024) + 1,
                            Type = 1, // Bitmap font
                            Height = height,
                            Width = width
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации шрифта: {ex.Message}");
                return null;
            }
        }

        private string GetRussianCharacters()
        {
            // Русские символы в порядке кодировки Windows-1251
            var chars = new List<char>();

            // Заглавные буквы (А-Я)
            for (char c = 'А'; c <= 'Я'; c++)
                chars.Add(c);

            // Добавляем Ё
            chars.Add('Ё');

            // Прописные буквы (а-я)
            for (char c = 'а'; c <= 'я'; c++)
                chars.Add(c);

            // Добавляем ё
            chars.Add('ё');

            return new string(chars.ToArray());
        }

        private string CropAndConvertToHex(Bitmap bmp, int width, int height)
        {
            var hexBuilder = new StringBuilder();

            // Определяем реальные границы символа
            int startX = bmp.Width, startY = bmp.Height, endX = 0, endY = 0;

            // Находим границы символа
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var pixel = bmp.GetPixel(x, y);
                    if (pixel.R < 128) // Черный пиксель
                    {
                        if (x < startX) startX = x;
                        if (y < startY) startY = y;
                        if (x > endX) endX = x;
                        if (y > endY) endY = y;
                    }
                }
            }

            // Если символ не найден, возвращаем пустые данные
            if (startX > endX || startY > endY)
                return new string('0', width * height * 2);

            int actualWidth = endX - startX + 1;
            int actualHeight = endY - startY + 1;

            // Конвертируем обрезанную область
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    var pixel = bmp.GetPixel(x, y);
                    byte value = (byte)(pixel.R < 128 ? 1 : 0); // 1 - черный, 0 - белый
                    hexBuilder.Append(value.ToString("X1")); // Один hex символ на пиксель
                }
            }

            return hexBuilder.ToString();
        }

        private string ConvertToWindows1251(string text)
        {
            // Конвертация в Windows-1251 кодировку
            Encoding windows1251 = Encoding.GetEncoding(1251);
            Encoding utf8 = Encoding.UTF8;

            byte[] utf8Bytes = utf8.GetBytes(text);
            byte[] win1251Bytes = Encoding.Convert(utf8, windows1251, utf8Bytes);

            return windows1251.GetString(win1251Bytes);
        }

        private string EscapeText(string text)
        {
            return text.Replace("\"", "\\\"")
                       .Replace("\\", "\\\\")
                       .Replace("\r", "")
                       .Replace("\n", "\\n");
        }
    }

    // Вспомогательный класс для хранения данных шрифта
    public class FontData
    {
        public string HexData { get; set; }
        public int Size { get; set; } // в KB
        public int Type { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
