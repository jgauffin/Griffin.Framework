using System;

namespace Griffin.Net.Protocols.Stomp.Frames.Server
{
    public class StompError : BasicFrame
    {
        public StompError(string errorMessage) : base("ERROR")
        {
            Message = errorMessage;
        }


        /// <summary>
        ///     If the errenous frame included a receipt header, the ERROR frame SHOULD set the receipt-id header to match the value of the receipt header of the frame which the error is related to.
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
                    throw new ArgumentNullException("message");
                Headers["message"] = value;
            }
        }
    }
}
