namespace DemoTest
{
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