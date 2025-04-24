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
            public static ConfigEntry<int> CE_LogLevel;//デバッグ用のログの出力LV　-1:出力しない 0~:第二引数に応じて出力
            public static ConfigEntry<bool> CE_Rule_BurdenMod;//R00:重荷状態を改変するかどうか
            public static ConfigEntry<bool> CE_Rule_LimitLiftingInstalled;//R01:持ち上げられるアイテムの重さを制限するかどうか
            public static ConfigEntry<bool> CE_Rule_ParasiteSupportWhenLifting;//R01:持ち上げ時に共生相手が手伝ってくれるかどうか
            public static ConfigEntry<int> CE_Rule_CarryWeightMulti;//R01:持ち上げ可能重量（＝this * WeightLimit）[%]
            public static ConfigEntry<bool> CE_Rule_ApplyWLMultiForEachRaces;//R02:重量限界を種族ごとに変動させるかどうか
            //for Rule02-------------------------------------------
            public static ConfigEntry<string> CE_SizeSS_List;
            public static ConfigEntry<string> CE_SizeS_List;
            //private static ConfigEntry<string> CE_SizeM_List;
            public static ConfigEntry<string> CE_SizeL_List;
            public static ConfigEntry<string> CE_SizeLL_List;
            public static ConfigEntry<int> CE_SizeSS_WLMulti;
            public static ConfigEntry<int> CE_SizeS_WLMulti;
            public static ConfigEntry<int> CE_SizeM_WLMulti;
            public static ConfigEntry<int> CE_SizeL_WLMulti;
            public static ConfigEntry<int> CE_SizeLL_WLMulti;
            public static ConfigEntry<int> CE_SizeEX_WLMulti;
            //---------------------------------------------for Rule02



        
            //config--------------------------------------------------------------------------------------------------------------
            //public static bool cf_Allow_F01_WL =>  CE_AllowFunction01WL.Value;
            internal static int cf_LogLevel =>  CE_LogLevel.Value;
            //Rule00
            internal static bool cf_Rule00_BurdenMod =>  CE_Rule_BurdenMod.Value;
            //Rule01
            internal static bool cf_Rule01_LimitLifting =>  CE_Rule_LimitLiftingInstalled.Value;
            internal static bool cf_Rule01_LiftingSupport =>  CE_Rule_ParasiteSupportWhenLifting.Value;
            internal static int cf_Rule01_CarryWeightMulti
            {
		        get {return Mathf.Clamp(CE_Rule_CarryWeightMulti.Value,10,10000);}
	        }
            //Rule02
            internal static bool cf_ApplyWLMForEachRaces =>CE_Rule_ApplyWLMultiForEachRaces.Value;
            private static List<string> cf_SizeSS_List => CE_SizeSS_List.Value.Split(',').Select(s => s.Trim()).ToList();
            private static List<string> cf_SizeS_List => CE_SizeS_List.Value.Split(',').Select(s => s.Trim()).ToList();
            private static List<string> cf_SizeL_List => CE_SizeL_List.Value.Split(',').Select(s => s.Trim()).ToList();
            private static List<string> cf_SizeLL_List => CE_SizeLL_List.Value.Split(',').Select(s => s.Trim()).ToList();
            private static List<string> cf_SizeEX_List => CE_SizeLL_List.Value.Split(',').Select(s => s.Trim()).ToList();

            internal static int cf_SizeSS_WLMulti
            {
		        get {return Mathf.Clamp(CE_SizeSS_WLMulti.Value,1,10000);}
	        }
            internal static int cf_SizeS_WLMulti
            {
		        get {return Mathf.Clamp(CE_SizeS_WLMulti.Value,1,10000);}
	        }
            internal static int cf_SizeM_WLMulti
            {
		        get {return Mathf.Clamp(CE_SizeM_WLMulti.Value,1,10000);}
	        }
            internal static int cf_SizeL_WLMulti
            {
		        get {return Mathf.Clamp(CE_SizeL_WLMulti.Value,1,10000);}
	        }
            internal static int cf_SizeLL_WLMulti
            {
		        get {return Mathf.Clamp(CE_SizeLL_WLMulti.Value,1,10000);}
	        }
            internal static int cf_SizeEX_WLMulti
            {
		        get {return Mathf.Clamp(CE_SizeEX_WLMulti.Value,1,10000);}
	        }
            internal static int GetWLMulti(int size)
            {
                switch(size)
                {
                    case 0 : return cf_SizeSS_WLMulti;
                    break;
                    case 1 : return cf_SizeS_WLMulti;
                    break;
                    case 2 : return cf_SizeM_WLMulti;
                    break;
                    case 3 : return cf_SizeL_WLMulti;
                    break;
                    case 4 : return cf_SizeLL_WLMulti;
                    break;
                    case 999 : return cf_SizeS_EXMulti;
                    break;
                    default : return 100;
                    break;
                }
            }
            internal static int GetSize(string raceid)
            {
                if(cf_SizeEX_List.Contains(raceid)){return 999;}
                if(cf_SizeLL_List.Contains(raceid)){return 4;}
                if(cf_SizeL_List.Contains(raceid)){return 3;}
                //if(cf_SizeM_List.Contains(raceid)){return 2;}
                if(cf_SizeS_List.Contains(raceid)){return 1;}
                if(cf_SizeSS_List.Contains(raceid)){return 0;}
                return 2;
            }
             //loading----------------------------------------------------------------------------------------------------------
            internal void LoadConfig()
            {
                //CE_AllowFunction01WL = Config.Bind("#00-General","AllowF01WL", true, "Allow control of function 01-WL");
                CE_LogLevel = Config.Bind("#zz-Debug","LogLevel", 0, "For debug use. If the value is -1, it won't output logs");

                CE_Rule_BurdenMod = Config.Bind("#Rule00", "BurdenCalcMod", true, "Change the calculation of the burden condition.");
                CE_Rule_LimitLiftingInstalled = Config.Bind("#Rule01", "LimitLifting", true, "Limit the weight of installed things that can be lifted.");
                CE_Rule_ParasiteSupportWhenLifting = Config.Bind("#Rule01", "LiftingSupport", true, "Parasitic mates help with the lifting.");
                CE_Rule_CarryWeightMulti = Config.Bind("#Rule01_CarryWeight", "CarryWeightMulti", 1000, "Multiplier of weight to be lifted [%]");
                CE_Rule_ApplyWLMultiForEachRaces = Config.Bind("#Rule02", "ApplyWLMultiForEachRaces", true, "Apply Weight Limit multiplier for each races.");

                CE_SizeSS_List = Config.Bind("#Rule02_Value-RaceSize", "Size SS List", "fairy,snail,slime,rat,quickling,metal", "[SS]A list of strings separated by commas.");
                CE_SizeS_List = Config.Bind("#Rule02_Value-RaceSize", "Size S List", "shiba,eldercrab,rabbit,frog,centipede,mandrake,beetle,mushroom,bat,eye,wasp,imp,hand,snake,spider,crab,cat,dog,wisp,chicken,animal", "[S]A list of strings separated by commas.");
                CE_SizeL_List = Config.Bind("#Rule02_Value-RaceSize", "Size L List", "troll,minotaur,bear,armor,phantom,rock,piece,machine,bike,fish", "[L]A list of strings separated by commas.");
                CE_SizeLL_List = Config.Bind("#Rule02_Value-RaceSize", "Size LL List", "ent,wyvern,giant,drake,dragon,dinosaur,cerberus", "[LL]A list of strings separated by commas.");
                CE_SizeEX_List = Config.Bind("#Rule02_Value-RaceSize", "Size EX List", "catgod,machinegod,undeadgod,god", "[Exclusive]A list of strings separated by commas.");

                CE_SizeSS_WLMulti = Config.Bind("#Rule02_Value-WLMulti", "SS WL Multi", 10, "[%]Multiplier of weight limit for races of size SS");
                CE_SizeS_WLMulti = Config.Bind("#Rule02_Value-WLMulti", "SS WL Multi", 25, "[%]Multiplier of weight limit for races of size S");
                CE_SizeM_WLMulti = Config.Bind("#Rule02_Value-WLMulti", "SS WL Multi", 50, "[%]Multiplier of weight limit for races of size M");
                CE_SizeL_WLMulti = Config.Bind("#Rule02_Value-WLMulti", "SS WL Multi", 75, "[%]Multiplier of weight limit for races of size L");
                CE_SizeLL_WLMulti = Config.Bind("#Rule02_Value-WLMulti", "SS WL Multi", 100, "[%]Multiplier of weight limit for races of size LL");
                CE_SizeEX_WLMulti = Config.Bind("#Rule02_Value-WLMulti", "SS WL Multi", 200, "[%]Multiplier of weight limit for races of exclusive list");

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
 