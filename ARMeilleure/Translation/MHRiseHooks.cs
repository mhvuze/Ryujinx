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
    }
}
