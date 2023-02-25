using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static p3ppc.manualSkillInheritance.Colours;
using static p3ppc.manualSkillInheritance.UI;

namespace p3ppc.manualSkillInheritance
{
    internal static unsafe class Colours
    {
        private static bool* _isFemc;
        internal static void Initialise(bool* isFemc)
        {
            _isFemc = isFemc;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Colour
        {
            internal byte R;
            internal byte G;
            internal byte B;
            internal byte A;
        }

        internal static Colour SelectedBg => *_isFemc ? FemaleColours.SelectedBg : MaleColours.SelectedBg;
        internal static Colour SelectedFg => *_isFemc ? FemaleColours.SelectedFg : MaleColours.SelectedFg;
        internal static Colour AlreadyChosenBg => *_isFemc ? FemaleColours.AlreadyChosenBg : MaleColours.AlreadyChosenBg;
        internal static Colour AlreadyChosenFg => *_isFemc ? FemaleColours.AlreadyChosenFg : MaleColours.AlreadyChosenFg;
        internal static Colour AlreadyChosenSelectedFg => *_isFemc ? FemaleColours.AlreadyChosenSelectedFg : MaleColours.AlreadyChosenSelectedFg;
        internal static Colour SkillFg => *_isFemc ? FemaleColours.SkillFg : MaleColours.SkillFg;
        internal static Colour SkillBg => *_isFemc ? FemaleColours.SkillBg : MaleColours.SkillBg;

    }

    internal static class MaleColours
    {
        internal static readonly Colour SelectedBg = new Colour { R = 201, G = 255, B = 155, A = 255 };
        internal static readonly Colour SelectedFg = new Colour { A = 16, B = 95, G = 0, R = 255 }; // Needs to be reversed for some reason
        internal static readonly Colour AlreadyChosenBg = new Colour { R = 35, G = 97, B = 143, A = 255 };
        internal static readonly Colour AlreadyChosenFg = new Colour { A = 128, B = 188, G = 255, R = 255 }; // Needs to be reversed for some reason
        internal static readonly Colour AlreadyChosenSelectedFg = new Colour { A = 0x10, B = 0x5D, G = 0x00, R = 175 }; // Needs to be reversed for some reason0
        internal static readonly Colour SkillFg = new Colour { R = 0, G = 59, B = 93, A = 255 };
        internal static readonly Colour SkillBg = new Colour { R = 107, G = 177, B = 255, A = 255 };
    }

    internal static class FemaleColours
    {
        internal static readonly Colour SelectedBg = new Colour { R = 201, G = 255, B = 155, A = 255 };
        internal static readonly Colour SelectedFg = new Colour { A = 16, B = 95, G = 0, R = 255 }; // Needs to be reversed for some reason
        internal static readonly Colour AlreadyChosenBg = new Colour { R = 150, G = 58, B = 72, A = 255 };
        internal static readonly Colour AlreadyChosenFg = new Colour { A = 255, B = 191, G = 219, R = 255 }; // Needs to be reversed for some reason
        internal static readonly Colour AlreadyChosenSelectedFg = new Colour { A = 0x10, B = 0x5D, G = 0x00, R = 175 }; // Needs to be reversed for some reason
        internal static readonly Colour SkillFg = new Colour { R = 105, G = 0, B = 9, A = 255 };
        internal static readonly Colour SkillBg = new Colour { R = 255, G = 191, B = 219, A = 255 };
    }
}
