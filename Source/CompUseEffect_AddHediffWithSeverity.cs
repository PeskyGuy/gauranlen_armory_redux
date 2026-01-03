using RimWorld;
using Verse;

namespace Pesky
{
    public class CompUseEffect_AddHediffWithSeverity : CompUseEffect
    {
        public CompProperties_UseEffectAddHediffWithSeverity Props
        {
            get => (CompProperties_UseEffectAddHediffWithSeverity)this.props;
        }

        public override void DoEffect(Pawn user)
        {
            if (this.Props.neededGene != null)
            {
                if (this.Props.neededGene == null)
                    return;
                Pawn_GeneTracker genes = user.genes;
                if ((genes != null ? (genes.HasActiveGene(this.Props.neededGene) ? 1 : 0) : 0) == 0)
                    return;
            }
            
            Hediff firstHediffOfDef = user.health.hediffSet.GetFirstHediffOfDef(this.Props.hediffDef);
            float num = this.Props.scaleSeverityByToxResistance ? 
                (1f - user.GetStatValue(StatDefOf.ToxicResistance)) * this.Props.severity : 
                this.Props.severity;
                
            if ((double)num > 0.0)
            {
                if (firstHediffOfDef == null)
                {
                    user.health.AddHediff(this.Props.hediffDef);
                    user.health.hediffSet.GetFirstHediffOfDef(this.Props.hediffDef).Severity = num;
                }
                else
                    firstHediffOfDef.Severity += num;
            }
            else
            {
                if (firstHediffOfDef == null)
                    return;
                firstHediffOfDef.Severity += num;
            }
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            return (AcceptanceReport)true;
        }
    }
}