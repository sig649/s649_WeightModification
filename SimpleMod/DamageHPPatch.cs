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
    説明：所持重量が超過状態でTickConditionsによって呼ばれるDamageHPをtweakするpatch
    理由：ダメージは重荷段階ではなく所持重量で決まるので、このMODにより大幅にダメージが増えてしまうため
    対策：重量限界を超えているアイテムのみをダメージのソースにする
*/
///////////////////////////////////////////////////////////////////////////////////////
namespace WeightModification
{//namespace main
    namespace DamageHPP
    {//namespace sub
        [HarmonyPatch]
        internal class PatchExe
        {//class[@@@@@@@@@@]
            //----nakami-------------------
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Card), "DamageHP", new Type[] { typeof(int), typeof(AttackSource), typeof(Card) } )]
            internal static bool DamageHPPrefix(Card __instance, ref int dmg, ref AttackSource aSource, ref Card origin)
            {
                if(aSource != AttackSource.Burden){return true;}
                int moddmg = 0;//dmg = MaxHP * (base.ChildrenWeight * 100 / WeightLimit) / 1000 + 1;  //vanilla
                int weightBurdenThings = 0;
                foreach (Thing t in __instance.things)
                {
                    if(t.ChildrenAndSelfWeight > __instance.WeightLimit)
                        {weightBurdenThings += t.ChildrenAndSelfWeight;}
                }
                moddmg /= __instance.ChildrenWeight / weightBurdenThings;
                if(moddmg <= 0){moddmg = 1;}
                __instance.DamageHP(moddmg, 0, 0, aSource, origin);
                return false;
            }
        }//class[@@@@@@@@@@]
    }//namespace sub
}//namespace main