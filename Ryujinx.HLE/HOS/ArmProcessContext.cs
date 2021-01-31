using ARMeilleure.State;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.Memory;
using System;

namespace Ryujinx.HLE.HOS
{
    public class ArmProcessContext : IProcessContext
    {
        private readonly MemoryManager _memoryManager;
        public CpuContext _cpuContext;

        public IVirtualMemoryManager AddressSpace => _memoryManager;

        public ArmProcessContext(MemoryManager memoryManager)
        {
            _memoryManager = memoryManager;
            _cpuContext = new CpuContext(memoryManager);
        }

        public void Execute(ExecutionContext context, ulong codeAddress) => _cpuContext.Execute(context, codeAddress);
        public void Dispose() => _memoryManager.Dispose();
    }
}
