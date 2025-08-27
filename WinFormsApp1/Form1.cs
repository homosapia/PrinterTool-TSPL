using System.Drawing.Printing;
using System.Management;
using System.Reflection;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private List<PrinterInfo> printers = new List<PrinterInfo>();
        private PrinterConnect printerConnect = new PrinterConnect();
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
                // ������� �������������� ������
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
                // ���� �� ���������
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
            if(encoding == null)
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
                // �������� ���������� � ��������� ����� WMI
                var query = "SELECT Name, PortName FROM Win32_Printer";
                var searcher = new ManagementObjectSearcher(query);

                foreach (ManagementObject printer in searcher.Get())
                {
                    string name = printer["Name"]?.ToString() ?? "";
                    string port = printer["PortName"]?.ToString() ?? "";

                    // ���������, �������� �� ��� TSC ��������� (�� �������� ��� �����)
                    bool isTSC = name.ToUpper().Contains("TSC") ||
                                name.ToUpper().Contains("TDP") ||
                                port.ToUpper().Contains("COM") ||
                                port.ToUpper().Contains("LPT");

                    var printerInfo = new PrinterInfo
                    {
                        Name = name,
                        Port = port,
                        IsTSCPrinter = isTSC
                    };

                    printers.Add(printerInfo);
                    Printers.Items.Add(printerInfo);
                }

                // �������� ������ �������, ���� ����
                if (Printers.Items.Count > 0)
                {
                    Printers.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ ��� �������� ���������: {ex.Message}");
            }


            try
            {
                // �������� ��� ��������� ���������
                var encodingInfos = EncodingHelper.GetAllEncodings();
                encodingInfos.ForEach(x => EncodingList.Items.Add(x));
                EncodingList.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ ��������� ������ ���������: {ex.Message}");
            }
        }

        private void �onnection_Click(object sender, EventArgs e)
        {
            PrinterInfo printerInfo = Printers.SelectedItem as PrinterInfo;
            if (printerInfo == null)
                MessageBox.Show($"�������� �������");

            // ���������� ���������� � ��������
            string printerDetails = printerConnect.GetPrinterDetails(printerInfo);
            MessageBox.Show(printerDetails, "���������� � ��������");

            // �������� ������������
            Cursor.Current = Cursors.WaitCursor;

            bool isConnected = printerConnect.ConnectToPrinter(printerInfo);

            Cursor.Current = Cursors.Default;

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

        private void print_Click(object sender, EventArgs e)
        {
            var printer = Printers.SelectedItem as PrinterInfo;
            if (printer == null || !printer.IsConnected)
            { MessageBox.Show($"�������� �������"); return; }

            string testCommand = textBox1.Text;

            EncodingItem encodingItem = EncodingList.SelectedItem as EncodingItem;
            var encoding = GetEncodingByCodePage(encodingItem.CodePage);
            if (encoding == null)
            {
                encoding = GetEncodingByName(encodingItem.Name);
            }
            var commandBytes = encoding.GetBytes(testCommand);

            // ���������� �������
            bool success = printerConnect.SendCommand(printer, commandBytes);

            if (success)
            {
                MessageBox.Show("������� ������� ����������");
            }
            else
            {
                MessageBox.Show("������ �������� �������");
            }
        }
    }
}
