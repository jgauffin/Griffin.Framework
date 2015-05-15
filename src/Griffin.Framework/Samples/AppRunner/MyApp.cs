using Griffin.ApplicationServices.AppDomains;

namespace AppRunner
{
    public class MyApp : IApplicationInitialize
    {
        public bool SupportsPause
        {
            get { return false; }
        }

        public void Start(string[] argv)
        {
        }

        public void Stop()
        {
        }

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public void OnUpdateAvailable(IUpdateInformation information)
        {
        }

        public bool RequestRestartPermission()
        {
            return true;
        }
    }
}