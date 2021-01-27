using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Watchlist
{
    public class Stock : INotifyPropertyChanged, IDisposable
    {
        private static string host = "www.tallacoptions.com";
        private static int port = 6670;
        private TcpClient tcpClient;
        private string open, high, low, last, volume, lastSize, bid, bidSize, ask, askSize;
        private Timer timer;
        private readonly TimeSpan pingInterval = TimeSpan.FromSeconds(60);

        public event PropertyChangedEventHandler PropertyChanged;

        #region Constructor

        public Stock(string ticker)
        {
            Ticker = ticker;
            open = high = low = last = volume = lastSize = bid = bidSize = ask = askSize = "N/A";
        }

        #endregion

        #region Properties

        public string Ticker { get; }

        public string Open
        {
            get
            {
                return open;
            }
            private set
            {
                open = value;
                NotifyPropertyChanged();
            }
        }

        public string High
        {
            get
            {
                return high;
            }
            private set
            {
                high = value;
                NotifyPropertyChanged();
            }
        }

        public string Low
        {
            get
            {
                return low;
            }
            private set
            {
                low = value;
                NotifyPropertyChanged();
            }
        }

        public string Last
        {
            get
            {
                return last;
            }
            private set
            {
                last = value;
                NotifyPropertyChanged();
            }
        }

        public string LastSize
        {
            get
            {
                return lastSize;
            }
            private set
            {
                lastSize = value;
                NotifyPropertyChanged();
            }
        }

        public string Volume
        {
            get
            {
                return volume;
            }
            private set
            {
                volume = value;
                NotifyPropertyChanged();
            }
        }

        public string Bid
        {
            get
            {
                return bid;
            }
            private set
            {
                bid = value;
                NotifyPropertyChanged();
            }
        }

        public string BidSize
        {
            get
            {
                return bidSize;
            }
            private set
            {
                bidSize = value;
                NotifyPropertyChanged();
            }
        }

        public string Ask
        {
            get
            {
                return ask;
            }
            private set
            {
                ask = value;
                NotifyPropertyChanged();
            }
        }

        public string AskSize
        {
            get
            {
                return askSize;
            }
            private set
            {
                askSize = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Public Methods

        public async Task Connect()
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port);
            var ns = tcpClient.GetStream();
            var sr = new StreamReader(ns);
            var sw = new StreamWriter(ns);
            await sr.ReadLineAsync();
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            var thread = new Thread(async () =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    while (ns.DataAvailable)
                    {
                        var msg = RecvData(ns);
                        if (!string.IsNullOrEmpty(msg)) //discard keep-alive messages
                        {
                            var data = msg.Split('\0');
                            Last = data[0];
                            LastSize = data[1];
                            Bid = data[2];
                            BidSize = data[3];
                            Ask = data[4];
                            AskSize = data[5];
                            Volume = data[6];
                            Open = data[7];
                            High = data[8];
                            Low = data[9];
                        }
                    }
                    await Task.Delay(50);
                }
            });
            thread.Start();

            await sw.WriteLineAsync(Ticker.ToUpper());
            await sw.FlushAsync();

            //set up keep-alive 
            timer = new Timer(KeepAlive, cts, pingInterval, pingInterval);  
        }

        public void Dispose()
        {
            PropertyChanged = null;
        }

        #endregion

        #region Private Methods

        private void NotifyPropertyChanged([CallerMemberName] string propertyName="")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void KeepAlive(object state)
        {
            var cts = state as CancellationTokenSource;
            try
            {
                var sw = new StreamWriter(tcpClient.GetStream());
                await sw.WriteLineAsync();
                await sw.FlushAsync();
            }
            catch (Exception)
            {
                timer.Dispose();
                cts.Cancel();
                await Connect();
            }
        }

        private string RecvData(NetworkStream strm)
        {
            MemoryStream memstrm = new MemoryStream();
            byte[] data = new byte[4];
            int recv = strm.Read(data, 0, 4);
            int size = BitConverter.ToInt32(data, 0);
            if (size == 0)  //keep alive message
            {
                return string.Empty;
            }
            int offset = 0;
            while (size > 0)
            {
                data = new byte[2048];
                recv = strm.Read(data, 0, size);
                memstrm.Write(data, offset, recv);
                offset += recv;
                size -= recv;
            }
            var val = memstrm.GetBuffer();
            return Encoding.UTF8.GetString(val);
        }

        #endregion
    }
}
