using DirectShowLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SightLineApp
{
    public partial class Form1 : Form
    {
        bool m_saveCrossToFile = true;
        int m_fullScreenStep = 0;
        int m_savePanelLocation;
        IGraphBuilder m_graphBuilder;
        IMediaControl mediaControl = null;
        IBaseFilter theCompressor = null;
        IAMTVTuner tuner = null;
        IBaseFilter crossbar = null;
        IBaseFilter BouncingBallFilter = null;
        IBaseFilter InfiniteTeeFilter = null;
        IBaseFilter DumpFilter = null;
        IBaseFilter ElecardAVCFilter = null;
        IBaseFilter ShapesFilter = null;
        //get the video window from the graph
        IVideoWindow videoWindow = null;
        bool m_running = true;
        Thread m_testThread;
        CrossMx [] m_cross = new CrossMx[5];

        public Form1()
        {
            InitializeComponent();

            this.KeyPreview = true;
            try
            {
                Control.CheckForIllegalCrossThreadCalls = false;
                BuildGraph();
                int hr;
                //get the video window from the graph
                
                videoWindow = (IVideoWindow)m_graphBuilder;

                //Set the owener of the videoWindow to an IntPtr of some sort (the Handle of any control - could be a form / button etc.)
                hr = videoWindow.put_Owner(panel1.Handle);
                DsError.ThrowExceptionForHR(hr);

                //Set the style of the video window
                hr = videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren);
                DsError.ThrowExceptionForHR(hr);

                // Position video window in client rect of main application window
                hr = videoWindow.SetWindowPosition(0, 0, panel1.Width, panel1.Height);

                videoWindow.put_MessageDrain(panel1.Handle);

                DsError.ThrowExceptionForHR(hr);

                // Make the video window visible
                hr = videoWindow.put_Visible(OABool.True);
                DsError.ThrowExceptionForHR(hr);
                btnPause.Enabled = true;
                btnStart.Enabled = true;
                btnStop.Enabled = true;

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }
        public void Stop()
        {
            m_running = false;
            while (m_testThread != null && m_testThread.IsAlive == true)
            {
                Thread.Sleep(12);
                Application.DoEvents();
            }                
            mediaControl.Stop();
            
        }
        public void Run()
        {
            mediaControl.Run();

            m_testThread = new Thread(TestThread);
            m_testThread.Start();
        }

        public void Pause()
        {
            mediaControl.Pause();
        }


        void AddPinTeeFilter()
        {
             

            Type comtype;
            Guid gTeeFilter = new Guid("F8388A40D5BB11D0BE5A0080C706568E");
            comtype = Type.GetTypeFromCLSID(gTeeFilter); 
            InfiniteTeeFilter = (IBaseFilter)Activator.CreateInstance(comtype);

            int hr = m_graphBuilder.AddFilter(InfiniteTeeFilter, "Infinite Tee Filter");

            DsError.ThrowExceptionForHR(hr);
        }


        void AddDumpFilter()
        {

            Type comtype;
            Guid gDumpFilter = new Guid("36A5F770FE4C11CEA8ED00AA002FEAB5");
            comtype = Type.GetTypeFromCLSID(gDumpFilter);
            DumpFilter = (IBaseFilter)Activator.CreateInstance(comtype);
            int hr = m_graphBuilder.AddFilter(DumpFilter, "Dump Filter");
            DsError.ThrowExceptionForHR(hr);
        }

        void AddElecardAVCVideoEncoder()
        {

            Type comtype;
            Guid gAvcFilter = new Guid("E09EDEC905E34AAA9554149F94C24780");
            comtype = Type.GetTypeFromCLSID(gAvcFilter);
            ElecardAVCFilter = (IBaseFilter)Activator.CreateInstance(comtype);
            int hr = m_graphBuilder.AddFilter(ElecardAVCFilter, "AVC Video Encoder");
            DsError.ThrowExceptionForHR(hr);

        }

        void BuildGraph()
        {
            int hr;
            m_graphBuilder = (IGraphBuilder)new FilterGraph();
          

            //Create the media control for controlling the graph
            mediaControl = (IMediaControl)m_graphBuilder;
        

            Guid gBouncingBall = new Guid("fd5010418ebe11ce818300aa00577da1");
            Guid gShapeGuid = new Guid("E52BEAB445FB4D5ABC9E2381E61DCC47");

            Type comtype;
            Type comtypeShapes;
            comtype = Type.GetTypeFromCLSID(gBouncingBall);
            comtypeShapes = Type.GetTypeFromCLSID(gShapeGuid);
            BouncingBallFilter = (IBaseFilter)Activator.CreateInstance(comtype);
            hr = m_graphBuilder.AddFilter(BouncingBallFilter, "Bouncing ball");
            DsError.ThrowExceptionForHR(hr);
            
            ShapesFilter = (IBaseFilter)Activator.CreateInstance(comtypeShapes);
            hr = m_graphBuilder.AddFilter(ShapesFilter, "Shapes Filter");           
            DsError.ThrowExceptionForHR(hr);

            if (m_saveCrossToFile == true)
            {
                AddPinTeeFilter();
                AddDumpFilter();
                AddElecardAVCVideoEncoder();

                DumpCom dump = new DumpCom(DumpFilter);
                if (dump.SetFileName("c:\\dump1.asf") != 0)
                {
                    MessageBox.Show("Error set file");
                }
            }

             

            int j = 0;
            for (int i = 0; i < m_cross.Length * 2; i += 2)
            {
                
                m_cross[j] = new CrossMx(i, i + 1, ShapesFilter);
                m_cross[j].Init(2000);
                j++;
            }

           
            if (m_saveCrossToFile)
            {
                IPin pinInfinteInput = DsFindPin.ByName(InfiniteTeeFilter, "Input");
                IPin pinInfinteOutput1 = DsFindPin.ByName(InfiniteTeeFilter, "Output1");
                IPin pinSourceOut = DsFindPin.ByName(BouncingBallFilter, "Out");
                IPin pinAVCInput = DsFindPin.ByName(ElecardAVCFilter, "Input");
                IPin pinAVCOutput = DsFindPin.ByName(ElecardAVCFilter, "Output");

                IPin pinShapeIn = DsFindPin.ByName(ShapesFilter, "Input");
                IPin pinShapeOut = DsFindPin.ByName(ShapesFilter, "Output");

                IPin pinDumpIn = DsFindPin.ByName(DumpFilter, "Input");

                hr = m_graphBuilder.Connect(pinSourceOut, pinShapeIn);
                DsError.ThrowExceptionForHR(hr);
                hr = m_graphBuilder.Connect(pinShapeOut, pinInfinteInput);
                DsError.ThrowExceptionForHR(hr);

                //hr = m_graphBuilder.Connect(pinInfinteOutput1, pinDumpIn);
                hr = m_graphBuilder.Connect(pinInfinteOutput1, pinAVCInput);
                DsError.ThrowExceptionForHR(hr);
                hr = m_graphBuilder.Connect(pinAVCOutput, pinDumpIn);
                DsError.ThrowExceptionForHR(hr);

                IPin pinInfinteOutput2 = DsFindPin.ByName(InfiniteTeeFilter, "Output2");

                // let DirectShow connects the front and back
                hr = m_graphBuilder.Render((DirectShowLib.IPin)pinInfinteOutput2);
                DsError.ThrowExceptionForHR(hr);
            }
            else
            {

                IPin pinSourceOut = DsFindPin.ByName(BouncingBallFilter, "Out");

                IPin pinShapeIn = DsFindPin.ByName(ShapesFilter, "Input");
                
                hr = m_graphBuilder.Connect(pinSourceOut, pinShapeIn);
                DsError.ThrowExceptionForHR(hr);
                IPin pinShapeOut = DsFindPin.ByName(ShapesFilter, "Output");
                // let DirectShow connects the front and back
                hr = m_graphBuilder.Render((DirectShowLib.IPin)pinShapeOut);
                DsError.ThrowExceptionForHR(hr);

            }
        }

        private IBaseFilter CreateFilter(Guid category, string friendlyname)
        {
            object source = null;
            Guid iid = typeof(IBaseFilter).GUID;
            foreach (DsDevice device in DsDevice.GetDevicesOfCat(category))
            {
                if (device.Name.CompareTo(friendlyname) == 0)
                {
                    device.Mon.BindToObject(null, null, ref iid, out source);
                    break;
                }
            }

            return (IBaseFilter)source;
        }

        private bool TryConnectToAny(IPin sourcePin, IBaseFilter destinationFilter)
        {
            IEnumPins pinEnum;
            int hr = destinationFilter.EnumPins(out pinEnum);
            DsError.ThrowExceptionForHR(hr);
            IPin[] pins = { null };
            while (pinEnum.Next(pins.Length, pins, IntPtr.Zero) == 0)
            {
                int err = m_graphBuilder.Connect(sourcePin, pins[0]);
                if (err == 0)
                    return true;
                Marshal.ReleaseComObject(pins[0]);
            }
            return false;
        }
        void CloseGraph()
        {
            //Release COM objects

            Stop();
            if (BouncingBallFilter != null)
                Marshal.ReleaseComObject(BouncingBallFilter);
            if (BouncingBallFilter != null)
                Marshal.ReleaseComObject(m_graphBuilder);
            m_graphBuilder = null;
             
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            Pause();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseGraph();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            panel1.Width = this.Width;
            // Position video window in client rect of main application window
            int hr = videoWindow.SetWindowPosition(0, 0, panel1.Width, panel1.Height);
            DsError.ThrowExceptionForHR(hr);

        }

        void TestThread()
        {
            int x = 0;

            Random rnd = new Random();
             
            while (m_running == true)
            {
                
                  
                this.Invoke((MethodInvoker)delegate()
                {
                    if (m_running == false)
                        return;
                    for (int i = 0; i < 20; i++)
                    {
                        m_cross[0].Push(rnd.Next(1, 640), rnd.Next(1, 480));
                        m_cross[1].Push(rnd.Next(1, 640), rnd.Next(1, 480));
                        m_cross[2].Push(rnd.Next(1, 640), rnd.Next(1, 480));
                        m_cross[3].Push(rnd.Next(1, 640), rnd.Next(1, 480));
                        m_cross[4].Push(rnd.Next(1, 640), rnd.Next(1, 480));
                    }
                    m_cross[0].Clear();
                    m_cross[1].Clear();
                    m_cross[2].Clear();
                    m_cross[3].Clear();
                    m_cross[4].Clear();

                    m_cross[0].Draw(14, Color.Red, 3);
                    m_cross[1].Draw(14, Color.Green, 3);
                    m_cross[2].Draw(14, Color.Yellow, 3);
                    m_cross[3].Draw(14, Color.Blue, 3);
                    m_cross[4].Draw(14, Color.Purple, 3);
                     

                    //m_cross[0].DrawFirst(4, Color.Red, 3);
                    
                    //m_cross[0].Draw(55 + x, 55, 10, Color.Red, 3);

                    /*
                    m_cross[1].Clear();
                    m_cross[1].Draw(200 + x, 200, 60, Color.Red, 14);


                    m_cross[2].Clear();
                    m_cross[2].Draw(200+ x, 300, 60, Color.Green, 14);


                    m_cross[3].Clear();
                    m_cross[3].Draw(200+ x, 400, 60, Color.Red, 14);


                    m_cross[4].Clear();
                    m_cross[4].Draw(200 + x, 460, 60, Color.Blue, 14);
                    */
                    Thread.Sleep(20);                    
                    x = (x + 10) % 200;
                    
                });
                Thread.Sleep(1);             
            }
        }

        private void fullScreen1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoFullScreen();
           
        }
        void GoFullScreen()
        {
            m_savePanelLocation = panel1.Top;
            panel1.Width = this.Width;
            panel1.Height = this.Height;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;

            panel1.Top = this.Top;
            panel1.Width = this.Width;
            panel1.Height = this.Height;
        }

        void ShowGuiAfterFullScreen(bool s)
        {
            btnPause.Visible = s;
            btnStart.Visible = s;
            btnStop.Visible = s;
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                panel1.Top = m_savePanelLocation;
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                ShowGuiAfterFullScreen(true);
            }
            if (e.KeyCode == Keys.F5)
            {
                GoFullScreen();
            }
        }

        /*
        void  TestThread()
        {
            while (m_running == true)
            {

                for (int x = 0; x < 100; x+=5)
                {
                    if (m_running == false)
                        return;
                    this.Invoke((MethodInvoker)delegate()
                    {
                        m_cross[x].Draw(10 + x, 10 + x, 200, Color.Red, 4);
                        //m_cross.AddLine(0, 0, 100, 100, 200, Color.Orange, 4);
                        Thread.Sleep(100);
                        //m_cross.Clear();
                    });
                    Thread.Sleep(1);
                }
              
            }
        }
        */
    }
}
