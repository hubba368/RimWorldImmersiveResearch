using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace ImmersiveResearch
{
    public class Experiment : Bill_Production
    {
        [Unsaved]
        public ExperimentStack expStack;

        private int loadID = -1;

        public new bool DeletedOrDereferenced
        {
            get
            {
                if (deleted)
                {
                    return true;
                }
                Thing thing = expStack.billGiver as Thing;
                if (thing != null && thing.Destroyed)
                {
                    return true;
                }
                return false;
            }
        }

        public Pawn uniquePawnDoer;
        public Thing uniqueThingIng;

        public Experiment()
        {
        }

        public Experiment(RecipeDef recipe)
        {
            this.recipe = recipe;
        }

        // unfortunate how it is not virtual in base class
        // TODO: create TexButton icons (RW UI textures are internal)
        public new Rect DoInterface(float x, float y, float width, int index)
        {
            Rect rect = new Rect(x, y, width, 53f);
            float num = 0f;
            if (!StatusString.NullOrEmpty())
            {
                num = Mathf.Max(17f, StatusLineMinHeight);
            }
            rect.height += num;
            Color color = Color.white;

            GUI.color = color;
            Text.Font = GameFont.Small;
            if (index % 2 == 0)
            {
                Widgets.DrawAltRect(rect);
            }
            GUI.BeginGroup(rect);
            Rect rect2 = new Rect(0f, 0f, 24f, 24f);
            if (expStack.IndexOf(this) > 0)
            {
                if (Widgets.ButtonText(rect2, "U"))
                {
                    expStack.Reorder(this, -1);
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }
                TooltipHandler.TipRegion(rect2, "ReorderBillUpTip".Translate());
            }
            if (expStack.IndexOf(this) < expStack.ListCount - 1)
            {
                Rect rect3 = new Rect(0f, 24f, 24f, 24f);
                if (Widgets.ButtonText(rect3, "D"))
                {
                    expStack.Reorder(this, 1);
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                }
                TooltipHandler.TipRegion(rect3, "ReorderBillDownTip".Translate());
            }
            Rect rect4 = new Rect(28f, 0f, rect.width - 48f - 20f, 24f);
            Widgets.Label(rect4, LabelCap);

            if(uniquePawnDoer != null)
            {
                Rect pawnBillDoerRect = new Rect(28f, 20f, rect.width - 48f - 20f, 24f);
                Widgets.Label(pawnBillDoerRect, "Reserved for: " + uniquePawnDoer.Name.ToString());
            }


            
            Rect rect5 = new Rect(300f, 0f, 70f, 24f);
            if (Widgets.ButtonText(rect5, "Delete"))
            {
                expStack.Delete(this);
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            TooltipHandler.TipRegion(rect5, "DeleteBillTip".Translate());

            Rect rect6 = new Rect(300f, 26f, 70f, 24f);
            //rect6.x -= rect6.width + 4f;
            if (Widgets.ButtonText(rect6, "Suspend"))
            {
                // in RW, setting a bill as suspended will only take effect if you stop the pawn from doing the bill job,
                // for example, if a pawn is doing a bill and you suspend it, it will keep doing the bill until you manually stop them or they stop themselves.
                suspended = !suspended;
                expStack.SetSuspended(this, suspended);
            }
            TooltipHandler.TipRegion(rect6, "SuspendBillTip".Translate());


            /* if (!StatusString.NullOrEmpty())
             {
                 Text.Font = GameFont.Tiny;
                 Rect rect8 = new Rect(24f, rect.height - num, rect.width - 24f, num);
                 Widgets.Label(rect8, StatusString);
                 //DoStatusLineInterface(rect8);
             }*/
            GUI.EndGroup();
            if (suspended)
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Rect rect9 = new Rect(rect.x + rect.width / 2f - 70f, rect.y + rect.height / 2f - 20f, 140f, 40f);
                GUI.DrawTexture(rect9, TexUI.GrayTextBG);
                Widgets.Label(rect9, "SuspendedCaps".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            }
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            return rect;
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            /*
            if (billStack.billGiver.LabelShort == "Experiment Bench")
            {
                // delete the experiment instance from expStack when Bill is completed
                Building_ExperimentBench bench = (Building_ExperimentBench)expStack.billGiver;
                int index = bench.ExpStack.IndexOfBillToExp(billDoer.CurJob.bill);
                Experiment Exp = (Experiment)bench.ExpStack[index];
                Log.Error(Exp.Label);
                bench.ExpStack.Delete(Exp);
            }
            if (billStack.billGiver.LabelShort == "Study Table")
            {
                // delete the experiment instance from expStack when Bill is completed
                Building_StudyTable bench = (Building_StudyTable)expStack.billGiver;
                int index = bench.ExpStack.IndexOfBillToExp(billDoer.CurJob.bill);
                Experiment Exp = (Experiment)bench.ExpStack[index];
                Log.Error(Exp.Label);
                bench.ExpStack.Delete(Exp);
            }*/
        }

        public new string GetUniqueLoadID()
        {
            return "Experiment_" + recipe.defName + "_" + loadID;
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref recipe, "recipe");
            Scribe_Values.Look(ref suspended, "suspended", defaultValue: false);
        }

        public override bool ShouldDoNow()
        {
            return base.ShouldDoNow();
        }
    }
}
