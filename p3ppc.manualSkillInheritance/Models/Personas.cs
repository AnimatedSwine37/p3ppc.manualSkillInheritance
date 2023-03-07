using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static p3ppc.manualSkillInheritance.UI.UI.PersonaMenu;
using static p3ppc.manualSkillInheritance.Models.Skills;

namespace p3ppc.manualSkillInheritance.Models
{
    internal unsafe class Personas
    {
        internal static bool HasSkill(Persona persona, Skill skill)
        {
            var currentSkills = persona.Skills;
            for (int i = 0; i < 8; i++)
            {
                if (currentSkills[i] == (short)skill)
                    return true;
            }
            return false;
        }

        internal static bool HasSkill(PersonaDisplayInfo* persona, Skill skill)
        {
            for (int i = 0; i < 8; i++)
            {
                if ((&persona->SkillsInfo.Skills)[i].Id == (short)skill)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Adds an inherited skill to a Persona
        /// </summary>
        /// <param name="displayPersona">The display persona</param>
        /// <param name="persona">The actual Persona</param>
        /// <param name="skill">The skill to add</param>
        /// <returns>The index of the newly added skill or -1 if none was added</returns>
        internal static int AddInheritedSkill(PersonaDisplayInfo* displayPersona, Persona* persona, Skill skill, Dictionary<Skill, short> removedNextSkills)
        {
            var currentSkills = persona->Skills;
            int emptySkillIndex = -1;
            for (int i = 0; i < 8; i++)
            {
                if (currentSkills[i] == (short)skill)
                {
                    emptySkillIndex = -1;
                    break;
                }
                else if (currentSkills[i] == (short)Skill.None)
                {
                    emptySkillIndex = i;
                    break;
                }
            }
            if (emptySkillIndex != -1)
            {
                (&displayPersona->SkillsInfo.Skills)[emptySkillIndex].Id = (short)skill;
                persona->Skills[emptySkillIndex] = (short)skill;
                persona->InheritedSkills[emptySkillIndex] = (short)skill;
                for (int i = 0; i < 32; i++)
                {
                    var nextSkill = (&displayPersona->SkillsInfo.NextSkills)[i];
                    if (nextSkill.Id == (short)skill)
                    {
                        removedNextSkills.Add(skill, displayPersona->SkillsInfo.NextSkillsLevels[i]);
                        RemoveNextSkill(&displayPersona->SkillsInfo, i);
                    }
                }
                Utils.LogDebug($"Added {skill} to {persona->Id}");
            }
            else
            {
                Utils.LogDebug($"Cannot add {skill} to {persona->Id}");
            }
            return emptySkillIndex;
        }

        private static void AddNextSkill(PersonaSkillsDisplayInfo* skillsInfo, Skill skill, short level)
        {
            short lastLevel = level;
            short lastSkill = (short)skill;
            var numNextSkills = skillsInfo->NumNextSkills;
            for (int i = 0; i < numNextSkills; i++)
            {
                if (skillsInfo->NextSkillsLevels[i] >= lastLevel)
                {
                    short tempLevel = skillsInfo->NextSkillsLevels[i];
                    short tempSkill = (&skillsInfo->NextSkills)[i].Id;
                    Utils.LogDebug($"Set skill at index {i} to {(Skill)lastSkill}");
                    skillsInfo->NextSkillsLevels[i] = lastLevel;
                    (&skillsInfo->NextSkills)[i].Id = lastSkill;
                    lastLevel = tempLevel;
                    lastSkill = tempSkill;
                }
            }
            Utils.LogDebug($"Set skill at index {numNextSkills} to {(Skill)lastSkill}");
            skillsInfo->NextSkillsLevels[numNextSkills] = lastLevel;
            (&skillsInfo->NextSkills)[numNextSkills].Id = lastSkill;
            skillsInfo->NumNextSkills++;
        }

        private static void RemoveNextSkill(PersonaSkillsDisplayInfo* skillsInfo, int index)
        {
            Utils.LogDebug($"Removing next skill at index {index}");
            for (int i = index; i < skillsInfo->NumNextSkills; i++)
            {
                Utils.LogDebug($"Set skill at index {i} to {(Skill)(&skillsInfo->NextSkills)[i + 1].Id}");
                skillsInfo->NextSkillsLevels[i] = skillsInfo->NextSkillsLevels[i + 1];
                (&skillsInfo->NextSkills)[i].Id = (&skillsInfo->NextSkills)[i + 1].Id;
            }
            if (skillsInfo->NumNextSkills == 32)
            {
                skillsInfo->NextSkillsLevels[31] = 0;
                (&skillsInfo->NextSkills)[31].Id = 0;
            }
            skillsInfo->NumNextSkills--;
        }


        /// <summary>
        /// Removes the last skill a Persona inherited
        /// </summary>
        /// <param name="displayPersona">The display persona</param>
        /// <param name="persona">The actual Persona</param>
        /// <returns>The Skill that was removed or Skill.None if none was removed (they haven't inherited anything yet)</returns>
        internal static Skill RemoveLastInheritedSkill(PersonaDisplayInfo* displayPersona, Persona* persona, Dictionary<Skill, short> removedNextSkills)
        {
            var mask = displayPersona->SkillsInfo.NewSkillsMask;
            for (int i = 7; i >= 0; i--)
            {
                if ((mask & 1 << i) != 0 && persona->Skills[i] > 0)
                {
                    Skill removedSkill = (Skill)persona->Skills[i];
                    Utils.LogDebug($"Removing {removedSkill} from {persona->Id}");
                    persona->Skills[i] = -1;
                    (&displayPersona->SkillsInfo.Skills)[i].Id = -1;
                    persona->Skills[i] = -1;
                    if (removedNextSkills.ContainsKey(removedSkill))
                    {
                        var nextSkilllLevel = removedNextSkills[removedSkill];
                        removedNextSkills.Remove(removedSkill);
                        AddNextSkill(&displayPersona->SkillsInfo, removedSkill, nextSkilllLevel);

                    }
                    persona->InheritedSkills[i] = -1;
                    return removedSkill;
                }
            }
            return Skill.None;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct Persona
        {
            [FieldOffset(0)]
            internal bool IsRegistered;

            [FieldOffset(2)]
            internal PersonaId Id;

            [FieldOffset(4)]
            internal short Level;

            [FieldOffset(8)]
            internal int Exp;

            [FieldOffset(12)]
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U2)]
            //internal Skill[] Skils;
            internal fixed short Skills[8];

            [FieldOffset(28)]
            internal byte Strength;

            [FieldOffset(29)]
            internal byte Magic;

            [FieldOffset(30)]
            internal byte Endurance;

            [FieldOffset(31)]
            internal byte Agility;

            [FieldOffset(32)]
            internal byte Luck;

            [FieldOffset(58)]
            internal fixed short InheritedSkills[8];
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct FusionPersona
        {
            [FieldOffset(4)]
            internal Persona Persona;
        }

        public enum PersonaId : short
        {
            Orpheus = 001,
            Susanoo = 002,
            Flauros = 003,
            Loki = 004,
            Nekomata = 005,
            Pyro_Jack = 006,
            Jack_Frost = 007,
            Scathach = 008,
            Rangda = 009,
            Nata_Taishi = 010,
            Cu_Chulainn = 011,
            Ose = 012,
            Kusi_Mitama = 013,
            Apsaras = 014,
            Laksmi = 015,
            Parvati = 016,
            KikuriHime = 017,
            Sati = 018,
            Sarasvati = 019,
            Unicorn = 020,
            Cybele = 021,
            Skadi = 022,
            Hariti = 023,
            Kali = 024,
            Ganga = 025,
            Taraka = 026,
            Lamia = 027,
            Odin = 028,
            King_Frost = 029,
            Oukuninushi = 030,
            Kingu = 031,
            Naga_Raja = 032,
            Forneus = 033,
            Kohryu = 034,
            Mithra = 035,
            Daisoujou = 036,
            Ananta = 037,
            Omoikane = 038,
            Principality = 039,
            Raphael = 040,
            Titania = 041,
            Oberon = 042,
            Narcissus = 043,
            Queen_Mab = 044,
            Leanan_Sidhe = 045,
            Pixie = 046,
            Uriel = 047,
            Surt = 048,
            Throne = 049,
            Ares = 050,
            Titan = 051,
            Chimera = 052,
            Ara_Mitama = 053,
            Valkyrie = 054,
            Melchizedek = 055,
            Dominion = 056,
            Siegfried = 057,
            Virtue = 058,
            Power = 059,
            Archangel = 060,
            Angel = 061,
            Alilat = 062,
            Arahabaki = 063,
            Nebiros = 064,
            Decarabia = 065,
            Kurama_Tengu = 066,
            Yomotsu_Shikome = 067,
            Naga = 068,
            Norn = 069,
            Atropos = 070,
            Orobas = 071,
            Lachesis = 072,
            Saki_Mitama = 073,
            Eligor = 074,
            Clotho = 075,
            Fortuna = 076,
            Thor = 077,
            Bishamonten = 078,
            TakeMikazuchi = 079,
            Jikokuten = 080,
            Hanuman = 081,
            Koumokuten = 082,
            Zouchouten = 083,
            Attis = 084,
            Vasuki = 085,
            Orthrus = 086,
            TakeMinakata = 087,
            Ubelluris = 088,
            Inugami = 089,
            Thanatos = 090,
            Alice = 091,
            Seth = 092,
            Mot = 093,
            Samael = 094,
            Vetala = 095,
            Loa = 096,
            Pale_Rider = 097,
            Michael = 098,
            Byakko = 099,
            Suzaku = 100,
            Seiryuu = 101,
            Nigi_Mitama = 102,
            Genbu = 103,
            Beelzebub = 104,
            Mother_Harlot = 105,
            Abaddon = 106,
            Lilith = 107,
            Incubus = 108,
            Succubus = 109,
            Lilim = 110,
            Chi_You = 111,
            Shiva = 112,
            Masakado = 113,
            Seiten_Taisei = 114,
            Yamatanoorochi = 115,
            Oumitsunu = 116,
            Helel = 117,
            Sandalphon = 118,
            Black_Frost = 119,
            Garuda = 120,
            Kaiwan = 121,
            Ganesha = 122,
            Nandi = 123,
            Chernobog = 124,
            Dionysus = 125,
            Narasimha = 126,
            Girimehkala = 127,
            Gurr = 128,
            Legion = 129,
            Berith = 130,
            Saturnus = 131,
            Vishnu = 132,
            Barong = 133,
            Jatayu = 134,
            Horus = 135,
            Quetzalcoatl = 136,
            Yatagarasu = 137,
            Messiah = 138,
            Asura = 139,
            Metatron = 140,
            Satan = 141,
            Gabriel = 142,
            Hokuto_Seikun = 143,
            Trumpeter = 144,
            Anubis = 145,
            Slime = 146,
            Hua_Po = 147,
            High_Pixie = 148,
            Yaksini = 149,
            Shiisaa = 150,
            Thoth = 151,
            Alp = 152,
            Mothman = 153,
            Kumbhanda = 154,
            Empusa = 155,
            Rakshasa = 156,
            Hecatoncheires = 157,
            Hell_Biker = 158,
            Ghoul = 159,
            Yurlungur = 160,
            Pazuzu = 161,
            Mara = 162,
            Kartikeya = 163,
            Baal_Zebul = 164,
            Suparna = 165,
            Lucifer = 166,
            Nidhoggr = 167,
            Atavaka = 168,
            Orpheus_Telos = 169,
            Mokoi = 170,
            Neko_Shogun = 171,
            Setanta = 172,
            Tam_Lin = 173,
            Orpheus0 = 174,
            Universe = 191,
            Io = 192,
            Isis = 193,
            Palladion = 194,
            Athena = 195,
            Penthesilea = 196,
            Artemisia = 197,
            Hermes = 198,
            Trismegistus = 199,
            Lucia = 200,
            Juno = 201,
            Polydeuces = 202,
            Caesar = 203,
            Nemesis = 204,
            KalaNemi = 205,
            Castor = 206,
            Cerberus = 207,
            Hypnos = 208,
            Moros = 209,
            Medea = 210,
            Psyche = 211,
        }
    }
}
