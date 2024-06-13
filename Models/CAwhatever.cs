using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO.Ports;
using CASDK2;


namespace WpfApp3
{
    public class CAwhatever
    {
        const int MAX_COUNT = 10;
        private int err = 0;
        private int caCount = 1;
        private CASDK2Ca200 objCa200 = null;
        private CASDK2Cas objCas = null;
        private CASDK2Ca[] objCa = new CASDK2Ca[MAX_COUNT];
        private CASDK2Probes[] objProbes = new CASDK2Probes[MAX_COUNT];
        private CASDK2OutputProbes[] objOutputProbes = new CASDK2OutputProbes[MAX_COUNT];
        private CASDK2Probe[] objProbe = new CASDK2Probe[MAX_COUNT];
        private CASDK2Memory[] objMemory = new CASDK2Memory[MAX_COUNT];


        const int MODE_Lvxy = 0;
        const int MODE_Tduv = 1;
        const int MODE_Lvduv = 5;
        const int MODE_FMA = 6;
        const int MODE_XYZ = 7;
        const int MODE_JEITA = 8;
        const int MODE_LvPeld = 9;

        public CAwhatever(int count)
        {
            if (count < 1)
            {
                caCount = 1;
            }
            else if (caCount > MAX_COUNT)
            {
                caCount = MAX_COUNT;
            }
            else
            {
                caCount = count;
            }
        }

        ~CAwhatever()
        {
            Discconect();
        }

        public void MultiConnect()
        {
            objCa200 = new CASDK2Ca200();
            for (int ca = 0; ca < caCount; ca++)
            {
                // Use another ID ex. ca+1
                // Connect devices CA1 and CA2 by USB
                err = objCa200.SetConfiguration(ca + 1, "1", 0, 38400, 0);
            }

            // Substitute object variables
            err = objCa200.get_Cas(ref objCas);
            for (int ca = 0; ca < caCount; ca++)
            {
                err = objCas.get_Item(ca + 1, ref objCa[ca]);
                err = objCa[ca].get_Probes(ref objProbes[ca]);
                err = objCa[ca].get_OutputProbes(ref objOutputProbes[ca]);
                err = objCa[ca].get_Memory(ref objMemory[ca]);
                err = objProbes[ca].get_Item(1, ref objProbe[ca]);
                err = objOutputProbes[ca].AddAll();
                err = objOutputProbes[ca].get_Item(1, ref objProbe[ca]);
            }
        }

        public void Discconect()
        {
            // Disconnect CA-410
            err = objCa200.DisconnectAll();
            objCa200 = null;
        }

        public void MultiSetting()
        {
            int freqmode = 4; // SyncMode : INT
            double freq = 60.0; // frequency = 60.0Hz
            int speed = 1; // Measurement speed : FAST
            int lvmode = 1; // Lv : cd/m2
            for (int ca = 0; ca < caCount; ca++)
            {
                err = objCa[ca].CalZero(); // Zero-Calibration
                err = objCa[ca].put_DisplayProbe("P1"); // Set display probe to P1
                err = objCa[ca].put_SyncMode(freqmode, freq); // Set sync mode and frequency
                err = objCa[ca].put_AveragingMode(speed); // Set measurement speed
                err = objCa[ca].put_BrightnessUnit(lvmode); // Set Brightness unit
            }
        }

        public List<string> MultiMeasurement()  //string  ako vysledok?
        {
            List<string> measurementResults = new List<string>();
            for (int ca = 0; ca < caCount; ca++)
            {
                err = objCa[ca].put_DisplayMode(MODE_Lvxy); // Mode:Color Lvxy
            }

            // 3.4.4 - ②
            // Use SendMsr() and ReceiveMsr() for multi probes
            err = objCas.SendMsr(); // Measure
            err = objCas.ReceiveMsr(); // Get results

            // Declaration variables for measurement data
            double[] Lv = new double[caCount];
            double[] sx = new double[caCount];
            double[] sy = new double[caCount];
            double[] X = new double[caCount];
            double[] Y = new double[caCount];
            double[] Z = new double[caCount];

            for (int ca = 0; ca < caCount; ca++)
            {
                // Get measurement data
                err = objProbe[ca].get_Lv(ref Lv[ca]);
                err = objProbe[ca].get_sx(ref sx[ca]);
                err = objProbe[ca].get_sy(ref sy[ca]);
                err = objProbe[ca].get_X(ref X[ca]);
                err = objProbe[ca].get_Y(ref Y[ca]);
                err = objProbe[ca].get_Z(ref Z[ca]);

                string measurementInfo = $"Lv: {Lv[ca]} x: {sx[ca]} y: {sy[ca]}\nX: {X[ca]} Y: {Y[ca]} Z: {Z[ca]}";
                measurementResults.Add(measurementInfo);
            }

            // Clean up
            Array.Clear(Lv, 0, caCount);
            Array.Clear(sx, 0, caCount);
            Array.Clear(sy, 0, caCount);
            Array.Clear(X, 0, caCount);
            Array.Clear(Y, 0, caCount);
            Array.Clear(Z, 0, caCount);
            return measurementResults;
        }
    }

}
