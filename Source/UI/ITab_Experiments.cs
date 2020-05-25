using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ImmersiveResearch
{
    public class ITab_Experiments : ITab_Bills
    {
        private float _viewHeight = 1000f;

        private Vector2 _scrollPosition = default(Vector2);

        private Experiment _mouseoverBill;

        private static readonly Vector2 _winSize = new Vector2(420f, 480f);

        protected Building_ExperimentBench SelectedExperimentTable => (Building_ExperimentBench)base.SelThing;

        public ITab_Experiments()
        {
            size = _winSize;
            labelKey = "TabBills";
            tutorTag = "Experiments";
        }

        protected override void FillTab()
        {
            Vector2 windowSize = _winSize;
            Rect rect1 = new Rect(0f, 0f, windowSize.x, windowSize.y).ContractedBy(10f);
             _mouseoverBill = SelectedExperimentTable.ExpStack.DoExperimentListing(rect1, SelectedExperimentTable, ref _scrollPosition, ref _viewHeight);           
        }


        public override void TabUpdate()
        {
            base.TabUpdate();
        }
    }
}
