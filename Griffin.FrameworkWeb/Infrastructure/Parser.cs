using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using MarkdownDeep;
using WebGrease.Css.Visitor;

namespace GriffinFrameworkWeb.Infrastructure
{
    public class Parser
    {
        private string _rootDirectory;
        private readonly string _rootUri;
        private string _currentFilePath;
        private string _currentUrlPath;

        public Parser(string rootDirectory, string rootUri)
        {
            _rootDirectory = rootDirectory;
            _rootUri = rootUri.Trim('/');
        }

        public string ParseString(string str)
        {
            var markdown = new Markdown();
            markdown.PrepareLink = OnHtmlLink;
            markdown.PrepareImage = OnImageLink;
            markdown.FormatCodeBlock = OnCodeBlock;
            return markdown.Transform(str);
        }

        private bool OnImageLink(HtmlTag arg1, bool arg2)
        {
            if (!arg1.attributes["src"].Contains("/"))
            {
                var url = string.Format("~/home/image/?src={0}/{1}", _currentUrlPath, arg1.attributes["src"]);
                arg1.attributes["src"] = VirtualPathUtility.ToAbsolute(url);
            }


            return true;
        }

        public string ParseUrl(string url)
        {
            url = url == null
                ? ""
                : url.Trim('/');


            if (url.StartsWith(_rootUri))
                url = url.Remove(0, _rootUri.Length).Trim('/');

            var path = GetFullPath(url);

            //direct link
            if (!path.EndsWith("index.md"))
            {
                var pos = url.LastIndexOf('/');
                _currentUrlPath = url.Substring(0, pos);
            }
            else
                _currentUrlPath = url;

            _currentFilePath = Path.GetDirectoryName(path);

            var text = File.ReadAllText(path);
            text = PreParseGithubCodeBlocks2(text);
            text = PreParseGithubTables(text);
            return ParseString(text);
        }

        private string PreParseGithubTables(string text)
        {
            var reader = new StringReader(text);
            var line = "";
            var sb = new StringBuilder();
            while ((line = reader.ReadLine()) != null)
            {
                if (!line.Contains(" | "))
                {
                    sb.AppendLine(line);
                    continue;
                }

                var firstLine = line;
                line = reader.ReadLine();
                if (line == null)
                {
                    sb.AppendLine(firstLine);
                    continue;
                }

                if (!line.All(x => x == ' ' || x == '|' || x == '-'))
                {
                    sb.AppendLine(line);
                    continue;
                }

                //got a table.
                sb.AppendLine(@"<table class=""table table-striped table-bordered""><thead><tr><th>");
                sb.AppendLine(firstLine.Replace(" | ", "</th><th>"));
                sb.AppendLine("</tr></thead><tbody>");
                while (true)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        sb.AppendLine("</tbody></table>");
                        sb.AppendLine("<br>\r\n");
                        break;
                    }

                    sb.Append("<tr><td>");
                    sb.Append(line.Replace(" | ", "</td><td>"));
                    sb.AppendLine("</td></tr>");
                }

            }

            return sb.ToString();

        }

        private string PreParseGithubCodeBlocks2(string text)
        {
            StringBuilder sb = new StringBuilder();
            var lastPos = 0;
            while (true)
            {
                
                var pos = text.IndexOf("\r\n```", lastPos);
                if (pos == -1)
                    break;

                sb.Append(text.Substring(lastPos, pos - lastPos));
                sb.AppendLine();
                pos += 5;
                var nlPos = text.IndexOfAny(new[] { '\r', '\n' }, pos);
                var codeLang = text.Substring(pos, nlPos - pos);


                lastPos = text.IndexOf("\r\n```\r\n", pos + 1);
                if (lastPos == -1)
                    lastPos = text.Length - 1;

                sb.AppendFormat(@"<pre><code data-lang=""{0}"" class=""language-{0}"">", codeLang);
                var code = text.Substring(nlPos + 2, lastPos - nlPos - 2);
                sb.Append(code.Replace(">", "&gt;").Replace("<", "&lt;"));
                sb.Append("</pre></code>\r\n");
                lastPos += 5;
                if (lastPos > text.Length)
                    break;
            }

            if (lastPos < text.Length - 1)
                sb.AppendLine(text.Substring(lastPos));

            return sb.ToString();
        }
        private string PreParseGithubCodeBlocks(string text)
        {
            var regex = new Regex("(\r\n|\r|\n)(```)(.+)(\r\n)", RegexOptions.Multiline);
            return regex.Replace(text, GithubCodeReplacer).Replace("```", "</code></pre>");
        }

        private string GithubCodeReplacer(Match match)
        {
            var code = match.Groups[3].Value;
            code = code.Replace("<", "&lt;").Replace(">", "&gt;");
            return string.Format(@"<pre><code data-lang=""{0}"" class=""language-{0}"">", code);
        }

        private string GetFullPath(string url)
        {
            if (url.StartsWith("doc/"))
                url = url.Remove(0, 4);
            if (url.StartsWith("/doc/"))
                url = url.Remove(0, 5);

            var fullPath = Path.Combine(_rootDirectory, url.Replace('/', '\\'));
            if (Directory.Exists(fullPath))
                fullPath = Path.Combine(fullPath, "index.md");
            else if (!fullPath.EndsWith(".md"))
                fullPath += ".md";

            return fullPath;
        }

        private string OnCodeBlock(Markdown arg1, string arg2)
        {
            return @"<pre class=""language-text""><code data-lang=""text"" class=""language-text"">" + arg2 + "</code></pre>";
        }

        private bool OnHtmlLink(HtmlTag arg)
        {
            var src = arg.attributes["href"];
            if (src.StartsWith("http"))
                return false;

            if (src.StartsWith("~"))
            {
                src = VirtualPathUtility.ToAbsolute(src);
                arg.attributes["href"] = src;
                return true;
            }

            if (!src.StartsWith("/"))
            {
                var fixedSrc = VirtualPathUtility.ToAbsolute("~/" + _rootUri + "/" + _currentUrlPath + "/" + src);
                arg.attributes["href"] = fixedSrc;
            }

            var path = GetLinkPath(src);
            if (!File.Exists(path))
                arg.attributes["style"] = "color: red";

            return true;
        }

        private string GetLinkPath(string url)
        {
            if (url.StartsWith(_currentUrlPath))
                url = url.Remove(0, _currentUrlPath.Length).TrimStart('/');
            if (url.StartsWith('/' + _currentUrlPath))
                url = url.Remove(0, _currentUrlPath.Length + 1).TrimStart('/');
            if (url.StartsWith("doc/"))
                Debugger.Break();

            var basePath = _currentFilePath;
            if (url.StartsWith("/doc/"))
            {
                url = url.Remove(0, 5);
                basePath = _rootDirectory;
            }

            var fullPath = Path.Combine(basePath, url.Replace('/', '\\'));

            if (Directory.Exists(fullPath))
                fullPath = Path.Combine(fullPath, "index.md");
            else if (!fullPath.EndsWith(".md"))
                fullPath += ".md";

            return fullPath;
        }
    }
}