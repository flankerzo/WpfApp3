using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp3.Views
{
    /// <summary>
    /// Interaction logic for CA_CS_Setup.xaml
    /// </summary>
    public partial class CA_CS_Setup : UserControl
    {
        public CA_CS_Setup()
        {
            InitializeComponent();
            LoadFilteredDevices();
        }
        private void LoadFilteredDevices()
        {
            List<Tuple<string, string>> targetDevices = new List<Tuple<string, string>>
            {
                Tuple.Create("132B", "210D"), // Example VID/PID pair 1 CA410
                //Tuple.Create("...", "..."),    // Example VID/PID pair 2 CA310
               // Tuple.Create("...", "..."),    // Example VID/PID pair 3 CS2000
                //Tuple.Create("...", "..."),    // Example VID/PID pair 4 VG870
                //Tuple.Create("...", "..."),    // Example VID/PID pair 5
                // Add more VID/PID pairs as needed
            };

            FilteredDevicesListView.ItemsSource = USBSerialConnector.USBDeviceManager.GetFilteredDevices(targetDevices);
        }
    }
}
