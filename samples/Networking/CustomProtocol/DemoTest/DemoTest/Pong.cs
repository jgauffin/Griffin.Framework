namespace DemoTest
{
    /// <summary>
    /// Simple C#/.Net object that gets serialized/encoded and sent from server to client, where it 
    /// gets decoded/deserialized.
    /// </summary>
    public class Pong
    {
        public string From { get; set; }
        public string To { get; set; }

        public override string ToString()
        {
            return "Pong " + To + " from " + From;
        }
    }
}