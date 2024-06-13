using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Management;
using System.Text.RegularExpressions;

namespace USBSerialConnector
{
    public class USBDeviceInfo
    {
        public string DeviceID { get; private set; }
        public string Name { get; private set; }
        public string VID { get; private set; }
        public string PID { get; private set; }
        public string COMPort { get; private set; }

        public USBDeviceInfo(string deviceID, string name, string vid, string pid, string comPort)
        {
            DeviceID = deviceID;
            Name = name;
            VID = vid;
            PID = pid;
            COMPort = comPort;
        }
    }
    public static class USBDeviceManager
    {
        public static List<USBDeviceInfo> GetAllDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity WHERE Caption LIKE '%(COM%'"))
                collection = searcher.Get();

            foreach (var device in collection)
            {
                string name = (string)device.GetPropertyValue("Name");
                string pnpDeviceID = (string)device.GetPropertyValue("PNPDeviceID");
                string vid = GetVIDFromPNPDeviceID(pnpDeviceID);
                string pid = GetPIDFromPNPDeviceID(pnpDeviceID); // Obtain PID from PNPDeviceID
                string comPort = GetCOMPortNumber(name); // Obtain COM port number from device name or other properties
                string deviceID = ""; // You can define the deviceID as per your requirements
                //dontmindthis
                // Extract VID and PID from PNPDeviceID string
                // Example: "USB\VID_XXXX&PID_YYYY" where XXXX is VID and YYYY is PID

                string customName = GetCustomDeviceName(vid, pid); // Get custom name based on VID and PID
                string finalName = string.IsNullOrEmpty(customName) ? name : customName; // Use custom name if available, otherwise use default name

                devices.Add(new USBDeviceInfo(deviceID, finalName, vid, pid, comPort));
            }

            collection.Dispose();

            return devices;
        }

        private static string GetCustomDeviceName(string vid, string pid)
        {
            if (customDeviceNames.ContainsKey(vid) && customDeviceNames[vid].ContainsKey(pid))
            {
                return customDeviceNames[vid][pid];
            }
            return null; // Custom name not found
        }

        private static Dictionary<string, Dictionary<string, string>> customDeviceNames = new Dictionary<string, Dictionary<string, string>>
{
            // Add custom names for devices based on VID and PID  CA410 132B", "210D"
            { "VID_XXXX&PID_YYYY", new Dictionary<string, string>
                {
                    { "YYYY", "Custom Device Name" } // Example: replace XXXX and YYYY with actual VID and PID values
                }
            },
            {
            "132B", new Dictionary<string, string>
            {
                { "210D", "CA410" }

            }
              },
            {
            "0B7D", new Dictionary<string, string>
            {
                { "0870", "Astro870" }
            }
              },
            {
            "1132B", new Dictionary<string, string>
            {
                { "2104", "CS2000" }
            }
              },
                // Add more entries as needed
            };

        /*
         * ASTRO
         %USB\VID_0B7D&PID_0870.DeviceDesc%=ITFUSBDV_V2.Dev, USB\VID_0B7D&PID_0870
%USB\VID_0B7D&PID_1830.DeviceDesc%=ITFUSBDV_V2.Dev, USB\VID_0B7D&PID_1830
%USB\VID_0B7D&PID_1831.DeviceDesc%=ITFUSBDV_V2.Dev, USB\VID_0B7D&PID_1831
        */

        public static string GetVIDFromPNPDeviceID(string pnpDeviceID)
        {
            Regex regex = new Regex(@"VID_([0-9A-Fa-f]+)");
            Match match = regex.Match(pnpDeviceID);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return ""; // VID not found
        }

        public static string GetPIDFromPNPDeviceID(string pnpDeviceID)
        {
            Regex regex = new Regex(@"PID_([0-9A-Fa-f]+)");
            Match match = regex.Match(pnpDeviceID);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return ""; // PID not found
        }

        public static int CountDevices(string vid, string pid)
        {
            List<USBDeviceInfo> devices = GetAllDevices();
            int count = 0;

            foreach (USBDeviceInfo device in devices)
            {
                if (device.VID == vid && device.PID == pid)
                {
                    count++;
                }
            }

            return count;
        }

        public static string GetCOMPortNumber(string deviceName)
        {
            // Example deviceName: "USB Serial Port (COM3)"
            // Extract COM port number: "COM3"

            int startIndex = deviceName.LastIndexOf("(COM");
            int endIndex = deviceName.LastIndexOf(")");
            if (startIndex != -1 && endIndex != -1)
            {
                return deviceName.Substring(startIndex + 1, endIndex - startIndex - 1);
            }

            return ""; // COM port number not found
        }

        public static List<USBDeviceInfo> GetFilteredDevices(List<Tuple<string, string>> targetDevices)
        {
            List<USBDeviceInfo> allDevices = GetAllDevices();
            List<USBDeviceInfo> filteredDevices = new List<USBDeviceInfo>();

            // Filter devices based on each target VID and PID pair
            foreach (var targetDevice in targetDevices)
            {
                string targetVID = targetDevice.Item1;
                string targetPID = targetDevice.Item2;

                foreach (USBDeviceInfo device in allDevices)
                {
                    if (device.VID == targetVID && device.PID == targetPID)
                    {
                        filteredDevices.Add(device);
                    }
                }
            }

            return filteredDevices;
        }

    }


}
