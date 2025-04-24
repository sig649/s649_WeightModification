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
    namespace ThrowablePatch
    {//namespace sub
        [HarmonyPatch]
        internal class TPMain
        {//class[TPMain]
            //---entry-----------------------
            private bool Rule_LimitThrowing => Main.cf_Rule03_LimitThrowing;
            private int value_ThrowableWM => Main.cf_Rule03_ThrowableWeightMulti;
            //----nakami-------------------
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ActThrow), "CanThrow")]
		    public static bool CanThrowPrefix(Chara c,Thing t,Card target,Point p)
		    {
			    if(!c.IsPC || !Rule_LimitThrowing){return true;}
                int throwableWeight = value_ThrowableWM * c.WeightLimit / 100;
			    //if(IFWMain.HasKeepHandlePenalty(c)){
                if(t.things == null)
                {
                    if(t.SelfWeight > throwableWeight && p != null){
					    //if(t != null){Debug.Log("[IFW]throw : "+ t.ToString() + "->" + t.SelfWeight.ToString());}
					    if(p.Distance(c.pos) <= 2){return true;}
					    //Msg.SayRaw("TooHeavy");
					return false;
				    }
                } else {
                    if(t.ChildrenAndSelfWeight > throwableWeight && p != null){
					    //if(t != null){Debug.Log("[IFW]throw : "+ t.ToString() + "->" + t.SelfWeight.ToString());}
					    if(p.Distance(c.pos) <= 2){return true;}
					    //Msg.SayRaw("TooHeavy");
					return false;
				    }
                }
				    
			    //}
			    return true;
		    }
        }//class[TPMain]
    }//namespace sub
}//namespace main