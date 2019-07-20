using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace ImmersiveResearch
{
    public class Experiment : IExposable, ILoadReferenceable
    {
        [Unsaved]
        public ExperimentStack expStack;

        private int loadID = -1;

        public RecipeDef recipe;

        public bool suspended;

        public ThingFilter ingredientFilter;

        public float ingredientSearchRadius = 999f;

        public IntRange allowedSkillRange = new IntRange(0, 20);

        public Pawn pawnRestriction;

        public bool deleted;

        public int lastIngredientSearchFailTicks = -99999;

        public const int MaxIngredientSearchRadius = 999;

        public const float ButSize = 24f;

        private const float InterfaceBaseHeight = 53f;

        private const float InterfaceStatusLineHeight = 17f;

        public Map Map => expStack.BillGiver.Map;

        public virtual string Label => recipe.label;

        public virtual string LabelCap => Label.CapitalizeFirst();

        public virtual bool CheckIngredientsIfSociallyProper => true;

        public virtual bool CompletableEver => true;

        protected virtual string StatusString => null;

        protected virtual float StatusLineMinHeight => 0f;

        protected virtual bool CanCopy => true;

        public bool DeletedOrDereferenced
        {
            get
            {
                if (deleted)
                {
                    return true;
                }
                Thing thing = expStack.BillGiver as Thing;
                if (thing != null && thing.Destroyed)
                {
                    return true;
                }
                return false;
            }
        }


        public Experiment()
        {
        }

        public Experiment(RecipeDef recipe)
        {
            this.recipe = recipe;
        }

        // copied from Bill implementation (Draws an instance in the experiment window)
        public Rect DoInterface(float x, float y, float width, int index)
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
            Rect rect4 = new Rect(28f, 0f, rect.width - 48f - 20f, rect.height + 5f);
            Widgets.Label(rect4, LabelCap);
            //DoConfigInterface(rect.AtZero(), color);
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
                this.suspended = !this.suspended;
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

        public string GetUniqueLoadID()
        {
            return "Experiment_" + recipe.defName + "_" + loadID;
        }

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref recipe, "recipe");
            Scribe_Values.Look(ref suspended, "suspended", defaultValue: false);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
            }
        }
    }
}
