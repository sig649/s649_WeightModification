using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

using BepInEx;
using HarmonyLib;

using UnityEngine;
using BepInEx.Configuration;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using WeightModification.PatchMain;
//using Mono.Cecil.Cil;

/////////////////////////////////////////////////////////////////////////////////////
/*
    説明：所持重量が超過状態でTickConditionsなどによって呼ばれる超過ダメージをtweakするpatch
    理由：ダメージは重荷段階ではなく所持重量(ChildrenWeight)で決まるので、このMODにより大幅にダメージが増えてしまうため
    対策：重量限界を超えているアイテムのみをダメージのソースにする
    実装：base.ChildrenWeightをburdenWeightに置き換える
    対象メソッド：TickConditions, Stumble
*/
///////////////////////////////////////////////////////////////////////////////////////
namespace WeightModification
{//namespace main
    namespace DamageHPTranspiler
    {//namespace sub
        [HarmonyPatch(typeof(Chara))]
        internal static class MyPatch
        {
            static int burdenWeight;
            static bool transpiler_allowed;
            //{ get; set { return Main.GetBurdenWeight(c_instance); } }

            static Chara c_instance;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Chara), "TickConditions")]
            internal static bool TCPrefix(Chara __instance) 
            {
                c_instance = __instance;
                transpiler_allowed = Main.IsAllowedRuleBurdenMod(__instance);
                CalcBurdenWeight();
                return true;
            }
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Chara), "Stumble")]
            internal static bool StumblePrefix(Chara __instance)
            {
                c_instance = __instance;
                transpiler_allowed = Main.IsAllowedRuleBurdenMod(__instance);
                CalcBurdenWeight();
                return true;
            }


            [HarmonyTranspiler]
            [HarmonyPatch(typeof(Chara), "TickConditions")]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codeList = new List<CodeInstruction>(instructions);
                var burdenField = AccessTools.Field(typeof(MyPatch), "burdenWeight");

                var get_ChildrenWeight = AccessTools.PropertyGetter(typeof(Chara), "ChildrenWeight");
                var damageHPMethod = AccessTools.Method(typeof(Card), "DamageHP", new Type[] { typeof(int), typeof(AttackSource), typeof(Card) }); // or actual type

                //除外処理
                if (!transpiler_allowed)
                {
                    // 置換を一切せずそのまま返す
                    return instructions;
                }

                for (int i = 0; i < codeList.Count; i++)
                {
                // `DamageHP` 呼び出しの直前に ChildrenWeight を使っているか確認
                    if (codeList[i].Calls(get_ChildrenWeight))
                    {
                        // 簡易的に、次に DamageHP 呼び出しがある場合に限定する（文脈一致）
                        for (int j = i + 1; j < codeList.Count && j < i + 10; j++)
                        {
                            if (codeList[j].Calls(damageHPMethod))
                            {
                                // ChildrenWeight → カスタム値に差し替え（ここではローカル変数0と仮定）
                                // ChildrenWeight → burdenWeight;
                                codeList[i - 1] = new CodeInstruction(OpCodes.Ldsfld, burdenField);
                                codeList.RemoveAt(i); // get_ChildrenWeight を削除
                                break;
                            }
                        }
                    }
                }
                return codeList.AsEnumerable();
            }

            [HarmonyTranspiler]
            [HarmonyPatch(typeof(Chara), "Stumble")]
            private static IEnumerable<CodeInstruction> TranspilerFprStumble(IEnumerable<CodeInstruction> instructions)
            {
                
                var codeList = new List<CodeInstruction>(instructions);
                var burdenField = AccessTools.Field(typeof(MyPatch), "burdenWeight");

                var get_ChildrenWeight = AccessTools.PropertyGetter(typeof(Chara), "ChildrenWeight");
               
                //var damageHPMethod = AccessTools.Method(typeof(Card), "DamageHP", new Type[] { typeof(int), typeof(AttackSource), typeof(Card) }); // or actual type

                //除外処理
                if (!transpiler_allowed)
                {
                    // 置換を一切せずそのまま返す
                    return instructions;
                }

                for (int i = 0; i < codeList.Count; i++)
                {
                    if (codeList[i].Calls(get_ChildrenWeight))
                    {
                        // ChildrenWeight プロパティ呼び出しの前のインスタンスロードも削除
                        if (i > 0 && codeList[i - 1].opcode == OpCodes.Ldarg_0)
                        {
                            codeList[i - 1] = new CodeInstruction(OpCodes.Ldsfld, burdenField); // burdenWeight をロード
                            codeList.RemoveAt(i); // get_ChildrenWeight 呼び出しを削除
                            i--; // index調整
                        }
                    }
                }
                return codeList.AsEnumerable();
            }

            private static void CalcBurdenWeight()
            {
                Chara c = c_instance;
                ThingContainer things = c.things;
                int bw = 0;
                int wl = c.WeightLimit;
                //除外処理
                if (!transpiler_allowed)
                {
                    // 置換を一切せずそのまま返す
                    burdenWeight = c.ChildrenWeight;
                    return;
                }
                if (things == null){ Main.Lg("[CBW]things is null",2);}
                foreach (Thing thing in things)
                {
                    int tw = thing.ChildrenAndSelfWeight;
                    if(tw >= wl)
                    {
                        bw += tw;
                    }
                }
                //for debug
                string dt = "";
                dt += "Method:" + "CBW" + "/";
                dt += "Name:" + Main.SName(c) + "/";
                dt += "cw:" + c.ChildrenWeight.ToString() + "->";
                dt += "bw:" + bw.ToString() + "/";
                Main.Lg(dt,2);
                //---------
                //return bw;
                burdenWeight = bw;
            }
            //private static int Higher(int a, int b){return (a > b)? a : b;}

        }

    }//namespace sub
}//namespace main


//trash---------------------------------------------------
/*
[HarmonyPrefix]
            [HarmonyPatch(typeof(Card), "DamageHP", new Type[] { typeof(int), typeof(AttackSource), typeof(Card) } )]
            internal static bool DamageHPPrefix(Card __instance, ref int dmg, ref AttackSource attackSource, ref Card origin)
            {
                //if(!Main.IsAllowedRuleBurdenMod(c)){return true;}

                if(attackSource != AttackSource.Burden){return true;}
                int moddmg = 0;//dmg = MaxHP * (base.ChildrenWeight * 100 / WeightLimit) / 1000 + 1;  //vanilla
                int weightBurdenThings = 0;
                foreach (Thing t in __instance.things)
                {
                    if(t.ChildrenAndSelfWeight > __instance.WeightLimit)
                        {weightBurdenThings += t.ChildrenAndSelfWeight;}
                }
                //moddmg = __instance.ChildrenWeight / weightBurdenThings;
                moddmg = dmg * weightBurdenThings / __instance.ChildrenWeight;
                
                if(moddmg <= 0){moddmg = 1;}
                __instance.DamageHP(moddmg, 0, 0, attackSource, origin);
                return false;
            }



*/