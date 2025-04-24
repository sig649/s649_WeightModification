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

namespace WeightModification
{//namespace main
    namespace WeightLimitPatch
    {//namespace sub
        [HarmonyPatch]
        internal class WLPMain
        {//class[WLPMain]
            //entry---------------------------
            internal class RaceSize
            {
                public string id { get; set; }
                public int value { get; set; }
                //public int GetValue()
            }
            /*
            private static bool HasRaceSize(List<RaceSize> sizelist, string c_race)
            {
                if(sizelist == null){return false;}
                foreach(RaceSize rs in sizelist)
                {
                    if(rs.id == c_race)
                    {
                        return true;
                    }
                }
                return false;
            }
            */
            private static int GetRSValue(List<RaceSize> sizelist, string c_race)
            {
                if(sizelist == null){return -999;}
                foreach(RaceSize rs in sizelist)
                {
                    if(rs.id == c_race)
                    {
                        return rs.value;
                    }
                }
                return -999;
            }
            private static bool Rule_ApplyWLMForEachRaces => Main.cf_ApplyWLMForEachRaces;
            var SizeList = new List<RaceSize>();
            
            //----nakami-------------------
            [HarmonyPostfix]
		    [HarmonyPatch(typeof(Chara), "WeightLimit", MethodType.Getter)]
    		public static void WeightLimit_PostPatch(Chara __instance, ref int __result)
    		{
                Chara c = __instance;
                if(!Rule_ApplyWLMForEachRaces){return;}

                string c_race = c.race.id;
                int c_size = GetRSValue(SizeList,c_race);
                //RaceSize rs = new RaceSize();
                if(c_size < 0)
                {
                    RaceSize rs = new RaceSize();
                    rs.id = c_race;
                    rs.value = Main.GetSize(c_race);
                    SizeList.add(rs);
                    c_size = rs.value;
                } 
    			if (c.IsPC){
    				//if(IFWMain.HasWeightLimitPenalty(c)){
    					//float rs = (float)(__result) * IFWMain.configWeightLimitMulti;
    				__result = __result * Main.GetWLMulti(c_size) / 100;
    				//}
	    		}
		    }
        }//class[WLPMain]
    }//namespace sub
}//namespace main