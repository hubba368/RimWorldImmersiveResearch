using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ImmersiveResearch
{
    /// <summary>
    ///  Custom implementation of RimWorld class 'StatsReportUtility'.
    /// </summary>
    public static class LoreWindowDrawingUtility
    {
        private static Vector2 scrollPosition;
        private static float listHeight;
        private static List<LoreDrawEntry> cachedDrawEntries = new List<LoreDrawEntry>();
        private static LoreDrawEntry selectedEntry;
        private static LoreDrawEntry mousedOverEntry;
        
        // fill up cachedEntries list with our Things
        public static void DrawLoreFullList(Rect inRect, IEnumerable<PawnKindDef> thingList)
        {
            if (LoreWindowDrawingUtility.cachedDrawEntries.NullOrEmpty<LoreDrawEntry>())
            {
                foreach(PawnKindDef pawn in thingList)
                {
                    string label = pawn.label;
                    string desc = "This is a " + label + " entity.";
                    LoreDrawEntry newEntry = new LoreDrawEntry(label, desc);
                    LoreWindowDrawingUtility.cachedDrawEntries.Add(newEntry);
                }
            }
            DrawLoreListWorker(inRect);
        }

        // Draw our list to the UI
        private static void DrawLoreListWorker(Rect refRect)
        {
            Rect rect1 = new Rect(refRect);
            rect1.width *= 0.5f;
            Rect rect2 = new Rect(refRect);
            rect2.x = rect1.xMax;
            rect2.width = refRect.xMax - rect2.x;
            Text.Font = GameFont.Small;
            Rect viewRect = new Rect(0.0f, 0.0f, rect1.width - 16f, listHeight);
            Widgets.BeginScrollView(rect1, ref scrollPosition, viewRect, true);
            float curY = 0.0f;
            string entryName = (string)null;
            mousedOverEntry = (LoreDrawEntry)null;

            for(int index = 0; index < LoreWindowDrawingUtility.cachedDrawEntries.Count; ++index)
            {
                Action<LoreDrawEntry> mouseClickEvent = MouseClickCallBackEvent;
                Action<LoreDrawEntry> mouseOverEvent = MouseOverCallBackEvent;

                curY += LoreWindowDrawingUtility.cachedDrawEntries[index].Draw(8f, curY, viewRect.width - 8f,
                   false, mouseClickEvent, mouseOverEvent, LoreWindowDrawingUtility.scrollPosition, rect1);
            }

            LoreWindowDrawingUtility.listHeight = curY + 100f;
            Widgets.EndScrollView();
            Rect rect3 = rect2.ContractedBy(10f);
            GUI.BeginGroup(rect3);
            LoreDrawEntry loreEntry = selectedEntry ?? mousedOverEntry ?? cachedDrawEntries.FirstOrDefault<LoreDrawEntry>();

            if (loreEntry != null)
            {
                // draw descriptions etc (text on the right side of the OG inspection window)
                Widgets.Label(rect3.AtZero(), loreEntry.EntryBasicDesc);
            }
            GUI.EndGroup();
        }


        private static void SelectEntry(LoreDrawEntry entry, bool playSound = true)
        {
            Log.Error("attempting click", false);
            LoreWindowDrawingUtility.selectedEntry = entry;
            if (!playSound)
            {
                return;
            }
            else
            {
                return;
            }
        }


        // Mouse events 
        private static void MouseClickCallBackEvent(LoreDrawEntry r)
        {
            SelectEntry(r, true);
        }

        private static void MouseOverCallBackEvent(LoreDrawEntry r)
        {
            LoreWindowDrawingUtility.mousedOverEntry = r;
        }
    }
}
