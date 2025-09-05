using InTheHand.Net.Sockets;
using InTheHand.Net;
using System.Text;

namespace WinFormsApp1
{
    public class BluetoothService : IDisposable
    {
        private BluetoothClient _bluetoothClient;
        private BluetoothDeviceInfo _connectedDevice;
        private Stream _connectedStream;
        private bool _isConnected = false;

        public BluetoothService()
        {
            _bluetoothClient = new BluetoothClient();
        }

        /// <summary>
        /// Получение списка доступных Bluetooth устройств
        /// </summary>
        public List<BluetoothDevice> GetAvailableDevices()
        {
            var devices = new List<BluetoothDevice>();

            try
            {
                // Поиск устройств с таймаутом 10 секунд
                var discoveredDevices = _bluetoothClient.PairedDevices;

                foreach (var device in discoveredDevices)
                {
                    devices.Add(new BluetoothDevice
                    {
                        DeviceName = device.DeviceName,
                        DeviceAddress = device.DeviceAddress.ToString(),
                        Connected = device.Connected,
                        Authenticated = device.Authenticated
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при поиске устройств: {ex.Message}");
            }

            return devices;
        }

        /// <summary>
        /// Подключение к Bluetooth устройству
        /// </summary>
        public bool ConnectToDeviceAsync(string deviceAddress)
        {
            try
            {
                Disconnect();

                var device = _bluetoothClient.PairedDevices.FirstOrDefault(x => x.DeviceAddress.ToString() == deviceAddress);

                // Проверяем, доступно ли устройство
                if (!device.Connected)
                {
                    // Устанавливаем соединение
                    var guid = new Guid("00001101-0000-1000-8000-00805F9B34FB"); // Standard serial port service

                    var endPoint = new BluetoothEndPoint(device.DeviceAddress, guid);

                    _bluetoothClient.Connect(endPoint);
                    _connectedDevice = device;
                    _connectedStream = _bluetoothClient.GetStream();
                    _isConnected = true;

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка подключения: {ex.Message}");
            }
        }

        /// <summary>
        /// Чтение данных из потока принтера с таймаутом
        /// </summary>
        public async Task<string> ReadResponseAsync(int timeoutMilliseconds = 5000)
        {
            if (!_isConnected || _connectedStream == null)
                throw new Exception("Нет подключения к устройству");

            try
            {
                byte[] buffer = new byte[1024];
                StringBuilder response = new StringBuilder();
                CancellationTokenSource cts = new CancellationTokenSource(timeoutMilliseconds);

                // Создаем задачу для чтения
                var readTask = Task.Run(async () =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        // Пытаемся прочитать данные
                        int bytesRead = await _connectedStream.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                        if (bytesRead > 0)
                        {
                            string chunk = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            response.Append(chunk);

                            // Проверяем, есть ли завершающий символ в ответе
                            if (chunk.Contains('\n') || chunk.Contains('\r'))
                                break;
                        }
                        else
                        {
                            // Если данных нет, делаем небольшую паузу
                            await Task.Delay(50, cts.Token);
                        }
                    }
                }, cts.Token);

                // Ждем завершения задачи чтения или таймаута
                await Task.WhenAny(readTask, Task.Delay(timeoutMilliseconds, cts.Token));

                if (!readTask.IsCompleted)
                    throw new TimeoutException("Таймаут ожидания ответа от принтера");

                return response.ToString();
            }
            catch (OperationCanceledException)
            {
                throw new Exception("Таймаут ожидания ответа от принтера");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка чтения данных: {ex.Message}");
            }
        }

        /// <summary>
        /// Отправка команды и получение ответа от принтера
        /// </summary>
        public async Task<string> SendCommandAndGetResponseAsync(byte[] data, int timeoutMilliseconds = 5000)
        {
            if (!_isConnected || _connectedStream == null)
                throw new Exception("Нет подключения к устройству");

            try
            {
                // Отправляем команду
                await _connectedStream.WriteAsync(data, 0, data.Length);
                await _connectedStream.FlushAsync();

                // Ждем и читаем ответ
                await Task.Delay(100); // Краткая пауза для обработки команд принтером
                return await ReadResponseAsync(timeoutMilliseconds);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обмена данными: {ex.Message}");
            }
        }

        /// <summary>
        /// Отключение от устройства
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _connectedStream?.Close();
                _connectedStream?.Dispose();
                _bluetoothClient?.Close();

                _isConnected = false;
                _connectedDevice = null;
                _connectedStream = null;
            }
            catch
            {
                // Игнорируем ошибки при отключении
            }
        }

        /// <summary>
        /// Проверка подключения
        /// </summary>
        public bool IsConnected => _isConnected;

        public void Dispose()
        {
            Disconnect();
            _bluetoothClient?.Dispose();
        }
    }
}
