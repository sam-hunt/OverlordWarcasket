using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// The resurrection core's gauge, Toxin's Gizmo_ToxTank pattern over CompResurrectionCore: the bar
// is the charges held, ticked at each charge, under Biotech's own "Mech resurrection charges"
// title (a Keyed string of the hard dependency, so every language has it). Nothing reloads the
// core and the regen has no target, so the bar is not draggable; the handle colour is still set
// for the base's texture build. The colours are red, the core's warning light, against the family's
// canister green (Toxin) and medical blue (Medic): the fill, a lighter red on hover and a darker
// red for the handle. Gizmo_Slider builds its bar textures on first draw, so the comp keeps one
// instance and rebuilds it when the capacity or the settings generation changes.
public class Gizmo_ResurrectionCore : Gizmo_Slider
{
    private static readonly Color CoreFill = new(0.62f, 0.12f, 0.12f);
    private static readonly Color CoreHighlight = new(0.85f, 0.30f, 0.30f);
    private static readonly Color CoreShade = new(0.40f, 0.07f, 0.07f);

    private static bool draggingBar;

    private readonly CompResurrectionCore core;

    public Gizmo_ResurrectionCore(CompResurrectionCore core)
    {
        this.core = core;
    }

    protected override float Target
    {
        get => 1f;
        set { }
    }

    protected override float ValuePercent => Mathf.Clamp01((float)core.Charges / core.MaxCharges);

    protected override string Title => "MechResurrectionCharges".Translate();

    protected override bool IsDraggable => false;

    protected override string BarLabel => core.LabelCharges;

    protected override int Increments => core.MaxCharges;

    protected override Color BarColor => CoreFill;

    protected override Color BarHighlightColor => CoreHighlight;

    protected override Color BarDragColor => CoreShade;

    protected override bool DraggingBar
    {
        get => draggingBar;
        set => draggingBar = value;
    }

    protected override IEnumerable<float> GetBarThresholds()
    {
        int max = core.MaxCharges;
        for (int i = 1; i < max; i++)
            yield return (float)i / max;
    }

    // Laid out as vanilla's hemogen gauge: the count under a tinted header, the recharge line
    // (the rate as a percentage of the capacity per day and the wait for the next charge, or
    // that the core is full, or that it never recharges), then what each weight class costs and
    // how fresh a corpse must be, read off the ability def's extension.
    protected override string GetTooltip()
    {
        var tip = new StringBuilder();
        tip.Append("MechResurrectionCharges".Translate().Resolve().Colorize(ColoredText.TipSectionTitleColor));
        tip.Append(": ").Append(core.LabelCharges);
        float rate = core.Props.regenPerDay;
        if (rate <= 0f)
            tip.Append('\n').Append("OLWC_GaugeNoRegen".Translate().Resolve());
        else if (core.Full)
            tip.Append('\n').Append("OLWC_GaugeFull".Translate(rate.ToStringPercent()).Resolve());
        else
            tip.Append('\n').Append("OLWC_GaugeRegen".Translate(rate.ToStringPercent(), core.TicksToNextCharge.ToStringTicksToPeriod()).Resolve());
        ResurrectMechExtension extension = OLWC_DefOf.OLWC_ResurrectMech.GetModExtension<ResurrectMechExtension>();
        if (extension == null)
            return tip.ToString();
        tip.Append("\n\n").Append("OLWC_GaugeCosts".Translate().Resolve());
        foreach (MechChargeCosts cost in extension.costs)
        {
            if (cost.weightClass != null)
                tip.Append('\n').Append("OLWC_GaugeCost".Translate(cost.weightClass.LabelCap, cost.cost).Resolve());
        }
        tip.Append('\n').Append("OLWC_GaugeCorpseAge".Translate(extension.maxCorpseAgeTicks.ToStringTicksToPeriod()).Resolve());
        return tip.ToString();
    }
}
