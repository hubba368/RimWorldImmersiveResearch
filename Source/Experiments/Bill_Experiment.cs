using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ImmersiveResearch
{
    public class Bill_Experiment : Experiment, IExposable
    {
        private BillStoreModeDef storeMode = BillStoreModeDefOf.BestStockpile;

        private Zone_Stockpile storeZone;

        public Zone_Stockpile includeFromZone;

        public FloatRange hpRange = FloatRange.ZeroToOne;

        public QualityRange qualityRange = QualityRange.All;

        public bool limitToAllowedStuff;

        public bool paused;

        public Bill_Experiment(RecipeDef recipe)
            : base(recipe)
        {
        }

        /*protected override void DoConfigInterface(Rect baseRect, Color baseColor)
        {
            Rect rect = new Rect(28f, 32f, 100f, 30f);
            GUI.color = new Color(1f, 1f, 1f, 0.65f);
            Widgets.Label(rect, RepeatInfoText);
            GUI.color = baseColor;
            WidgetRow widgetRow = new WidgetRow(baseRect.xMax, baseRect.y + 29f, UIDirection.LeftThenUp);
            if (widgetRow.ButtonText("Details".Translate() + "..."))
            {
                Find.WindowStack.Add(new Dialog_ExperimentConfig(this, ((Thing)billStack.billGiver).Position));
            }
            base.DoConfigInterface(baseRect, baseColor);
        }

        public override bool ShouldDoNow()
        {
            return false;
        }*/

    }
}
