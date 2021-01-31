
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ARMeilleure.State;
using ARMeilleure.Translation;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Kernel.Process;
using System.IO;

namespace Ryujinx.HLE
{

    public class HashLoggerSingleton
    {
        private static volatile HashLoggerSingleton instance;
        private static object syncRoot = new object();

        private StreamWriter file;
        private static object fileLock = new object();

        private HashLoggerSingleton()
        {
            file = new StreamWriter("HashLog.txt");
        }

        public static HashLoggerSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new HashLoggerSingleton();
                    }
                }

                return instance;
            }
        }

        public void LogHash(string str, ulong hash)
        {
            lock (fileLock)
            {
                file.WriteLine("{0}\t{1:X}", str, hash);
            }
        }
    }


    public class MHRiseHook
    {
        private ArmProcessContext _context;
        //private GuestFunction _hook;
        private TranslatedFunction _transfunc_via_str_makehash;
        private TranslatedFunction _transfunc_via_murmur_hash_calc32;
        private TranslatedFunction _transfunc_file_related_inlined_murmur_hash;

        public MHRiseHook()
        {

            //_hook = new GuestFunction(HookedFunction);
        }

        internal void Initialize(ArmProcessContext context)
        {
            if (context == null)
                return;

            _context = context;

            /*
            _transfunc_via_str_makehash = new TranslatedFunction(HookViaStrMakeHash, 0x480, false);
            _context._cpuContext._translator.AddFunc(0xCBD0620, _transfunc_via_str_makehash);

            _transfunc_via_murmur_hash_calc32 = new TranslatedFunction(HookViaMurmurHashCalc32, 0x480, false);
            _context._cpuContext._translator.AddFunc(0xCBA9F70, _transfunc_via_murmur_hash_calc32);
            */


            _transfunc_file_related_inlined_murmur_hash = new TranslatedFunction(FileRelatedMurmurHash, 0x610, false);
            _context._cpuContext._translator.AddFunc(0xCBDBFB0, _transfunc_file_related_inlined_murmur_hash);
        }


        private int Utf16Strlen(Memory.IVirtualMemoryManager addressSpace, ulong x0)
        {
            int i = 0;
            for (; i <= 8192 /*Sanity-check limit*/; i++)
            {
                var data = addressSpace.GetSpan(x0 + (ulong)(i * 2), 2);
                if (data[0] == 0 && data[1] == 0)
                {
                    return i;
                }
            }
            return i;
        }

        // via_str::makeHash internal @ 0x4BCC620
        public ulong HookViaStrMakeHash(IntPtr nativeContextPtr)
        {
            //Console.WriteLine("hooked via_str::makeHash!");

            unsafe
            {
                NativeContext.NativeCtxStorage* ctx = (NativeContext.NativeCtxStorage*)nativeContextPtr;
                var callAddr = ctx->CallAddress;

                var x0 = ctx->X[0];

                //var strPtr = _context.AddressSpace.Read<ulong>(x0);
                var len = Utf16Strlen(_context.AddressSpace, x0);
                var strDataStart = _context.AddressSpace.GetSpan(x0, len * 2).ToArray();
                string input = Encoding.Unicode.GetString(strDataStart);

                //var ok = _context._cpuContext._translator._hookOrgFuncs.TryGetValue(0xCBD0620, out TranslatedFunction orgFunc);
                TranslatedFunction orgFunc = _context._cpuContext._translator.GetOrTranslateHookedOriginalFunction(0xCBD0620);
                var ryuRetVal = orgFunc._func(nativeContextPtr);

                var hash = ctx->X[0];

                Console.WriteLine("via_str::makeHash: \"{0}\" -> {1:X}", input, hash);
                HashLoggerSingleton.Instance.LogHash(input, hash);

                return ryuRetVal;
            }
        }

        // via.murmur_hash::calc32 internal @ 0x4BA5F70
        public ulong HookViaMurmurHashCalc32(IntPtr nativeContextPtr)
        {
            //Console.WriteLine("hooked via.murmur_hash::Calc32!");

            unsafe
            {
                NativeContext.NativeCtxStorage* ctx = (NativeContext.NativeCtxStorage*)nativeContextPtr;
                var callAddr = ctx->CallAddress;

                var x0 = ctx->X[0];
                var x1 = ctx->X[1];

                //var strPtr = _context.AddressSpace.Read<ulong>(x0);
                var strDataStart = _context.AddressSpace.GetSpan(x0, (int)x1).ToArray();
                string input = Encoding.UTF8.GetString(strDataStart);

                //var ok = _context._cpuContext._translator._hookOrgFuncs.TryGetValue(0xCBA9F70, out TranslatedFunction orgFunc);
                TranslatedFunction orgFunc = _context._cpuContext._translator.GetOrTranslateHookedOriginalFunction(0xCBA9F70);
                var ryuRetVal = orgFunc._func(nativeContextPtr);

                var hash = ctx->X[0];

                Console.WriteLine("via.murmur_hash::Calc32: \"{0}\" -> {1:X}", input, hash);
                HashLoggerSingleton.Instance.LogHash(input, hash);


                return ryuRetVal;
            }
        }


        public ulong FileRelatedMurmurHash(IntPtr nativeContextPtr)
        {
            //Console.WriteLine("hooked FileRelatedMurmurHash!");

            unsafe
            {
                NativeContext.NativeCtxStorage* ctx = (NativeContext.NativeCtxStorage*)nativeContextPtr;
                var callAddr = ctx->CallAddress;

                var x0 = ctx->X[0];

                //var strPtr = _context.AddressSpace.Read<ulong>(x0);
                var len = Utf16Strlen(_context.AddressSpace, x0);
                var strDataStart = _context.AddressSpace.GetSpan(x0, len * 2).ToArray();
                string input = Encoding.Unicode.GetString(strDataStart);

                //var ok = _context._cpuContext._translator._hookOrgFuncs.TryGetValue(0xCBD0620, out TranslatedFunction orgFunc);
                TranslatedFunction orgFunc = _context._cpuContext._translator.GetOrTranslateHookedOriginalFunction(0xCBDBFB0);
                var ryuRetVal = orgFunc._func(nativeContextPtr);

                var hash = ctx->X[0];

                Console.WriteLine("FileRelatedMurmurHash: \"{0}\" -> {1:X}", input, hash);
                HashLoggerSingleton.Instance.LogHash(input, hash);

                return ryuRetVal;
            }
        }


    }
}