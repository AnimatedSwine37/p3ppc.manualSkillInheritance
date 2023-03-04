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
using static p3ppc.manualSkillInheritance.Colours;
using static p3ppc.manualSkillInheritance.PersonaMenu;
using static p3ppc.manualSkillInheritance.Skills;

namespace p3ppc.manualSkillInheritance
{
    internal unsafe class UI
    {
        internal RenderSkillHelpDelegate RenderSkillHelp;
        internal RenderSkillDelegate RenderSkill;
        private PlaySoundEffectDelegate _playSoundEffect;
        internal RenderSprTextureDelegate RenderSprTexture;
        internal LoadCampFileDelegate LoadCampFile;
        internal QueueCombineMessageDelegate QueueCombineMessage;
        internal GetMessageStateDelegate GetMessageState;
        internal QueuedMessageWaitingDelegate QueuedMessageWaiting;
        internal AfterQueuedMessageDelegate AfterQueuedMessage;

        private IReverseWrapper<GetSkillColourDelegate> _getSkillColourReverseWrapper;
        private IAsmHook _skillColourHook;
        private IAsmHook _setupSkillTextColourHook;
        private IAsmHook _skillTextColourHook;

        internal bool* IsFemc;

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

            startupScanner.AddMainModuleScan("48 89 5C 24 ?? 57 48 81 EC 80 00 00 00 0F 29 74 24 ?? 0F 29 7C 24 ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 48 8B D9 0F 28 F3 48 8D 0D ?? ?? ?? ?? 0F 28 FA 8B FA E8 ?? ?? ?? ?? 48 85 DB 75 ?? 8B 15 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? 83 C2 02 E8 ?? ?? ?? ?? 0F B6 94 24 ?? ?? ?? ??", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find RenderSprTexture, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found RenderSprTexture at 0x{result.Offset + Utils.BaseAddress:X}");

                RenderSprTexture = hooks.CreateWrapper<RenderSprTextureDelegate>(Utils.BaseAddress + result.Offset, out _);
            });

            startupScanner.AddMainModuleScan("40 53 48 83 EC 20 48 8B D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 4C 8B 0D ?? ?? ?? ?? 4D 85 C9 74 ?? 49 8D 41 ??", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find LoadCampFileDelegate, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found LoadCampFileDelegate at 0x{result.Offset + Utils.BaseAddress:X}");

                LoadCampFile = hooks.CreateWrapper<LoadCampFileDelegate>(Utils.BaseAddress + result.Offset, out _);
            });

            startupScanner.AddMainModuleScan("48 8D 35 ?? ?? ?? ?? 0F 28 05 ?? ?? ?? ??", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find IsFemc, stuff won't be coloured right :(");
                    return;
                }

                IsFemc = (bool*)Utils.GetGlobalAddress(Utils.BaseAddress + result.Offset - 4);
                Colours.Initialise(IsFemc);
                Utils.LogDebug($"Found IsFemc at 0x{(nuint)IsFemc:X}");
            });

            // Hook to change the colour of skill boxes
            startupScanner.AddMainModuleScan("0F B6 B4 24 ?? ?? ?? ?? B8 00 10 00 00", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find PersonaSkillColour, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found PersonaSkillColour at 0x{result.Offset + Utils.BaseAddress:X}");

                string[] function =
                {
                    "use64",
                    "test rax, 0xFFFFFFFFFF000000", // If the alpha is 0 it's flashing green so don't touch it (I don't understand how it's meant to work)
                    "je endHook",
                    "push rcx\npush rdx\npush r8\npush r9\npush r10\npush r11",
                    "mov rcx, r14", // Move display info in param1
                    "mov rdx, rax", // Move the current colour into param2
                    "sub rsp, 32",
                    $"{hooks.Utilities.GetAbsoluteCallMnemonics(GetSkillColour, out _getSkillColourReverseWrapper)}",
                    "add rsp, 32",
                    "mov dword [rsp + 416], eax",
                    "pop r11\npop r10\npop r9\npop r8\npop rdx\npop rcx",
                    "label endHook"
                };
                _skillColourHook = hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
            });

            // Hook to setup changing the colour of skill text
            startupScanner.AddMainModuleScan("66 41 3B 46 ?? 4C 8B B4 24 ?? ?? ?? ??", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find SetupSkillTextColour, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found SetupSkillTextColour at 0x{result.Offset + Utils.BaseAddress:X}");

                string[] function =
                {
                    "use64",
                    "cmp word [r14 + 556], -1", // Check if displayInfo->SkillsInfo.NumNextSkills == -1, if so we need to customise the text colour
                    "jne endHook",
                    "push rdx",
                    "push r9",
                    "lea r9,[rsp+0x88]",
                    "mov edx, dword [r14 + 116]",
                    "mov dword [r9 + 4], edx", // Put the text colour in [textInfo + 4]
                    "mov word [r9 + 2], 69", // Set the text type to 69 (will indicate later that the custom colour should be used)
                    "pop r9",
                    "pop rdx",
                    "label endHook",
                };
                _setupSkillTextColourHook = hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
            });

            // Hook to change the colour of skill text
            startupScanner.AddMainModuleScan("8B C3 BA 00 5F 3C 00", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find SkillTextColour, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found SkillTextColour at 0x{result.Offset + Utils.BaseAddress:X}");

                string[] function =
                {
                    "use64",
                    "cmp cx, 69",
                    "jne endHook", // If it's not -1 we shouldn't change the colour
                    "mov dword ebx, [r9 + 4]", // Put the custom colour in ebx
                    $"{hooks.Utilities.GetAbsoluteJumpMnemonics(Utils.BaseAddress + result.Offset + 22, true)}", // Jump over the stuff that normally would set the colour
                    "label endHook"
                };
                _skillTextColourHook = hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
            });

            startupScanner.AddMainModuleScan("48 89 5C 24 ?? 57 48 83 EC 20 48 0F BE D9 89 D7 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? 48 8B 04 ?? 48 8D 1C ?? 8B 08", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find QueueCombineMessage, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found QueueCombineMessage at 0x{result.Offset + Utils.BaseAddress:X}");

                QueueCombineMessage = hooks.CreateWrapper<QueueCombineMessageDelegate>(Utils.BaseAddress + result.Offset, out _);
            });

            startupScanner.AddMainModuleScan("40 53 48 83 EC 20 48 0F BE D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? 48 8B 0C ?? 48 85 C9 74 ?? 83 39 00", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find GetMessageState, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found GetMessageState at 0x{result.Offset + Utils.BaseAddress:X}");

                GetMessageState = hooks.CreateWrapper<GetMessageStateDelegate>(Utils.BaseAddress + result.Offset, out _);
            });

            startupScanner.AddMainModuleScan("48 89 5C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 20 48 0F BE D9", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find QueuedMessageWaiting, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found QueuedMessageWaiting at 0x{result.Offset + Utils.BaseAddress:X}");

                QueuedMessageWaiting = hooks.CreateWrapper<QueuedMessageWaitingDelegate>(Utils.BaseAddress + result.Offset, out _);
            });
            
            startupScanner.AddMainModuleScan("48 89 5C 24 ?? 57 48 83 EC 20 48 0F BE D9 89 D7 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? 48 8B 04 ?? 48 8D 1C ?? 48 85 C0", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find AfterQueuedMessage, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found AfterQueuedMessage at 0x{result.Offset + Utils.BaseAddress:X}");

                AfterQueuedMessage = hooks.CreateWrapper<AfterQueuedMessageDelegate>(Utils.BaseAddress + result.Offset, out _);
            });

        }

        private Colour GetSkillColour(PersonaDisplayInfo* displayInfo, Colour currentColour)
        {
            // Only change the colour if we actually should
            if (displayInfo->SkillsInfo.NumNextSkills != -1)
                return currentColour;
            var newColour = displayInfo->SkillsInfo.NextSkills.BgColour;
            //Utils.LogDebug($"Changing colour for {(Skill)displayInfo->SkillsInfo.Skills.Id} to {newColour.R}, {newColour.G}, {newColour.B}, {newColour.A}");
            return newColour;
        }

        internal void PlaySoundEffect(SoundEffect sound)
        {
            if (_soundEffectParams.TryGetValue(sound, out var soundParams))
            {
                _playSoundEffect(soundParams[0], soundParams[1], soundParams[2], soundParams[3]);
            }
        }

        internal enum SoundEffect
        {
            Confirm,
            Back,
            Error,
            SelectionMoved,
            SelectionJumped
        }

        private readonly Dictionary<SoundEffect, short[]> _soundEffectParams = new Dictionary<SoundEffect, short[]>
        {
            { SoundEffect.Confirm, new short[] { 0, 0, 0, 1} },
            { SoundEffect.Back, new short[] { 0, 0, 0, 2} },
            { SoundEffect.Error, new short[] { 0, 0, 0, 8} },
            { SoundEffect.SelectionMoved, new short[] { 0, 1, 0, 0 } },
            { SoundEffect.SelectionJumped, new short[] { 0, 1, 0, 5} }

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

        /// <summary>
        /// Some function that seems to always be called after a queued message is done
        /// Not sure if it's really needed
        /// </summary>
        /// <param name="contextId">The context id of the message when you queued it</param>
        /// <param name="param_2">Something, just use 1, idk</param>
        [Function(CallingConventions.Microsoft)]
        internal delegate bool AfterQueuedMessageDelegate(byte contextId, int param_2);

        /// <summary>
        /// Some function that needs to be run when waiting for a queued message
        /// </summary>
        /// <param name="contextId">The context id of the message when you queued it</param>
        /// <param name="param_2">Something, just use 1, idk</param>
        [Function(CallingConventions.Microsoft)]
        internal delegate void QueuedMessageWaitingDelegate(byte contextId, int param_2);

        /// <summary>
        /// Gets the message you are up to in a series of messages or 0 if you're done
        /// </summary>
        /// <param name="contextId">The context id of the message when you queued it</param>
        /// <returns>The 1 based index of the message you're up to or 0 if you're done with the messages</returns>
        [Function(CallingConventions.Microsoft)]
        internal delegate int GetMessageStateDelegate(byte contextId);

        [Function(CallingConventions.Microsoft)]
        internal delegate uint QueueCombineMessageDelegate(byte contextId, int messageId);

        [Function(CallingConventions.Microsoft)]
        private delegate Colour GetSkillColourDelegate(PersonaDisplayInfo* displayInfo, Colour currentColour);

        [Function(CallingConventions.Microsoft)]
        internal delegate nuint LoadCampFileDelegate(string fileName);

        [Function(CallingConventions.Microsoft)]
        internal delegate void RenderSprTextureDelegate(nuint spr, int spriteIndex, float x, float y, int param_5, byte r, byte g, byte b, byte a, short param_10, short param_11, short param_12, short param_13, short param_14);

        [Function(CallingConventions.Microsoft)]
        internal delegate void PlaySoundEffectDelegate(short param_1, short param_2, short param_3, short param_4);

        [Function(CallingConventions.Microsoft)]
        internal delegate void RenderSkillHelpDelegate(Position positioning, int param_2, byte alpha, Skill skillId);

        [Function(CallingConventions.Microsoft)]
        internal delegate void RenderSkillDelegate(Position positioning, int param_2, byte alpha, PersonaDisplayInfo* persona, uint skillSlot, float param_6);
    }
}
