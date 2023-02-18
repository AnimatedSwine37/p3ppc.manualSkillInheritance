﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p3ppc.manualSkillInheritance
{
    internal class Skills
    {
        public enum Skill : int
        {
            Slash_Attack = 0000,
            Agi = 0001,
            Agilao = 0002,
            Agidyne = 0003,
            Maragi = 0004,
            Maragilao = 0005,
            Maragidyne = 0006,
            Maralagidyne = 0007,
            Ragnarok = 0009,
            Garu = 0010,
            Garula = 0011,
            Garudyne = 0012,
            Magaru = 0013,
            Magarula = 0014,
            Magarudyne = 0015,
            Panta_Rhei = 0018,
            Bufu = 0019,
            Bufula = 0020,
            Bufudyne = 0021,
            Mabufu = 0022,
            Mabufula = 0023,
            Mabufudyne = 0024,
            Niflheim = 0027,
            Zio = 0028,
            Zionga = 0029,
            Ziodyne = 0030,
            Mazio = 0031,
            Mazionga = 0032,
            Maziodyne = 0033,
            Thunder_Reign = 0036,
            Megido = 0037,
            Megidola = 0038,
            Megidolaon = 0039,
            Last_Resort = 0040,
            Black_Viper = 0043,
            Morning_Star = 0044,
            Hama = 0045,
            Mahama = 0046,
            Hamaon = 0047,
            Mahamaon = 0048,
            Samsara = 0050,
            Mudo = 0051,
            Mamudo = 0052,
            Mudoon = 0053,
            Mamudoon = 0054,
            Die_for_Me = 0056,
            Marin_Karin = 0057,
            Sexy_Dance = 0058,
            Bewilder = 0059,
            Eerie_Sound = 0060,
            Pulinpa = 0061,
            Tentarafoo = 0062,
            Evil_Touch = 0063,
            Evil_Smile = 0064,
            Ghastly_Wail = 0065,
            Provoke = 0066,
            Infuriate = 0067,
            Poisma = 0068,
            Poison_Mist = 0069,
            Virus_Breath = 0070,
            Foul_Breath = 0073,
            Stagnant_Air = 0074,
            Life_Drain = 0075,
            Spirit_Drain = 0076,
            Maragidyne0 = 0080,
            Magarudyne0 = 0081,
            Mabufudyne0 = 0082,
            Maziodyne0 = 0083,
            Megidolaon0 = 0084,
            Mahamaon0 = 0085,
            Mamudoon0 = 0086,
            Sexy_Dance0 = 0087,
            Eerie_Sound0 = 0088,
            Tentarafoo0 = 0089,
            Evil_Smile0 = 0090,
            Poison_Mist0 = 0091,
            Holy_Arrow = 0092,
            Life_Drain0 = 0093,
            Spirit_Drain0 = 0094,
            Death = 0096,
            Yell = 0097,
            Yell0 = 0098,
            Death0 = 0099,
            Death1 = 0100,
            Death2 = 0101,
            Moonless_Gown = 0102,
            Moonless_Gown0 = 0103,
            Summon = 0104,
            Slash_Attack0 = 0105,
            Pierce_Attack = 0106,
            Strike_Attack = 0107,
            Yell1 = 0108,
            Pierce_Attack0 = 0109,
            Fire_Attack = 0110,
            Almighty_Attack = 0111,
            Bash = 0112,
            Assault_Dive = 0113,
            Kill_Rush = 0114,
            Swift_Strike = 0115,
            Sonic_Punch = 0116,
            Heat_Wave = 0117,
            Gigantic_Fist = 0118,
            Akasha_Arts = 0119,
            Gods_Hand = 0120,
            Cleave = 0121,
            Fatal_End = 0122,
            Getsuei = 0123,
            Zanei = 0124,
            Mighty_Swing = 0125,
            Double_Fangs = 0126,
            Blade_of_Fury = 0127,
            Deathbound = 0128,
            Tempest_Slash = 0129,
            Heavens_Blade = 0130,
            Pralaya = 0131,
            Power_Slash = 0132,
            Gale_Slash = 0133,
            Brave_Blade = 0134,
            Herculean_Strike = 0135,
            Vicious_Strike = 0136,
            Single_Shot = 0137,
            Twin_Shot = 0138,
            Holy_Arrow0 = 0139,
            Torrent_Shot = 0140,
            Arrow_Rain = 0141,
            Myriad_Arrows = 0142,
            Primal_Force = 0143,
            Vorpal_Blade = 0144,
            Weary_Thrust = 0145,
            Cruel_Attack = 0146,
            Vile_Assault = 0147,
            Poison_Arrow = 0148,
            Weary_Thrust0 = 0149,
            Vorpal_Blade0 = 0150,
            Junpei_CoOp = 0151,
            Yukari_CoOp = 0152,
            Akihiko_CoOp = 0153,
            Mitsuru_CoOp = 0154,
            Aigis_CoOp = 0155,
            Ken_CoOp = 0156,
            Shinjiro_CoOp = 0157,
            Koromaru_CoOp = 0158,
            pluscharm_low = 0160,
            pluscharm_med = 0161,
            pluscharm_high = 0162,
            plusdistress_low = 0163,
            plusdistress_med = 0164,
            plusdistress_high = 0165,
            pluspanic_low = 0166,
            pluspanic_med = 0167,
            pluspanic_high = 0168,
            plusfear_low = 0169,
            plusfear_med = 0170,
            plusfear_high = 0171,
            plusrage_low = 0172,
            plusrage_med = 0173,
            plusrage_high = 0174,
            pluspoison_low = 0175,
            Dia = 0192,
            Diarama = 0193,
            Diarahan = 0194,
            Media = 0195,
            Mediarama = 0196,
            Mediarahan = 0197,
            Salvation = 0198,
            Patra = 0199,
            Me_Patra = 0200,
            Re_Patra = 0201,
            Posumudi = 0202,
            Amrita = 0203,
            Recarm = 0204,
            Samarecarm = 0205,
            Tarunda = 0206,
            Matarunda = 0207,
            Sukunda = 0208,
            Masukunda = 0209,
            Rakunda = 0210,
            Marakunda = 0211,
            Dekunda = 0212,
            Tarukaja = 0214,
            Matarukaja = 0215,
            Sukukaja = 0216,
            Masukukaja = 0217,
            Rakukaja = 0218,
            Marakukaja = 0219,
            Power_Charge = 0220,
            Mind_Charge = 0221,
            Dekaja = 0223,
            Tetrakarn = 0224,
            Makarakarn = 0225,
            Rebellion = 0226,
            Revolution = 0227,
            Fire_Break = 0229,
            Ice_Break = 0230,
            Wind_Break = 0231,
            Elec_Break = 0232,
            Traesto = 0233,
            Trafuri = 0234,
            Healing_Wave = 0235,
            Recarmdra = 0236,
            Charmdi = 0237,
            Enradi = 0238,
            Junpei_CoOp0 = 0239,
            Yukari_CoOp0 = 0240,
            Akihiko_CoOp0 = 0241,
            Mitsuru_CoOp0 = 0242,
            Aigis_CoOp0 = 0243,
            Ken_CoOp0 = 0244,
            Shinjiro_CoOp0 = 0245,
            Koromaru_CoOp0 = 0246,
            Aigis_CoOp1 = 0247,
            Aigis_CoOp2 = 0248,
            Aigis_CoOp3 = 0249,
            Aigis_CoOp4 = 0250,
            Summer_Dream = 0256,
            Summer_Dream0 = 0257,
            Summer_Dream1 = 0258,
            Summer_Dream2 = 0259,
            Summer_Dream3 = 0260,
            Summer_Dream4 = 0261,
            Summer_Dream5 = 0262,
            Summer_Dream6 = 0263,
            Summer_Dream7 = 0264,
            Summer_Dream8 = 0265,
            Summer_Dream9 = 0266,
            Summer_Dream10 = 0267,
            Summer_Dream11 = 0268,
            Summer_Dream12 = 0269,
            Summer_Dream13 = 0270,
            Summer_Dream14 = 0271,
            Jack_Brothers = 0272,
            Ardhanari = 0273,
            Trickster = 0274,
            Infinity = 0275,
            Valhalla = 0276,
            Summer_Dream15 = 0277,
            Armageddon = 0278,
            Cadenza = 0279,
            Scarlet_Havoc = 0280,
            Frolic = 0281,
            Dreamfest = 0282,
            King_and_I = 0283,
            Best_Friends = 0284,
            Shadow_Hound = 0285,
            Thunder_Call = 0286,
            Last_Judge = 0287,
            Raktapaksha = 0288,
            Justice = 0289,
            Trickster0 = 0290,
            Primal_Darkness = 0293,
            Dark_Embrace = 0294,
            Primal_Darkness0 = 0295,
            Primal_Darkness1 = 0296,
            Heartbreaker = 0297,
            Pierce_Attack1 = 0298,
            Pierce_Attack2 = 0299,
            Giga_Spark = 0300,
            Sacrifice = 0301,
            Pierce_Attack3 = 0304,
            Fire_Attack0 = 0305,
            Summon0 = 0306,
            Summon1 = 0307,
            ArmedReady = 0308,
            Sacrifice0 = 0309,
            Arcana_Shift = 0310,
            Arcana_Shift0 = 0311,
            Arcana_Shift1 = 0312,
            Arcana_Shift2 = 0313,
            Arcana_Shift3 = 0314,
            Arcana_Shift4 = 0315,
            Arcana_Shift5 = 0316,
            Arcana_Shift6 = 0317,
            Arcana_Shift7 = 0318,
            Night_Queen = 0319,
            AllOut_2 = 0320,
            AllOut_3 = 0321,
            AllOut_4 = 0322,
            Plume_of_Dusk = 0323,
            Analyze = 0324,
            Oracle = 0325,
            Full_Analysis = 0326,
            Oracle0 = 0327,
            Oracle1 = 0328,
            Oracle2 = 0329,
            Oracle3 = 0330,
            Oracle4 = 0331,
            AllOut_2L = 0332,
            AllOut_3L = 0333,
            AllOut_4L = 0334,
            Support_Scan = 0335,
            Third_Eye = 0336,
            Mind_DJ = 0337,
            DJ_Queen = 0338,
            Escape_Route = 0339,
            Healing_Wave0 = 0340,
            Summon2 = 0341,
            Orgia_Mode = 0342,
            Great_Seal = 0343,
            Great_Seal0 = 0344,
            Ice_Attack = 0346,
            Wind_Attack = 0347,
            Elec_Attack = 0348,
            Fire_Attack1 = 0349,
            Pierce_Attack4 = 0350,
            Strike_Attack0 = 0351,
            Summon3 = 0352,
            Summon4 = 0353,
            Summon5 = 0354,
            Prophecy_of_Ruin = 0355,
            Heartbreaker0 = 0356,
            Unite = 0357,
            Separate = 0358,
            Pierce_Attack5 = 0359,
            Pierce_Attack6 = 0360,
            Samarecarm0 = 0361,
            Samarecarm1 = 0362,
            Charge = 0363,
            Giga_Spark0 = 0364,
            Wheel_of_Fortune = 0365,
            Wheel_of_Fortune0 = 0366,
            Wheel_of_Fortune1 = 0367,
            Paradigm_Shift = 0368,
            Wheel_of_Fortune2 = 0369,
            Wheel_of_Fortune3 = 0370,
            E_Attack_Up = 0371,
            P_Attack_Up = 0372,
            E_Attack_Down = 0373,
            P_Attack_Down = 0374,
            E_Defense_Up = 0375,
            P_Defense_Up = 0376,
            E_Defense_Down = 0377,
            P_Defense_Down = 0378,
            E_Panic = 0379,
            P_Panic = 0380,
            E_Rage = 0381,
            P_Rage = 0382,
            E_Distress = 0383,
            P_Distress = 0384,
            E_Fear = 0385,
            P_Fear = 0386,
            E_Damage_Low = 0387,
            P_Damage_Low = 0388,
            E_Damage_Mid = 0389,
            P_Damage_Mid = 0390,
            E_Damage_High = 0391,
            P_Damage_High = 0392,
            E_Heal = 0393,
            P_Heal = 0394,
            Almighty_Attack0 = 0395,
            Arcana_Shift8 = 0396,
            Arcana_Shift9 = 0397,
            Arcana_Shift10 = 0398,
            Arcana_Shift11 = 0399,
            Band_Aid = 0400,
            Medicine = 0401,
            Bead = 0402,
            Snuff_Soul = 0403,
            Chewing_Soul = 0404,
            Precious_Egg = 0405,
            Soma = 0406,
            Muscle_Drink = 0411,
            Odd_Morsel = 0412,
            Rancid_Gravy = 0413,
            Powerful_Drug = 0414,
            Cold_Medicine = 0415,
            Energy_Drink = 0416,
            Homunculus = 0438,
            Plume_of_Dusk0 = 0439,
            Resist_Slash = 0464,
            Null_Slash = 0465,
            Repel_Slash = 0466,
            Absorb_Slash = 0467,
            Resist_Strike = 0468,
            Null_Strike = 0469,
            Repel_Strike = 0470,
            Absorb_Strike = 0471,
            Resist_Pierce = 0472,
            Null_Pierce = 0473,
            Repel_Pierce = 0474,
            Absorb_Pierce = 0475,
            Resist_Fire = 0476,
            Null_Fire = 0477,
            Repel_Fire = 0478,
            Absorb_Fire = 0479,
            Resist_Ice = 0480,
            Null_Ice = 0481,
            Repel_Ice = 0482,
            Absorb_Ice = 0483,
            Resist_Elec = 0484,
            Null_Elec = 0485,
            Repel_Elec = 0486,
            Absorb_Elec = 0487,
            Resist_Wind = 0488,
            Null_Wind = 0489,
            Repel_Wind = 0490,
            Absorb_Wind = 0491,
            Resist_Light = 0492,
            Null_Light = 0493,
            Repel_Light = 0494,
            Resist_Dark = 0495,
            Null_Dark = 0496,
            Repel_Dark = 0497,
            Null_Charm = 0498,
            Null_Distress = 0499,
            Null_Panic = 0500,
            Null_Fear = 0501,
            Null_Rage = 0502,
            Null_Freeze = 0503,
            Null_Shock = 0504,
            Null_Poison = 0505,
            Unshaken_Will = 0506,
            Masakados = 0507,
            Dodge_Slash = 0508,
            Evade_Slash = 0509,
            Dodge_Strike = 0510,
            Evade_Strike = 0511,
            Dodge_Pierce = 0512,
            Evade_Pierce = 0513,
            Dodge_Fire = 0514,
            Evade_Fire = 0515,
            Dodge_Ice = 0516,
            Evade_Ice = 0517,
            Dodge_Wind = 0518,
            Evade_Wind = 0519,
            Dodge_Elec = 0520,
            Evade_Elec = 0521,
            Stamina_Up_1 = 0522,
            Stamina_Up_2 = 0523,
            Stamina_Up_3 = 0524,
            Premonition = 0526,
            Angelic_Grace = 0527,
            Fire_Boost = 0528,
            Fire_Amp = 0529,
            Ice_Boost = 0530,
            Ice_Amp = 0531,
            Elec_Boost = 0532,
            Elec_Amp = 0533,
            Wind_Boost = 0534,
            Wind_Amp = 0535,
            Fast_Retreat = 0536,
            HP_Up_1 = 0537,
            HP_Up_2 = 0538,
            HP_Up_3 = 0539,
            SP_Up_1 = 0540,
            SP_Up_2 = 0541,
            SP_Up_3 = 0542,
            Raging_Tiger = 0543,
            Counter = 0544,
            Counterstrike = 0545,
            High_Counter = 0546,
            Regenerate_1 = 0547,
            Regenerate_2 = 0548,
            Regenerate_3 = 0549,
            Invigorate_1 = 0550,
            Invigorate_2 = 0551,
            Invigorate_3 = 0552,
            Growth_1 = 0553,
            Growth_2 = 0554,
            Growth_3 = 0555,
            AutoTarukaja = 0557,
            AutoRakukaja = 0558,
            AutoSukukaja = 0559,
            Alertness = 0560,
            Sharp_Student = 0561,
            Apt_Pupil = 0562,
            Ali_Dance = 0563,
            Firm_Stance = 0564,
            Spell_Master = 0565,
            Arms_Master = 0566,
            HP_Up = 0567,
            SP_Up = 0568,
            Divine_Grace = 0570,
            Endure = 0571,
            Enduring_Soul = 0572,
            Heavy_Master = 0573,
            Magic_Skill_Up = 0574,
            Phys_Skill_Up = 0575,
            Rosary = 0580,
            Prayer_Beads = 0581,
            Spear_Master = 0582,
            Bow_Master = 0583,
            _1hSwd_Master = 0584,
            _2hSwd_Master = 0585,
            Fist_Master = 0586,
            Survive_Light = 0587,
            Survive_Dark = 0588,
            AutoMaraku = 0589,
            AutoMataru = 0590,
            AutoMasuku = 0591,
            Charm_Boost = 0592,
            Poison_Boost = 0593,
            Distress_Boost = 0594,
            Panic_Boost = 0595,
            Fear_Boost = 0596,
            Rage_Boost = 0597,
            Ailment_Boost = 0598,
            Hama_Boost = 0599,
            Mudo_Boost = 0600,
            Endure_Light = 0601,
            Endure_Dark = 0602,
            Weapons_Master = 0603,
            Cool_Breeze = 0605,
            Victory_Cry = 0606,
            Spring_of_Life = 0607,
            Spring_of_Life0 = 0608,
            Infinite_Endure = 0609,
            FastHeal = 0610,
            InstaHeal = 0611,
            Resist_Charm = 0612,
            Resist_Distress = 0613,
            Resist_Panic = 0614,
            Resist_Fear = 0615,
            Resist_Rage = 0616,
            Resist_Freeze = 0617,
            Resist_Shock = 0618,
            Resist_Poison = 0619,
            HP_Up_High = 0620,
            SP_Up_High = 0621,
            High_Endure = 0622,
            Blank = 0623,
        }
    }
}
