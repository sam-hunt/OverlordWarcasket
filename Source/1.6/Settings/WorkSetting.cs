using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// The pilot's work: one checkbox per work type the trait mirror leaves to the player
// (WorkProfile.Toggleable, the openable work types), what a Tunable is for a number. Until the
// player flips one it is WorkProfile.OpenByDefault, the fixed rule the mirror applies on its
// own (Research and Smithing open, Crafting, Tailoring and Dark study blocked, each open
// regardless once VFEP opens it),
// read live so the default follows whatever the loaded mods make of VFEP's trait. Only an
// override is saved, by the work type's defName, so it outlives the def instances a play-data
// reload replaces and a work type whose DLC or mod is off keeps its override for when it is
// back; flipping a box back to its default clears it. Written onto the trait by the settings'
// ApplyToDefs through WorkProfile.Apply.
public sealed class WorkSetting
{
    private const string Key = "Work";

    // The default's word in the tooltip, in the colours of the box's states: pure green for
    // the tick, vanilla's readable red for the cross.
    private static readonly Color EnabledColor = Color.green;
    private static readonly Color DisabledColor = ColorLibrary.RedReadable;

    private Dictionary<string, bool> overrides = [];

    public bool IsOpen(WorkTypeDef work) =>
        overrides.TryGetValue(work.defName, out bool open) ? open : WorkProfile.OpenByDefault(work);

    // The toggleable work types the pilot may take on, in WorkProfile.Toggleable's order.
    public IEnumerable<WorkTypeDef> Open => WorkProfile.Toggleable.Where(IsOpen);

    // The work tab's name for a work type; a modded one without it shows its defName.
    public static string Name(WorkTypeDef work) =>
        work.labelShort.NullOrEmpty() ? work.defName : work.labelShort;

    public void Reset() => overrides.Clear();

    public void ExposeData()
    {
        Scribe_Collections.Look(ref overrides, Key, LookMode.Value, LookMode.Value);
        overrides ??= [];
    }

    // One checkbox row per toggleable work type, laid out as a Tunable row is: the work tab's
    // name for it on the left and the box at the row's end; a click anywhere on the row flips
    // it. No "(default)" marker on the row: the whole row's tooltip is the work type's own
    // description, then "OLWC_WorkNote_<defName>" where a work type hides work its description
    // does not name (Research carries hacking, Smithing the mech work), "OLWC_WorkDesc", then
    // the default as its own paragraph with its state's word in the box's colour.
    public void DoRows(Listing_Standard listing)
    {
        foreach (WorkTypeDef work in WorkProfile.Toggleable)
            DoRow(listing, work);
    }

    private void DoRow(Listing_Standard listing, WorkTypeDef work)
    {
        Rect row = listing.GetRect(Tunable.RowHeight);
        row.xMin += Tunable.RowIndent;
        row.xMax -= Tunable.RowEndPad;
        Widgets.DrawHighlightIfMouseover(row);
        bool byDefault = WorkProfile.OpenByDefault(work);
        TooltipHandler.TipRegion(row, Tooltip(work, byDefault));

        bool open = IsOpen(work);
        Widgets.CheckboxLabeled(row, Name(work).CapitalizeFirst(), ref open);
        if (open == byDefault)
            overrides.Remove(work.defName);
        else
            overrides[work.defName] = open;
        listing.Gap(Tunable.RowGap);
    }

    // Resolved by hand: a TaggedString handed over as a string loses its tags (the implicit
    // conversion strips them), colour tags included.
    private static string Tooltip(WorkTypeDef work, bool byDefault)
    {
        string state = byDefault
            ? "OLWC_WorkOpen".Translate().Colorize(EnabledColor)
            : "OLWC_WorkBlocked".Translate().Colorize(DisabledColor);
        string ours = "OLWC_WorkDesc".Translate().Resolve() + "\n\n" + "OLWC_WorkDefault".Translate(state).Resolve();
        string note = "OLWC_WorkNote_" + work.defName;
        if (note.CanTranslate())
            ours = note.Translate().Resolve() + "\n\n" + ours;
        return work.description.NullOrEmpty() ? ours : work.description + "\n\n" + ours;
    }
}
