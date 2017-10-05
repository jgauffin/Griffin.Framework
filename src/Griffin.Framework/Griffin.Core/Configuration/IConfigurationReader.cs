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
    public interface IConfigurationReader
    {
        string ReadAppSetting(string name);
        string ReadValue(string sectionName, string propertyName);
    }
}
