using System;
using System.IO;
using System.Security.AccessControl;

namespace Griffin.IO
{
    /// <summary>
    /// Missing .NET features for Directory management.
    /// </summary>
    public class DirectoryUtils
    {
        /// <summary>
        /// Copies files from one folder to another. Source security settings are included.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="copySubDirs">if set to <c>true</c>, sub directories are also copied.</param>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// destination
        /// </exception>
        public static void Copy(string source, string destination, bool copySubDirs)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
            var security = new DirectoryInfo(source).GetAccessControl();
            Copy(source, destination, copySubDirs, security);
        }

        /// <summary>
        /// Copies files from one directory to another.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="copySubDirs">if set to <c>true</c>, copy all contents from the sub directories too.</param>
        /// <param name="security">The security.</param>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// destination
        /// or
        /// security
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">Source directory does not exist or could not be found: 
        ///                     + source</exception>
        public static void Copy(string source, string destination, bool copySubDirs, DirectorySecurity security)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
            if (security == null) throw new ArgumentNullException("security");
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