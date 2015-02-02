using System.Runtime.CompilerServices;
using Griffin.ApplicationServices;

namespace Griffin.Signals
{
    /// <summary>
    ///     Signals are used to keep track of important states in your application. You can for instance keep track
    ///     of how you threads and background jobs are performing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Signals are integrated into:
    /// </para>
    /// <list type="bullet">
    /// <item> <see cref="ApplicationServiceManager"/></item>
    /// <item> <see cref="ApplicationServiceThread"/></item>
    /// <item> <see cref="ApplicationServiceTimer"/></item>
    /// <item> <see cref="BackgroundJobManager"/></item>
    /// </list>
    /// <para>
    /// which allows you to get state reports out of the box if you use those classes.
    /// </para>
    ///     <para>
    ///         A basic example where a signal is used to prevent the log from getting spammed when message queue processing
    ///         fails:
    ///     </para>
    ///     <code>
    /// public void YourMethod()
    /// {
    ///     while (!_shutDownSignal.Wait(0))
    ///     {
    ///         try
    ///         {
    ///             var msg = _queue.ReadMessage();
    ///             
    ///             // process message
    ///             // [...]
    ///             
    ///             if (Signal.Reset("MyService.MessageReader"))
    ///                 _logger.Info("We are fully functional again");
    ///         }
    ///         catch (Exception ex)
    ///         {
    ///             if (Signal.Raise("MyService.MessageReader"))
    ///             {
    ///                 _logger.Fatal("MyService started to fail", ex);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    ///     <para>
    ///         Griffin.Framework will soon contain a lightweight server to allow you to receive the signals and get a quick
    ///         overview if everything in your application is working as expected. In the mean
    ///         time you can receive the signals in your own HTTP server by doing the following.
    ///     </para>
    ///     <para>
    ///         To start with you have to activate the submitter:
    ///     </para>
    ///     <code>
    /// SignalSubmitter.Configure("yourAppName", new Uri("http://yourServer/yourUrl"));
    /// SignalSubmitter.UploadAllSignals();
    /// </code>
    ///     <para>
    ///         Once done you can simply raise the signal to get it uploaded:
    ///     </para>
    ///     <code>
    /// public void YourMethod()
    /// {
    ///     while (!_shutDownSignal.Wait(0))
    ///     {
    ///         try
    ///         {
    ///             var msg = _queue.ReadMessage();
    ///             
    ///             //process message
    ///             
    ///             Signal.Reset("MyService.MessageReader");
    ///         }
    ///         catch (Exception ex)
    ///         {
    ///             Signal.Raise("MyService.MessageReader");
    ///         }
    ///     }
    /// }
    /// </code>
    ///     <para>
    ///         For clarity you can also keep a reference to the signal:
    ///     </para>
    ///     <code>
    /// public class YourClass
    /// {
    ///     Signal _readerSignal = Signal.Create("MyService.MessageReader");
    ///     
    ///     public void YourMethod()
    ///     {
    ///         while (!_shutDownSignal.Wait(0))
    ///         {
    ///             try
    ///             {
    ///                 var msg = _queue.ReadMessage();
    ///                 
    ///                 // process message
    ///                 // [...]
    /// 				
    ///                 _readerSignal.Reset();
    ///             }
    ///             catch (Exception ex)
    ///             {
    ///                 _readerSignal.Raise("Failed to read message", ex);
    ///             }
    ///         }
    ///     }
    /// }
    /// 
    /// </code>
    /// </remarks>
    [CompilerGenerated]
    internal class NamespaceDoc
    {
    }
}