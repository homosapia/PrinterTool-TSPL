using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;
using BinaryKits.Zpl.Viewer;
using BinaryKits.Zpl.Viewer.ElementDrawers;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using System;
using System.Data.SqlTypes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using ZXing.Rendering;
using static System.Resources.ResXFileRef;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private List<BluetoothDevice> bluetoothDevices = new List<BluetoothDevice>();
        private List<PrinterInfo> printers = new List<PrinterInfo>();
        private PrinterConnect printerConnect = new PrinterConnect();
        private BluetoothService bluetoothService = new BluetoothService();
        public Form1()
        {
            InitializeComponent();
        }


        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        public static Encoding GetEncodingByCodePage(int codePage)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding(codePage);
            }
            catch (ArgumentException)
            {
                // Пробуем альтернативные методы
                try
                {
                    return Encoding.GetEncoding(codePage.ToString());
                }
                catch
                {
                    return null;
                }
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        public static Encoding GetEncodingByName(string name)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding(name);
            }
            catch
            {
                // Ищем по синонимам
                var encoding = ComprehensiveEncodingsList.AllEncodings
                    .FirstOrDefault(e => e.Value.Contains(name, StringComparison.OrdinalIgnoreCase));

                if (encoding.Value != null)
                {
                    return GetEncodingByCodePage(encoding.Key);
                }

                return null;
            }
        }

        private void Convert_Click(object sender, EventArgs e)
        {
            EncodingItem encodingItem = EncodingList.SelectedItem as EncodingItem;

            var encoding = GetEncodingByCodePage(encodingItem.CodePage);
            if (encoding == null)
            {
                encoding = GetEncodingByName(encodingItem.Name);
            }

            var list = encoding.GetBytes(textBox1.Text);

            textBox2.Text = string.Join(" ", list);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Printers.Items.Clear();
            printers.Clear();

            try
            {
                var devicesList = bluetoothService.GetAvailableDevices();
                foreach (var device in devicesList) 
                {
                    PrinterInfo printerInfo = new PrinterInfo()
                    {
                        Name = device.DeviceName,
                        Port = device.DeviceAddress,
                        ConnectionType = ConnectionMethods.bluetooth,
                    };
                    bluetoothDevices.Add(device);
                    Printers.Items.Add(printerInfo);
                }

                // Получаем информацию о принтерах через WMI
                var query = "SELECT Name, PortName FROM Win32_Printer";
                var searcher = new ManagementObjectSearcher(query);

                foreach (ManagementObject printer in searcher.Get())
                {
                    string name = printer["Name"]?.ToString() ?? "";
                    string port = printer["PortName"]?.ToString() ?? "";

                    // Проверяем, является ли это TSC принтером (по названию или порту)
                    bool isTSC = name.ToUpper().Contains("TSC") ||
                                name.ToUpper().Contains("TDP") ||
                                port.ToUpper().Contains("COM") ||
                                port.ToUpper().Contains("LPT");

                    var printerInfo = new PrinterInfo
                    {
                        Name = name,
                        Port = port,
                        IsTSCPrinter = isTSC,
                        ConnectionType = ConnectionMethods.wiredDevice,

                    };

                    printers.Add(printerInfo);
                    Printers.Items.Add(printerInfo);
                }

                // Выбираем первый принтер, если есть
                if (Printers.Items.Count > 0)
                {
                    Printers.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке принтеров: {ex.Message}");
            }


            try
            {
                // Получаем все доступные кодировки
                var encodingInfos = EncodingHelper.GetAllEncodings();
                encodingInfos.ForEach(x => EncodingList.Items.Add(x));
                EncodingList.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения списка кодировок: {ex.Message}");
            }

            double InchInMillimeters = 25.4;
            int DPI = 203;

            int labelWidthMm = 58;
            int labelHeightMm = 40;

            int heightDots = (int)((labelHeightMm / InchInMillimeters) * DPI);//высота в точках 
            int widthDots = (int)((labelWidthMm / InchInMillimeters) * DPI);//ширина в точках

            var font = new ZplFont(50, 50);
            var elements = new List<ZplElementBase>();
            elements.Add(new ZplTextField("привет я кириллица", 50, 100, font));
            var options = new ZplRenderOptions { SourcePrintDpi = 203, TargetPrintDpi = 203 };
            var output = new ZplEngine(elements).ToZplString(options);

            IPrinterStorage printerStorage = new PrinterStorage();

            var drawer = new ZplElementDrawer(printerStorage);

            var analyzer = new ZplAnalyzer(printerStorage);
            var analyzeInfo = analyzer.Analyze(output);

            foreach (var labelInfo in analyzeInfo.LabelInfos)
            {
                Bytes = drawer.Draw(labelInfo.ZplElements, labelWidthMm, labelHeightMm);
            }

            File.WriteAllBytes("qsdaee_original.bmp", Bytes);

            int startX = -1;
            int startY = -1;

            int endX = 0;
            int endY = 0;

            List<byte> bytes = new List<byte>();
            int width = 0;
            int height = 0;
            using (var ms = new MemoryStream(Bytes))
            using (var bitmap = new Bitmap(ms))
            {
                width = bitmap.Width;
                height = bitmap.Height;

                // Определяем границы изображения
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        var pixel = bitmap.GetPixel(x, y);
                        if (pixel.A > 0)
                        {
                            if (startX == -1 || startX > x) startX = x;
                            if (endX < x) endX = x;

                            if (startY == -1) startY = y;
                            if (endY < y) endY = y;
                        }
                    }
                }

                // Вычисляем размеры обрезанного изображения
                int cropWidth = endX - startX + 1;
                int cropHeight = endY - startY + 1;

                // Формируем данные для TSPL BITMAP
                int bytesPerRow = (cropWidth + 7) / 8; // Количество байт на строку

                for (int y = startY; y <= endY; y++)
                {
                    List<byte> rowBytes = new List<byte>();
                    byte currentByte = 0;
                    int bitPosition = 0;

                    for (int x = startX; x <= endX; x++)
                    {
                        var pixel = bitmap.GetPixel(x, y);
                        if (pixel.A > 0) // Пиксель черный (активный)
                        {
                            currentByte |= (byte)(1 << (7 - bitPosition));
                        }

                        bitPosition++;

                        // Когда набрали 8 бит или достигли конца строки
                        if (bitPosition == 8 || x == endX)
                        {
                            rowBytes.Add(currentByte);
                            currentByte = 0;
                            bitPosition = 0;
                        }
                    }

                    rowBytes.Reverse();
                    bytes.AddRange(rowBytes);
                }

                byte[] bmpBytes = CreateMonochromeBmp(cropWidth, cropHeight, bytes.ToArray());

                File.WriteAllBytes("qsdaee_transformed.bmp", bmpBytes);

                //textBox1.Text = GenerateTsplCommand(labelWidthMm, labelHeightMm, bytes.ToArray(), startX, startY, bytesPerRow, cropHeight);
                textBox1.Text = GenerateTsplFont();
            }
        }

        private byte[] CreateMonochromeBmp(int width, int height, byte[] pixelData)
        {
            int bytesPerRow = ((width + 31) / 32) * 4;
            int fileSize = 62 + pixelData.Length; // 14 + 40 + 8 + pixelData.Length

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // BMP File Header (14 bytes)
                writer.Write(new byte[] { 0x42, 0x4D }); // BM signature
                writer.Write(fileSize);                  // File size
                writer.Write((int)0);                    // Reserved
                writer.Write(62);                        // Offset to pixel data

                // DIB Header (40 bytes) - BITMAPINFOHEADER
                writer.Write(40);                        // Header size
                writer.Write(width);                     // Width
                writer.Write(height);                    // Height
                writer.Write((short)1);                  // Planes
                writer.Write((short)1);                  // Bits per pixel (monochrome)
                writer.Write(0);                         // Compression
                writer.Write(pixelData.Length);          // Image size
                writer.Write(0);                         // X pixels per meter
                writer.Write(0);                         // Y pixels per meter
                writer.Write(0);                         // Colors used
                writer.Write(0);                         // Important colors

                // Color palette (8 bytes) - для монохромного BMP
                writer.Write(new byte[] { 0, 0, 0, 0 }); // Black (0)
                writer.Write(new byte[] { 255, 255, 255, 0 }); // White (1)

                // Pixel data
                writer.Write(pixelData);

                return ms.ToArray();
            }
        }

        Dictionary<string, string> hexadecimalSystem = new Dictionary<string, string>()
        {
            { "0000", "0" },
            { "0001", "1" },
            { "0010", "2" },
            { "0011", "3" },
            { "0100", "4" },
            { "0101", "5" },
            { "0110", "6" },
            { "0111", "7" },
            { "1000", "8" },
            { "1001", "9" },
            { "1010", "A" },
            { "1011", "B" },
            { "1100", "C" },
            { "1101", "D" },
            { "1110", "E" },
            { "1111", "F" },
        };

        public string GenerateTsplCommand(int widthMM, int heightMM, byte[] bitmapBytes, int startX, int startY, int bitmapWidth, int bitmapHeight)
        {
            StringBuilder tspl = new StringBuilder();

            // Размеры этикетки
            tspl.AppendLine($"SIZE {widthMM} mm, {heightMM} mm");
            tspl.AppendLine("DIRECTION 0,0");
            tspl.AppendLine("REFERENCE 0,0");
            tspl.AppendLine("OFFSET 0 mm");
            tspl.AppendLine("SET PEEL OFF");
            tspl.AppendLine("SET CUTTER OFF");
            tspl.AppendLine("SET TEAR ON");
            tspl.AppendLine("CLS");

            string bitData = string.Empty;
            foreach (byte value in bitmapBytes)
            {
                bitData += Convert.ToString(value, 2).PadLeft(8, '0');
            }

            //string hexadecimalString2 = string.Empty;
            //for (int i = 0; i < bitData.Length; i+=4)
            //{
            //    string text = string.Join("", bitData.Skip(i).Take(4));
            //    hexadecimalSystem.TryGetValue(text, out string hexadecimalChar);
            //    hexadecimalString2 += hexadecimalChar;
            //}

            //Преобразуем байты в HEX строку
            StringBuilder hexBuilder = new StringBuilder();
            foreach (byte b in bitmapBytes)
            {
                string dfd = b.ToString("X2");
                hexBuilder.Append(dfd); // Два HEX символа на байт
            }
            string hexadecimalString = hexBuilder.ToString();

            // Правильная команда BITMAP
            tspl.AppendLine($"BITMAP {startX},{startY},{bitmapWidth},{bitmapHeight},1,{hexadecimalString}");
            tspl.AppendLine("PRINT 1,1");

            return tspl.ToString();
        }

        private string GenerateTsplFont()
        {
            TSPLFontGenerator tSPLFontGenerator = new TSPLFontGenerator();

            var fonts = tSPLFontGenerator.GetSystemFonts();

            var font = fonts[2];

            string tspl = tSPLFontGenerator.GenerateTSPLScriptWithRussianFont("fsdsdfsdf", font, 12, FontStyle.Regular);
            return tspl;
        }


        private string GenerateTsplCommandDOWNLOAD(int labelWidthMm, int labelHeightMm, byte[] imageData, int startX, int startY, int bytesPerRow, int cropHeight)
        {
            // Генерируем уникальное имя файла
            string fileName = "IMAGE1.PCX";

            // Вычисляем общий размер данных в байтах
            int totalBytes = imageData.Length;

            // Формируем команду DOWNLOAD
            StringBuilder tsplCommand = new StringBuilder();

            // Команда DOWNLOAD для загрузки изображения в память принтера
            tsplCommand.AppendLine($"DOWNLOAD F,\"{fileName}\",{totalBytes},");

            foreach (byte b in imageData)
            {
                string dfd = b.ToString("X2");
                tsplCommand.Append(dfd); // Два HEX символа на байт
            }

            tsplCommand.AppendLine();

            // Добавляем команду для печати изображения
            tsplCommand.AppendLine($"SIZE {labelWidthMm} mm, {labelHeightMm} mm");
            tsplCommand.AppendLine("CLS");
            tsplCommand.AppendLine($"PUTPCX {startX},{startY},\"{fileName}\"");
            tsplCommand.AppendLine("PRINT 1,1");

            return tsplCommand.ToString();
        }


        public byte[] Bytes;

        private void Сonnection_Click(object sender, EventArgs e)
        {
            PrinterInfo printerInfo = Printers.SelectedItem as PrinterInfo;
            if (printerInfo == null)
                MessageBox.Show($"Выберите принтер");

            bool isConnected = false;
            if (printerInfo.ConnectionType == ConnectionMethods.wiredDevice)
            {
                // Отображаем информацию о принтере
                string printerDetails = printerConnect.GetPrinterDetails(printerInfo);
                MessageBox.Show(printerDetails, "Информация о принтере");
                // Пытаемся подключиться
                Cursor.Current = Cursors.WaitCursor;
                isConnected = printerConnect.ConnectToPrinter(printerInfo);
                Cursor.Current = Cursors.Default;
            }

            if(printerInfo.ConnectionType == ConnectionMethods.bluetooth)
            {
                try
                {
                    isConnected = bluetoothService.ConnectToDeviceAsync(printerInfo.Port);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            if (isConnected)
            {
                printerInfo.IsConnected = true;
                Status.Checked = printerInfo.IsConnected;
            }
            else
            {
                printerInfo.IsConnected = false;
                Status.Checked = printerInfo.IsConnected;
            }
        }

        private async void print_Click(object sender, EventArgs e)
        {
            var printer = Printers.SelectedItem as PrinterInfo;
            if (printer == null || !printer.IsConnected)
            { MessageBox.Show($"Выберите принтер"); return; }

            string testCommand = textBox1.Text;

            EncodingItem encodingItem = EncodingList.SelectedItem as EncodingItem;
            var encoding = Encoding.GetEncoding(1251);
            if (encoding == null)
            {
                encoding = GetEncodingByName(encodingItem.Name);
            }
            var commandBytes = encoding.GetBytes(testCommand);

            bool success = false;
            if (printer.ConnectionType == ConnectionMethods.wiredDevice)
            {
                // Отправляем команду
                success = printerConnect.SendCommand(printer, commandBytes);
            }

            if(printer.ConnectionType == ConnectionMethods.bluetooth)
            {
                try
                {
                    string text = await bluetoothService.SendCommandAndGetResponseAsync(commandBytes);
                    MessageBox.Show(text);
                }
                catch(Exception ex) 
                {
                    MessageBox.Show(ex.Message);
                }

                //string[] switchCommands = {
                //    "\n~!T\n",
                //    //"SIZE 57mm,40mm\r\nCLS\r\nTEXT 115,39,\"3\",0,1,1,\"А Тест принтер\"\r\nPRINT 1,1",
                //    "CODEPAGE UTF-8\r\nSIZE 57mm,40mm\r\nCLS\r\nTEXT 115,39,\"3\",0,1,1,\"А Тест принтер\"\r\nPRINT 1,1",
                //    //"CODEPAGE UTF-8\r\nSIZE 57mm,40mm\r\nCLS\r\nTEXT 115,39,\"3\",0,1,1,\"А Тест принтер\"\r\nPRINT 1,1",
                //};

                //foreach (var command in switchCommands)
                //{
                //    byte[] data = Encoding.ASCII.GetBytes(command);
                //    await bluetoothService.SendBytesAsync(data);
                //    await Task.Delay(1000);
                //}

                //foreach (var command in switchCommands)
                //{
                //    byte[] data = Encoding.UTF8.GetBytes(command);
                //    string text = await bluetoothService.SendCommandAndGetResponseAsync(data);
                //    await Task.Delay(1000);
                //}

                //foreach (var command in switchCommands)
                //{
                //    byte[] data = Encoding.GetEncoding(1252).GetBytes(command);
                //    await bluetoothService.SendBytesAsync(data);
                //    await Task.Delay(1000);
                //}

                //foreach (var command in switchCommands)
                //{
                //    byte[] data = Encoding.UTF8.GetBytes(command);
                //    await bluetoothService.SendBytesAsync(data);
                //    await Task.Delay(1000); 
                //}

                //foreach (var command in switchCommands)
                //{
                //    byte[] data = Encoding.Unicode.GetBytes(command);
                //    await bluetoothService.SendBytesAsync(data);
                //    await Task.Delay(1000);
                //}

                //foreach (var command in switchCommands)
                //{
                //    byte[] data = Encoding.BigEndianUnicode.GetBytes(command);
                //    await bluetoothService.SendBytesAsync(data);
                //    await Task.Delay(1000);
                //}

                return;
            }

            if (success)
            {
                MessageBox.Show("Команда успешно отправлена");
            }
            else
            {
                MessageBox.Show("Ошибка отправки команды");
            }
        }

        public void PrintViaBluetooth(string zplCode)
        {
            try
            {
                // Получаем список доступных COM портов (Bluetooth принтеры обычно используют COM порты)
                string[] ports = SerialPort.GetPortNames();

                // Здесь нужно знать конкретный COM порт вашего принтера
                using (SerialPort serialPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One))
                {
                    serialPort.Open();
                    serialPort.Write(zplCode);
                    serialPort.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка печати: {ex.Message}");
            }
        }

        private void PrintALL_Click(object sender, EventArgs e)
        {
            var printer = Printers.SelectedItem as PrinterInfo;
            if (printer == null || !printer.IsConnected)
            { MessageBox.Show($"Выберите принтер"); return; }

            if(EncodingList.Items.Count == 0)
            {
                MessageBox.Show($"Кодировки не найдены"); return;
            }

            foreach (EncodingItem encodingItem in EncodingList.Items)
            {
                var encoding = GetEncodingByCodePage(encodingItem.CodePage);
                if (encoding == null)
                {
                    encoding = GetEncodingByName(encodingItem.Name);
                }
                var commandBytes = encoding.GetBytes(textBox1.Text);

                // Отправляем команду
                bool success = printerConnect.SendCommand(printer, commandBytes);

                if (success)
                {
                }
                else
                {
                    MessageBox.Show("Ошибка отправки команды");
                }

                Thread.Sleep(1000);
            }
        }
    }
}
