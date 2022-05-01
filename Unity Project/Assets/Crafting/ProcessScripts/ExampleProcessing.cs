using System.Collections.Generic;
using UnityEngine;
namespace DissertationTool.Assets.Crafting.ProcessScripts
{
    public class ExampleProcessing : MonoBehaviour
    {
        public static void ProcessSand(Dictionary<string,float> _data) {
            float sc = _data.ContainsKey("coarseness") ? _data["coarseness"] : 0;
            string output = "";
            switch(sc) {
                case float coarseness when (coarseness < 1): 
                {
                    output = "Nothing made!";
                    break;
                }
                case float coarseness when (coarseness <= 5): 
                {
                    output = "Small Sand Pile Made!";
                    break;
                }
                case float coarseness when (coarseness <= 10): 
                {
                    output = "Sand Pile Made!";
                    break;
                }
                case float coarseness when (coarseness <= 50): 
                {
                    output = "Medium Sand Pile Made!";
                    break;
                }
                case float coarseness when (coarseness <= 100): 
                {
                    output = "Large Sand Pile Made!";
                    break;
                }
                default :{
                    output = "Desert Made!!";
                    break;
                 }
            }
            Debug.Log($"ProcessSand Result: {output} (Coarseness: {sc}");
        }
    }
}