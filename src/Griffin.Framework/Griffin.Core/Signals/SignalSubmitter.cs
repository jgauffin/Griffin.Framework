using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading;
using Griffin.Logging;

namespace Griffin.Signals
{
    /// <summary>
    ///     Used to post signals to a HTTP service.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use <see cref="Configure" /> to allow the submitter to upload reports. Then use <see cref="UploadAllSignals" />
    ///         if you want the class to upload all signals, or manually subscribe
    ///         on <see cref="Signal.SignalRaised" /> and <see cref="Signal.SignalSupressed" /> to upload signals by calling
    ///         <see cref="Send" />.
    ///     </para>
    ///     <para>
    ///         Serializes the data to JSON (by using <see cref="SignalDTO" />) and then do a HTTP post to the server that you
    ///         configured by using .
    ///     </para>
    /// </remarks>
    public class SignalSubmitter
    {
        private static readonly SignalSubmitter _instance = new SignalSubmitter();
        private static string _appName;
        private static Uri _uri;
        private readonly ILogger _logger = LogManager.GetLogger<SignalSubmitter>();
        private readonly ConcurrentQueue<SignalDTO> _signalsToSend = new ConcurrentQueue<SignalDTO>();
        private readonly ManualResetEvent _submitSignalsEvent = new ManualResetEvent(false);
        private Thread _thread;

        private SignalSubmitter()
        {
            _thread = new Thread(PostSignalsThreadFunc);
            _thread.IsBackground = true;
            _thread.Start();
        }

        /// <summary>
        ///     Activate the submitter.
        /// </summary>
        /// <param name="appliationName">Your application name</param>
        /// <param name="destination"></param>
        public static void Configure(string appliationName, Uri destination)
        {
            if (appliationName == null) throw new ArgumentNullException("appliationName");
            if (destination == null) throw new ArgumentNullException("destination");

            _appName = appliationName;
            _uri = destination;
        }

        /// <summary>
        ///     Enqueue a signal for upload
        /// </summary>
        /// <param name="signal">signal to serialize as JSON and upload.</param>
        /// <remarks>
        ///     <para>
        ///         The actual upload will be done in the background.
        ///     </para>
        ///     <para>
        ///         Will do nothing if <see cref="Configure" /> have not been used.
        ///     </para>
        /// </remarks>
        public static void Send(Signal signal)
        {
            if (signal == null) throw new ArgumentNullException("signal");
            if (_uri == null)
                return;

            _instance._signalsToSend.Enqueue(new SignalDTO(_appName, signal));
        }

        /// <summary>
        ///     Enqueue a signal for upload
        /// </summary>
        /// <param name="signal">signal to serialize as JSON and upload.</param>
        /// <remarks>
        ///     <para>
        ///         The actual upload will be done in the background.
        ///     </para>
        ///     <para>
        ///         Will do nothing if <see cref="Configure" /> have not been used.
        ///     </para>
        /// </remarks>
        public static void Send(SignalDTO signal)
        {
            if (signal == null) throw new ArgumentNullException("signal");
            if (_uri == null)
                return;

            _instance._signalsToSend.Enqueue(signal);
        }

        /// <summary>
        ///     Will subscribe on the <see cref="Signal.SignalRaised" /> and <see cref="Signal.SignalSupressed" /> to be able to
        ///     upload all signals.
        /// </summary>
        /// <exception cref="InvalidOperationException">You must Configure() first.</exception>
        public static void UploadAllSignals()
        {
            if (_appName == null)
                throw new InvalidOperationException("You must Configure() first.");

            Signal.SignalRaised += (sender, args) => Send(new SignalDTO(_appName, (Signal)sender)
            {
                CallingMethod = args.CallingMethod,
                Exception = args.Exception.ToString(),
                Reason = args.Reason
            });
            Signal.SignalSupressed += (sender, args) => Send((Signal) sender);
        }

        private void PostSignalsThreadFunc()
        {
            while (true)
            {
                if (!_submitSignalsEvent.WaitOne(500))
                    continue;

                try
                {
                    SignalDTO dto;
                    if (!_signalsToSend.TryDequeue(out dto))
                    {
                        _submitSignalsEvent.Reset();
                        Thread.Yield();
                        if (!_signalsToSend.TryDequeue(out dto))
                            continue;
                    }

                    var json = SimpleJson.SimpleJson.SerializeObject(dto);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    var request = (HttpWebRequest) WebRequest.Create(_uri);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.GetRequestStream().Write(bytes, 0, bytes.Length);

                    try
                    {
                        var resposne = request.GetResponse();
                        resposne.Close();
                    }
                    catch (WebException)
                    {
                        _signalsToSend.Enqueue(dto);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to upload signals.", ex);
                }
            }
        }
    }
}