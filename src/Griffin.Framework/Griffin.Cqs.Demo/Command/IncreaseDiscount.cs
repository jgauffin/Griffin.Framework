namespace Griffin.Cqs.Demo.Command
{
    public class IncreaseDiscount : DotNetCqs.Command
    {
        public IncreaseDiscount(int percent)
        {
            Percent = percent;
        }

        protected IncreaseDiscount()
        {
        }

        public int Percent { get; private set; }
    }
}