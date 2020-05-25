using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ImmersiveResearch
{
    [StaticConstructorOnStartup]
    public static class ImmersiveResearchTextures
    {
        public static readonly Texture2D ResearchSizeSmallIcon = ContentFinder<Texture2D>.Get("UI/Icons/small");
        public static readonly Texture2D ResearchSizeMediumIcon = ContentFinder<Texture2D>.Get("UI/Icons/medium");
        public static readonly Texture2D ResearchSizeLargeIcon = ContentFinder<Texture2D>.Get("UI/Icons/large");
        public static readonly Texture2D ResearchSizeEssentialIcon = ContentFinder<Texture2D>.Get("UI/Icons/essential");
        public static readonly Texture2D ResearchSizeModIcon = ContentFinder<Texture2D>.Get("UI/Icons/mod");

        public static readonly Texture2D MechanicalIcon = ContentFinder<Texture2D>.Get("UI/Icons/mechanical");
        public static readonly Texture2D BiologicalIcon = ContentFinder<Texture2D>.Get("UI/Icons/biological");
        public static readonly Texture2D TextilesIcon = ContentFinder<Texture2D>.Get("UI/Icons/textiles");
        public static readonly Texture2D CulturalIcon = ContentFinder<Texture2D>.Get("UI/Icons/Cultural");
        public static readonly Texture2D ConstructionIcon = ContentFinder<Texture2D>.Get("UI/Icons/Construction");
        public static readonly Texture2D MetallurgyIcon = ContentFinder<Texture2D>.Get("UI/Icons/metallurgy");
        public static readonly Texture2D WeaponryIcon = ContentFinder<Texture2D>.Get("UI/Icons/weaponry");
        public static readonly Texture2D ElectricalIcon = ContentFinder<Texture2D>.Get("UI/Icons/electrical");
        public static readonly Texture2D MedicalIcon = ContentFinder<Texture2D>.Get("UI/Icons/medical");
        public static readonly Texture2D AdvancedIcon = ContentFinder<Texture2D>.Get("UI/Icons/advanced");
        public static readonly Texture2D SpacerIcon = ContentFinder<Texture2D>.Get("UI/Icons/spacer");
        public static readonly Texture2D UltratechIcon = ContentFinder<Texture2D>.Get("UI/Icons/ultratech");
        public static readonly Texture2D SpacecraftIcon = ContentFinder<Texture2D>.Get("UI/Icons/spacecraft");
        public static readonly Texture2D ModIcon = ContentFinder<Texture2D>.Get("UI/Icons/mod");
    }
}
