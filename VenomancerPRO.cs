using System;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Extensions;
using SharpDX;

namespace VenomancerPRO
{
    internal class VenomancerPRO : Variables
    {
        public static readonly uint[] PlagueWardDamage = { 0, 13, 22, 31, 40 };
        public static readonly uint[] PlagueWardDamageDelay = { 0, 5, 10, 15, 20 };

        public static void Init()
        {
            Options.MenuInit();

            Events.OnLoad += OnLoad;
            Events.OnClose += OnClose;
        }

        private static void OnClose(object sender, EventArgs e)
        {
            Game.OnUpdate -= ComboUsage;
            Drawing.OnDraw -= TargetIndicator;
            Game.OnUpdate -= WardsControl;
            loaded = false;
            me = null;
            target = null;
        }

        private static void OnLoad(object sender, EventArgs e)
        {
            if (!loaded)
            {
                me = ObjectManager.LocalHero;
                if (!Game.IsInGame || me == null || me.Name != heroName)
                {
                    return;
                }

                loaded = true;
                Game.PrintMessage(
                    "<font face='Calibri Bold'><font color='#04B404'>" + AssemblyName +
                    " loaded.</font> (coded by <font color='#0404B4'>DeadShotNinja</font>) v" + Assembly.GetExecutingAssembly().GetName().Version,
                    MessageType.LogMessage);
                GetAbilities();                
                Game.OnUpdate += ComboUsage;
                Drawing.OnDraw += TargetIndicator;
                Game.OnUpdate += WardsControl;
            }

            if (me == null || !me.IsValid)
            {
                loaded = false;
            }            
        }

        private static void ComboUsage(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame || Game.IsChatOpen)
                return;

            target = me.ClosestToMouseTarget(ClosestToMouseRange.GetValue<Slider>().Value);
            if (Game.IsKeyDown(comboKey.GetValue<KeyBind>().Key))
            {
                GetAbilities();

                if (target == null || !target.IsValid || !target.IsVisible || target.IsIllusion || !target.IsAlive ||
                    me.IsChanneling() || target.IsInvul() || HasModifiers()) return;

                    if (!Utils.SleepCheck("combosleep")) return;

                    Orbwalk();
                
                    if (soulring != null && soulring.CanBeCasted() && soulRing.GetValue<bool>())
                        soulring.UseAbility();

                    if (!target.UnitState.HasFlag(UnitState.Hexed) && !target.UnitState.HasFlag(UnitState.Stunned))
                        UseItem(sheep, sheep.GetCastRange());

                    UseBlink();
                    CastAbility(gale, gale.GetCastRange());
                    CastAbility(ward, ward.GetCastRange());

                    UseItem(atos, atos.GetCastRange(), 140);
                    UseItem(medal, medal.GetCastRange());
                    UseItem(orchid, orchid.GetCastRange());
                    UseItem(bloodthorn, bloodthorn.GetCastRange());
                    UseItem(veil, veil.GetCastRange());
                    UseItem(ethereal, ethereal.GetCastRange());

                    UseDagon();

                    CastUltimate();

                    UseItem(shivas, shivas.GetCastRange());

                    Utils.Sleep(150, "combosleep");
                
            }
        }

        private static void GetAbilities()
        {
            if (!Utils.SleepCheck("GetAbilities")) return;
            blink = me.FindItem("item_blink");
            soulring = me.FindItem("item_soul_ring");
            medal = me.FindItem("item_medallion_of_courage");
            bloodthorn = me.FindItem("item_bloodthorn");
            force_staff = me.FindItem("item_force_staff");
            cyclone = me.FindItem("item_cyclone");
            orchid = me.FindItem("item_orchid");
            sheep = me.FindItem("item_sheepstick");
            veil = me.FindItem("item_veil_of_discord");
            shivas = me.FindItem("item_shivas_guard");
            dagon = me.GetDagon();
            atos = me.FindItem("item_rod_of_atos");
            ethereal = me.FindItem("item_ethereal_blade");
            gale = me.FindSpell("venomancer_venomous_gale");
            ward = me.FindSpell("venomancer_plague_ward");
            nova = me.FindSpell("venomancer_poison_nova");
            Utils.Sleep(1000, "GetAbilities");
        }

        private static bool HasModifiers()
        {
            if (target.HasModifiers(modifiersNames, false) ||
                (bladeMail.GetValue<bool>() && target.HasModifier("modifier_item_blade_mail_reflect")) ||
                !Utils.SleepCheck("HasModifiers"))
                return true;
            Utils.Sleep(100, "HasModifiers");
            return false;
        }

        private static void TargetIndicator(EventArgs args)
        {
            if (!drawTarget.GetValue<bool>())
            {
                if (circle == null) return;
                circle.Dispose();
                circle = null;
                return;
            }
            if (target != null && target.IsValid && !target.IsIllusion && target.IsAlive && target.IsVisible &&
                me.IsAlive)
            {
                DrawTarget();
            }
            else if (circle != null)
            {
                circle.Dispose();
                circle = null;
            }
        }

        private static void DrawTarget()
        {
            heroIcon = Drawing.GetTexture("materials/ensage_ui/miniheroes/venomancer");
            iconSize = new Vector2(HUDInfo.GetHpBarSizeY() * 2);

            if (
                !Drawing.WorldToScreen(target.Position + new Vector3(0, 0, target.HealthBarOffset / 3), out screenPosition))
                return;

            screenPosition += new Vector2(-iconSize.X, 0);
            Drawing.DrawRect(screenPosition, iconSize, heroIcon);

            if (circle == null)
            {
                circle = new ParticleEffect(@"particles\ui_mouseactions\range_finder_tower_aoe.vpcf", target);
                circle.SetControlPoint(2, me.Position);
                circle.SetControlPoint(6, new Vector3(1, 0, 0));
                circle.SetControlPoint(7, target.Position);
            }
            else
            {
                circle.SetControlPoint(2, me.Position);
                circle.SetControlPoint(6, new Vector3(1, 0, 0));
                circle.SetControlPoint(7, target.Position);
            }
        }
        
        private static void CastAbility(Ability ability, float range)
        {
            if (ability == null || !ability.CanBeCasted() || ability.IsInAbilityPhase ||
                !target.IsValidTarget(range, true, me.NetworkPosition) ||
                !Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(ability.Name))
            {
                return;
            }

            if (target.Distance2D(me.Position) > 500)
            {
                if (target.UnitState.HasFlag(UnitState.Hexed))
                {
                    if (!target.IsMagicImmune())
                    {
                        ability.UseAbility(Prediction.InFront(target, target.MovementSpeed - 100));
                    }
                    else
                    {
                        ward.UseAbility(Prediction.InFront(target, target.MovementSpeed - 100));
                    }
                }
                else
                {
                    if (!target.IsMagicImmune())
                    {
                        ability.UseAbility(Prediction.InFront(target, target.MovementSpeed - 100));
                    }
                    else
                    {
                        ward.UseAbility(Prediction.InFront(target, target.MovementSpeed - 100));
                    }
                }
            }
            else
            {
                if (!target.IsMagicImmune())
                {
                    ability.UseAbility(target.NetworkPosition);
                }
                else
                {
                    ward.UseAbility(target.NetworkPosition);
                }
            }
        }

        private static void CastUltimate()
        {
            if (nova == null
                || !Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(nova.Name)
                || !nova.CanBeCasted()
                || target.IsMagicImmune()
                || !IsFullDebuffed()
                || target.Health * 100 / target.MaximumHealth < Menu.Item("noCastUlti").GetValue<Slider>().Value
                || !Utils.SleepCheck("ebsleep")
                || !Utils.SleepCheck("slowsleep"))
            {
                return;
            }
            else
            {
                if (target.Distance2D(me.Position) < ultimateRadius.GetValue<Slider>().Value) //Nova max radius is 830.
                {
                    nova.UseAbility();
                }
            }
        }        

        private static void UseDagon()
        {
            if (dagon == null
                || !dagon.CanBeCasted()
                || target.IsMagicImmune()
                || !(target.NetworkPosition.Distance2D(me) - target.RingRadius <= dagon.CastRange)
                || !Menu.Item("items").GetValue<AbilityToggler>().IsEnabled("item_dagon")
                || !IsFullDebuffed()
                || !Utils.SleepCheck("ebsleep")) return;
            dagon.UseAbility(target);
        }

        private static void UseItem(Item item, float range, int speed = 0)
        {
            if (item == null || !item.CanBeCasted() || target.IsMagicImmune() || target.MovementSpeed < speed ||
                target.HasModifier(item.Name) || !target.IsValidTarget(range, true, me.NetworkPosition) ||
                !Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(item.Name))
                return;

            if (item.Name.Contains("ethereal") && IsFullDebuffed())
            {
                item.UseAbility(target);
                Utils.Sleep(me.NetworkPosition.Distance2D(target.NetworkPosition) / 1200 * 1000, "ebsleep");
                return;
            }

            if (item.IsAbilityBehavior(AbilityBehavior.UnitTarget) && !item.Name.Contains("item_dagon"))
            {
                item.UseAbility(target);
                return;
            }

            if (item.IsAbilityBehavior(AbilityBehavior.Point))
            {
                item.UseAbility(target.NetworkPosition);
                return;
            }

            if (item.IsAbilityBehavior(AbilityBehavior.Immediate))
            {
                item.UseAbility();
            }
        }
        
        private static bool IsFullDebuffed()
        {
            if ((atos != null && atos.CanBeCasted() &&
                 Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(atos.Name) &&
                 !target.HasModifier("modifier_item_rod_of_atos"))
                ||
                (veil != null && veil.CanBeCasted() &&
                 Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(veil.Name) &&
                 !target.HasModifier("modifier_item_veil_of_discord"))
                ||
                (orchid != null && orchid.CanBeCasted() &&
                 Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(orchid.Name) &&
                 !target.HasModifier("modifier_item_orchid_malevolence"))
                ||
                (ethereal != null && ethereal.CanBeCasted() &&
                 Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(ethereal.Name) &&
                 !target.HasModifier("modifier_item_ethereal_blade_slow"))
                ||
                (bloodthorn != null && bloodthorn.CanBeCasted() &&
                 Menu.Item("items").GetValue<AbilityToggler>().IsEnabled(bloodthorn.Name) &&
                 !target.HasModifier("modifier_item_bloodthorn")))
                return false;
            return true;
        }

        private static void Orbwalk()
        {
            switch (moveMode.GetValue<bool>())
            {
                case true:
                    Orbwalking.Orbwalk(target);
                    break;
                case false:
                    break;
            }
        }

        private static void UseBlink()
        {
            if (!useBlink.GetValue<bool>() || blink == null || !blink.CanBeCasted() ||
                target.Distance2D(me.Position) < 600 || !Utils.SleepCheck("blink")) return;
            predictXYZ = target.NetworkActivity == NetworkActivity.Move
                ? Prediction.InFront(target,
                    (float)(target.MovementSpeed * (Game.Ping / 1000 + 0.3 + target.GetTurnTime(target))))
                : target.Position;

            if (me.Position.Distance2D(predictXYZ) > 1200)
            {
                predictXYZ = (predictXYZ - me.Position) * 1200 / predictXYZ.Distance2D(me.Position) + me.Position;
            }

            blink.UseAbility(predictXYZ);
            Utils.Sleep(500, "blink");
        }

        private static void WardsControl(EventArgs args)
        {
            if (!Game.IsInGame || !Utils.SleepCheck("Veno"))
            {
                return;                
            }

            Utils.Sleep(125, "Veno");

            var me = ObjectManager.LocalHero;

            if (!_loaded)
            {
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                _loaded = true;
            }

            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Venomancer)
                return;

            var plagueWardLevel = me.FindSpell("venomancer_plague_ward").Level;

            var enemies = ObjectManager.GetEntities<Hero>().Where(hero => hero.IsAlive && !hero.IsIllusion && hero.IsVisible && hero.Team == me.GetEnemyTeam()).ToList();
            var creeps = ObjectManager.GetEntities<Creep>().Where(creep => (creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane || creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege) && creep.IsAlive && creep.IsVisible && creep.IsSpawned).ToList();
            var plaguewards = ObjectManager.GetEntities<Unit>().Where(unit => unit.ClassID == ClassID.CDOTA_BaseNPC_Venomancer_PlagueWard && unit.IsAlive && unit.IsVisible).ToList();

            if (!plaguewards.Any() || !(plagueWardLevel > 0))
            {
                return;
            }

            // Hurass Option
            if (enemies.Any() && harassKey.GetValue<KeyBind>().Active)
            {
                foreach (var enemy in enemies)
                {
                    if (enemy.Health > 0)
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
            }
            // CS Option
            if (creeps.Any() && !harassKey.GetValue<KeyBind>().Active)
            {
                //if (enemies.Any())
                //{
                //    foreach (var enemy in enemies)
                //    {
                //        if (enemy.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_venomancer_poison_sting_ward") == null && enemy.Health > 0)
                //        {
                //            foreach (var plagueward in plaguewards)
                //            {
                //                if (GetDistance2D(enemy.Position, plagueward.Position) < plagueward.AttackRange && Utils.SleepCheck(plagueward.Handle.ToString()))
                //                {
                //                    Game.PrintMessage("CS mode ATTACK", MessageType.LogMessage);
                //                    plagueward.Attack(enemy);
                //                    Utils.Sleep(1000, plagueward.Handle.ToString());
                //                }
                //            }
                //        }
                //    }
                //}                
                foreach (var creep in creeps)
                {
                    if (creep.Team == me.GetEnemyTeam() && creep.Health > 0 && creep.Health < PlagueWardDamage[plagueWardLevel] * (1 - creep.DamageResist) + PlagueWardDamageDelay[plagueWardLevel])
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
                foreach (var plagueward in plaguewards)
                {
                    if (plagueward.Team == me.Team && plagueward.Health > (PlagueWardDamage[plagueWardLevel] * 1) && plagueward.Health < (PlagueWardDamage[plagueWardLevel] * 1 + 88))
                        foreach (var plagueward2 in plaguewards)
                        {
                            if (GetDistance2D(plagueward.Position, plagueward2.Position) < plagueward2.AttackRange && Utils.SleepCheck(plagueward2.Handle.ToString()))
                            {
                                plagueward2.Attack(plagueward);
                                Utils.Sleep(1000, plagueward2.Handle.ToString());
                            }
                        }
                }
            }
            else            
                return;            
        }
        private static float GetDistance2D(Vector3 p1, Vector3 p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}
