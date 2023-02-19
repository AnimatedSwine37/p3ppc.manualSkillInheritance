using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static p3ppc.manualSkillInheritance.Skills;

namespace p3ppc.manualSkillInheritance
{
    internal class UI
    {
        internal RenderSkillHelpDelegate RenderSkillHelp;
        
        internal UI(IStartupScanner startupScanner, IReloadedHooks hooks)
        {
            startupScanner.AddMainModuleScan("48 89 E0 48 89 58 ?? 48 89 68 ?? 56 57 41 54", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find RenderSkillHelp, aborting initialisation");
                    return;
                }
                Utils.LogDebug($"Found RenderSkillHelp at 0x{result.Offset + Utils.BaseAddress:X}");

                RenderSkillHelp = hooks.CreateWrapper<RenderSkillHelpDelegate>(Utils.BaseAddress + result.Offset, out _);
            });
        }

        // The floats go from xmm6-10
        [Function(CallingConventions.Microsoft)]
        internal delegate void RenderSkillHelpDelegate(long positioning, int param_2, byte alpha, Skill skillId);
    }
}
