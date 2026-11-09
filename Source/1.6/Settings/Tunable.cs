using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// One player-tunable number, held in the units the settings store it in (the def field's, or
// the ones they convert that field to), shown as they are or scaled by DisplayScale. Until the
// player moves its slider it is DefaultValue, read from the def field it overrides when defs
// load (the settings' LoadDefaultsFromDefs), so the def and its header stay the record of the
// shipped tuning and another mod's XML patch moves the default too. Only an override is saved;
// a slider landing within half a step of the default (on a banded row, on the stop nearest it)
// clears it, and a saved override outside the slider's range (saved before the range moved)
// reads as its nearest end.
// The file is shared across the warcasket family and must stay byte-identical between the mods
// but for the namespace and the key prefix; change it in one and copy it to the others.
public sealed class Tunable
{
    private const float Unset = -1f;

    // A row is the label, the slider and the value side by side, as a vanilla SliderLabeled row
    // is, with a gap below it so neighbouring rows read apart, indented so it sits under its
    // section header (Listing.Indent only moves the cursor and would push the row off the right)
    // and padded on the right so it does not run up against the scrollbar.
    // The value column is sized to the widest value a row can show, so the slider runs out to
    // it and the row ends near the window's edge; every format here is at most four digits and
    // a point, or fewer digits and a unit letter or "x" narrower than a digit ("2.75s", "x2.5"),
    // so "00.00" with the default suffix (as translated) bounds it.
    // The settings' summary and checkbox rows lay out with these constants too.
    internal const float RowHeight = 30f;
    internal const float RowGap = 6f;
    internal const float RowIndent = Listing.DefaultIndent;
    internal const float RowEndPad = Listing.DefaultIndent;
    private const float LabelShare = 0.4f;
    private const string WidestValue = "00.00";
    private const float SliderPad = 10f;

    private readonly string key;
    private readonly float min;
    private readonly float max;
    private readonly float step;
    private readonly Func<float, string> format;
    private float value = Unset;
    private (float upTo, float step)[] stepBands;
    private float[] stops;

    public Tunable(string key, float min, float max, float step, Func<float, string> format)
    {
        this.key = key;
        this.min = min;
        this.max = max;
        this.step = step;
        this.format = format;
        DefaultValue = Unset;
    }

    public float DefaultValue { get; set; }

    // Stored units per shown unit, for a row that stores one unit and shows another (a fuel row
    // storing chemfuel, so the saved number means the same with any tank, and showing the tank's
    // own fuel at its rate). The slider, the value, the default and the tooltip all show scaled;
    // Value and IntValue stay stored.
    public float DisplayScale { get; set; } = 1f;

    // Filled into the label as {0} when set (the name of the fuel a row counts in).
    public string LabelArg { get; set; }

    // Filled into the tooltip as {1} onward when set, after the default's {0}: figures the
    // tooltip quotes from other defs, read when the defaults are, so they follow whatever the
    // loaded mods make of those defs.
    public string[] DescArgs { get; set; }

    // Piecewise steps, for a row that wants a finer grid at the low end: each band covers the
    // values above the previous band's upTo (the minimum, for the first) up to its own, in
    // ascending order, the last ending at the maximum. A band's stops are the multiples of its
    // step within it, and the minimum is the first stop, so a band may start off its own grid
    // (ten seconds, then the half hours). The slider moves through the stops evenly, each one
    // as wide as the next, so a fine low band gets as much of the track as its stops count.
    // Null means the single step applies over the whole range. Bands are in stored units and
    // bypass StoredStep's whole-shown-unit rounding, so a banded row keeps DisplayScale at 1.
    public (float upTo, float step)[] StepBands
    {
        get => stepBands;
        set
        {
            stepBands = value;
            stops = value == null || value.Length == 0 ? null : Stops(min, max, value);
        }
    }

    public float Value => value < 0f ? DefaultValue : Mathf.Clamp(value, min, max);

    public int IntValue => Mathf.RoundToInt(Value);

    public void Reset() => value = Unset;

    public void ExposeData() => Scribe_Values.Look(ref value, key, Unset);

    // The stored value a slider released at shown lands on: the nearest step up from the
    // minimum, then clamped to the range. The step is the row's in stored units; a scale that
    // would make it a fraction of a shown unit rounds it to whole shown units instead, so a
    // scaled row never shows a value its format rounds away. Pure, for the tests.
    public static float Snap(float shown, float min, float max, float step, float scale)
    {
        float stored = Mathf.Clamp(shown * scale, min, max);
        float storedStep = StoredStep(step, scale);
        return Mathf.Clamp(min + Mathf.Round((stored - min) / storedStep) * storedStep, min, max);
    }

    public static float StoredStep(float step, float scale) =>
        scale == 1f ? step : Mathf.Max(1f, Mathf.Round(step / scale)) * scale;

    // The stops of a banded row, ascending, by StepBands' rule. A tolerance of a thousandth of
    // a step keeps float error from dropping a stop on a band's end or doubling one on its
    // start. Pure, for the tests.
    public static float[] Stops(float min, float max, (float upTo, float step)[] bands)
    {
        var stops = new List<float> { min };
        float start = min;
        for (int i = 0; i < bands.Length && start < max; i++)
        {
            float end = i == bands.Length - 1 ? max : Mathf.Min(bands[i].upTo, max);
            float step = bands[i].step;
            for (int k = Mathf.FloorToInt(start / step + 1e-3f) + 1; k * step <= end + step * 1e-3f; k++)
                stops.Add(k * step);
            start = end;
        }
        return stops.ToArray();
    }

    // The index of the stop nearest a stored value, the lower on a tie. Pure, for the tests.
    public static int Nearest(float[] stops, float stored)
    {
        int best = 0;
        for (int i = 1; i < stops.Length; i++)
            if (Mathf.Abs(stops[i] - stored) < Mathf.Abs(stops[best] - stored))
                best = i;
        return best;
    }

    // Label "OLWC_<key>" (with any unit, unless the format shows it; or LabelArg) on the left,
    // the slider in the middle, the current value on the right with "(default)" while unset,
    // and "OLWC_<key>Desc" with the default and any DescArgs as the whole row's tooltip.
    public void DoRow(Listing_Standard listing)
    {
        Rect row = listing.GetRect(RowHeight);
        row.xMin += RowIndent;
        row.xMax -= RowEndPad;
        Rect labelRect = row.LeftPart(LabelShare);
        float valueWidth = Text.CalcSize(WidestValue + "OLWC_DefaultSuffix".Translate()).x;
        var valueRect = new Rect(row.xMax - valueWidth, row.y, valueWidth, row.height);
        var sliderRect = new Rect(labelRect.xMax + SliderPad, row.y,
            valueRect.x - labelRect.xMax - 2f * SliderPad, row.height);

        Widgets.DrawHighlightIfMouseover(row);
        TooltipHandler.TipRegion(row, Tooltip());

        string shown = Format(Value);
        if (value < 0f)
            shown += "OLWC_DefaultSuffix".Translate();
        TextAnchor anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(labelRect, LabelArg == null ? ("OLWC_" + key).Translate() : ("OLWC_" + key).Translate(LabelArg));
        Widgets.Label(valueRect, shown);
        Text.Anchor = anchor;

        float snapped;
        bool isDefault;
        if (stops == null)
        {
            float current = Value / DisplayScale;
            float raw = Widgets.HorizontalSlider(sliderRect, current, min / DisplayScale, max / DisplayScale, middleAlignment: true);
            listing.Gap(RowGap);
            if (raw == current)
                return;
            snapped = Snap(raw, min, max, step, DisplayScale);
            isDefault = Mathf.Abs(snapped - DefaultValue) < StoredStep(step, DisplayScale) * 0.5f;
        }
        else
        {
            // The slider runs over stop indices; a value off the stops (an off-grid default or
            // an old save) sits at its nearest one until moved.
            int current = Nearest(stops, Value);
            int index = Mathf.RoundToInt(Widgets.HorizontalSlider(sliderRect, current, 0f, stops.Length - 1, middleAlignment: true));
            listing.Gap(RowGap);
            if (index == current)
                return;
            snapped = stops[index];
            isDefault = index == Nearest(stops, DefaultValue);
        }
        value = isDefault ? Unset : snapped;
    }

    private string Format(float stored) => format(stored / DisplayScale);

    private string Tooltip()
    {
        string descKey = "OLWC_" + key + "Desc";
        if (DescArgs == null)
            return descKey.Translate(Format(DefaultValue));
        var args = new NamedArgument[DescArgs.Length + 1];
        args[0] = Format(DefaultValue);
        for (int i = 0; i < DescArgs.Length; i++)
            args[i + 1] = DescArgs[i];
        return descKey.Translate(args);
    }
}
