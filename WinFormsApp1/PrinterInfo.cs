namespace WinFormsApp1
{
    public class PrinterInfo
    {
        public string Name { get; set; }
        public string Port { get; set; }
        public ConnectionMethods ConnectionType { get; set; }
        public bool IsTSCPrinter { get; set; }
        public bool IsConnected { get; set; }

        public override string ToString()
        {
            return $"{Name}-{ConnectionType.ToString()}";
        }
    }
}
