using System.Runtime.CompilerServices;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     Funktioner för att hantera tjänster, dvs klasser som utför arbete så länge som applikationen lever (
    ///     <see cref="IApplicationService" />), samt för att
    ///     hantera uppgifter som behöver utföras i bakgrunden (<see cref="IBackgroundJob" />).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Det finns två basklasser som du kan använda för att implementera din logik. Det första är
    ///         <see cref="ApplicationServiceThread" /> för kod som ska köras under hela tiden som application servicen är
    ///         igång. Det andra är
    ///         <see cref="ApplicationServiceTimer" /> som är till för att utföra jobb i bakgrunden. Notera att för den senare
    ///         så rekommenderas det att du använder <see cref="IBackgroundJob" /> istället då den stödjer Dependency
    ///         Injection.
    ///     </para>
    /// </remarks>
    /// <seealso cref="IApplicationService" />
    /// <seealso cref="IBackgroundJob" />
    [CompilerGenerated]
    internal class NamespaceDoc
    {
    }
}