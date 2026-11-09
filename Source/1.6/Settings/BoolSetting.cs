using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// One player-tunable switch, what a Tunable is for a number: until the player flips it, it is
// DefaultValue, read from the def field it overrides when defs load (the settings'
// LoadDefaultsFromDefs), so the def and its header stay the record and another mod's XML patch
// moves the default too. Only an override is saved, under the key; flipping the box back to its
// default clears it. The row's label and tooltip are "OLWC_" + key and that with "Desc", as a
// Tunable's are, the tooltip taking the default's word in the box's colour.
public sealed class BoolSetting
{
    // The default's word in the tooltip, in the colours of the box's states: pure green for the
    // tick, vanilla's readable red for the cross.
    private static readonly Color EnabledColor = Color.green;
    private static readonly Color DisabledColor = ColorLibrary.RedReadable;

    private readonly string key;
    private bool? value;

    public BoolSetting(string key)
    {
        this.key = key;
    }

    public bool DefaultValue { get; set; }

    public bool Value => value ?? DefaultValue;

    public void Reset() => value = null;

    public void ExposeData()
    {
        bool saved = value ?? DefaultValue;
        bool isSet = value.HasValue;
        Scribe_Values.Look(ref isSet, key + "Set");
        Scribe_Values.Look(ref saved, key);
        if (Scribe.mode == LoadSaveMode.LoadingVars)
            value = isSet ? saved : null;
    }

    // Laid out as a Tunable row: the label on the left, the box at the row's end; a click
    // anywhere on the row flips it. The tooltip is "OLWC_<key>Desc" with the default as its
    // own paragraph in the box's colour, resolved by hand since a TaggedString handed over as
    // a string loses its colour tags.
    public void DoRow(Listing_Standard listing)
    {
        Rect row = listing.GetRect(Tunable.RowHeight);
        row.xMin += Tunable.RowIndent;
        row.xMax -= Tunable.RowEndPad;
        Widgets.DrawHighlightIfMouseover(row);
        string state = DefaultValue
            ? "OLWC_SwitchOn".Translate().Colorize(EnabledColor)
            : "OLWC_SwitchOff".Translate().Colorize(DisabledColor);
        TooltipHandler.TipRegion(row, ("OLWC_" + key + "Desc").Translate().Resolve() + "\n\n" + "OLWC_SwitchDefault".Translate(state).Resolve());

        bool on = Value;
        Widgets.CheckboxLabeled(row, ("OLWC_" + key).Translate(), ref on);
        value = on == DefaultValue ? null : on;
        listing.Gap(Tunable.RowGap);
    }
}
