using ARMeilleure.Memory;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ARMeilleure.Translation
{
    class MHRiseHooks
    {
        private readonly IMemoryManager _memory;

        public MHRiseHooks(IMemoryManager memory)
        {
            _memory = memory;
        }

        public void CalculateHashHook([ReturnValue] ulong hash, ulong param1, ulong param2, ulong fileNamePtr)
        {
            string fileName = string.Empty;

            if (fileNamePtr != 0UL)
            {
                ulong offset = 0;
                while (true)
                {
                    ushort value = _memory.Read<ushort>(fileNamePtr + offset);
                    if (value == 0)
                    {
                        break;
                    }

                    fileName += (char)value;
                    offset += 2;
                }
            }

            Logger.Info?.Print(LogClass.Cpu, $"Calculate hash called, FileName = {fileName}, Hash = 0x{hash:X16}");
        }

        public void HookViaMurmurHashCalc32([ReturnValue] ulong output, ulong param1, int param2, int param3, uint ptr)
        {
            Logger.Info?.Print(LogClass.Cpu, $"0x{output:X16}, 0x{param1:X16}, 0x{param2:X16}, 0x{param3:X16}, 0x{ptr:X16}");
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
            //if (!fileList.Any(newFileName.Contains))
            //Logger.Info?.Print(LogClass.Cpu, $"New file found: {fileName}, 0x{output:X16}");
            fileList.Add(newFileName);
        }

        public static List<string> fileList = new List<string>();
        public static string fileListPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mhrise", "mhrise.list");

        public static void LoadFileList()
        {
            DirectoryInfo logDir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mhrise"));
            logDir.Create();

            if (!File.Exists(fileListPath)) File.Create(fileListPath);

            fileList = new List<string>(File.ReadAllLines(fileListPath));
            Logger.Info?.Print(LogClass.Cpu, $"Loaded {fileListPath} with {fileList.Count} entries.");
        }

        public static void SaveFileList()
        {
            DirectoryInfo logDir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mhrise"));
            logDir.Create();

            fileList = fileList.Distinct().ToList();

            File.WriteAllLines(fileListPath, fileList);
            Logger.Info?.Print(LogClass.Cpu, $"Saved {fileListPath} with {fileList.Count} entries.");
        }
    }
}
