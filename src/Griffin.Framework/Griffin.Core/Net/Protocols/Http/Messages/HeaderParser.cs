using System;
using System.Text;
using Griffin.Net.Channels;
using System.Reflection;
using System.Threading.Tasks;
using Griffin.Net.Buffers;

namespace Griffin.Net.Protocols.Http.Messages
{
    /// <summary>
    ///     Used to parse the HTTP header.
    /// </summary>
    public class HeaderParser
    {
        private readonly StringBuilder _headerName = new StringBuilder();
        private readonly StringBuilder _headerValue = new StringBuilder();
        private int _lookAhead = -1;
        private Action<char> _parserMethod;
        private bool _isCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderParser" /> class.
        /// </summary>
        public HeaderParser()
        {
            _parserMethod = FirstLine;
        }

        /// <summary>
        /// Returns the current state that the parser is processing.
        /// </summary>
        public string State => _parserMethod.GetMethodInfo().Name;

        /// <summary>
        ///     Will try to parse everything in the buffer
        /// </summary>
        /// <param name="buffer">Buffer to read from, offset = where to start read, count = amount of bytes initially read into the buffer.</param>
        /// <param name="channel">Used to fill the buffer if data is missing</param>
        /// <returns>offset where the parser ended.</returns>
        /// <remarks>
        ///     <para>
        ///         Do note that the parser is for the header only. The <see cref="Completed" /> event will
        ///         indicate that there might be body bytes left in the buffer. You have to handle them by yourself.
        ///     </para>
        /// </remarks>
        public async Task Parse(IBufferSegment buffer, IInboundBinaryChannel channel)
        {
            await EnsureData(buffer, channel);

            while (true)
            {
                var ch = (char)Read(buffer);
                _parserMethod(ch);
                if (_isCompleted)
                    return;

                await EnsureData(buffer, channel);
            }
        }

        private static async Task EnsureData(IBufferSegment buffer, IInboundBinaryChannel channel)
        {
            if (buffer.BytesLeft() > 0)
            {
                return;
            }

            buffer.Count = 0;
            buffer.Offset = 0;
            await channel.ReceiveAsync(buffer);
            if (buffer.Count == 0)
                throw new ParseException("Channel got disconnected");
        }

        private int Read(IBufferSegment buffer)
        {
            if (_lookAhead == -1) 
                return buffer.Buffer[buffer.Offset++];

            var tmp = _lookAhead;
            _lookAhead = -1;
            return tmp;
        }

        private void FirstLine(char ch)
        {
            //ignore empty lines before the request line
            if (ch == '\r' || ch == '\n' && _headerName.Length == 0)
                return;

            if (ch == '\r')
                return;
            if (ch == '\n')
            {
                var line = _headerName.ToString().Split(new char[] { ' ' }, 3);
                if (line.Length != 3)
                    throw new BadRequestException("Expected first line to contain three words according to the HTTP specification, but got: '" + _headerName + "'");

                RequestLineParsed(line[0], line[1], line[2]);

                _headerName.Clear();
                _parserMethod = Name_StripWhiteSpacesBefore;
                return;
            }

            _headerName.Append(ch);
        }

        private void Name_StripWhiteSpacesBefore(char ch)
        {
            if (IsHorizontalWhitespace(ch))
                return;

            _parserMethod = Name_ParseUntilComma;
            _lookAhead = ch;
        }

        private void Name_ParseUntilComma(char ch)
        {
            if (ch == ':')
            {
                _parserMethod = Value_StripWhitespacesBefore;
                return;
            }

            _headerName.Append(ch);
        }

        private void Value_StripWhitespacesBefore(char ch)
        {
            if (IsHorizontalWhitespace(ch))
                return;

            _parserMethod = Value_ParseUntilQuoteOrNewLine;
            _lookAhead = ch;
        }

        private void Value_ParseUntilQuoteOrNewLine(char ch)
        {
            if (ch == '"')
            {
                _parserMethod = Value_ParseQuoted;
                return;
            }

            if (ch == '\r')
                return;
            if (ch == '\n')
            {
                _parserMethod = Value_CompletedOrMultiLine;
                return;
            }

            _headerValue.Append(ch);
        }

        private void Value_ParseQuoted(char ch)
        {
            if (ch == '"')
            {
                // exited the quoted string
                _parserMethod = Value_ParseUntilQuoteOrNewLine;
                return;
            }

            _headerValue.Append(ch);
        }

        private void Value_CompletedOrMultiLine(char ch)
        {
            if (IsHorizontalWhitespace(ch))
            {
                _headerValue.Append(' ');
                _parserMethod = Value_StripWhitespacesBefore;
                return;
            }
            if (ch == '\r')
                return; //empty line

            HeaderParsed(_headerName.ToString(), _headerValue.ToString());
            ResetLineParsing();
            _parserMethod = Name_StripWhiteSpacesBefore;

            if (ch == '\n')
            {
                Reset();
                _isCompleted = true;
                return;
            }


            _lookAhead = ch;
        }

        private bool IsHorizontalWhitespace(char ch)
        {
            return ch == ' ' || ch == '\t';
        }

        /// <summary>
        /// We've parsed a header and it's value.
        /// </summary>
        public MessageHeaderHandler HeaderParsed = delegate { };

        /// <summary>
        /// We've parsed a request line, meaning that all headers is for a HTTP Request.
        /// </summary>
        public MessageNameHandler RequestLineParsed = delegate { };

        /// <summary>
        /// Reset parser state
        /// </summary>
        public void Reset()
        {
            ResetLineParsing();
            _parserMethod = FirstLine;
        }

        /// <summary>
        /// Resets the line parsing so that a new header can be parsed.
        /// </summary>
        protected void ResetLineParsing()
        {
            _headerName.Clear();
            _headerValue.Clear();
        }

    }

    /// <summary>
    /// Callback for <see cref="HeaderParser"/>
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The value.</param>
    public delegate void MessageHeaderHandler(string name, string value);

    /// <summary>
    /// Callback for <see cref="HeaderParser"/> when a HTTP status line have been received.
    /// </summary>
    /// <param name="part1">HttpVerb or HttpVersion depending on if it's a request or a response</param>
    /// <param name="part2">PathAndQuery or StatusCode depending on if it's a request or a response.</param>
    /// <param name="part3">HttpVersion or StatusDescription depending on if it's a request or a response</param>
    public delegate void MessageNameHandler(string part1, string part2, string part3);
}