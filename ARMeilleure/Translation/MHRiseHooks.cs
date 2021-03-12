using ARMeilleure.Memory;
using Ryujinx.Common.Logging;

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

            //Logger.Info?.Print(LogClass.Cpu, $"{fileName}, 0x{output:X16}");
            Logger.Info?.Print(LogClass.Cpu, $"0x{ptr:X16}, 0x{output:X16}");
        }

        public void Strings_ulong_ulong([ReturnValue] ulong output, ulong ptr)
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

            Logger.Info?.Print(LogClass.Cpu, $"{fileName}, 0x{output:X16}");
        }
    }
}
