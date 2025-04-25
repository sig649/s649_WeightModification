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
    namespace HoldCardPatch
    {//namespace sub
        [HarmonyPatch]
        internal class HCPMain
        {//class[HCPMain]
            //----entry---------------------
            private static bool Rule_LimitLifting => Main.cf_Rule01_LimitLifting;
            private static bool Rule_LiftingSupport => Main.cf_Rule01_LiftingSupport;
            private static int Rule_CarryWeightMulti => Main.cf_Rule01_CarryWeightMulti;

            //entry--method-----------------
            
            private static int SizeDif(int ch, int ph)
            {
                //int ch = c.bio.height;
                //int ph = p.bio.height;
                if(ch >= ph)
                {
                    if(ch / ph > 100){return 2;} 
                    else if (ch / ph > 10){return 1;}
                    else {return 0;}
                } else {
                    return -1 * SizeDif(ph,ch);
                }
                
            }

            internal static int WeightCanKeepLift(Chara c)
            {//設置されたアイテムを持ち上げるのに必要な値
		        int result;
                int dif;
                int cwl, pwl;
                int multi = Rule_CarryWeightMulti;
                if(c.parasite == null || !Rule_LiftingSupport)
                {   //基本値　＝　重量限界 * 倍率[%]
                    result = c.WeightLimit * multi / 100;
                } 
                else 
                {   //共生相手がいる場合は補助が働くが、体格差の影響を受ける。これによって結果が基本値より下がることはない
                    //相手が自分よりデカくて重量限界も多いほうが効果的。
                    dif = SizeDif(c.bio.height, c.parasite.bio.height);
                    cwl = c.WeightLimit;
                    pwl = c.parasite.WeightLimit;

                    switch(dif)
                    {
                        case 2 : result = Higher(cwl, (cwl * 99 + pwl)/ 100) * multi / 100;
                        break;
                        case 1 : result = Higher(cwl, (cwl * 9 + pwl)/ 10) * multi / 100;
                        break;
                        case 0 : result = Higher(cwl, (cwl + pwl)/ 2) * multi / 100;
                        break;
                        case -1 : result = Higher(cwl, (cwl + pwl * 9)/ 10) * multi / 100;
                        break;
                        case -2 : result = Higher(cwl, (cwl + pwl * 99)/ 100) * multi / 100;
                        break;
                        default : result = cwl * multi / 100;
                        break;
                    }
                    
                }
        		return result;
        		//return (int)( WeightCanKeepHandle(c) * configMultiWeightCanKeepLift);
        	}
            private static int Higher(int a, int b){return (a > b)? a : b;}

            //----nakami-------------------

            [HarmonyPrefix]
		    [HarmonyPatch(typeof(Chara), "HoldCard")]
    		private static bool PrePatch(Chara __instance, Card t)
    		{//harmony HoldCard
    			Chara c = __instance;
    			if(Rule_LimitLifting && t.IsInstalled && c.IsPC)
    			{
                    int wckl = WeightCanKeepLift(c);
    				if(t.SelfWeight > wckl)
    				{
                        //SE.Beep();
    					if(c.parasite != null && Rule_LiftingSupport)
                        {
    						//Debug.Log("[IFW]Motemasen");
    						Msg.Say(Main.SName(t) +"は"+ Main.SName(c.parasite) +"に手伝ってもらっても重すぎる[" + t.SelfWeight.ToString() + "/" + wckl.ToString() + "]");
    					} 
                        else 
                        {
                            Msg.Say(Main.SName(t) +"は"+ Main.SName(c) +"には重すぎる[" + t.SelfWeight.ToString() + "/" + wckl.ToString() + "]");
    						
                        }
                        return false;
			        }
                }
                return true;
		    }//harmony HoldCard	
        }//class[HCPMain]
    
    }//namespace sub
}//namespace main