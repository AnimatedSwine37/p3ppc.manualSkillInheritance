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
            startupScanner.AddMainModuleScan("40 53 48 83 EC 20 48 89 CB 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 8B 15 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? 83 C2 02 E8 ?? ?? ?? ?? BA 58 03 00 00", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find LoadFile, won't be able to replace any bustups :(");
                    return;
                }
                Utils.LogDebug($"Found LoadFile at 0x{result.Offset + Utils.BaseAddress:X}");

                _loadFile = hooks.CreateWrapper<LoadFileDelegate>(result.Offset + Utils.BaseAddress, out _);
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
