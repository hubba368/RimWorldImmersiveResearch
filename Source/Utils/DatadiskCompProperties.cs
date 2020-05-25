using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace ImmersiveResearch
{
    class DatadiskCompProperties : CompProperties
    {
        public DatadiskCompProperties()
        {
            this.compClass = typeof(DatadiskCompProperties);
        }

        public DatadiskCompProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
}
