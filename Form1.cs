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
        public String _cpuTemp;
        public String _gpuTemp;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            Thread thread = new Thread(new ThreadStart(WorkThreadFunction));
            thread.Start();
            this.Visible = false;
        }

        public void CreateTextIcon()
        {
            Font fontToUse = new Font("Microsoft Sans Serif", 16, FontStyle.Regular, GraphicsUnit.Pixel);
            Brush brushToUse = new SolidBrush(Color.White);
            Bitmap bitmapText = new Bitmap(16, 16);
            Graphics g = System.Drawing.Graphics.FromImage(bitmapText);

            IntPtr cpuIcon;

            g.Clear(Color.Transparent);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            g.DrawString(_cpuTemp, fontToUse, brushToUse, -4, -2);
            cpuIcon = (bitmapText.GetHicon());

            IntPtr gpuIcon;

            g.Clear(Color.Transparent);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            g.DrawString(_gpuTemp, fontToUse, brushToUse, -4, -2);
            gpuIcon = (bitmapText.GetHicon());

            notifyIcon1.Icon = System.Drawing.Icon.FromHandle(cpuIcon);
            notifyIcon2.Icon = System.Drawing.Icon.FromHandle(gpuIcon);
            //DestroyIcon(hIcon.ToInt32);
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
        public void GetSystemInfo() {
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.GPUEnabled = true;
            computer.Accept(updateVisitor);
            try
            {
                for (int i = 0; i < computer.Hardware.Length; i++) {
                    if (computer.Hardware[i].HardwareType == HardwareType.CPU) {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++) {
                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature && computer.Hardware[i].Sensors[j].Name == "CPU Package") {
                                _cpuTemp = computer.Hardware[i].Sensors[j].Value.ToString();
                                break;
                            }
                        }
                    } else if(computer.Hardware[i].HardwareType == HardwareType.GpuNvidia || computer.Hardware[i].HardwareType == HardwareType.GpuAti) {
                        for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++) {
                            if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature) {
                                _gpuTemp = computer.Hardware[i].Sensors[j].Value.ToString();
                                break;
                            }
                        }
                    }
                }
                computer.Close();
            } catch (Exception ex) {
                Console.WriteLine(ex);
            } 
        }
        public void WorkThreadFunction() {
            try {
                while (true) {
                    GetSystemInfo();
                    label1.BeginInvoke(new InvokeDelegate(SetCPUText));
                    label2.BeginInvoke(new InvokeDelegate(SetGPUText));
                    CreateTextIcon();
                    Thread.Sleep(500);
                }
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        private void SetCPUText() {
            label1.Text = _cpuTemp;
        }

        private void SetGPUText() {
            label2.Text = _gpuTemp;
        }
    }
}
