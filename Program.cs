using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace VenoPRO
{
    internal class Program
    {
        private static bool _loaded;
        private static readonly Menu Menu = new Menu("VenoPRO", "venomancer", true, "npc_dota_hero_venomancer", true);
        private static readonly uint[] PlagueWardDamage = { 10, 19, 29, 38 };
        private static Hero _globalTarget;

        private static readonly List<string> Items = new List<string>
        {
            "item_silver_edge",
            "item_mjollnir",
            "item_veil_of_discord",
            "item_blink",
            "item_orchid",
            "item_sheepstick"
        };

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            var dict = new Dictionary<string, bool>
            {
                {Items[0],true},
                {Items[1],true},
                {Items[2],true},
                {Items[3],true},
                {Items[4],true},
                {Items[5],true}
            };
            Menu.AddItem(new MenuItem("hotkey", "Hotkey").SetValue(new KeyBind('G', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("Items", "Items:").SetValue(new AbilityToggler(dict)));
            Menu.AddItem(new MenuItem("minHp", "Min Hp %").SetValue(new Slider(15)));
            Menu.AddItem(new MenuItem("LockTarget", "Lock Target").SetValue(true));

            Menu.AddToMainMenu();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!_loaded) return;

            if (_globalTarget == null || !_globalTarget.IsAlive) return;
            var pos = Drawing.WorldToScreen(_globalTarget.Position);
            Drawing.DrawText("Target", pos, new Vector2(0, 50), Color.Green, FontFlags.AntiAlias | FontFlags.DropShadow);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            var me = ObjectManager.LocalHero;

            if (!_loaded)
            {
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                _loaded = true;
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Menu.DisplayName + " By EditedMultiCode" +
                    " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version, MessageType.LogMessage);
            }

            // For Combo
            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                return;
            }
            if (!me.IsAlive || Game.IsPaused) return;

            if (!Menu.Item("hotkey").GetValue<KeyBind>().Active)
            {
                _globalTarget = null;
                return;
            }

            if (_globalTarget == null || !_globalTarget.IsValid || !Menu.Item("LockTarget").GetValue<bool>())
            {
                _globalTarget = ClosestToMouse(me, 300);
            }
            if (_globalTarget == null || !_globalTarget.IsValid || !_globalTarget.IsAlive || !me.CanCast()) return;

            DoCombo(me, _globalTarget);

            // Veno ward controll
            if (!Game.IsInGame || !Utils.SleepCheck("VenoPRO"))
                return;
            Utils.Sleep(125, "VenoPRO");

            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Venomancer)
            {
                _loaded = false;
                return;
            }

            var plagueWardLevel = me.FindSpell("venomancer_plague_ward").Level - 1;

            var enemies = ObjectManager.GetEntities<Hero>().Where(hero => hero.IsAlive && !hero.IsIllusion && hero.IsVisible && hero.Team == me.GetEnemyTeam()).ToList();
            var creeps = ObjectManager.GetEntities<Creep>().Where(creep => (creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane || creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege) && creep.IsAlive && creep.IsVisible && creep.IsSpawned).ToList();
            var plaguewards = ObjectManager.GetEntities<Unit>().Where(unit => unit.ClassID == ClassID.CDOTA_BaseNPC_Venomancer_PlagueWard && unit.IsAlive && unit.IsVisible).ToList();

            if (!enemies.Any() || !creeps.Any() || !plaguewards.Any() || !(plagueWardLevel > 0))
                return;

            foreach (var enemy in enemies)
            {
                if (enemy.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_venomancer_poison_sting_ward") == null && enemy.Health > 0)
                {
                    foreach (var plagueward in plaguewards)
                    {
                        if (GetDistance2D(enemy.Position, plagueward.Position) < plagueward.AttackRange && Utils.SleepCheck(plagueward.Handle.ToString()))
                        {
                            plagueward.Attack(enemy);
                            Utils.Sleep(1000, plagueward.Handle.ToString());
                        }
                    }
                }
            }

            foreach (var creep in creeps)
            {
                if (creep.Team == me.GetEnemyTeam() && creep.Health > 0 && creep.Health < (PlagueWardDamage[plagueWardLevel] * (1 - creep.DamageResist) + 20))
                    foreach (var plagueward in plaguewards)
                    {
                        if (GetDistance2D(creep.Position, plagueward.Position) < plagueward.AttackRange && Utils.SleepCheck(plagueward.Handle.ToString()))
                        {
                            plagueward.Attack(creep);
                            Utils.Sleep(1000, plagueward.Handle.ToString());
                        }
                    }
                else if (creep.Team == me.Team && creep.Health > (PlagueWardDamage[plagueWardLevel] * (1 - creep.DamageResist)) && creep.Health < (PlagueWardDamage[plagueWardLevel] * (1 - creep.DamageResist) + 88))
                    foreach (var plagueward in plaguewards)
                    {
                        if (GetDistance2D(creep.Position, plagueward.Position) < plagueward.AttackRange && Utils.SleepCheck(plagueward.Handle.ToString()))
                        {
                            plagueward.Attack(creep);
                            Utils.Sleep(1000, plagueward.Handle.ToString());
                        }
                    }
            }
        }

        private static void DoCombo(Hero me, Hero target)
        {
            var venomousGale = me.Spellbook.SpellQ;
            var plagueWard = me.Spellbook.SpellE;
            var poisonNova = me.Spellbook.SpellR;
            var distance = me.Distance2D(target);

            var inSb = me.Modifiers.Any(x => x.Name == "modifier_item_silver_edge_windwalk" || x.Name == "modifier_item_invisibility_edge_windwalk");
            if (inSb)
            {
                if (!Utils.SleepCheck("attacking")) return;
                me.Attack(target);
                Utils.Sleep(200, "attacking");
                return;
            }
            if (Utils.SleepCheck("items"))
            {
                /*foreach (var item in me.Inventory.Items)
                {
                    Game.PrintMessage(item.Name+": "+item.AbilityBehavior,MessageType.ChatMessage);
                }*/
                var items =
                    me.Inventory.Items.Where(
                        x =>
                            Items.Contains(x.Name) && x.CanBeCasted() && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(x.Name) &&
                            (x.CastRange == 0 || x.CastRange >= distance) && Utils.SleepCheck(x.Name)).ToList();
                foreach (var item in items)
                {
                    switch (item.ClassID)
                    {
                        case ClassID.CDOTA_Item_BlinkDagger:
                            var p = Prediction.InFront(target, 100);
                            var dist = me.Distance2D(p);
                            if (dist <= 1150 && dist >= 400 && poisonNova != null && poisonNova.CanBeCasted())
                            {
                                item.UseAbility(p);
                                Utils.Sleep(200, item.Name);
                            }
                            break;
                        default:
                            if (item.IsAbilityBehavior(AbilityBehavior.UnitTarget))
                            {
                                if (!target.IsStunned() && !target.IsHexed())
                                {
                                    item.UseAbility(target);
                                }
                                item.UseAbility(me);
                            }
                            else
                            {
                                item.UseAbility();
                            }
                            Utils.Sleep(200, item.Name);
                            break;
                    }
                }
            }

            if (Utils.SleepCheck("vg") && venomousGale != null && venomousGale.CanBeCasted() && venomousGale.CanHit(target))
            {
                venomousGale.UseAbility();
                Utils.Sleep(100, "vg");
            }
            var angle = (float)Math.Max(
                Math.Abs(me.RotationRad - Utils.DegreeToRadian(me.FindAngleBetween(target.Position))) - 0.20, 0);
            if (Utils.SleepCheck("poisonNova") && poisonNova != null && poisonNova.CanBeCasted() && distance <= 300 && angle == 0)
            {
                poisonNova.UseAbility();
                Utils.Sleep(100, "pounce");
            }

            if (me.Health <= me.MaximumHealth / 100 * Menu.Item("minHp").GetValue<Slider>().Value && Utils.SleepCheck("dance") && poisonNova != null && poisonNova.CanBeCasted())
            {
                poisonNova.UseAbility();
                Utils.Sleep(100, "poisonNova");
            }

            if (!Utils.SleepCheck("attacking")) return;
            me.Attack(target);
            Utils.Sleep(200, "attacking");
        }

        private static Hero ClosestToMouse(Hero source, float range = 600)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes =
                ObjectManager.GetEntities<Hero>()
                    .Where(
                        x =>
                            x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible
                            && x.Distance2D(mousePosition) <= range && !x.IsMagicImmune()).OrderBy(source.Distance2D);
            return enemyHeroes.FirstOrDefault();
        }

        private static float GetDistance2D(Vector3 p1, Vector3 p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}