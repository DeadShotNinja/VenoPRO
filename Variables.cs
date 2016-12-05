using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;

namespace VenomancerPRO
{
    internal class Variables
    {
        public const string AssemblyName = "VenomancerPRO";

        public static string heroName;

        public static string[] modifiersNames =
        {
            "modifier_medusa_stone_gaze_stone",
            "modifier_winter_wyvern_winters_curse",
            "modifier_item_lotus_orb_active"
        };

        public static Dictionary<string, Ability> Abilities;

        public static Dictionary<string, bool> abilitiesDictionary = new Dictionary<string, bool>
        {
            {"venomancer_poison_nova", true},            
            {"venomancer_plague_ward", true},
            {"venomancer_venomous_gale", true}
        };        

        public static Dictionary<string, bool> itemsDictionary = new Dictionary<string, bool>
        {
            {"item_medallion_of_courage", true},
            {"item_rod_of_atos", true},
            {"item_sheepstick", true},                 
            {"item_shivas_guard", true},
            {"item_dagon", true},
            {"item_ethereal_blade", true},
            {"item_orchid", true},
            {"item_bloodthorn", true},
            {"item_veil_of_discord", true}
        };

        public static Menu Menu;

        public static Menu items;
        
        public static Menu abilities;

        public static Menu noCastUlti;

        public static Menu targetOptions;

        public static Menu wardsOptions;

        public static MenuItem comboKey;

        public static MenuItem harassKey;

        public static MenuItem drawTarget;

        public static MenuItem moveMode;
        
        public static MenuItem ClosestToMouseRange;
        
        public static MenuItem soulRing;

        public static MenuItem bladeMail;

        public static MenuItem useBlink;

        public static MenuItem nocastulti;

        public static MenuItem denyAlly;

        public static MenuItem ultimateRadius;

        public static bool loaded, _loaded;

        public static Ability nova, ward, gale;

        public static Item soulring, force_staff, cyclone, orchid, sheep, veil, shivas, dagon, atos, ethereal, medal, bloodthorn, blink;

        public static Hero me, target;

        public static Vector2 iconSize, screenPosition;

        public static Vector3 predictXYZ;

        public static DotaTexture heroIcon;

        public static ParticleEffect circle;

        //public static readonly uint[] PlagueWardDamage = { 13, 22, 31, 40 };
    }
}
