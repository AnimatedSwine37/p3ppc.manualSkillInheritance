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
        private PlaySoundEffectDelegate _playSoundEffect;

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

            startupScanner.AddMainModuleScan("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B7 F9 0F BF DA 48 8D 0D ?? ?? ?? ?? 41 0F B7 F1 41 0F B7 E8 E8 ?? ?? ?? ?? 89 DA", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find PlaySoundEffect, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found PlaySoundEffect at 0x{result.Offset + Utils.BaseAddress:X}");

                _playSoundEffect = hooks.CreateWrapper<PlaySoundEffectDelegate>(Utils.BaseAddress + result.Offset, out _);
            });

        }

        internal void PlaySoundEffect(SoundEffect sound)
        {
            if(_soundEffectParams.TryGetValue(sound, out var soundParams)) {
                _playSoundEffect(soundParams[0], soundParams[1], soundParams[2], soundParams[3]);
            }
        }

        internal enum SoundEffect
        {
            Confirm,
            Back,
            Error,
            SelectionChanged,
        }

        private readonly Dictionary<SoundEffect, short[]> _soundEffectParams = new Dictionary<SoundEffect, short[]>
        {
            { SoundEffect.Confirm, new short[] { 0, 0, 0, 1} },
            { SoundEffect.Back, new short[] { 0, 0, 0, 2} },
            { SoundEffect.Error, new short[] { 0, 0, 0, 8} },
            { SoundEffect.SelectionChanged, new short[] { 0, 1, 0, 0} }
        };

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
        internal delegate void PlaySoundEffectDelegate(short param_1, short param_2, short param_3, short param_4);

        [Function(CallingConventions.Microsoft)]
        internal delegate void RenderSkillHelpDelegate(Position positioning, int param_2, byte alpha, Skill skillId);

        [Function(CallingConventions.Microsoft)]
        internal delegate void RenderSkillDelegate(Position positioning, int param_2, byte alpha, PersonaDisplayInfo* persona, uint skillSlot, float param_6);
    }
}
