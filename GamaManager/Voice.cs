using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GamaManager
{
    internal class Voice
    {
        private string ip;
        private string path = @"C:\Gleb\game-manager\buffer.wav";
        public string Ip
        {
            get { return ip; }
            set { ip = value; }
        }
        private int port;
        private Thread rec_thread;
        public int VPort
        {
            get { return port; }
            set { port = value; }
        }
        
        private WaveIn sourceStream = null;
        private Byte[] Data_ary;
        private NetworkStream ns;
        private WaveFileWriter waveWriter = null;
        private System.Windows.Threading.DispatcherTimer c_v = null;
        private Socket connector, sc, sock = null;
        public static TimeSpan GetSoundLength (string filename)
        {
            return TimeSpan.FromSeconds(5);
        }

        private void WriteBytes ()
        {
            if (ns != null)
            {
                SoundPlayer sp = new SoundPlayer(ns);
                sp.Play();
            }
        }

        private void Dispose ()
        {
            c_v.Stop();
            if (sourceStream != null)
            {
                sourceStream.StopRecording();
                sourceStream.Dispose();
            }
            if (waveWriter != null)
            {
                waveWriter.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        public void Send (string ip, int port)
        {
            this.Ip = ip;
            this.VPort = port;
            c_v = new System.Windows.Threading.DispatcherTimer();
            c_v.Interval = TimeSpan.FromMilliseconds(1000);
            c_v.IsEnabled = false;
            c_v.Tick += c_v_Tick;
            Recordwav();
        }

        private void Recordwav ()
        {
            sourceStream = new WaveIn();
            int devicenum = 0;
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                if (WaveIn.GetCapabilities(i).ProductName.Contains("icrophone"))
                {
                    devicenum = i;
                }
                sourceStream.DeviceNumber = devicenum;
                sourceStream.WaveFormat = new WaveFormat(22000, WaveIn.GetCapabilities(devicenum).Channels);
                sourceStream.DataAvailable += new EventHandler<WaveInEventArgs>(sourceStream_DataAvailable);


                waveWriter = new WaveFileWriter(path, sourceStream.WaveFormat);

                sourceStream.StartRecording();

                c_v.Start();

            }
        }

        void c_v_Tick (object sender, EventArgs e)
        {
            this.Dispose();
            Send_Bytes();
        }

        private void Send_Bytes ()
        {
            Data_ary = File.ReadAllBytes(path);
            connector = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ie = new IPEndPoint(IPAddress.Parse(this.Ip), this.VPort);
            ie.Address = IPAddress.Loopback;
            connector.Connect(ie);
            connector.Send(Data_ary, Data_ary.Length, 0);
            connector.Close();
        }

        private void sourceStream_DataAvailable (object sender, WaveInEventArgs e)
        {
            if (waveWriter == null)
            {
                return;
            }
            waveWriter.WriteData(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Flush();
        }

        public void Receive (int port)
        {
            this.VPort = port;
            rec_thread = new Thread(new ThreadStart(VoiceReceive));
            rec_thread.Start();
        }

        public void VoiceReceive ()
        {
            sc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ie = new IPEndPoint(0, this.VPort);
            sc.Bind(ie);
            sc.Listen(0);
            sock = sc.Accept();
            ns = new NetworkStream(sock);
            WriteBytes();
            sc.Close();
            while (true)
            {
                VoiceReceive();
            }
        }


    }
}
