using NAudio.Wave;
using System.Net;
using System.Net.Sockets;

namespace Client.Services.Audio
{
    internal class VoiceCallHandler
    {
        public bool isReceiving { get; private set; }
        public bool IsSending { get; private set; }
        IPEndPoint remoteEndPointReceive;
        IPEndPoint remoteEndPointSend;
        UdpClient udpClientReceive;
        UdpClient udpClientSend;
        NAudio.Wave.WaveInEvent waveIn;
        Thread receiveThread;

        public VoiceCallHandler(IPEndPoint remoteEndPointReceive, IPEndPoint remoteEndPointSend)
        {
            this.remoteEndPointReceive = remoteEndPointReceive;
            this.remoteEndPointSend = remoteEndPointSend;
            udpClientReceive = new UdpClient(remoteEndPointReceive.Port, AddressFamily.InterNetwork);
            udpClientSend = new UdpClient(remoteEndPointSend.Address.ToString(), remoteEndPointSend.Port);

            waveIn = new NAudio.Wave.WaveInEvent
            {
                DeviceNumber = 0, // TODO Device change
                WaveFormat = new NAudio.Wave.WaveFormat(rate: 44100, bits: 32, channels: 1),
                BufferMilliseconds = 200
            };
        }

        public void Receive() // кидай на інший потік коли хочеш припинити слухання isReceiving = false
        {
            isReceiving = true;
            receiveThread = new Thread(() =>
            {
                while (isReceiving)
                {
                    byte[] receivedData = udpClientReceive.Receive(ref remoteEndPointReceive);

                    WaveOutEvent _waveOut = new WaveOutEvent();
                    IWaveProvider provider = new RawSourceWaveStream(new MemoryStream(receivedData), new WaveFormat(44100, 32, 1));
                    _waveOut.Init(provider);
                    _waveOut.Play();

                    Thread.Sleep(10);
                }
            });
            receiveThread.Start();
        }
        public void StopReceive() { isReceiving = false; }
        public void Send()
        {
            IsSending = true;

            waveIn.DataAvailable += (object? sender, NAudio.Wave.WaveInEventArgs e) =>
            {
                udpClientSend.Send(e.Buffer, e.Buffer.Length);
            };
            waveIn.StartRecording();
        }
        public void StopSend() { IsSending = false; waveIn.StopRecording(); }
    }
}
