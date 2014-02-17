namespace Griffin.Net.Buffers
{
    public interface IBufferSlice
    {
        int Offset { get; }
        int Capacity { get;  }
        byte[] Buffer { get;  }
    }
}