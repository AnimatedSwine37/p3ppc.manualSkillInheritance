using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static p3ppc.manualSkillInheritance.Models.Personas;

namespace p3ppc.manualSkillInheritance.UI.UI
{
    internal unsafe class FusionMenu
    {
        [StructLayout(LayoutKind.Explicit)]
        internal struct FusionMenuInfo
        {
            [FieldOffset(210)]
            internal FusionMenuState State;

            [FieldOffset(212)]
            internal FusionMenuState LastState;

            [FieldOffset(216)]
            internal byte SelectionContextId;

            [FieldOffset(254)]
            internal short NumMenus;

            [FieldOffset(256)]
            internal short SelectedMenu;

            [FieldOffset(4752)]
            internal FusionPersona* ResultPersona;
        }

        internal enum FusionMenuState : short
        {
            MainMenu = 4,
            PersonaSelection = 6,
            PersonaSelectionLoading = 7,
            PersonaStatus = 9,
            FusionResultStatus = 0xA,
            FusionResultConfirm = 0xB,
            FusionResultLevelTooHigh = 0xC,
            FusionAnimation = 0x11,
            FusionResultGreeting = 0x15,
            FusionSLGainMessage = 0x16,
            FusionSLGainAnimation = 0x17,
            FusionSLAboutToLvlUp = 0x18,
            FusionSLPowerStart = 0x19,
            FusionSLPowerExpAnimation = 0x1b,
            FusionSLowerLvlUp = 0x1C,
            FusionSLPowerDone = 0x1D,
            FusionSlPowerDone2 = 0x1E,
            FusionPersonaEmerged = 0x20,
            TalkSelection = 0x22,
            TalkMessage = 0x23,
            FusionSkillSelection = 0x69,
        }

    }
}
