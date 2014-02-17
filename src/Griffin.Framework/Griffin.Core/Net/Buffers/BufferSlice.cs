using System;

namespace Griffin.Net.Buffers
{
    public class BufferSlice : IBufferSlice
    {
        public BufferSlice(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset+count > buffer.Length)
                throw new ArgumentOutOfRangeException("offset", offset, "Offset+Count must be less than the buffer length.");

            Capacity = count;
            Offset = offset;
            Buffer = buffer;
        }

        public int Offset { get; private set; }
        public int Capacity { get; private set; }
        public byte[] Buffer { get; private  set; }
    }

}
