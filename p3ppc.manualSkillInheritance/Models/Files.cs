using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace p3ppc.manualSkillInheritance.Models
{
    internal unsafe class FileUtils
    {
        private LoadFileDelegate _loadFile;
        
        internal FileUtils(IReloadedHooks hooks, IStartupScanner startupScanner)
        {
            startupScanner.AddMainModuleScan("E8 ?? ?? ?? ?? 48 89 43 ?? B8 01 00 00 00 66 89 43 ??", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find LoadSprFile, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found LoadSprFile call at 0x{result.Offset + Utils.BaseAddress:X}");
                var address = Utils.GetGlobalAddress(result.Offset + Utils.BaseAddress + 1);
                Utils.LogDebug($"Found LoadSprFile at 0x{address:X}");
                
                _loadFile = hooks.CreateWrapper<LoadFileDelegate>((long)address, out _);
            });
        }

        internal GameFile* LoadFile(string path)
        {
            if (_loadFile == null)
            {
                return (GameFile*)0;
            }
            GameFile* file = _loadFile(path, 0);
            Utils.LogDebug($"{path} is at 0x{(nuint)file:X}");
            return file;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct GameFile
        {
            [FieldOffset(0)]
            public FileLoadStatus LoadStatus;
        }

        internal enum FileLoadStatus : byte
        {
            Done = 5
        }

        [Function(CallingConventions.Microsoft)]
        private delegate GameFile* LoadFileDelegate(string path, int index);

    }
}
