namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     Ett bakgrundsjobb (för exempelvis databasunderhåll).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Alla jobb exekveras ungefär efter ett förutbestämt tidsintervall (inställbart för den inbyggda exekveraren
    ///         <see
    ///             cref="BackgroundJobManager" />
    ///         . Om ditt jobb behöver exekveras långsammare än det inställda intervallet så får du skapa en egen tidsstämpel som du gör en jämförelse
    ///         med varje gång <c>Execute()</c> metoden blir anropad.
    ///     </para>
    ///     <para>
    ///         Alla klasser som implementerar detta interface kommer att hittas med hjälp av en inversion of control container (om
    ///         <see
    ///             cref="BackgroundJobManager" />
    ///         används). Notera att klasserna kommer därför
    ///         ha den livstid som är inställd när klassen registrerades i din IoC container. Så om din klass har en kort livstid behöver du använda en <c>static</c> variabel
    ///         för tidskontrollen.
    ///     </para>
    ///     <para>
    ///         För mer information om själva exekveringen så läs om <see cref="BackgroundJobManager" />.
    ///     </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// [ContainerService]
    /// public class RensaGamlaTåg : IBackgroundJob
    /// {
    ///     private readonly IUnitOfWork _uow;
    ///     private static DateTime _senastKörning;
    /// 
    ///     public RensaGamlaTåg(IUnitOfWork uow)
    ///     {
    ///         if (uow == null) throw new ArgumentNullException("uow");
    /// 
    ///         _uow = uow;
    ///     }
    /// 
    ///     public void Execute()
    ///     {
    ///         if (_senastKörning.Date >= DateTime.Today)
    ///             return;
    ///         _senastKörning = DateTime.Today;
    /// 
    ///         using (var cmd = _uow.CreateCommand())
    ///         {
    ///             cmd.CommandText = "DELETE FROM Tåg WHERE SkapadTidpunkt < @datum";
    ///             cmd.AddParameter("datum", DateTime.Today.AddDays(-10));
    ///             cmd.ExecuteNonQuery();
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public interface IBackgroundJob
    {
        /// <summary>
        ///     Exekvera jobbet.
        /// </summary>
        /// <remarks>
        ///     Eventuella undantag hanteras av klassen som exekverar jobbet.
        /// </remarks>
        void Execute();
    }
}