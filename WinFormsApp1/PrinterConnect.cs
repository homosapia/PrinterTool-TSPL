using System.IO.Ports;
using System.Management;
using System.Text;

namespace WinFormsApp1
{
    public class PrinterConnect
    {
        private SerialPort serialPort = null;

        // Метод для подключения к принтеру
        public bool ConnectToPrinter(PrinterInfo printer)
        {
            try
            {
                // Если принтер на COM порту
                if (printer.Port.ToUpper().StartsWith("COM"))
                {
                    return ConnectViaSerialPort(printer.Port);
                }
                // Если принтер на USB, LPT или сетевой
                else
                {
                    return ConnectViaRawPrint(printer.Name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}");
                return false;
            }
        }

        // Подключение через COM порт
        public bool ConnectViaSerialPort(string portName)
        {
            try
            {
                // Закрываем предыдущее соединение
                DisconnectPrinter();

                serialPort = new SerialPort(portName)
                {
                    BaudRate = 9600,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadTimeout = 2000,
                    WriteTimeout = 2000
                };

                serialPort.Open();

                // Проверяем соединение отправкой тестовой команды
                return TestPrinterConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка COM порта {portName}: {ex.Message}");
                return false;
            }
        }

        // Подключение через RAW печать (USB/сеть)
        private bool ConnectViaRawPrint(string printerName)
        {
            try
            {
                // Проверяем доступность принтера
                return CheckPrinterStatus(printerName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка доступа к принтеру {printerName}: {ex.Message}");
                return false;
            }
        }

        // Проверка статуса принтера через WMI
        private bool CheckPrinterStatus(string printerName)
        {
            try
            {
                var query = $"SELECT * FROM Win32_Printer WHERE Name = '{printerName.Replace("'", "''")}'";
                var searcher = new ManagementObjectSearcher(query);

                foreach (ManagementObject printer in searcher.Get())
                {
                    string status = printer["Status"]?.ToString() ?? "";
                    bool workOffline = bool.Parse(printer["WorkOffline"]?.ToString() ?? "false");

                    if (workOffline)
                    {
                        MessageBox.Show("Принтер работает в автономном режиме");
                        return false;
                    }

                    if (status.Contains("Error") || status.Contains("Offline"))
                    {
                        MessageBox.Show($"Статус принтера: {status}");
                        return false;
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки статуса: {ex.Message}");
                return false;
            }
        }

        // Тестирование соединения отправкой TSPL команды
        private bool TestPrinterConnection()
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    // Отправляем команду запроса статуса
                    string statusCommand = "\x1B!?\r\n"; // ESC команда для запроса статуса
                    serialPort.Write(statusCommand);

                    // Ждем ответа (не все принтеры поддерживают)
                    Thread.Sleep(100);

                    // Пробуем прочитать ответ
                    int bytesToRead = serialPort.BytesToRead;
                    if (bytesToRead > 0)
                    {
                        byte[] buffer = new byte[bytesToRead];
                        serialPort.Read(buffer, 0, bytesToRead);
                        string response = Encoding.ASCII.GetString(buffer);
                        return true;
                    }

                    // Если ответа нет, отправляем тестовую команду
                    string testCommand = "SIZE 100 mm, 50 mm\r\nGAP 2 mm, 0 mm\r\nCLS\r\nPRINT 1\r\n";
                    serialPort.Write(testCommand);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Отключение от принтера
        private void DisconnectPrinter()
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                    serialPort.Dispose();
                    serialPort = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отключения: {ex.Message}");
            }
        }

        // Получение подробной информации о принтере
        public string GetPrinterDetails(PrinterInfo printer)
        {
            try
            {
                var query = $"SELECT * FROM Win32_Printer WHERE Name = '{printer.Name.Replace("'", "''")}'";
                var searcher = new ManagementObjectSearcher(query);
                var details = new StringBuilder();

                foreach (ManagementObject printerObj in searcher.Get())
                {
                    details.AppendLine($"Имя: {printerObj["Name"]}");
                    details.AppendLine($"Порт: {printerObj["PortName"]}");
                    details.AppendLine($"Статус: {printerObj["Status"]}");
                    details.AppendLine($"Автономный: {printerObj["WorkOffline"]}");
                    details.AppendLine($"По умолчанию: {printerObj["Default"]}");
                    details.AppendLine($"Драйвер: {printerObj["DriverName"]}");
                    details.AppendLine($"Расположение: {printerObj["Location"]}");
                }

                return details.ToString();
            }
            catch (Exception ex)
            {
                return $"Ошибка получения информации: {ex.Message}";
            }
        }

        // Метод для отправки команды на принтер
        public bool SendCommand(PrinterInfo printer, byte[] commandData)
        {
            try
            {
                if (printer.Port.ToUpper().StartsWith("COM"))
                {
                    return SendCommandSerial(printer.Port, commandData);
                }
                else
                {
                    return SendCommandRaw(printer.Name, commandData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки команды: {ex.Message}");
                return false;
            }
        }

        // Отправка через COM порт
        private bool SendCommandSerial(string portName, byte[] commandData)
        {
            try
            {
                if (serialPort == null || !serialPort.IsOpen)
                {
                    if (!ConnectViaSerialPort(portName))
                        return false;
                }

                serialPort.Write(commandData, 0, commandData.Length);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки через COM порт: {ex.Message}");
                return false;
            }
        }

        // Отправка через RAW печать (USB/сеть)
        private bool SendCommandRaw(string printerName, byte[] commandData)
        {
            try
            {
                return RawPrinterHelper.SendBytesToPrinter(printerName, commandData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки на принтер: {ex.Message}");
                return false;
            }
        }

    }
}
