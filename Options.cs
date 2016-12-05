using Ensage.Common.Menu;

namespace VenomancerPRO
{
    internal class Options : Variables
    {
        public static void MenuInit()
        {
            heroName = "npc_dota_hero_venomancer";
            Menu = new Menu(AssemblyName, AssemblyName, true, heroName, true);
            comboKey = new MenuItem("comboKey", "Combo Key").SetValue(new KeyBind(70, KeyBindType.Press)).SetTooltip("Full combo in logical order.");
            harassKey = new MenuItem("harassKey", "Wards Harass Key").SetValue(new KeyBind(68, KeyBindType.Toggle)).SetTooltip("ON = Harass/OFF = CS");
            useBlink = new MenuItem("useBlink", "Use Blink Dagger").SetValue(false).SetTooltip("Will auto blink with combo key held down.");
            soulRing = new MenuItem("soulRing", "Soulring").SetValue(true).SetTooltip("Will use soul ring before combo.");
            bladeMail = new MenuItem("bladeMail", "Check for BladeMail").SetValue(false).SetTooltip("Will now combo if target used blademail.");
            drawTarget = new MenuItem("drawTarget", "Target indicator").SetValue(true).SetTooltip("Shows red circle around your target.");
            moveMode = new MenuItem("moveMode", "Orbwalk").SetValue(true).SetTooltip("Will orbwalk to mouse while combo key is held down.");
            ClosestToMouseRange = new MenuItem("ClosestToMouseRange", "Closest to mouse range").SetValue(new Slider(600, 500, 1200)).SetTooltip("Will look for enemy in selected range around your mouse pointer.");
            nocastulti = new MenuItem("noCastUlti", "Ult will not be cast if % of enemy HP is under: ").SetValue(new Slider(25));
            denyAlly = new MenuItem("denyAlly", "Deny Allies").SetValue(false).SetTooltip("Will deny ally under denyable debuffs.");


            noCastUlti = new Menu("Ultimate", "Ultimate");
            items = new Menu("Items", "Items");
            abilities = new Menu("Abilities", "Abilities");
            targetOptions = new Menu("Target Options", "Target Options");
            wardsOptions = new Menu("Wards Options", "Wards Options");

            Menu.AddItem(comboKey);

            Menu.AddSubMenu(items);
            Menu.AddSubMenu(abilities);
            Menu.AddSubMenu(noCastUlti);
            Menu.AddSubMenu(targetOptions);
            Menu.AddSubMenu(wardsOptions);

            items.AddItem(new MenuItem("items", "Items").SetValue(new AbilityToggler(itemsDictionary)));
            items.AddItem(useBlink);
            items.AddItem(soulRing);
            items.AddItem(bladeMail);
            abilities.AddItem(new MenuItem("abilities", "Abilities").SetValue(new AbilityToggler(abilitiesDictionary)));
            noCastUlti.AddItem(nocastulti);
            targetOptions.AddItem(moveMode);
            targetOptions.AddItem(ClosestToMouseRange);
            targetOptions.AddItem(drawTarget);
            wardsOptions.AddItem(harassKey);
            wardsOptions.AddItem(denyAlly);

            Menu.AddToMainMenu();
        }

    }
}