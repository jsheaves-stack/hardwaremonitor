using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;

namespace cpuTemp4
{
    public partial class Form1 : Form {
        public delegate void InvokeDelegate();
        private String cpuTemp;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            Thread thread = new Thread(new ThreadStart(WorkThreadFunction));
            thread.Start();
        }

        public class UpdateVisitor : IVisitor {
            public void VisitComputer(IComputer computer) {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware) {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }

        static String GetSystemInfo() {
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.Accept(updateVisitor);
            String temp;
            for (int i = 0; i < computer.Hardware.Length; i++) {
                Console.WriteLine(computer.Hardware[i]);
                if (computer.Hardware[i].HardwareType == HardwareType.CPU) {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++) {
                        if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature && computer.Hardware[i].Sensors[j].Name == "CPU Package") {
                            temp = computer.Hardware[i].Sensors[j].Value.ToString();
                            computer.Close();
                            return temp;
                        }
                    }
                }
            }
            return "";
        }

        public void WorkThreadFunction() {
            try {
                while (true) {
                    cpuTemp = GetSystemInfo();
                    label1.BeginInvoke(new InvokeDelegate(SetText));
                    Thread.Sleep(500);
                }
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        private void SetText() {
            label1.Text = cpuTemp;
        }
    }
}
