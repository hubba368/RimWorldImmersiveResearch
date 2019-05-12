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
        //UNUSED
        private static Vector2 _scrollPosition;
        private static float _listHeight;
        private static List<LoreDrawEntry> _cachedDrawEntries = new List<LoreDrawEntry>();
        private static LoreDrawEntry _selectedEntry;
        private static LoreDrawEntry _mousedOverEntry;
        
        // fill up cachedEntries list with our Things
        public static void DrawLoreFullList(Rect inRect, IEnumerable<PawnKindDef> thingList)
        {
            if (LoreWindowDrawingUtility._cachedDrawEntries.NullOrEmpty<LoreDrawEntry>())
            {
                foreach(PawnKindDef pawn in thingList)
                {
                    string label = pawn.label;
                    string desc = "This is a " + label + " entity.";
                    LoreDrawEntry newEntry = new LoreDrawEntry(label, desc);
                    LoreWindowDrawingUtility._cachedDrawEntries.Add(newEntry);
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
            Rect viewRect = new Rect(0.0f, 0.0f, rect1.width - 16f, _listHeight);
            Widgets.BeginScrollView(rect1, ref _scrollPosition, viewRect, true);
            float curY = 0.0f;
            string entryName = (string)null;
            _mousedOverEntry = (LoreDrawEntry)null;

            for(int index = 0; index < LoreWindowDrawingUtility._cachedDrawEntries.Count; ++index)
            {
                Action<LoreDrawEntry> mouseClickEvent = MouseClickCallBackEvent;
                Action<LoreDrawEntry> mouseOverEvent = MouseOverCallBackEvent;

                curY += LoreWindowDrawingUtility._cachedDrawEntries[index].Draw(8f, curY, viewRect.width - 8f,
                   false, mouseClickEvent, mouseOverEvent, LoreWindowDrawingUtility._scrollPosition, rect1);
            }

            LoreWindowDrawingUtility._listHeight = curY + 100f;
            Widgets.EndScrollView();
            Rect rect3 = rect2.ContractedBy(10f);
            GUI.BeginGroup(rect3);
            LoreDrawEntry loreEntry = _selectedEntry ?? _mousedOverEntry ?? _cachedDrawEntries.FirstOrDefault<LoreDrawEntry>();

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
            LoreWindowDrawingUtility._selectedEntry = entry;
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
            LoreWindowDrawingUtility._mousedOverEntry = r;
        }
    }
}
