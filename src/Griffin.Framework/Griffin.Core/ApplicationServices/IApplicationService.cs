namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     A service which are doing some kind of work during the entire application lifetime (i.e. sort of an instance service).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This library can use your inversion of control container to find and execute all services registered in it. The librar will also catch any unhandled exception
    /// and restart services that fail. You can also use your configuration file to start/stop services during runtime.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>Let's say that you have a service which reads from a message queue:</para>
    /// <code>
    ///  public class RequestQueueReader : IApplicationService, IGuardedService
    ///  {
    ///      private readonly ILog _logger = LogManager.GetLogger(typeof (RequestQueueReader));
    ///      private readonly IContainer _container;
    ///      private readonly QueueReader _reader;
    ///      private int _retryCounter;
    /// 
    ///      public RequestQueueReader(IContainer container)
    ///      {
    ///          _container  container;
    /// 
    ///          var queueName = ConfigurationManager.AppSettings["RequestQueue.Name"];
    ///          if (queueName == null)
    ///              throw new ConfigurationErrorsException(
    ///                  "Did not find 'RequestQueue.Name' in appSettings. Configure it.");
    /// 
    ///          _reader = new QueueReader(queueName, new XmlMessageSerializer(new[] {typeof (RequestMsg)}));
    ///          _reader.MessageReceived += OnMessageRead;
    ///          _reader.Faulted += OnFaulted;
    ///      }
    /// 
    /// 
    ///      public void Start()
    ///      {
    ///          _reader.Start();
    ///      }
    /// 
    ///      public void Stop()
    ///      {
    ///          _reader.Stop();
    ///      }
    /// 
    ///      public bool IsRunning
    ///      {
    ///          get
    ///          {
    ///              return _reader.IsRunning;
    ///          }
    ///      }
    /// 
    ///      private void OnMessageRead(object sender, MessageReceivedEventArgs e)
    ///      {
    ///          var message = (Anno) e.Message;
    ///          _annoMessageHandler.Handle(message);
    ///          _retryCounter = 0;
    ///      }
    ///  }
    /// </code>
    /// <para>
    /// För att kontrollera om tjänsten snurrar eller inte så använder du app/web.config (du kan när som helst under körning ändra flaggan för att starta/stoppa utskickaren):
    /// </para>
    /// <code>
    /// <![CDATA[
    /// <configuration>
    ///  <appSettings>
    ///    <add key="AnnoMessageSSBThread.Enabled" value="true"/>
    ///  <appSettings>
    /// </configuration>
    /// ]]>
    /// </code>
    /// <para>
    /// Sedan så skapar vi vår service manager (hur du konfar autofac får du läsa på nätet):
    /// </para>
    /// <code>
    /// // container = autofac.
    /// var rootLocator = new AutofacServiceLocator(container);
    /// 
    /// // vid uppstart skapar du managern och startar tjänsterna
    /// _serviceManager = new ApplicationServiceManager(rootLocator);
    /// _serviceManager.Start();
    /// 
    /// 
    /// // .. och sedan där du stänger ned din applikation så anropar du:
    /// _serviceManager.Stop();
    /// </code>
    /// 
    /// </example>
    /// <seealso cref="ApplicationServiceManager"/>
    public interface IApplicationService
    {
        /// <summary>
        ///     Starta vad det nu är som tjänsten hanterar.
        /// </summary>
        void Start();

        /// <summary>
        ///     Stäng ned det som tjänsten hanterar
        /// </summary>
        void Stop();
    }
}