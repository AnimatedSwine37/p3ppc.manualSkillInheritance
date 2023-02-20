using p3ppc.manualSkillInheritance.Configuration;
using p3ppc.manualSkillInheritance.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Memory.Sources;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;
using static p3ppc.manualSkillInheritance.FusionMenu;
using static p3ppc.manualSkillInheritance.PersonaMenu;
using static p3ppc.manualSkillInheritance.Personas;
using static p3ppc.manualSkillInheritance.Skills;
using static p3ppc.manualSkillInheritance.UI;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

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

        private UI _ui;

        private IAsmHook _setInheritanceHook;
        private IAsmHook _fusionResultsConfirmHook;
        private IAsmHook _fusionResultsHook;
        private IAsmHook _personaMenuDisplayHook;
        private IAsmHook _addInheritedSkillsHook;
        private IAsmHook _skillHelpDescriptionHook;
        private IAsmHook _resultsMenuOpeningHook;
        private IReverseWrapper<LogInheritanceDelegate> _logInheritanceReverseWrapper;
        private IReverseWrapper<StartChooseInheritanceDelegate> _startChooseInheritanceReverseWrapper;
        private IReverseWrapper<ChooseInheritanceDelegate> _chooseInheritanceReverseWrapper;
        private IReverseWrapper<PersonaMenuDisplayDelegate> _personaMenuDisplayReverseWrapper;
        private IReverseWrapper<ResultsMenuOpeningDelegate> _resultsMenuOpeningReverseWrapper;
        private Input* _input;

        private bool _inSkillSelection = false;
        private Dictionary<nuint, List<Skill>> _inheritanceSkills = new();
        private int _selectedSkillIndex = 0;
        private PersonaDisplayInfo* _currentPersona;
        private Skill _selectedSkill;
        private List<Skill>? _currentSkills;

        private nuint _allowFusionConfirmation;

        public Mod(ModContext context)
        {
            //Debugger.Launch();
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

            _ui = new UI(startupScanner, _hooks);

            _allowFusionConfirmation = _memory.Allocate(1);

            //PList<int>.Initialise(startupScanner, _hooks);

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
                    $"cmp [qword {_allowFusionConfirmation}], byte 1",
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

                _input = (Input*)Utils.GetGlobalAddress(result.Offset + Utils.BaseAddress + 42);
                Utils.LogDebug($"Input is at 0x{(nuint)_input:X}");

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
                    //// Go to the fusion confirmation
                    $"mov [qword {_allowFusionConfirmation}], byte 1", // We want to allow the confirmation
                    "jmp endHook",
                    // Return since we're in the skill selection
                    "label retFunction",
                    "pop rax",
                    "ret",
                    // Continue with normal stuff
                    "label continueNormally",
                    $"mov [qword {_allowFusionConfirmation}], byte 0", // We want to capture confirmation and take it into the skill selection menu
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
                    $"{_hooks.Utilities.GetAbsoluteCallMnemonics(ResultsMenuOpening, out _resultsMenuOpeningReverseWrapper)}",
                    "add rsp, 40",
                    "pop r11\npop r10\npop r9\npop r8\npop rdx\npop rcx\npop rax",
                };
                _resultsMenuOpeningHook = _hooks.CreateAsmHook(function, result.Offset + Utils.BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
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
            Utils.LogDebug($"{persona.Id} can inherit {string.Join(", ", skillsList)}");

            if (_inheritanceSkills.ContainsKey((nuint)personaPtr))
                _inheritanceSkills.Remove((nuint)personaPtr);
            _inheritanceSkills.Add((nuint)personaPtr, skillsList);
        }

        private void StartChooseInheritance(FusionMenuInfo* info)
        {
            Utils.LogDebug($"Opening choose inheritance menu for {info->ResultPersona->Persona.Id}");
            _currentPersona = null;
            _currentSkills = null;
            _selectedSkillIndex = 0;
            _selectedSkill = Skill.None;
            _inSkillSelection = true;
        }

        private InheritanceState ChooseInheritanceMenu(FusionMenuInfo* info)
        {
            if (_currentPersona == null || (!_inSkillSelection && _currentPersona == null))
                return InheritanceState.NotInMenu;

            var persona = &info->ResultPersona->Persona;

            // Back in the menu after selecting no to the confirmation
            if (!_inSkillSelection && _currentPersona != null)
            {
                var mask = _currentPersona->SkillsInfo.NewSkillsMask;
                for (int i = 7; i >= 0; i--)
                {
                    if ((mask & (1 << i)) != 0 && persona->Skills[i] > 0)
                    {
                        Utils.LogDebug($"Removing {(Skill)persona->Skills[i]} from {persona->Id}");
                        persona->Skills[i] = -1;
                        (&_currentPersona->SkillsInfo.Skills)[i].Id = -1;
                        break;
                    }
                }
                _inSkillSelection = true;
                return InheritanceState.ChoosingSkills;
            }

            if(_currentSkills == null)
            {
                if (!_inheritanceSkills.TryGetValue((nuint)info->ResultPersona + 4, out var skills))
                {
                    Utils.LogError($"No inheritance skills found for {persona->Id}, leaving menu");
                    _inSkillSelection = false;
                    return InheritanceState.DoneChoosingSkills;
                }
                _currentSkills = skills;
            }

            if (_selectedSkill == Skill.None)
                _selectedSkill = _currentSkills[_selectedSkillIndex];

            if (_input->HasFlag(Input.Up) && _currentSkills.Count > 1)
            {
                _ui.PlaySoundEffect(SoundEffect.SelectionChanged);
                if (_selectedSkillIndex > 0)
                    _selectedSkillIndex--;
                else
                    _selectedSkillIndex = _currentSkills.Count - 1;
                _selectedSkill = _currentSkills[_selectedSkillIndex];
            }
            if (_input->HasFlag(Input.Down) && _currentSkills.Count > 1)
            {
                _ui.PlaySoundEffect(SoundEffect.SelectionChanged);
                if (_selectedSkillIndex < _currentSkills.Count - 1)
                    _selectedSkillIndex++;
                else
                    _selectedSkillIndex = 0;
                _selectedSkill = _currentSkills[_selectedSkillIndex];
            }
            if (_input->HasFlag(Input.Confirm) )
            {
                var currentSkills = persona->Skills;
                bool alreadyHasSkill = false;
                int emptySkillIndex = -1;
                for (int i = 0; i < 8; i++)
                {
                    if (currentSkills[i] == (short)_selectedSkill)
                        alreadyHasSkill = true;
                    else if (currentSkills[i] == (short)Skill.None)
                    {
                        emptySkillIndex = i;
                        break;
                    }
                }
                if (!alreadyHasSkill && emptySkillIndex != -1)
                {
                    _ui.PlaySoundEffect(SoundEffect.Confirm);

                    (&_currentPersona->SkillsInfo.Skills)[emptySkillIndex].Id = (short)_selectedSkill;
                    persona->Skills[emptySkillIndex] = (short)_selectedSkill;
                    Utils.LogDebug($"Added {_selectedSkill} to {persona->Id}");
                    if ((_currentPersona->SkillsInfo.NewSkillsMask & (1 << emptySkillIndex + 1)) == 0)
                    {
                        Utils.LogDebug($"Done selecting skills for {persona->Id}");
                        _inSkillSelection = false;
                        return InheritanceState.DoneChoosingSkills;
                    }
                }
                else
                {
                    _ui.PlaySoundEffect(SoundEffect.Error);
                    Utils.LogDebug($"Cannot add {_selectedSkill} to {persona->Id}");
                }
            }

            if (_input->HasFlag(Input.Escape))
            {
                _ui.PlaySoundEffect(SoundEffect.Back);
                var mask = _currentPersona->SkillsInfo.NewSkillsMask;
                bool skillRemoved = false;
                for (int i = 7; i >= 0; i--)
                {
                    if ((mask & (1 << i)) != 0 && persona->Skills[i] > 0)
                    {
                        var removedSkill = (Skill)persona->Skills[i];
                        Utils.LogDebug($"Removing {removedSkill} from {persona->Id}");
                        persona->Skills[i] = -1;
                        (&_currentPersona->SkillsInfo.Skills)[i].Id = -1;
                        skillRemoved = true;
                        // Move the cursor back to the skill that was just removed
                        var displayIndex = _currentSkills.IndexOf(removedSkill);
                        if(displayIndex != -1)
                        {
                            _selectedSkillIndex = displayIndex;
                            _selectedSkill = removedSkill;
                        }
                        break;
                    }
                }
                if (!skillRemoved)
                {
                    Utils.LogDebug("Cancelling inheritance choice");
                    _inSkillSelection = false;
                    _currentPersona = null;
                    *_input &= ~Input.Escape; // Absorb the escape so the whole results menu doesn't close
                    return InheritanceState.NotInMenu;
                }
            }

            return InheritanceState.ChoosingSkills;
        }

        private void PersonaMenuDisplay(PersonaMenuInfo* info)
        {
            if (!_inSkillSelection)
                return;

            if (_currentPersona == null)
                _currentPersona = &info->Persona;

            if (_ui.RenderSkillHelp != null)
                _ui.RenderSkillHelp(new Position { X = 252, Y = 132 }, 0, 0xFF, _selectedSkill);

            if (_ui.RenderSkill != null && _currentSkills != null)
            {
                PersonaDisplayInfo persona = new PersonaDisplayInfo();
                persona.SkillsInfo.NumSkills = 1;
                Position pos = new Position { X = 100, Y = 84.5f };
                // Work out where to start displaying stuff so skills can scroll
                bool startFound = false;
                int startIndex = _selectedSkillIndex;
                for(int i = 0; i < 2; i++)
                {
                    if (startIndex - 1 >= 0)
                        startIndex--;
                    else
                    {
                        startFound = true;
                        break;
                    }
                }
                if(!startFound)
                {
                    while (_currentSkills.Count - startIndex < 5 && startIndex != 0)
                        startIndex--;
                }
                // Display the skills (up to 5)
                for (int i = startIndex; i < startIndex + 5; i++)
                {
                    if (i >= _currentSkills.Count) break;
                    persona.SkillsInfo.Skills.Id = (short)_currentSkills[i];
                    persona.SelectedSlot = i == _selectedSkillIndex ? 0 : -1;
                    _ui.RenderSkill(pos, 0, 0xFF, &persona, 0, 0);
                    pos.Y += 17;
                }
            }
        }

        private void ResultsMenuOpening()
        {
            Utils.LogDebug("Opening results menu");
            _currentPersona = null;
            _currentSkills = null;
            _inSkillSelection = false;
        }

        private enum InheritanceState
        {
            NotInMenu,
            ChoosingSkills,
            DoneChoosingSkills,
        }

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