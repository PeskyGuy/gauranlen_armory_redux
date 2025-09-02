using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Pesky
{
    public class CompProperties_SunkeeperTransform : CompProperties
    {
        public int cooldownTicks = 900000;
        public string buildingDefName = "SolarDryadPod";
        
        public CompProperties_SunkeeperTransform()
        {
            compClass = typeof(CompSunkeeperTransform);
        }
    }

    public class CompSunkeeperTransform : ThingComp
    {
        private int cooldownTicksRemaining = 0;
        private bool isTransformed = false;
        private Building transformedBuilding;
        
        public CompProperties_SunkeeperTransform Props => (CompProperties_SunkeeperTransform)props;
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }
        
        public override void CompTick()
        {
            base.CompTick();
            
            if (cooldownTicksRemaining > 0)
            {
                cooldownTicksRemaining--;
            }
        }
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!isTransformed)
            {
                Command_Action transformCommand = new Command_Action
                {
                    defaultLabel = "Transform to Solar Pod",
                    defaultDesc = cooldownTicksRemaining > 0 
                        ? $"Transform this dryad into a solar pod.\n\nCooldown: {(cooldownTicksRemaining / 60000f):F1} days remaining"
                        : "Transform this dryad into a solar pod.",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/SolarPodTransform"),
                    action = cooldownTicksRemaining > 0 ? (System.Action)(() => { }) : () => TransformToBuilding()
                };
                
                if (cooldownTicksRemaining > 0)
                {
                    transformCommand.Disable("On cooldown");
                }
                
                yield return transformCommand;
                
                // Dev mode cooldown reset
                if (Prefs.DevMode && cooldownTicksRemaining > 0)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Reset Cooldown",
                        defaultDesc = "Reset the transformation cooldown (Dev mode only)",
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower"),
                        action = () => cooldownTicksRemaining = 0
                    };
                }
            }
        }
        
        private void TransformToBuilding()
        {
            Pawn pawn = parent as Pawn;
            if (pawn == null) return;
            
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            
            ThingDef buildingDef = DefDatabase<ThingDef>.GetNamed(Props.buildingDefName);
            Building building = (Building)ThingMaker.MakeThing(buildingDef);
            
            transformedBuilding = building;
            isTransformed = true;
            
            pawn.DeSpawn();
            GenSpawn.Spawn(building, position, map);
            
            CompSunkeeperTransformThing tracker = building.GetComp<CompSunkeeperTransformThing>();
            if (tracker != null)
            {
                tracker.originalPawn = pawn;
            }
        }
        
        public void TransformBackToDryad()
        {
            if (transformedBuilding == null) return;
            
            IntVec3 position = transformedBuilding.Position;
            Map map = transformedBuilding.Map;
            
            transformedBuilding.DeSpawn();
            
            Pawn pawn = parent as Pawn;
            if (pawn != null)
            {
                GenSpawn.Spawn(pawn, position, map);
            }
            
            isTransformed = false;
            transformedBuilding = null;
            cooldownTicksRemaining = Props.cooldownTicks;
        }
        
        public void OnBuildingDestroyed()
        {
            isTransformed = false;
            transformedBuilding = null;
            cooldownTicksRemaining = Props.cooldownTicks;
        }
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref cooldownTicksRemaining, "cooldownTicksRemaining", 0);
            Scribe_Values.Look(ref isTransformed, "isTransformed", false);
            Scribe_References.Look(ref transformedBuilding, "transformedBuilding");
        }
        
        public override string CompInspectStringExtra()
        {
            if (!isTransformed && cooldownTicksRemaining > 0)
            {
                float cooldownDays = cooldownTicksRemaining / 60000f;
                return $"Transform cooldown: {cooldownDays:F1}d";
            }
            else if (!isTransformed)
            {
                return "Ready to transform";
            }
            return null;
        }
    }
    
    public class CompProperties_SunkeeperTransformThing : CompProperties
    {
        public CompProperties_SunkeeperTransformThing()
        {
            compClass = typeof(CompSunkeeperTransformThing);
        }
    }
    
    public class CompSunkeeperTransformThing : ThingComp
    {
        public Pawn originalPawn;
        
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            
            if (originalPawn != null && previousMap != null)
            {
                CompSunkeeperTransform transformComp = originalPawn.GetComp<CompSunkeeperTransform>();
                if (transformComp != null)
                {
                    transformComp.OnBuildingDestroyed();
                    
                    IntVec3 spawnPos = parent.Position;
                    GenSpawn.Spawn(originalPawn, spawnPos, previousMap);
                }
            }
        }
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (originalPawn != null)
            {
                CompSunkeeperTransform transformComp = originalPawn.GetComp<CompSunkeeperTransform>();
                if (transformComp != null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Transform back to Dryad",
                        defaultDesc = "Transform this solar pod back into a dryad.",
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/SolarDryadTransform"),
                        action = () => transformComp.TransformBackToDryad()
                    };
                }
            }
        }
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref originalPawn, "originalPawn");
        }
    }
}