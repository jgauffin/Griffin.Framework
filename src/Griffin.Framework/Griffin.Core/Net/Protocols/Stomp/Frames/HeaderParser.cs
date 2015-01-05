using System;
using System.Text;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Stomp.Frames
{
    /// <summary>
    ///     Used to parse the STOMP header.
    /// </summary>
    public class HeaderParser
    {
        private readonly StringBuilder _headerName = new StringBuilder();
        private readonly StringBuilder _headerValue = new StringBuilder();
        private Action _completed;
        private FrameNameHandler _frameNameParsed;
        private FrameHeaderHandler _headerParsed;
        private bool _isCompleted;
        private int _lookAhead = -1;
        private Action<char> _parserMethod;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HeaderParser" /> class.
        /// </summary>
        public HeaderParser()
        {
            _parserMethod = Firstline_Before;
        }


        /// <summary>
        ///     The header part of the request/response has been parsed successfully. The remaining bytes is for the body
        /// </summary>
        public Action Completed
        {
            get { return _completed; }
            set
            {
                if (value == null)
                    _completed = () => { };
                else
                    _completed = value;
            }
        }

        /// <summary>
        ///     We've parsed a header and it's value.
        /// </summary>
        public FrameHeaderHandler HeaderParsed
        {
            get { return _headerParsed; }
            set
            {
                if (value == null)
                    _headerParsed = (name, s) => { };
                else
                    _headerParsed = value;
            }
        }

        /// <summary>
        ///     The frame method has been parsed.
        /// </summary>
        public FrameNameHandler FrameNameParsed
        {
            get { return _frameNameParsed; }
            set
            {
                if (value == null)
                    _frameNameParsed = name => { };
                else
                    _frameNameParsed = value;
            }
        }

        /// <summary>
        ///     Will try to parse everything in the buffer
        /// </summary>
        /// <param name="buffer">Buffer to read from.</param>
        /// <param name="offset">offset where the parser ended.</param>
        /// <remarks>
        ///     <para>
        ///         Do note that the parser is for the header only. The <see cref="Completed" /> event will
        ///         indicate that there might be body bytes left in the buffer. You have to handle them by yourself.
        ///     </para>
        /// </remarks>
        public int Parse(ISocketBuffer buffer, int offset)
        {
            int theByte;
            while ((theByte = Read(buffer, ref offset)) != -1)
            {
                var ch = (char) theByte;
                _parserMethod(ch);
                if (_isCompleted)
                    break;
            }

            _isCompleted = false;
            return offset;
        }

        /// <summary>
        ///     Reset parser state
        /// </summary>
        public void Reset()
        {
            ResetLineParsing();
            _parserMethod = Firstline_Before;
        }

        /// <summary>
        ///     Completed parsing a header line, reset so we can parse another one.
        /// </summary>
        protected void ResetLineParsing()
        {
            _headerName.Clear();
            _headerValue.Clear();
        }

        private void FirstLine(char ch)
        {
            if (ch == '\r')
                return;
            if (ch == '\n')
            {
                _frameNameParsed(_headerName.ToString());
                _headerName.Clear();
                _parserMethod = Name_StripWhiteSpacesBefore;
                return;
            }
            _headerName.Append(ch);
        }

        private void Firstline_Before(char ch)
        {
            if (ch == '\r' || ch == '\n' || ch == 0)
                return;

            _lookAhead = ch;
            _parserMethod = FirstLine;
        }

        private bool IsHorizontalWhitespace(char ch)
        {
            return ch == ' ' || ch == '\t';
        }

        private void Name_ParseUntilColonOrWhiteSpaces(char ch)
        {
            if (ch == ':')
            {
                _parserMethod = Value_StripWhitespacesBefore;
                return;
            }
            if (IsHorizontalWhitespace(ch))
                return;

            _headerName.Append(ch);
        }

        private void Name_StripWhiteSpacesBefore(char ch)
        {
            if (IsHorizontalWhitespace(ch))
                return;

            _parserMethod = Name_ParseUntilColonOrWhiteSpaces;
            _lookAhead = ch;
        }

        private int Read(ISocketBuffer buffer, ref int offset)
        {
            if (_lookAhead != -1)
            {
                var tmp = _lookAhead;
                _lookAhead = -1;
                return tmp;
            }

            if (buffer.BaseOffset + offset >= buffer.BytesTransferred)
                return -1;

            return buffer.Buffer[offset++];
        }

        private void TriggerHeaderCompleted()
        {
            _isCompleted = true;
            Completed();
            Reset();
        }

        private void Value_CompletedOrMultiLine(char ch)
        {
            if (IsHorizontalWhitespace(ch))
            {
                _parserMethod = Value_StripWhitespacesBefore;
                return;
            }
            if (ch == '\r')
                return; //empty line


            _headerParsed(_headerName.ToString(), _headerValue.ToString());
            ResetLineParsing();
            _parserMethod = Name_StripWhiteSpacesBefore;

            if (ch == '\n')
            {
                //Header completed
                TriggerHeaderCompleted();
                return;
            }


            _lookAhead = ch;
        }

        private void Value_ParseQuoted(char ch)
        {
            if (ch == '"')
            {
                // exited the quouted string
                _parserMethod = Value_ParseUntilQouteOrNewLine;
                return;
            }

            _headerValue.Append(ch);
        }

        private void Value_ParseUntilQouteOrNewLine(char ch)
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

        private void Value_StripWhitespacesBefore(char ch)
        {
            if (IsHorizontalWhitespace(ch) || ch == ':')
                return;

            _parserMethod = Value_ParseUntilQouteOrNewLine;
            _lookAhead = ch;
        }
    }
}