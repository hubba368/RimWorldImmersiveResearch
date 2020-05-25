using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace ImmersiveResearch
{
    class ResearchCompProperties : CompProperties
    {
        public ResearchCompProperties()
        {
            this.compClass = typeof(ResearchThingComp);
        }

        public ResearchCompProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
}
