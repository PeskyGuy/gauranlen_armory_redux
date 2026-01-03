using RimWorld;
using Verse;

namespace Pesky
{
    public class CompProperties_UseEffectAddHediffWithSeverity : CompProperties_UseEffect
    {
        public HediffDef hediffDef;
        public float severity = 1f;
        public bool scaleSeverityByToxResistance = false;
        public GeneDef neededGene;

        public CompProperties_UseEffectAddHediffWithSeverity()
        {
            this.compClass = typeof(CompUseEffect_AddHediffWithSeverity);
        }
    }
}