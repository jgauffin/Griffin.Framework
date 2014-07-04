using System.IO;
using System.Security.AccessControl;

namespace Griffin.IO
{
    public class DirectoryUtils
    {
        public static void Copy(string source, string destination, bool copySubDirs)
        {
            var security = Directory.GetAccessControl(source);
            Copy(source, destination, copySubDirs, security);
        }

        public static void Copy(string source, string destination, bool copySubDirs, DirectorySecurity security)
        {
            var dir = new DirectoryInfo(source);
            var dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + source);
            }

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination, security);
            }

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destination, file.Name);
                file.CopyTo(temppath, false);
            }

            if (!copySubDirs)
                return;

            foreach (var subdir in dirs)
            {
                var temppath = Path.Combine(destination, subdir.Name);
                Copy(subdir.FullName, temppath, true);
            }
        }
    }
}