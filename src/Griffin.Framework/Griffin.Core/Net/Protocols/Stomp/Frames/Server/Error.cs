using System;

namespace Griffin.Net.Protocols.Stomp.Frames.Server
{
    /// <summary>
    /// ERROR frame
    /// </summary>
    public class StompError : BasicFrame
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StompError"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public StompError(string errorMessage) : base("ERROR")
        {
            Message = errorMessage;
        }


        /// <summary>
        ///     If the erroneous frame included a receipt header, the ERROR frame SHOULD set the receipt-id header to match the value of the receipt header of the frame which the error is related to.
        /// </summary>
        public string ReceiptId
        {
            get { return Headers["receipt-id"]; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                Headers["receipt-id"] = value;
            }
        }

        /// <summary>
        ///     Short description of what went wrong.
        /// </summary>
        public string Message
        {
            get { return Headers["message"]; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                Headers["message"] = value;
            }
        }
    }
}
