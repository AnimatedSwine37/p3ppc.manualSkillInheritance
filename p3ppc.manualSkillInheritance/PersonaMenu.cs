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

        [StructLayout(LayoutKind.Explicit)]
        internal struct PersonaSkillsDisplayInfo
        {
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            [FieldOffset(0)]
            internal PersonaDisplaySkill Skills;

            [FieldOffset(96)]
            internal short NumSkills;

            [FieldOffset(98)]
            internal short NewSkillsMask;

            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            [FieldOffset(100)]
            internal PersonaDisplaySkill NextSkills;

            [FieldOffset(484)]
            internal fixed short NextSkillsLevels[32];

            [FieldOffset(548)]
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
