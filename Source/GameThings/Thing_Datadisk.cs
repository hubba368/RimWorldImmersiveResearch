using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ImmersiveResearch
{
    class Thing_Datadisk : ThingWithComps
    {
        private string _uniqueDescription;

        public string UniqueDescription { get => _uniqueDescription; }

        public override void PostMake()
        {
            base.PostMake();
            this.GetComp<DatadiskThingComp>().datadiskDescription = LoreComputerHarmonyPatches.GenerateRandomDatadiskDescription(this);
            _uniqueDescription = this.GetComp<DatadiskThingComp>().datadiskDescription;
        }

        public override string GetInspectString()
        {
            if (this.TryGetComp<DatadiskThingComp>() == null)
            {
                Log.Error("datadisk thing comp is null.");
                return "";
            }

            StringBuilder sBuilder = new StringBuilder();

            sBuilder.Append("Datadisk Contents: " + "\n");
            sBuilder.Append(_uniqueDescription);
            return sBuilder.ToString();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _uniqueDescription, "datadiskDescription");
        }
    }
}
