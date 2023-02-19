using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace p3ppc.manualSkillInheritance
{
    internal unsafe class PersonaMenu
    {
        [StructLayout(LayoutKind.Explicit)]
        internal struct PersonaMenuInfo
        {
            [FieldOffset(160)]
            internal PersonaDisplayInfo Persona;
        }
        
        [StructLayout(LayoutKind.Explicit)]
        internal struct PersonaDisplayInfo
        {
            [FieldOffset(8)]
            internal PersonaSkillsDisplayInfo SkillsInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PersonaSkillsDisplayInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal PersonaDisplaySkill Skills;

            internal short NumSkills;

            internal short NewSkillsMask;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            internal PersonaDisplaySkill NextSkills;

            internal fixed short NextSkillsLevels[32];

            internal short NumNextLevels;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PersonaDisplaySkill
        {
            short unk;

            internal short Id;
            
            fixed byte unk2[8];
        }
    }
}
