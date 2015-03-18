namespace Griffin.Net.Buffers
{
    /// <summary>
    /// A slice of a larger buffer
    /// </summary>
    /// <remarks>
    /// <para>
    /// 
    /// To reduce the amount of allocations we can allocate a large buffer and then slice it into smaller chunks. This interface represents a chunk.
    /// 
    /// </para>
    /// </remarks>
    public interface IBufferSlice
    {
        /// <summary>
        /// Where this slice starts
        /// </summary>
        int Offset { get; }

        /// <summary>
        /// Number of bytes allocated for this slice
        /// </summary>
        int Capacity { get;  }

        /// <summary>
        /// Buffer that this is a slice of.
        /// </summary>
        byte[] Buffer { get;  }
    }
}