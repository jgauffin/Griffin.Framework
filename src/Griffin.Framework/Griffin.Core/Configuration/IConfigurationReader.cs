namespace Griffin.Configuration
{
    /// <summary>
    /// Purpose of this class is to abstract away configuration reads.
    /// </summary>
    /// <remarks>
    /// <para>
    /// .NET standard dropped ConfigurationManager, so this is our alternative.
    /// </para>
    /// </remarks>
    /// TODO: Change this to a more specific class (with methods like IsServiceEnabled)
    public interface IConfigurationReader
    {
        string ReadAppSetting(string name);
        string ReadValue(string sectionName, string propertyName);
    }
}
