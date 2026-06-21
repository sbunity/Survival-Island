using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class HungerIncreaser : MonoBehaviour
    {
        public void IncreasePlayerHunger(float hungerPercentageToSet)
        {
            float requiredEnergyAmount = (EnergyController.Data.MaxEnergyPoints * hungerPercentageToSet);
            if(EnergyController.EnergyPoints > requiredEnergyAmount)
            {
                float decreaseAmount = EnergyController.EnergyPoints - requiredEnergyAmount;

                EnergyController.RemoveEnergyPoints(decreaseAmount);
            }
        }

    }
}
