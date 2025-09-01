using RimWorld;
using UnityEngine;
using Verse;

namespace Pesky
{
    public class CompPowerPlantSolarDryad : CompPowerPlant
    {
        private const float NightPower = 0.0f;

        protected override float DesiredPowerOutput
        {
            get
            {
                return Mathf.Lerp(0.0f, -this.Props.PowerConsumption, this.parent.Map.skyManager.CurSkyGlow) * this.RoofedPowerOutputFactor;
            }
        }

        private float RoofedPowerOutputFactor
        {
            get
            {
                int num1 = 0;
                int num2 = 0;
                foreach (IntVec3 c in this.parent.OccupiedRect())
                {
                    ++num1;
                    if (this.parent.Map.roofGrid.Roofed(c))
                        ++num2;
                }
                return (float)(num1 - num2) / (float)num1;
            }
        }

    }
}