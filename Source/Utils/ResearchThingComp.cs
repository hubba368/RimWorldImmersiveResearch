using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ImmersiveResearch
{
    class ResearchThingComp : ThingComp
    {
        public ResearchCompProperties Properties => (ResearchCompProperties)this.Properties;

        public string pawnExperimentAuthorName;
        public ResearchProjectDef researchDef;
        public string researchDefName;

        public override void CompTick()
        {
            base.CompTick();
        }

        public void AddPawnAuthor(string author)
        {
            pawnExperimentAuthorName = author;
        }

        public void AddResearch(string projDef)
        {
            //Properties.researchDef = projDef;
            researchDefName = projDef;
        }
    }
}
