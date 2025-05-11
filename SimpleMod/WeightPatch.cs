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
    説明：Burdenの仕組みを改変するpatch
    理由：ロールプレイによる重量制限とそれに伴う所持重量の緩和を同時に行う
    仕様：(CalcBurdenとGetBurdenをtweakする→)ChildrenWeightをpostpatchするに変更→やっぱりCB,GBをtweak
        ：DamageHP,Stumbleもtweak
　　　　：CW = GetHighestWeight(c)とする。GetHWは所持アイテムの中で一番重い重量を返す
    影響：TickConditions,CalcBurden,GetBurden,Stumble
    対象：//////Chara.ChildrenWeight
    条件：Cf_Rule00_BurdenModPlayer Cf_Rule00_BurdenModNonPlayer
*/
///////////////////////////////////////////////////////////////////////////////////////


namespace WeightModification
{//namespace main
    namespace WeightPatch
    {//namespace sub
        [HarmonyPatch]
        internal class WeightMain
        {//class[WeightMain]
            //----entry----------------------------------
            private static readonly bool Cf_Rule00_BurdenModPlayer = Main.Cf_Rule00_BurdenModPlayer;
            private static readonly bool Cf_Rule00_BurdenModNonPlayer = Main.Cf_Rule00_BurdenModNonPlayer;

            //static Card c_bef = null;
            //static int orgres_bef = 0;
            //static int result_bef = 0;


            //----harmony------------------------------------
            
            


            [HarmonyPrefix]
            [HarmonyPatch(typeof(Chara), "GetBurden")]
            internal static bool GBfix(Chara __instance, int __result, ref Card t, ref int num)
            {
                int hw = GetHighestThingsWeight(__instance);
                int num2 = (hw + ((t != null) ? ((num == -1) ? t.ChildrenAndSelfWeight : (t.SelfWeight * num)) : 0)) * 100 / __instance.WeightLimit;
                if (num2 < 0)
                {
                    num2 = 1000;
                }
                if (EClass.debug.ignoreWeight && __instance.IsPC)
                {
                    num2 = 0;
                }
                int num3 = ((num2 >= 100) ? ((num2 - 100) / 10 + 1) : 0);
                if (num3 > 9)
                {
                    num3 = 9;
                }
                __result = num3;
                //for debug
                string dt = "Nm:WeightPatch/Cl:WeightMain/";
                dt += "harmony:GetB:pre/";
                dt += "Name:" + SName(__instance) + "/";
                dt += "HW:" + hw.ToString() + "/";
                dt += "CW:" + __instance.ChildrenWeight.ToString() + "/";
                dt += "WL:" + __instance.WeightLimit.ToString() + "/";
                dt += "res:" + __result.ToString() + "/";
                Lg(dt,3);
                //---------
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Chara), "CalcBurden")]
            internal static bool CBfix(Chara __instance)
            {
                int bw = GetHighestThingsWeight(__instance);

                int num = bw * 100 / Mathf.Max(1, __instance.WeightLimit);
                if (num < 0)
                {
                    num = 1000;
                }
                if (EClass.debug.ignoreWeight && __instance.IsPC)
                {
                    num = 0;
                }
                __instance.burden.Set(num);
                __instance.SetDirtySpeed();
                //for debug
                string dt = "Nm:WeightPatch/Cl:WeightMain/";
                dt += "harmony:CalcB:pre/";
                dt += "Name:" + SName(__instance) + "/";
                dt += "CW:" + __instance.ChildrenWeight.ToString() + "/";
                dt += "WL:" + __instance.WeightLimit.ToString() + "/";
                dt += "BW:" + bw.ToString() + "/";
                //dt += "res:" + __result.ToString() + "/";
                Lg(dt, 3);
                //---------
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Chara), "Stumble")]
            internal static bool StumbleParch(Chara __instance, ref int mtp)
            {
                int bw = GetHighestThingsWeight(__instance);

                bool flag = EClass._map.FindThing((Thing t) => t.IsInstalled && t.pos.Equals(EClass.pc.pos) && t.trait is TraitStairsUp) != null;
                __instance.Say(flag ? "dmgBurdenStairs" : "dmgBurdenFallDown", __instance);
                int num = __instance.MaxHP;
                if (__instance.Evalue(1421) > 0)
                {
                    num = __instance.mana.max;
                }
                int num2 = (num * (bw * 100 / __instance.WeightLimit) / (flag ? 100 : 200) + 1) * mtp / 100;
                if (__instance.hp <= 0)
                {
                    num2 *= 2;
                }
                __instance.DamageHP(num2, flag ? AttackSource.BurdenStairs : AttackSource.BurdenFallDown);
                //for debug
                string dt = "Nm:WeightPatch/Cl:WeightMain/";
                dt += "harmony:Stumble:pre/";
                dt += "Name:" + SName(__instance) + "/";
                dt += "CW:" + __instance.ChildrenWeight.ToString() + "/";
                dt += "WL:" + __instance.WeightLimit.ToString() + "/";
                dt += "num2:" + num2.ToString() + "/";
                //dt += "res:" + __result.ToString() + "/";
                Lg(dt, 3);
                //---------
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Card), "DamageHP", new Type[] { typeof(int), typeof(AttackSource), typeof(Card) })]
            internal static bool DamageHPPrefix(Card __instance, ref int dmg, ref AttackSource attackSource, ref Card origin)
            {
                //if(!Main.IsAllowedRuleBurdenMod()){return true;}

                if (attackSource != AttackSource.Burden) { return true; }
                int moddmg;//dmg = MaxHP * (base.ChildrenWeight * 100 / WeightLimit) / 1000 + 1;  //vanilla
                int weightBurdenThings = GetHighestThingsWeight(__instance); ;
                //foreach (Thing t in __instance.things)
                //{
                //    if (t.ChildrenAndSelfWeight > __instance.WeightLimit)
                //    { weightBurdenThings += t.ChildrenAndSelfWeight; }
                //}
                //moddmg = __instance.ChildrenWeight / weightBurdenThings;
                moddmg = dmg * weightBurdenThings / __instance.ChildrenWeight;

                if (moddmg <= 0) { moddmg = 1; }
                __instance.DamageHP(moddmg, 0, 0, attackSource, origin);
                //for debug
                string dt = "Nm:WeightPatch/Cl:WeightMain/";
                dt += "harmony:Stumble:pre/";
                dt += "Name:" + SName(__instance) + "/";
                dt += "CW:" + __instance.ChildrenWeight.ToString() + "/";
                dt += "WL:" + __instance.WeightLimit.ToString() + "/";
                dt += "dmg:" + dmg.ToString() + "/";
                dt += "moddmg:" + moddmg.ToString() + "/";
                //dt += "res:" + __result.ToString() + "/";
                Lg(dt, 3);
                //---------

                return false;
            }

            //local  method-----------------------------------------------------------------
            private static void Lg(string text, int lv = 0){
                Main.Lg(text,lv);
            }
            private static string SName(Card c){
                return Main.SName(c);
            }
            private static string SName(Chara c){
                return Main.SName(c);
            }
            private static string SName(Thing c){
                return Main.SName(c);
            }
            private static int Higher(int a, int b){return (a > b)? a : b;}

            private static int GetHighestThingsWeight(Card c)
            {
                int hw = 0;
                if(c.things == null){Lg("[GHTW]things is null",2);}
                foreach (Thing thing in c.things)
                {
                    //Lg("thing/" + thing.ToString(),2);
                    int tw = thing.ChildrenAndSelfWeight;
                    hw = Higher(hw,tw);//if(hw < tw){hw = tw;}
                    //_childrenWeight += thing.ChildrenAndSelfWeight;
                }
                if(hw < 0) { hw = 0;}
                //for debug
                //string dt = "Nm:WeightPatch/Cl:WeightMain";
                //dt += "Method:" + "GHTW" + "/";
                //dt += "Name:" + SName(__instance) + "/";
                //dt += "hw:" + hw.ToString() + "/";
                //Lg(dt,3);
                //---------
                return hw;
            }

            private static int GetBurdenWeight(Card c)
            {
                int bw = 0;
                if (c.things == null) { Lg("[GHTW]things is null", 2); }
                foreach (Thing thing in c.things)
                {
                    //Lg("thing/" + thing.ToString(),2);
                    int tw = thing.ChildrenAndSelfWeight;
                    if(tw > c.WeightLimit) { bw += tw; }
                    //hw = Higher(hw, tw);//if(hw < tw){hw = tw;}
                    //_childrenWeight += thing.ChildrenAndSelfWeight;
                }
                if (bw < 0) { bw = 0; }
                //for debug
                //string dt = "Nm:WeightPatch/Cl:WeightMain";
                //dt += "Method:" + "GHTW" + "/";
                //dt += "Name:" + SName(__instance) + "/";
                //dt += "hw:" + hw.ToString() + "/";
                //Lg(dt,3);
                //---------
                return bw;
            }
        }//class[WeightMain]
        
    }//namespace sub
}//namespace main



//trash---------------------------------------------------------------------
/*
[HarmonyPrefix]
            [HarmonyPatch(typeof(Card), "ChildrenWeight", MethodType.Getter)]
            internal static bool ChildrenWeightPatch(Card __instance, int __result, int ____childrenWeight)//source EA23.128N
            {//method:ChildrenWeightPatch
                //Card c = __instance;
                
                if (__instance.dirtyWeight)
                {
                    ____childrenWeight = 0;
                    if (!(__instance.trait is TraitMagicChest))
                    {
                        int hw = 0;
                        foreach (Thing thing in __instance.things)
                        {
                            int tw = thing.ChildrenAndSelfWeight;
                            if(hw > tw){hw = tw;}
                            //_childrenWeight += thing.ChildrenAndSelfWeight;
                        }
                        ____childrenWeight = hw;
                        __instance.dirtyWeight = false;
                        (__instance.parent as Card)?.SetDirtyWeight();
                        if (__instance.isChara && __instance.IsPCFaction)
                        {
                            __instance.Chara.CalcBurden();
                        }
                        int cw =  ____childrenWeight;
                        if (cw < 0 || cw > 10000000)
                        {
                            ____childrenWeight = 10000000;
                        }
                    }
                    
                }
                __result =  ____childrenWeight * Mathf.Max(100 - __instance.Evalue(404), 0) / 100;
                //for debug
                string dt = "[WM]";
                dt += "Fook:" + "CW" + "/";
                dt += "Name:" + SName(__instance) + "/";
                dt += "Res:" + __result.ToString() + "/";
                Lg(dt,1);
                //---------
                return false;
            }//method:ChildrenWeightPatch
*/

/*
                [HarmonyPostfix]
                [HarmonyPatch(typeof(Card), "ChildrenAndSelfWeight", MethodType.Getter)]
                internal static void ChildrenAndSelfWeightPostfix(Card __instance, int __result)
                {
                    int cw = __instance.ChildrenWeight;
                    int sw = __instance.SelfWeight;
                    __result = (cw > sw)? cw : sw;
                    //for debug
                    string dt = "[WM]";
                    dt += "Fook:" + "CSW" + "/";
                    dt += "Name:" + SName(__instance) + "/";
                    dt += "Res:" + __result.ToString() + "/";
                    Lg(dt,0);
                    //---------
                }

                [HarmonyPostfix]
                [HarmonyPatch(typeof(Card), "ChildrenAndSelfWeightSingle", MethodType.Getter)]
                internal static void ChildrenAndSelfWeightSinglePostfix(Card __instance, int __result)
                {
                    __result = __instance.ChildrenAndSelfWeight;

                    //for debug
                    string dt = "[WM]";
                    dt += "Fook:" + "CSWS" + "/";
                    dt += "Name:" + SName(__instance) + "/";
                    dt += "Res:" + __result.ToString() + "/";
                    Lg(dt,0);
                    //---------
                }
                */
/*
[HarmonyPrefix]
[HarmonyPatch(typeof(Chara), "CalcBurden")]
internal static bool CalcBurdenPrefix(Chara __instance)//sourceEA23.128
{//method CalcBurdenPrefix
    if (!Main.IsAllowedRuleBurdenMod(__instance)) { return true; }

    int hw = GetHighestThingsWeight(__instance.things);
    int bd = hw * 100 / Mathf.Max(1, __instance.WeightLimit);
    //ThingContainer things = __instance.things;
   // /*
    //foreach (Thing thing in __instance.things)
    //{
    //    int tw = thing.ChildrenAndSelfWeight;
    //    if(hw > tw){hw = tw;}
    //    //_childrenWeight += thing.ChildrenAndSelfWeight;
    //}
    //

    if (bd < 0)
    {
        bd = 1000;
    }
    if (EClass.debug.ignoreWeight && __instance.IsPC)
    {
        bd = 0;
    }
    __instance.burden.Set(bd);
    __instance.SetDirtySpeed();

    //for debug-----------------------------------------------------
    string dt = "";
    dt += "NM:" + "WP" + "/";
    dt += "Cl:" + "WM" + "/";
    dt += "Fk:" + "CB" + "/";
    dt += "Name:" + SName(__instance) + "/";
    //dt += "T:" + string.Join(" , ", array)__instance.things + "/";
    dt += "hw:" + hw.ToString() + "/";
    dt += "bd:" + bd.ToString() + "/";
    dt += "WL:" + __instance.WeightLimit.ToString() + "/";
    dt += "cw:" + __instance.ChildrenWeight.ToString() + "/";
    Lg(dt, 2);
    //------------------------------------------------------------debug
    return false;
}//method CalcBurdenPrefix

*/

/*
 * [HarmonyPrefix]
            [HarmonyPatch(typeof(Chara), "GetBurden")]
            internal static bool GetBurdenPrefix(Chara __instance, int __result, ref Card t, ref int num)//source EA23.128
            {//method GetBurdenPrefix
                if(!Main.IsAllowedRuleBurdenMod(__instance)){return true;}

                int hw = GetHighestThingsWeight(__instance.things);
                //int num2 = (base.ChildrenWeight + ((t != null) ? ((num == -1) ? t.ChildrenAndSelfWeight : (t.SelfWeight * num)) : 0)) * 100 / WeightLimit;
                int num2 = (Higher(hw, ((t != null) ? ((num == -1) ? t.ChildrenAndSelfWeight : (t.SelfWeight * num)) : 0))) * 100 / __instance.WeightLimit;

                if (num2 < 0)
                {
                    num2 = 1000;
                }
                if (EClass.debug.ignoreWeight && __instance.IsPC)
                {
                    num2 = 0;
                }
                int num3 = ((num2 >= 100) ? ((num2 - 100) / 10 + 1) : 0);
                if (num3 > 9)
                {
                    num3 = 9;
                }
                __result =  num3;

                //for debug------------------------------------------------------------
                string dt = "";
                dt += "Fook:" + "GetBurden" + "/";
                dt += "Name:" + SName(__instance) + "/";
                dt += "res:" + __result.ToString() + "/";
                dt += "WL:" + __instance.WeightLimit.ToString() + "/";
                
                dt += "hw:" + hw.ToString() + "/";
                int tw = 0;
                if(t != null)
                {
                    tw = (num == -1)? t.ChildrenAndSelfWeight : (t.SelfWeight * num);
                    dt += "tw:" + tw.ToString() + "/";
                }
                Lg(dt,1);
                //-----------------------------------------------------------------debug

                return false;
            }//method GetBurdenPrefix
 * 
 * 
 * 
 */
/*
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Card), "ChildrenWeight", MethodType.Getter)]
            internal static void CardCWPostfix(Card __instance, int __result) 
            {
                Card c = __instance;
                int orgres = __result;

                //除外
                if (!c.isChara)
                {
                    return;
                }
                if ((c.IsPC && !Cf_Rule00_BurdenModPlayer) || (!c.IsPC && !Cf_Rule00_BurdenModNonPlayer))
                {
                    return;
                }

                __result = GetHighestThingsWeight(c);

                //for debug
                if(c_bef != c || result_bef != __result)
                {
                    c_bef = c;
                    //orgres_bef = orgres;
                    result_bef = __result;
                    string dt = "Nm:WeightPatch/Cl:WeightMain/";
                    dt += "Harmony:CardCWPostfix/";
                    dt += "Name:" + SName(c) + "/";
                    dt += "org:" + orgres.ToString() + "/";
                    dt += "mod:" + __result.ToString() + "/";

                    Lg(dt, 1);
                }
                
                //---------
            }*/
/*
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Chara), "ChildrenWeight", MethodType.Getter)]
            internal static void CharaCWPostfix(Chara __instance, int __result)
            {
                Chara c = __instance;
                int orgres = __result;

                //除外
                if (!c.isChara)
                {
                    return;
                }
                if ((c.IsPC && !Cf_Rule00_BurdenModPlayer) || (!c.IsPC && !Cf_Rule00_BurdenModNonPlayer))
                {
                    return;
                }

                __result = GetHighestThingsWeight(c);

                //for debug
                if (c_bef != c || orgres_bef != orgres || result_bef != __result)
                {
                    c_bef = c;
                    orgres_bef = orgres;
                    result_bef = __result;
                    string dt = "Nm:WeightPatch/Cl:WeightMain/";
                    dt += "Harmony:CharaCWPostfix/";
                    dt += "Name:" + SName(c) + "/";
                    dt += "org:" + orgres.ToString() + "/";
                    dt += "mod:" + __result.ToString() + "/";

                    Lg(dt, 1);
                }

                //---------
            }*/