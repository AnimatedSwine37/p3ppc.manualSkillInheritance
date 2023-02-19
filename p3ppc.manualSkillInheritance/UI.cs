using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static p3ppc.manualSkillInheritance.PersonaMenu;
using static p3ppc.manualSkillInheritance.Skills;

namespace p3ppc.manualSkillInheritance
{
    internal unsafe class UI
    {
        internal RenderSkillHelpDelegate RenderSkillHelp;
        internal RenderSkillDelegate RenderSkill;
        
        internal UI(IStartupScanner startupScanner, IReloadedHooks hooks)
        {
            startupScanner.AddMainModuleScan("48 89 E0 48 89 58 ?? 48 89 68 ?? 56 57 41 54", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find RenderSkillHelp, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found RenderSkillHelp at 0x{result.Offset + Utils.BaseAddress:X}");

                RenderSkillHelp = hooks.CreateWrapper<RenderSkillHelpDelegate>(Utils.BaseAddress + result.Offset, out _);
            });

            startupScanner.AddMainModuleScan("48 8B C4 48 81 EC 48 01 00 00", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find RenderSkill, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found RenderSkill at 0x{result.Offset + Utils.BaseAddress:X}");

                RenderSkill = hooks.CreateWrapper<RenderSkillDelegate>(Utils.BaseAddress + result.Offset, out _);
            });

        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Position
        {
            /// <summary>
            /// Starts from the left edge, increasing moves right
            /// </summary>
            internal float X;
            /// <summary>
            /// Goes from the top edge, increasing moves down
            /// </summary>
            internal float Y;
        }

        [Function(CallingConventions.Microsoft)]
        internal delegate void RenderSkillHelpDelegate(Position positioning, int param_2, byte alpha, Skill skillId);

        [Function(CallingConventions.Microsoft)]
        internal delegate void RenderSkillDelegate(Position positioning, int param_2, byte alpha, PersonaDisplayInfo* persona, uint skillSlot, float param_6);
    }
}
