using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NukeBinObjExtension
{
    /// <summary>
    /// Nuke Service.
    /// </summary>
    internal class NukeService
    {
        /// <summary>
        /// Nuke the bin and obj folders for the specified project path
        /// </summary>
        public async Task NukeAsync(string projectPath)
        {
            await Task.Delay(1);

            if (string.IsNullOrWhiteSpace(projectPath))
            {
                throw new InvalidOperationException("Project path is invalid.");
            }

            var directory = Path.GetDirectoryName(projectPath);

            if (directory == null)
            {
                throw new InvalidOperationException("Project directory is invalid.");
            }

            var binFolder = Path.Combine(directory, "bin");

            if (Directory.Exists(binFolder))
            {
                Directory.Delete(binFolder, true);
            }

            var objFolder = Path.Combine(directory, "obj");

            if (Directory.Exists(objFolder))
            {
                Directory.Delete(objFolder, true);
            }
        }
    }
}
