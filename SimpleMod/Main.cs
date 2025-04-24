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

namespace WeightModification
{// namespace main
    namespace PatchMain
    {// namespace sub
        [BepInPlugin("s649_WeightModification", "Carry Weight Modification", "0.0.0.0")]
        public class Main : BaseUnityPlugin
        {// class[Main]

            //////-----Config Entry---------------------------------------------------------------------------------- 
            //private static ConfigEntry<bool> CE_AllowFunction01WL;
            private static ConfigEntry<int> CE_LogLevel;
            private static ConfigEntry<bool> CE_Rule_BurdenMod;
            private static ConfigEntry<bool> CE_Rule_LimitLiftingThing;
            private static ConfigEntry<bool> CE_Rule_ParasiteSupportWhenLifting;
            private static ConfigEntry<int> CE_Rule_CarryWeightMulti;
        
            //config--------------------------------------------------------------------------------------------------------------
            //public static bool cf_Allow_F01_WL =>  CE_AllowFunction01WL.Value;
            public static int cf_LogLevel =>  CE_LogLevel.Value;
            public static bool cf_Rule00_BurdenMod =>  CE_Rule_BurdenMod.Value;

            public static bool cf_Rule01_LimitLifting =>  CE_Rule_LimitLiftingThing.Value;
            public static bool cf_Rule01_LiftingSupport =>  CE_Rule_ParasiteSupportWhenLifting.Value;
            public static int cf_Rule01_CarryWeightMulti
            {
		        get {return Mathf.Clamp(CE_Rule_CarryWeightMulti.Value,10,10000);}
	        }



            
             //loading----------------------------------------------------------------------------------------------------------
            internal void LoadConfig()
            {
                //CE_AllowFunction01WL = Config.Bind("#00-General","AllowF01WL", true, "Allow control of function 01-WL");
                CE_LogLevel = Config.Bind("#zz-Debug","LogLevel", 0, "For debug use. If the value is -1, it won't output logs");
                CE_Rule_BurdenMod = Config.Bind("#Rule-00", "BurdenCalcMod", true, "Change the calculation of the burden condition.");
                CE_Rule_LimitLiftingThing = Config.Bind("#Rule-01", "LimitLifting", true, "Limit the weight of installed things that can be lifted.");
                CE_Rule_ParasiteSupportWhenLifting = Config.Bind("#Rule-01", "LiftingSupport", true, "Parasitic mates help with the lifting.");
                CE_Rule_CarryWeightMulti = Config.Bind("#Rule-01", "CarryWeightMulti", 500, "Multiplier of weight to be lifted [%]");

            }
            private void Start()
            {//method[Start]        
                LoadConfig();   
                var harmony = new Harmony("Main");
                new Harmony("Main").PatchAll();
            }//method[Start]

            //methods------------------------------------------------------------------------------------------------------------
            internal static void Lg(string text, int lv = 0)
            {
                if(cf_LogLevel >= lv){Debug.Log(text);}
            }

            internal static string SName(Chara c)
            {
                return c.GetName(NameStyle.Simple);
            }
            internal static string SName(Card c)
            {
                return c.GetName(NameStyle.Simple);
            }
            internal static string SName(Thing t)
            {
                return t.GetName(NameStyle.Simple);
            }
        
        }//class[Main]
    }//namespace sub
}//namespace main




//------------template--------------------------------------------------------------------------------------------
/*

------namespace---------------------------
namespace WeightModification
{//namespace main
    namespace sub@@@@@@@@@@@@
    {//namespace sub
    
    }//namespace sub
}//namespace main

------class---------------------------------
[HarmonyPatch]
internal class @@@@@@@@@@@
{//class[@@@@@@@@@@]
    //----nakami-------------------
}//class[@@@@@@@@@@]

------harmony--method----------------------------------
[HarmonyPostfix]
[HarmonyPatch(typeof(Chara), "+++++++")]
internal static void Postfix(type arg){}

[HarmonyPrefix]
[HarmonyPatch(typeof(------), "+++++++")]
internal static bool Prefix(){}

---------harmony class and method----------------------
[HarmonyPatch]
internal class PreExe
{//prefix[pppppppppp]
    [HarmonyPrefix]
    [HarmonyPatch(typeof(------), "+++++++")]
    internal static bool Prefix(){}
}//prefix[ppppppppppppppppp]

[HarmonyPatch]
internal class PostExe
{//postfix[oooooooo]
    [HarmonyPostfix]
    [HarmonyPatch(typeof(------), "+++++++")]
    internal static void Postfix(type arg){}
}//postfix[oooooooo]

*/

//////trash box//////////////////////////////////////////////////////////////////////////////////////////////////

// private static ConfigEntry<bool> flagEnableLogging;

        //public static bool propFlagEnablelLogging
        //{
            //get => flagEnableLogging.Value;
            //set => flagEnableLogging = value;
        //}

/*
[HarmonyPatch]
    public class PostExe{
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Map), "MineFloor")]
        public static void Postfix(Map __instance, Point point, Chara c, bool recoverBlock, bool removePlatform){
            string text = "[LS]MF [";
            text += "Map:" + __instance.ToString() + "][";
            text += "P:" + point.ToString() + "][";
            text += "C:" + c.ToString() + "][";
            text += "rB:" + recoverBlock.ToString() + "][";
            text += "rP:" + removePlatform.ToString() + "][";
            text += "]";
            //Debug.Log(text);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Map), "DropBlockComponent")]
        public static void Postfix(Point point,TileRow r,SourceMaterial.Row mat, bool recoverBlock, bool isPlatform, Chara c){
            string text = "[LS]DBC [";
            //text += "Map:" + __instance.ToString() + "][";
            text += "P:" + point.ToString() + "][";
            text += "r:" + r.ToString() + "][";
            text += "rid:" + r.id.ToString() + "][";
            text += "mat:" + mat.ToString() + "][";
            text += "rB:" + recoverBlock.ToString() + "][";
            text += "iP:" + isPlatform.ToString() + "][";
            //text += "c:" + c.ToString() + "][";
            text += "]";
            Debug.Log(text);
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ThingGen), "CreateRawMaterial")]
        public static void Postfix(SourceMaterial.Row row){
            Debug.Log("[LS]TG->CRM : " + row.ToString());
        }
    }
*/

//public static void Lg(string t)
        //{
        //    UnityEngine.Debug.Log(t);
        //}
        //public static bool IsOnGlobalMap(){
        //    return (EClass.pc.currentZone.id == "ntyris") ? true : false;
        //}

        
            //flagEnableLogging = Config.Bind("#0General", "ENABLE_LOGGING", true, "Enable Logging");

            //UnityEngine.Debug.Log("[LS]Start [configLog:" + propFlagEnablelLogging.ToString() + "]");
 