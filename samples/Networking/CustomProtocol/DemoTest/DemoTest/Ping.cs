namespace DemoTest
{
    /// <summary>
    /// Simple C#/.Net object that gets serialized/encoded and sent from client to server, where it 
    /// gets decoded/deserialized.
    /// </summary>
    public class Ping
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return "Ping from " + Name;
        }
    }
}
