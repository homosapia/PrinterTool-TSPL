namespace WinFormsApp1
{
    public class BluetoothDevice
    {
        public string DeviceName { get; set; }
        public string DeviceAddress { get; set; }
        public bool Connected { get; set; }
        public bool Authenticated { get; set; }

        public override string ToString()
        {
            return $"{DeviceName} ({DeviceAddress})";
        }
    }
}
