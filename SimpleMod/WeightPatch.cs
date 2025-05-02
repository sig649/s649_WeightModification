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
    仕様：(CalcBurdenとGetBurdenをtweakする→)ChildrenWeightをpostpatchするに変更
        ：
　　　　：CW = GetHighestWeight(c)とする。GetHWは所持アイテムの中で一番重い重量を返す
    影響：TickConditions,CalcBurden,GetBurden,Stumble
    対象：Chara.ChildrenWeight
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


            //----harmony------------------------------------
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Card), "ChildrenWeight", MethodType.Getter)]
            internal static void Postfix(Card __instance, int __result) 
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
                string dt = "Nm:WeightPatch/Cl:WeightMain";
                dt += "Harmony:Postfix/";
                dt += "Name:" + SName(c) + "/";
                dt += "org:" + orgres.ToString() + "/";
                dt += "mod:" + __result.ToString() + "/";

                Lg(dt, 1);
                //---------
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
                string dt = "Nm:WeightPatch/Cl:WeightMain";
                dt += "Method:" + "GHTW" + "/";
                //dt += "Name:" + SName(__instance) + "/";
                dt += "hw:" + hw.ToString() + "/";
                Lg(dt,2);
                //---------
                return hw;
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