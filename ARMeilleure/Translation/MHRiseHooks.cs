using ARMeilleure.Memory;
using Dropbox.Api;
using Dropbox.Api.Files;
using Ryujinx.Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARMeilleure.Translation
{
    class MHRiseHooks
    {
        // demo v1.0.0: 0x4BD7FB0, demo v1.0.2: 0x4C113E0
        public static ulong MHRiseFileHookAddress = 0x8004000 + 0x4C113E0;

        private readonly IMemoryManager _memory;

        public MHRiseHooks(IMemoryManager memory)
        {
            _memory = memory;
        }

        public void FileRelatedMurmurHash([ReturnValue] ulong output, ulong ptr)
        {
            string fileName = string.Empty;

            if (ptr != 0UL)
            {
                ulong offset = 0;
                while (true)
                {
                    ushort value = _memory.Read<ushort>(ptr + offset);
                    if (value == 0)
                    {
                        break;
                    }

                    fileName += (char)value;
                    offset += 2;
                }
            }

            string newFileName = fileName.Replace("rom:/", "").Replace("/", "\\");
            //Logger.Info?.Print(LogClass.Cpu, $"{fileName}, 0x{output:X16}");

            if (!fileList.Contains(newFileName))
            {
                fileList.Add(newFileName);
                fileListAddons.Add(newFileName);
            }
        }

        public static List<string> fileList = new List<string>();
        public static List<string> fileListAddons = new List<string>();

        public static string fileListPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mhrise", "mhrise.list");
        public static string fileListNameAddons = $"mhrise_new_{DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss")}.txt";
        public static string fileListPathAddons = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mhrise", fileListNameAddons);
        public static DirectoryInfo logDir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mhrise"));

        public static DropboxClient dbx = new DropboxClient(Keys.Keys.DropboxAppKey);

        public static void LoadFileList()
        {
            if (!logDir.Exists) logDir.Create();

            if (File.Exists(fileListPath))
            {
                fileList = new List<string>(File.ReadAllLines(fileListPath));
                Logger.Info?.Print(LogClass.Cpu, $"Loaded {fileListPath} with {fileList.Count} entries.");
            }
        }

        public static void SaveFileList()
        {
            if (!logDir.Exists) logDir.Create();

            fileList = fileList.Distinct().ToList();
            File.WriteAllLines(fileListPath, fileList);
            Logger.Info?.Print(LogClass.Cpu, $"Saved {fileListPath} with {fileList.Count} entries.");

            if (fileListAddons.Count > 0)
            {
                fileListAddons = fileListAddons.Distinct().ToList();
                File.WriteAllLines(fileListPathAddons, fileListAddons);
                dbx.Files.UploadAsync($"/logs/{fileListNameAddons}", WriteMode.Overwrite.Instance, body: new MemoryStream(File.ReadAllBytes(fileListPathAddons)));
                Logger.Info?.Print(LogClass.Cpu, $"Saved {fileListPathAddons} with {fileListAddons.Count} entries.");
            }
        }
    }
}
