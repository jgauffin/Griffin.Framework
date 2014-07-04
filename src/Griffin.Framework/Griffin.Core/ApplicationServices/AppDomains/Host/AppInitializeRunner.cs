using System;
using System.Text;

namespace Griffin.ApplicationServices.AppDomains.Host
{
    /// <summary>
    ///     Used to control the application within the new appdomain
    /// </summary>
    /// <remarks>
    /// <para>Runs inside the new app domain to be able to allow the <see cref="HostedAppDomain"/> to control it.</para>
    /// <para>this class starts your <see cref="IApplicationInitialize"/> inside the new appdomain to allow your application to run.</para>
    /// </remarks>
    public class AppInitializeRunner : MarshalByRefObject
    {
        private readonly CommunicationChannel _channel = new CommunicationChannel();
        private IApplicationInitialize _service;
        private Type _type;

        private void ProcessCommand(string command, string[] argv)
        {
            switch (command)
            {
                case "start":
                    var type = argv[0];
                    LaunchService(type, argv[1]);
                    break;
                case "stop":
                    StopService();
                    _channel.Write("stopped");
                    _channel.Stop();
                    break;
            }
        }

        public void Start(string pipeName, string id)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            _channel.Start(pipeName, id);
        }

        private void OnUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            var str = GetStringAsBase64(e.ExceptionObject.ToString());
            _channel.Send("AppDomainException", str);
        }


        private string GetStringAsBase64(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(bytes);
        }

        private void LaunchService(string typeName, string args)
        {
            var type = Type.GetType(typeName, true);
            _service = (IApplicationInitialize) Activator.CreateInstance(type);
            _service.Start(args.Split(';'));
        }

        public void StopService()
        {
            _service.Stop();
        }
    }
}