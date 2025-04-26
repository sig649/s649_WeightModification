using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepInEx;
using HarmonyLib;

using UnityEngine;
using BepInEx.Configuration;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using WeightModification.PatchMain;

/////////////////////////////////////////////////////////////////////////////////////
/*
    説明：Chara.StumbleメソッドのDamageHPをtweakするpatch
    理由：ダメージは重荷段階ではなく所持重量で決まるので、バニラより大幅にダメージが増えてしまうため
    対策：重量限界を超えているアイテムの重さのみをダメージのソースにする
*/
///////////////////////////////////////////////////////////////////////////////////////
namespace WeightModification
{//namespace main
    namespace StumbleP
    {//namespace sub
        [HarmonyPatch]
        internal class PatchExe
        {//class[@@@@@@@@@@]
            //----nakami-------------------
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Chara), "Stumble")]
            internal static bool StumblePrefix(Chara __instance, ref int mtp)
            {
                bool flag = EClass._map.FindThing((Thing t) => t.IsInstalled && t.pos.Equals(EClass.pc.pos) && t.trait is TraitStairsUp) != null;
                __instance.Say(flag ? "dmgBurdenStairs" : "dmgBurdenFallDown", __instance);
                int num = __instance.MaxHP;
                if (__instance.Evalue(1421) > 0)
                {
                    num = __instance.mana.max;
                }
                int weightBurdenThings = 0;
                foreach (Thing thing in __instance.things)
                {
                    if(thing.ChildrenAndSelfWeight > __instance.WeightLimit)
                        {weightBurdenThings += thing.ChildrenAndSelfWeight;}
                }

                int num2 = (num * (weightBurdenThings * 100 / __instance.WeightLimit) / (flag ? 100 : 200) + 1) * mtp / 100;
                if (__instance.hp <= 0)
                {
                    num2 *= 2;
                }
                __instance.DamageHP(num2, flag ? AttackSource.BurdenStairs : AttackSource.BurdenFallDown);
                return false;
                /*
                //if(aSource != AttackSource.Burden){return true;}
                int moddmg = 0;//dmg = MaxHP * (base.ChildrenWeight * 100 / WeightLimit) / 1000 + 1;  //vanilla
                int weightBurdenThings = 0;
                foreach (Thing t in __instance.things)
                {
                    if(t.ChildrenAndSelfWeight > __instance.WeightLimit)
                        {weightBurdenThings += t.ChildrenAndSelfWeight;}
                }
                moddmg = __instance.MAXHP * (weightBurdenThings * 100 / __instance.WeightLimit) / 1000 + 1;
                DamageHP(moddmg, 0, 0, aSource, origin);
                return false;
                */
            }
        }//class[@@@@@@@@@@]
    }//namespace sub
}//namespace main


/*  Source:vanilla
{
        bool flag = EClass._map.FindThing((Thing t) => t.IsInstalled && t.pos.Equals(EClass.pc.pos) && t.trait is TraitStairsUp) != null;
        Say(flag ? "dmgBurdenStairs" : "dmgBurdenFallDown", this);
        int num = MaxHP;
        if (Evalue(1421) > 0)
        {
            num = mana.max;
        }
        int num2 = (num * (base.ChildrenWeight * 100 / WeightLimit) / (flag ? 100 : 200) + 1) * mtp / 100;
        if (base.hp <= 0)
        {
            num2 *= 2;
        }
        DamageHP(num2, flag ? AttackSource.BurdenStairs : AttackSource.BurdenFallDown);
    }
*/