﻿using p3ppc.manualSkillInheritance.Configuration;
using p3ppc.manualSkillInheritance.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Memory.Sources;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;
using static p3ppc.manualSkillInheritance.UI.UI.FusionMenu;
using static p3ppc.manualSkillInheritance.Models.Inputs;
using static p3ppc.manualSkillInheritance.UI.UI.PersonaMenu;
using static p3ppc.manualSkillInheritance.Models.Personas;
using static p3ppc.manualSkillInheritance.Models.Skills;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;
using p3ppc.manualSkillInheritance.Models;
using p3ppc.manualSkillInheritance.UI;
using static p3ppc.manualSkillInheritance.UI.UIUtils;
using static p3ppc.manualSkillInheritance.Models.FileUtils;
using static p3ppc.manualSkillInheritance.UI.Colours;
using static p3ppc.manualSkillInheritance.UI.Sprites;
using CriFs.V2.Hook.Interfaces;

namespace p3ppc.manualSkillInheritance
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public unsafe class Mod : ModBase // <= Do not Remove.
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod _owner;

        /// <summary>
        /// Provides access to this mod's configuration.
        /// </summary>
        private Config _configuration;

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;

        private IMemory _memory;

        private UIUtils _ui;
        private FileUtils _files;

        private IAsmHook _setInheritanceHook;
        private IAsmHook _fusionResultsConfirmHook;
        private IAsmHook _fusionResultsHook;
        private IAsmHook _personaMenuDisplayHook;
        private IAsmHook _addInheritedSkillsHook;
        private IAsmHook _skillHelpDescriptionHook;
        private IAsmHook _resultsMenuOpeningHook;
        private IAsmHook _specialResultsMenuOpeningHook;
        private IReverseWrapper<LogInheritanceDelegate> _logInheritanceReverseWrapper;
        private IReverseWrapper<StartChooseInheritanceDelegate> _startChooseInheritanceReverseWrapper;
        private IReverseWrapper<ChooseInheritanceDelegate> _chooseInheritanceReverseWrapper;
        private IReverseWrapper<PersonaMenuDisplayDelegate> _personaMenuDisplayReverseWrapper;
        private IReverseWrapper<ResultsMenuOpeningDelegate> _resultsMenuOpeningReverseWrapper;
        private IReverseWrapper<RenderPromptTextDelegate> _renderPromptTextReverseWrapper;
        private InputStruct* _input;
        private IAsmHook _fusionResultsInitPersonaHook;
        private IAsmHook _renderButtonPromptTextHook;
        private IAsmHook _renderButtonPromptTextJapaneseHook;
        private IAsmHook _renderButtonPromptJapaneseHook;
        private IAsmHook _renderButtonPromptHook;
        private IHook<RenderSkillNameDelegate> _renderSkillName;

        private InheritanceState _state = InheritanceState.NotInMenu;
        private Dictionary<nuint, List<Skill>> _inheritanceSkills = new();
        private Dictionary<Skill, short> _removedNextSkills = new();
        private int _selectedSkillIndex = 0;
        private int _selectedSkillDisplayIndex = 0;
        private PersonaDisplayInfo* _currentPersona;
        private Skill _selectedSkill;
        private List<Skill>? _currentSkills;
        private GameFile* _inheritanceSpr;
        private bool* _inInheritanceMenu;
        private nint _inInheritanceMenuPtr;

        private bool* _allowFusionConfirmation;
        private nint _allowFusionConfirmationPtr;

        private TimeSpan movementDelay = TimeSpan.FromMilliseconds(100);
        private TimeSpan movementInitialDelay = TimeSpan.FromMilliseconds(230);

        private Dictionary<Position, ColourIncreasePair> _outlineColours = new();

        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;

            _memory = Memory.Instance;

            Utils.Initialise(_logger, _configuration);

            var startupScannerController = _modLoader.GetController<IStartupScanner>();
            if (startupScannerController == null || !startupScannerController.TryGetTarget(out var startupScanner))
            {
                Utils.LogError($"Unable to get controller for Reloaded SigScan Library, stuff won't work :(");
                return;
            }

            var criFsController = _modLoader.GetController<ICriFsRedirectorApi>();
            if (criFsController == null || !criFsController.TryGetTarget(out var criFsApi))
            {
                Utils.LogError($"Unable to get controller for CriFs Lib, things will not work :(");
                return;
            }

            criFsApi.AddProbingPath($"Files{Path.DirectorySeparatorChar}{_configuration.TextLanguage}");

            _ui = new UIUtils(startupScanner, _hooks, _configuration);
            _files = new FileUtils(_hooks, startupScanner);
            
            _allowFusionConfirmation = (bool*)_memory.Allocate(4);
            _allowFusionConfirmationPtr = _hooks.Utilities.WritePointer((nint)_allowFusionConfirmation);
            Utils.LogDebug($"Allocated allowFusionConfirmation to 0x{(nint)_allowFusionConfirmation:X}");
            Utils.LogDebug($"Allocated allowFusionConfirmationPtr to 0x{_allowFusionConfirmationPtr:X}");

            _inInheritanceMenu = (bool*)_memory.Allocate(4);
            _inInheritanceMenuPtr = _hooks.Utilities.WritePointer((nint)_inInheritanceMenu);
            Utils.LogDebug($"Allocated inInheritanceMenu to 0x{(nint)_inInheritanceMenu:X}");
            Utils.LogDebug($"Allocated inInheritanceMenuPtr to 0x{_inInheritanceMenuPtr:X}");

            startupScanner.AddMainModuleScan("48 C1 E1 02 E8 ?? ?? ?? ?? 4C 8B E0", result =>
                {
                    if (!result.Found)
                    {
                        Utils.LogError($"Unable to find SetInheritance, stuff won't work :(");
                        return;
                    }
                    Utils.LogDebug($"Found SetInheritance at 0x{result.Offset + Utils.BaseAddress:X}");

                    string[] function =
                    {
                        "use64",
                        "push rax\npush rcx\npush rdx\npush r8\npush r9\npush r11",
                        "mov rcx, r15",
                        "mov rdx, r13",
                        "sub rsp, 32",
                        $"{_hooks.Utilities.GetAbsoluteCallMnemonics(LogInheritance, out _logInheritanceReverseWrapper)}",
                        "add rsp, 32",
                        "pop r11\npop r9\npop r8\npop rdx\npop rcx\npop rax"
                    };
                    _setInheritanceHook = _hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
                });

            startupScanner.AddMainModuleScan("48 8B 4F ?? 48 83 C1 04", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find FusionResultsConfirm, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found FusionResultsConfirm at 0x{result.Offset + Utils.BaseAddress:X}");

                string[] function =
                {
                    "use64",
                    $"{Utils.CmpValue(_allowFusionConfirmationPtr, 1, "byte")}",
                    "je endHook", // Let the confirmation go through as we've already chosen skills
                    "push rax\npush rcx\npush rdx\npush r8\npush r9\npush r11",
                    "mov rcx, rbx",
                    "sub rsp, 32",
                    $"{_hooks.Utilities.GetAbsoluteCallMnemonics(StartChooseInheritance, out _startChooseInheritanceReverseWrapper)}",
                    "add rsp, 32",
                    "pop r11\npop r9\npop r8\npop rdx\npop rcx\npop rax",
                    "add rsp, 0x70",
                    "pop rdi",
                    "ret",
                    "label endHook"
                };
                _fusionResultsConfirmHook = _hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
            });

            startupScanner.AddMainModuleScan("48 89 5C 24 ?? 57 48 83 EC 70 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 48 8B D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 8B 05 ?? ?? ?? ??", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find FusionResultsMenu, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found FusionResultsMenu at 0x{result.Offset + Utils.BaseAddress:X}");

                _input = (InputStruct*)Utils.GetGlobalAddress(result.Offset + Utils.BaseAddress + 42);
                Inputs.Initialise(_input);
                Utils.LogDebug($"InputFlag is at 0x{(nuint)_input:X}");

                string[] function =
                {
                    "use64",
                    "push rax\npush rcx\npush rdx\npush r8\npush r9\npush r10\npush r11",
                    "sub rsp, 32",
                    $"{_hooks.Utilities.GetAbsoluteCallMnemonics(ChooseInheritanceMenu, out _chooseInheritanceReverseWrapper)}",
                    "add rsp, 32",
                    "pop r11\npop r10\npop r9\npop r8\npop rdx\npop rcx",
                    // Check what we should do
                    $"cmp eax, {(int)InheritanceState.NotInMenu}",
                    "je continueNormally", // If not in the skill selection menu go on with the normal stuff
                    $"cmp eax, {(int)InheritanceState.ChoosingSkills}",
                    "je retFunction",
                    $"cmp eax, {(int)InheritanceState.ChooseSkillsMessage}",
                    "je retFunction",
                    //// Go to the fusion confirmation
                    $"{Utils.SetValue(_allowFusionConfirmationPtr, 1, "byte")}", // We want to allow the confirmation
                    "jmp endHook",
                    // Return since we're in the skill selection
                    "label retFunction",
                    "pop rax",
                    "ret",
                    // Continue with normal stuff
                    "label continueNormally",
                    $"{Utils.SetValue(_allowFusionConfirmationPtr, 0, "byte")}", // We want to capture confirmation and take it into the skill selection menu
                    "label endHook",
                    "pop rax",
                };
                _fusionResultsHook = _hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
            });

            startupScanner.AddMainModuleScan("E8 ?? ?? ?? ?? 48 81 C4 A8 01 00 00 5B", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find PersonaMenuDisplay, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found PersonaMenuDisplay at 0x{result.Offset + Utils.BaseAddress:X}");

                string[] function =
                {
                    "use64",
                    "push rax\npush rcx\npush rdx\npush r8\npush r9\npush r10\npush r11",
                    "mov rcx, rbx",
                    "sub rsp, 32",
                    $"{_hooks.Utilities.GetAbsoluteCallMnemonics(PersonaMenuDisplay, out _personaMenuDisplayReverseWrapper)}",
                    "add rsp, 32",
                    "pop r11\npop r10\npop r9\npop r8\npop rdx\npop rcx\npop rax",
                };
                _personaMenuDisplayHook = _hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteAfter).Activate();
            });

            startupScanner.AddMainModuleScan("E8 ?? ?? ?? ?? 48 8B CB 41 FF CE", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find AddInheritedSkills, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found AddInheritedSkills at 0x{result.Offset + Utils.BaseAddress:X}");

                string[] function =
                {
                    "use64",
                    "mov dx, -1" // Make all of the base inherited skills -1 (nothing) so we can add our own
                };
                _addInheritedSkillsHook = _hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
            });

            startupScanner.AddMainModuleScan("E8 ?? ?? ?? ?? 66 85 F6 0F 84 ?? ?? ?? ?? 48 63 05 ?? ?? ?? ??", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find RenderSkillHelpDescription, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found RenderSkillHelpDescription at 0x{result.Offset + Utils.BaseAddress:X}");

                string[] function =
                {
                    "use64",
                    "cmp si, 0" // Instead of test si, si cmp so we can jle
                };
                _skillHelpDescriptionHook = _hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteAfter).Activate();

                _memory.Write(result.Offset + Utils.BaseAddress + 9, (byte)0x8E); // Change the jz to jle so it doesn't write descriptions for -1 skills
            });

            string resultsMenuOpening = _hooks.Utilities.GetAbsoluteCallMnemonics(ResultsMenuOpening, out _resultsMenuOpeningReverseWrapper);

            startupScanner.AddMainModuleScan("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 8D 43 ??", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find OpenFusionResults, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found OpenFusionResults at 0x{result.Offset + Utils.BaseAddress:X}");

                string[] function =
                {
                    "use64",
                    "push rax\npush rcx\npush rdx\npush r8\npush r9\npush r10\npush r11",
                    "sub rsp, 40",
                    $"{resultsMenuOpening}",
                    "add rsp, 40",
                    "pop r11\npop r10\npop r9\npop r8\npop rdx\npop rcx\npop rax",
                };
                _resultsMenuOpeningHook = _hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
            });

            startupScanner.AddMainModuleScan("E8 ?? ?? ?? ?? 48 8B 7C 24 ?? 48 8B B4 24 ?? ?? ?? ?? EB ??", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find OpenSpecialFusionResults, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found OpenSpecialFusionResults at 0x{result.Offset + Utils.BaseAddress:X}");

                string[] function =
                {
                    "use64",
                    "push rax\npush rcx\npush rdx\npush r8\npush r9\npush r10\npush r11",
                    "sub rsp, 40",
                    $"{resultsMenuOpening}",
                    "add rsp, 40",
                    "pop r11\npop r10\npop r9\npop r8\npop rdx\npop rcx\npop rax",
                };
                _specialResultsMenuOpeningHook = _hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
            });

            startupScanner.AddMainModuleScan("4E 8B 84 ?? ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 0F", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find FusionResultsInitPersona, stuff won't work :(");
                    return;
                }
                Utils.LogDebug($"Found FusionResultsInitPersona at 0x{result.Offset + Utils.BaseAddress:X}");

                string[] function =
                {
                    "use64",
                    "push rax",
                    "push rdx",
                    "mov rax, 2",
                    "mov rdx, [rcx+0x48]",
                    "lea rdx, [rdx+0xA8]", // The PersonaSkillsDisplayInfo of the new Persona
                    "label loopStart",
                    // Go through each skill and set them all to 0 so stuff doesn't break when switching between Personas
                    "mov word [rdx + rax], 0",
                    "add rax, 12",
                    "cmp rax, 12*8",
                    "jle loopStart",
                    "pop rdx",
                    "pop rax",
                };

                _fusionResultsInitPersonaHook = _hooks.CreateAsmHook(function, Utils.BaseAddress + result.Offset, AsmHookBehaviour.ExecuteAfter).Activate();
            });

            startupScanner.AddMainModuleScan("48 89 5C 24 ?? 57 48 81 EC 80 00 00 00 0F 29 74 24 ?? 49 8B F9", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find RenderSkillName, thick dashes for empty skills won't be able to be rendered :(");
                    return;
                }
                Utils.LogDebug($"Found RenderSkillName at 0x{result.Offset + Utils.BaseAddress:X}");

                _renderSkillName = _hooks.CreateHook<RenderSkillNameDelegate>(RenderSkillName, Utils.BaseAddress + result.Offset).Activate();
            });

            string renderPromptTextCall = _hooks.Utilities.GetAbsoluteCallMnemonics(RenderPromptText, out _renderPromptTextReverseWrapper);

            startupScanner.AddMainModuleScan("E8 ?? ?? ?? ?? 8B B4 24 ?? ?? ?? ?? F3 0F 10 3D ?? ?? ?? ??", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find RenderButtonPromptText, correct button prompts won't show :(");
                    return;
                }
                Utils.LogDebug($"Found RenderButtonPromptText at 0x{result.Offset + Utils.BaseAddress:X}");

                string[] function =
                {
                    "use64",
                    "cmp dword [rcx+8], 0x10",
                    "jne endHook",
                    $"{Utils.CmpValue(_inInheritanceMenuPtr, 1, "byte")}",
                    "jne endHook",
                    "push rax\npush rcx\npush rdx\npush r8\npush r9\npush r10\npush r11",
                    "sub rsp, 40",
                    $"{renderPromptTextCall}",
                    "add rsp, 40",
                    "pop r11\npop r10\npop r9\npop r8\npop rdx\npop rcx\npop rax",
                    // Skip over the normal text rendering
                    $"mov esi, dword [rsp + 0xd8]",
                    $"{_hooks.Utilities.GetAbsoluteJumpMnemonics(Utils.BaseAddress + result.Offset + 12, true)}",
                    "label endHook"
                };

                _renderButtonPromptTextHook = _hooks.CreateAsmHook(function, Utils.BaseAddress + result.Offset, AsmHookBehaviour.ExecuteFirst).Activate();
            });

            startupScanner.AddMainModuleScan("F3 44 0F 11 54 24 ?? E8 ?? ?? ?? ?? F3 0F 10 3D ?? ?? ?? ?? 41 0F 28 D9", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find RenderButtonPromptTextJapanese, correct button prompts won't show :(");
                    return;
                }
                Utils.LogDebug($"Found RenderButtonPromptTextJapanese at 0x{result.Offset + Utils.BaseAddress:X}");

                string[] function =
                {
                    "use64",
                    $"{Utils.CmpValue(_inInheritanceMenuPtr, 1, "byte")}",
                    "jne endHook",
                    "push rax\npush rcx\npush rdx\npush r8\npush r9\npush r10\npush r11",
                    "sub rsp, 40",
                    $"{renderPromptTextCall}",
                    "add rsp, 40",
                    "pop r11\npop r10\npop r9\npop r8\npop rdx\npop rcx\npop rax",
                    // Skip over the normal text rendering
                    $"{_hooks.Utilities.GetAbsoluteJumpMnemonics(Utils.BaseAddress + result.Offset + 12, true)}",
                    "label endHook"
                };

                _renderButtonPromptTextJapaneseHook = _hooks.CreateAsmHook(function, Utils.BaseAddress + result.Offset, AsmHookBehaviour.ExecuteFirst).Activate();
            });

            startupScanner.AddMainModuleScan("66 0F 6E F3 0F 5B F6 0F 28 CE", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find RenderButtonPrompt, button prompts will be positioned weirdly :(");
                    return;
                }
                Utils.LogDebug($"Found RenderButtonPrompt at 0x{result.Offset + Utils.BaseAddress:X}");

                // Change the positioning of the skill info button prompt (there's definitely a better way to do this lol)
                string[] function =
                {
                    "use64",
                    $"{Utils.CmpValue(_inInheritanceMenuPtr, 1, "byte")}",
                    "jne endHook",
                    "cmp ebx, 299",
                    "je change",
                    "cmp ebx, 288",
                    "je changeSpanish",
                    "cmp ebx, 298",
                    "jne endHook",
                    "label change",
                    "mov ebx, 283",
                    "jmp endHook",
                    "label changeSpanish",
                    "mov ebx, 310", // The text is much smaller in Spanish so move button more to the right
                    "label endHook"
                };

                _renderButtonPromptHook = _hooks.CreateAsmHook(function, Utils.BaseAddress + result.Offset, AsmHookBehaviour.ExecuteFirst).Activate();
            });

            startupScanner.AddMainModuleScan("B9 0F 00 00 00 C6 44 24 ?? FF E8 ?? ?? ?? ?? 44 88 74 24 ?? 0F 57 DB C6 44 24 ?? FF 41 0F 28 D1", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find RenderButtonPromptJapanese, button prompts will be positioned weirdly :(");
                    return;
                }
                Utils.LogDebug($"Found RenderButtonPromptJapanese at 0x{result.Offset + Utils.BaseAddress:X}");

                float* promptPos = (float*)_memory.Allocate(4);
                Utils.LogDebug($"Allocated promptPos to 0x{(nuint)promptPos:X}");
                nint promptPosPtr = _hooks.Utilities.WritePointer((nint)promptPos);
                Utils.LogDebug($"Allocated promptPosPtr to 0x{promptPosPtr:X}");
                *promptPos = 311;

                // Change the positioning of the skill info button prompt
                string[] function =
                {
                    "use64",
                    $"{Utils.CmpValue(_inInheritanceMenuPtr, 1, "byte")}",
                    "jne endHook",
                    "push rax",
                    $"mov rax, [qword {promptPosPtr}]",
                    $"movss xmm1, [rax]", // Move the skill info button prompt to the right
                    "pop rax",
                    "label endHook"
                };

                _renderButtonPromptJapaneseHook = _hooks.CreateAsmHook(function, Utils.BaseAddress + result.Offset, AsmHookBehaviour.ExecuteFirst).Activate();
            });
        }

        private void LogInheritance(nuint skillsListAddr, Persona* personaPtr)
        {
            Persona persona = *personaPtr;
            string baseSkills = "";
            for (int i = 0; i < 8; i++)
            {
                if (persona.Skills[i] != 0)
                    baseSkills += (Skill)persona.Skills[i] + ", ";
            }
            Utils.LogDebug($"{persona.Id} has skills {baseSkills}");
            PList<Skill> skills = new PList<Skill>(skillsListAddr);
            List<Skill> skillsList = new();
            for (int i = 0; i < skills.Count; i++)
                skillsList.Add(skills[i]);
            skillsList.Sort();
            Utils.LogDebug($"{persona.Id} can inherit {string.Join(", ", skillsList)}");

            if (_inheritanceSkills.ContainsKey((nuint)personaPtr))
                _inheritanceSkills.Remove((nuint)personaPtr);
            _inheritanceSkills.Add((nuint)personaPtr, skillsList);
        }

        private void StartChooseInheritance(FusionMenuInfo* info)
        {
            Utils.LogDebug($"Opening choose inheritance menu for {info->ResultPersona->Persona.Id}");
            _outlineColours.Clear();
            _currentPersona = null;
            _currentSkills = null;
            _selectedSkillIndex = 0;
            _selectedSkillDisplayIndex = 0;
            _selectedSkill = Skill.None;
            if (_ui.QueueCombineMessage != null && _ui.QueuedMessageWaiting != null && _ui.GetMessageState != null && _ui.AfterQueuedMessage != null)
            {
                _ui.QueueCombineMessage(info->SelectionContextId, 102);
                _state = InheritanceState.ChooseSkillsMessage;
            }
            else
            {
                _state = InheritanceState.ChoosingSkills;
                *_inInheritanceMenu = true;
            }
        }

        private InheritanceState ChooseInheritanceMenu(FusionMenuInfo* info)
        {
            if (_currentPersona == null || (_state == InheritanceState.NotInMenu && _currentPersona == null))
                return InheritanceState.NotInMenu;

            var persona = &info->ResultPersona->Persona;

            // Back in the menu after selecting no to the confirmation
            if (_state == InheritanceState.NotInMenu && _currentPersona != null)
            {
                _outlineColours.Clear();
                RemoveLastInheritedSkill(_currentPersona, persona, _removedNextSkills);
                _state = InheritanceState.ChoosingSkills;
                *_inInheritanceMenu = true;
                return InheritanceState.ChoosingSkills;
            }

            if (_currentSkills == null)
            {
                if (!_inheritanceSkills.TryGetValue((nuint)info->ResultPersona + 4, out var skills))
                {
                    Utils.LogError($"No inheritance skills found for {persona->Id}, leaving menu");
                    _state = InheritanceState.NotInMenu;
                    *_inInheritanceMenu = false;
                    return InheritanceState.DoneChoosingSkills;
                }
                _currentSkills = skills;
            }

            if (_selectedSkill == Skill.None)
                _selectedSkill = _currentSkills[_selectedSkillIndex];

            if (_state == InheritanceState.ChooseSkillsMessage)
            {
                _ui.QueuedMessageWaiting(info->SelectionContextId, 1);
                if (_ui.GetMessageState(info->SelectionContextId) != 0)
                    return InheritanceState.ChoosingSkills;
                Utils.LogDebug($"Done with choose skills message");
                _outlineColours.Clear();
                _ui.AfterQueuedMessage(info->SelectionContextId, 1);
                _state = InheritanceState.ChoosingSkills;
                *_inInheritanceMenu = true;
            }

            if (_inputs->Pressed.HasFlag(InputFlag.SubMenu))
            {
                if (_state == InheritanceState.MenuHidden)
                {
                    _state = InheritanceState.ChoosingSkills;
                    _ui.PlaySoundEffect(SoundEffect.MenuClosed);
                }
                else
                {
                    _state = InheritanceState.MenuHidden;
                    _ui.PlaySoundEffect(SoundEffect.MenuOpened);
                    return InheritanceState.ChoosingSkills;
                }
            }
            else if (_state == InheritanceState.MenuHidden)
            {
                if (_inputs->Pressed.HasFlag(InputFlag.Confirm) || _inputs->Pressed.HasFlag(InputFlag.Escape))
                {
                    _ui.PlaySoundEffect(SoundEffect.MenuClosed);
                    _state = InheritanceState.ChoosingSkills;
                    return InheritanceState.ChoosingSkills;
                }
                else
                {
                    return InheritanceState.ChoosingSkills;
                }
            }

            if (IsHeld(Input.Up, movementDelay, movementInitialDelay) && _currentSkills.Count > 1)
            {
                if (_selectedSkillIndex > 0)
                {
                    _ui.PlaySoundEffect(SoundEffect.SelectionMoved);
                    // If we can see the first skill or the display is the bottom 2 move it up
                    if (_selectedSkillDisplayIndex != 0 && (_selectedSkillDisplayIndex > 2 || _selectedSkillIndex - _selectedSkillDisplayIndex == 0))
                        _selectedSkillDisplayIndex--;
                    _selectedSkillIndex--;
                }
                else
                {
                    // Only wrap around if you press the button, not holding it
                    if (_inputs->Pressed.HasFlag(InputFlag.Up) || _inputs->ThumbstickPressed.HasFlag(InputFlag.Up))
                    {
                        _ui.PlaySoundEffect(SoundEffect.SelectionMoved);
                        _selectedSkillIndex = _currentSkills.Count - 1;
                        _selectedSkillDisplayIndex = 4;
                        if (_currentSkills.Count < 5)
                            _selectedSkillDisplayIndex = _currentSkills.Count - 1;
                    }
                }
                _selectedSkill = _currentSkills[_selectedSkillIndex];
            }
            if (IsHeld(Input.Down, movementDelay, movementInitialDelay) && _currentSkills.Count > 1)
            {
                if (_selectedSkillIndex < _currentSkills.Count - 1)
                {
                    _ui.PlaySoundEffect(SoundEffect.SelectionMoved);
                    // If we can see the last skill or the display is the top 2, move it down
                    if (_selectedSkillDisplayIndex != 4 && (_selectedSkillDisplayIndex < 2 || _selectedSkillIndex - _selectedSkillDisplayIndex >= _currentSkills.Count - 5))
                        _selectedSkillDisplayIndex++;
                    _selectedSkillIndex++;
                }
                else
                {
                    // Only wrap around if you press the button, not holding it
                    if (_inputs->Pressed.HasFlag(InputFlag.Down) || _inputs->ThumbstickPressed.HasFlag(InputFlag.Down))
                    {
                        _ui.PlaySoundEffect(SoundEffect.SelectionMoved);
                        _selectedSkillIndex = 0;
                        _selectedSkillDisplayIndex = 0;
                    }
                }
                _selectedSkill = _currentSkills[_selectedSkillIndex];
            }
            if (IsHeld(Input.Right, movementDelay, movementInitialDelay))
            {
                bool didMove = false;
                // Go down 5 or until the last skill can be seen
                for (int i = 0; i < 5; i++)
                {
                    if (_selectedSkillIndex - _selectedSkillDisplayIndex >= _currentSkills.Count - 5)
                        break;
                    _selectedSkillIndex++;
                    didMove = true;
                }
                if (didMove)
                    _ui.PlaySoundEffect(SoundEffect.SelectionJumped);
                _selectedSkill = _currentSkills[_selectedSkillIndex];
            }
            if (IsHeld(Input.Left, movementDelay, movementInitialDelay))
            {
                bool didMove = false;
                // Go up 5 or until the first skill can be seen
                for (int i = 0; i < 5; i++)
                {
                    if (_selectedSkillIndex - _selectedSkillDisplayIndex == 0)
                        break;
                    _selectedSkillIndex--;
                    didMove = true;
                }
                if (didMove)
                    _ui.PlaySoundEffect(SoundEffect.SelectionJumped);
                _selectedSkill = _currentSkills[_selectedSkillIndex];
            }
            if (_input->Pressed.HasFlag(InputFlag.Confirm))
            {
                var newSkillIndex = AddInheritedSkill(_currentPersona, persona, _selectedSkill, _removedNextSkills);
                if (newSkillIndex != -1)
                {
                    _ui.PlaySoundEffect(SoundEffect.Confirm);
                    _outlineColours.Clear();

                    if ((_currentPersona->SkillsInfo.NewSkillsMask & (1 << newSkillIndex + 1)) == 0)
                    {
                        Utils.LogDebug($"Done selecting skills for {persona->Id}");
                        _state = InheritanceState.NotInMenu;
                        *_inInheritanceMenu = false;
                        return InheritanceState.DoneChoosingSkills;
                    }
                }
                else
                {
                    _ui.PlaySoundEffect(SoundEffect.Error);
                }
            }

            if (_input->Pressed.HasFlag(InputFlag.Escape))
            {
                _ui.PlaySoundEffect(SoundEffect.Back);
                var removedSkill = RemoveLastInheritedSkill(_currentPersona, persona, _removedNextSkills);
                if (removedSkill != Skill.None)
                {
                    _outlineColours.Clear();
                    // Move the cursor back to the skill that was just removed
                    var displayIndex = _currentSkills.IndexOf(removedSkill);
                    if (displayIndex != -1)
                    {
                        _selectedSkillIndex = displayIndex;
                        _selectedSkill = removedSkill;
                        // Attempt to align the display index to have two skills on either side
                        if (_selectedSkillIndex < 2)
                            _selectedSkillDisplayIndex = _selectedSkillIndex;
                        else if (_selectedSkillIndex >= _currentSkills.Count - 2)
                            _selectedSkillDisplayIndex = (_currentSkills.Count < 5 ? _currentSkills.Count : 5) - (_currentSkills.Count - _selectedSkillIndex);
                        else
                            _selectedSkillDisplayIndex = 2;
                    }
                }
                else
                {
                    Utils.LogDebug("Cancelling inheritance choice");
                    _outlineColours.Clear();
                    _state = InheritanceState.NotInMenu;
                    _currentPersona = null;
                    _input->Pressed &= ~InputFlag.Escape; // Absorb the escape so the whole results menu doesn't close
                    *_inInheritanceMenu = false;
                    return InheritanceState.NotInMenu;
                }
            }

            return InheritanceState.ChoosingSkills;
        }

        private void PersonaMenuDisplay(PersonaMenuInfo* info)
        {
            if (_state == InheritanceState.ChooseSkillsMessage && _currentPersona == null)
            {
                _currentPersona = &info->Persona;
                Utils.LogDebug($"Current persona is at 0x{(nuint)_currentPersona:X}");
            }

            // Render the box around the next skill selected when the menu is hidden
            if (_state == InheritanceState.MenuHidden && _inheritanceSpr != (GameFile*)0 && _inheritanceSpr->LoadStatus == FileLoadStatus.Done)
            {
                int nextSkillIndexx = FirstIndexOfSkill(&info->Persona, Skill.None);
                if (nextSkillIndexx != -1)
                {
                    float xPos = nextSkillIndexx < 4 ? 20.25f : 169.25f;
                    float yPos = 183 + (nextSkillIndexx % 4) * 17;
                    var outlineColour = GetOutlineColour(new Position { X = xPos, Y = yPos }, false);
                    _ui.RenderSprTexture(_inheritanceSpr, (int)InheritanceSprite.SkillRectangle, xPos, yPos, 0, outlineColour.R, outlineColour.G, outlineColour.B, outlineColour.A, 0x1000, 0x1000, 0, 0, 0);
                }
            }

            if (_state != InheritanceState.ChoosingSkills)
                return;

            if (_currentPersona == null)
                _currentPersona = &info->Persona;

            // Render background
            var baseSpr = _ui.LoadCampFile("c_main_01.spr");
            if (baseSpr == (GameFile*)0)
            {
                Utils.LogError($"Error loading c_main_01.spr");
                return;
            }

            if (_inheritanceSpr == (GameFile*)0)
            {
                _inheritanceSpr = _files.LoadFile("facility/combine/inheritance.spr");
                if (_inheritanceSpr == (GameFile*)0)
                {
                    Utils.LogError($"Error loading facility/combine/inheritance.spr");
                    return;
                }
            }

            if (_inheritanceSpr->LoadStatus != FileLoadStatus.Done)
            {
                Utils.LogDebug($"LoadStatus for inheritance.spr is {_inheritanceSpr->LoadStatus}");
                return;
            }

            //Utils.LogDebug($"Loaded inheritance.spr at 0x{(nuint)inheritanceSpr:X}");
            //Utils.LogDebug($"Loaded c_main_01.spr at 0x{(nuint)baseSpr:X}");

            // Background rectangles
            var bgColour = Colours.SkillFg;
            _ui.RenderSprTexture(baseSpr, 0x1b4, 97, 78, 0, bgColour.R, bgColour.G, bgColour.B, 0xFF, 0x1000, 0x1000, 0, 0, 0);
            _ui.RenderSprTexture(baseSpr, 0x1b4, 97, 118, 0, bgColour.R, bgColour.G, bgColour.B, 0xFF, 0x1000, 0x1000, 0, 0, 0);
            // Choose a skill text
            var textColour = Colours.SkillBg;
            if(_configuration.AlternateChooseSkills)
                _ui.RenderSprTexture(_inheritanceSpr, (int)InheritanceSprite.ChooseSkills2, 264, 86, 0, textColour.R, textColour.G, textColour.B, 0xFF, 0x1000, 0x1000, 0, 0, 0);
            else
                _ui.RenderSprTexture(_inheritanceSpr, (int)InheritanceSprite.ChooseSkills, 259, 81, 0, textColour.R, textColour.G, textColour.B, 0xFF, 0x1000, 0x1000, 0, 0, 0);

            // Render skill help
            if (_ui.RenderSkillHelp != null)
                _ui.RenderSkillHelp(new Position { X = 252, Y = 132 }, 0, 0xFF, _selectedSkill);

            _ui.RenderSprTexture(_inheritanceSpr, (int)InheritanceSprite.Divider, 259, 78.65f, 0, textColour.R, textColour.G, textColour.B, 0xFF, 0x1000, 0x1000, 0, 0, 0);

            if (_currentSkills != null && _currentSkills.Count > 5)
            {
                // Render scroll bar bg
                _ui.RenderSprTexture(_inheritanceSpr, (int)InheritanceSprite.ScrollBg, 251, 82.6f, 0, textColour.R, textColour.G, textColour.B, 0xFF, 0x1000, 0x1000, 0, 0, 0);

                float incrementSize = (138.6f - 83) / (_currentSkills.Count - 5);
                int startIndex = _selectedSkillIndex - _selectedSkillDisplayIndex;

                // Render scroll bar slider (up to 138.6 y)
                _ui.RenderSprTexture(_inheritanceSpr, (int)InheritanceSprite.ScrollSlider, 251.28f, 83 + (incrementSize * startIndex), 0, bgColour.R, bgColour.G, bgColour.B, 0xFF, 0x1000, 0x1000, 0, 0, 0);
            }

            // Render skill buttons
            if (_ui.RenderSkill != null && _currentSkills != null)
            {
                PersonaDisplayInfo persona = new PersonaDisplayInfo();
                persona.SkillsInfo.NumSkills = 1;
                Position pos = new Position { X = 100, Y = 84.5f };
                int startIndex = _selectedSkillIndex - _selectedSkillDisplayIndex;

                // Display the skills (up to 5)
                for (int i = startIndex; i < startIndex + 5; i++)
                {
                    if (i >= _currentSkills.Count || i < 0) break;
                    persona.SkillsInfo.Skills.Id = (short)_currentSkills[i];
                    persona.SelectedSlot = -1;
                    // Different colourPair for the selected skill
                    if (i == _selectedSkillIndex)
                    {
                        persona.SkillsInfo.NextSkills.BgColour = Colours.SelectedBg;
                        persona.SkillsInfo.NextSkills.FgColour = Colours.SelectedFg;
                        if (HasSkill(_currentPersona, _currentSkills[i]))
                            persona.SkillsInfo.NextSkills.FgColour = Colours.AlreadyChosenSelectedFg;
                        persona.SkillsInfo.NumNextSkills = -1;
                    }
                    // Different colourPair for skills that have already been selected
                    else if (HasSkill(_currentPersona, _currentSkills[i]))
                    {
                        persona.SkillsInfo.NextSkills.BgColour = Colours.AlreadyChosenBg;
                        persona.SkillsInfo.NextSkills.FgColour = Colours.AlreadyChosenFg;
                        persona.SkillsInfo.NumNextSkills = -1;
                    }
                    else
                    {
                        persona.SkillsInfo.NumNextSkills = 0;
                    }

                    _ui.RenderSkill(pos, 0, 0xFF, &persona, 0, 0);
                    pos.Y += 17;
                }
            }

            // Render the box around the next skill selected
            int nextSkillIndex = FirstIndexOfSkill(&info->Persona, Skill.None);
            if (nextSkillIndex != -1)
            {
                float xPos = nextSkillIndex < 4 ? 20.25f : 169.25f;
                float yPos = 183 + (nextSkillIndex % 4) * 17;
                var outlineColour = GetOutlineColour(new Position { X = xPos, Y = yPos }, false);
                _ui.RenderSprTexture(_inheritanceSpr, (int)InheritanceSprite.SkillRectangle, xPos, yPos, 0, outlineColour.R, outlineColour.G, outlineColour.B, outlineColour.A, 0x1000, 0x1000, 0, 0, 0);
            }
        }

        private void ResultsMenuOpening()
        {
            Utils.LogDebug("Opening results menu");
            _currentPersona = null;
            _currentSkills = null;
            _removedNextSkills.Clear();
            _inheritanceSpr = (GameFile*)0;
            _outlineColours.Clear();
            _state = InheritanceState.NotInMenu;
        }

        private void RenderSkillName(Position position, long param_2, byte alpha, SkillTextDisplayInfo* textInfo, long param_5, float param_6)
        {
            if (textInfo->Skill != Skill.None || _configuration.EmptySkillsUseText)
            {
                _renderSkillName.OriginalFunction(position, param_2, alpha, textInfo, param_5, param_6);
            }
            if (textInfo->Skill != Skill.None)
                return;

            // Render rectangle around all empty skills (done only to next when selecting skills)
            if (_state != InheritanceState.ChoosingSkills && _state != InheritanceState.MenuHidden && _inheritanceSpr != (GameFile*)0 && _inheritanceSpr->LoadStatus == FileLoadStatus.Done)
            {
                var xPos = position.X - 72.75f;
                var yPos = position.Y + 0.5f;
                var outlineColour = GetOutlineColour(new Position { X = xPos, Y = yPos }, true);
                _ui.RenderSprTexture(_inheritanceSpr, (int)InheritanceSprite.SkillRectangle, xPos, yPos, 0, outlineColour.R, outlineColour.G, outlineColour.B, alpha < 255 ? alpha : outlineColour.A, 0x1000, 0x1000, 0, 0, 0);
            }

            if (!_configuration.EmptySkillsUseText)
            {
                if (_inheritanceSpr == (GameFile*)0)
                {
                    _inheritanceSpr = _files.LoadFile("facility/combine/inheritance.spr");
                    if (_inheritanceSpr == (GameFile*)0)
                    {
                        Utils.LogError($"Error loading facility/combine/inheritance.spr");
                        return;
                    }
                }
                if (_inheritanceSpr->LoadStatus != FileLoadStatus.Done)
                    return;
                Colours.Colour textColour;
                switch (textInfo->TextType)
                {
                    case SkillTextType.Normal:
                        textColour = Colours.SkillFg;
                        break;
                    case SkillTextType.Selected:
                        textColour = Colours.SkillBg;
                        break;
                    case SkillTextType.New:
                        textColour = Colours.SelectedBg;
                        break;
                    default:
                        textColour = Colours.SkillFg;
                        break;
                }
                for (int i = 0; i < 5; i++)
                {
                    _ui.RenderSprTexture(_inheritanceSpr, (int)InheritanceSprite.BlankDash, position.X - 39.25f + (17.1f * i), position.Y + 7.5f, 0, textColour.R, textColour.G, textColour.B, alpha, 0x1000, 0x1000, 0, 0, 0);
                }
            }
        }


        private void RenderPromptText()
        {
            // Wait for something else to load it
            if (_inheritanceSpr == (GameFile*)0 || _inheritanceSpr->LoadStatus != FileLoadStatus.Done)
                return;
            var sprIndex = _state == InheritanceState.MenuHidden ? (int)InheritanceSprite.DisplayOn : (int)InheritanceSprite.DisplayOff;
            var xpos = 304; 
            var ypos = 256.5f;
            if (_configuration.TextLanguage == Config.Language.Spanish) xpos = 328;
            else if (_configuration.TextLanguage == Config.Language.Japanese)
            {
                xpos = 310;
                ypos = 255.5f;
            }
            _ui.RenderSprTexture(_inheritanceSpr, sprIndex, xpos, ypos, 0, 255, 255, 255, 255, 0x1000, 0x1000, 0, 0, 0);
        }

        private Colour GetOutlineColour(Position position, bool alphaBasedOnSlot)
        {
            if (!_outlineColours.TryGetValue(position, out var colourPair))
            {
                var colour = Colours.Outline;
                // Calculate the alpha based on the slot the skill is in to get a cool pattern
                // TODO fix this so it actually makes the wave pattern like in p4g
                //if (alphaBasedOnSlot)
                //{
                //    int index = (int)(position.Y - 132) / 17;
                //    if (position.X >= 169.25)
                //        index = 3 - index; // make it so the pattern is reversed between the two sides

                //    //colour.A = (byte)((255 - _configuration.MinOutlineAlpha) / 4 * index); // Evenly split up the slots between the max and min alpha
                //    colour.A = (byte)(255 - 25 * index); // Evenly split up the slots between the max and min alpha
                //    Utils.LogDebug($"Starting alpha for item at y index {index} and x pos {position.X} at {colour.A}");
                //}
                colourPair = new ColourIncreasePair(colour);
                _outlineColours.Add(position, colourPair);
                return colour;
            }
            else
            {
                var colour = colourPair.Colour;
                _outlineColours[position].Colour.A = (byte)(colourPair.Colour.A + (colourPair.Increase* _configuration.OutlineAlphaChangeMultipler));
                // Go back down after reaching max alpha
                if (colourPair.Increase > 0 && colourPair.Colour.A >= 255-_configuration.OutlineAlphaChangeMultipler+1)
                    _outlineColours[position].Increase = -1;
                // Go back up after eaching the minimum alpha
                else if (colourPair.Increase < 0 && colourPair.Colour.A <= _configuration.MinOutlineAlpha)
                    _outlineColours[position].Increase = 1;
                return colour;
            }
        }

        private class ColourIncreasePair
        {
            internal Colour Colour;
            internal int Increase;

            internal ColourIncreasePair(Colour colour, int increase = -1)
            {
                Colour = colour;
                Increase = increase;
            }
        }

        private enum InheritanceState
        {
            NotInMenu,
            ChooseSkillsMessage,
            ChoosingSkills,
            DoneChoosingSkills,
            MenuHidden,
        }

        [Function(CallingConventions.Microsoft)]
        private delegate void RenderPromptTextDelegate();

        [Function(CallingConventions.Microsoft)]
        private delegate void RenderSkillNameDelegate(Position position, long param_2, byte alpha, SkillTextDisplayInfo* textInfo, long param_5, float param_6);

        [Function(CallingConventions.Microsoft)]
        private delegate void ResultsMenuOpeningDelegate();

        [Function(CallingConventions.Microsoft)]
        private delegate void PersonaMenuDisplayDelegate(PersonaMenuInfo* info);

        [Function(CallingConventions.Microsoft)]
        private delegate InheritanceState ChooseInheritanceDelegate(FusionMenuInfo* info);

        [Function(CallingConventions.Microsoft)]
        private delegate void StartChooseInheritanceDelegate(FusionMenuInfo* info);

        [Function(CallingConventions.Microsoft)]
        private delegate void LogInheritanceDelegate(nuint skillsListAddr, Persona* personaPtr);

        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
        {
            // Apply settings from configuration.
            // ... your code here.
            _configuration = configuration;
            _ui.ConfigUpdated(configuration);
            _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        }
        #endregion

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}