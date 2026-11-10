using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// The resurrection core's gauge, Toxin's Gizmo_ToxTank pattern over CompResurrectionCore: the bar
// is the charges held, ticked at each charge, under Biotech's own "Mech resurrection charges"
// title (a Keyed string of the hard dependency, so every language has it). For the player's
// wearers the drag handle sets TargetCharges, the level the pilot keeps the core at by fetching
// ammo unasked (Patches/CompApparelReloadable_Reload); it starts at zero, the recharge being the
// core's main supply, and an ordered reload ignores it. The colours are red, the core's warning
// light, against the family's canister green (Toxin) and medical blue (Medic): the fill, a
// lighter red on hover and a darker red for the handle, which Command_LoadCore takes for its
// background. Gizmo_Slider builds its bar textures and reads its target on first draw, so the
// comp keeps one instance and rebuilds it when the capacity or the settings generation changes.
public class Gizmo_ResurrectionCore : Gizmo_Slider
{
    private static readonly Color CoreFill = new(0.62f, 0.12f, 0.12f);
    private static readonly Color CoreHighlight = new(0.85f, 0.30f, 0.30f);
    internal static readonly Color CoreShade = new(0.40f, 0.07f, 0.07f);

    private static bool draggingBar;

    private readonly CompResurrectionCore core;

    public Gizmo_ResurrectionCore(CompResurrectionCore core)
    {
        this.core = core;
    }

    protected override float Target
    {
        get => (float)core.TargetCharges / core.MaxCharges;
        set => core.TargetCharges = Mathf.RoundToInt(value * core.MaxCharges);
    }

    protected override float ValuePercent => Mathf.Clamp01((float)core.Charges / core.MaxCharges);

    protected override string Title => "MechResurrectionCharges".Translate();

    protected override bool IsDraggable => core.ReloadsPerCharge && core.Wearer?.Faction == Faction.OfPlayer;

    protected override string BarLabel => core.LabelRemaining;

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
    // (the rate in charges per day and the wait for the next charge, or that the core is full,
    // or that it never recharges), the reload line (vanilla's own stat label over what a charge
    // costs, as the armor's stat sheet words it) and, where the bar drags, the level the handle
    // sets, then what each weight class costs and how fresh a corpse must be, read off the
    // ability def's extension.
    protected override string GetTooltip()
    {
        var tip = new StringBuilder();
        tip.Append("MechResurrectionCharges".Translate().Resolve().Colorize(ColoredText.TipSectionTitleColor));
        tip.Append(": ").Append(core.LabelRemaining);
        float rate = core.Props.chargesPerDay;
        if (rate <= 0f)
            tip.Append('\n').Append("OLWC_GaugeNoRegen".Translate().Resolve());
        else if (core.Full)
            tip.Append('\n').Append("OLWC_GaugeFull".Translate(rate.ToString("0.##")).Resolve());
        else
            tip.Append('\n').Append("OLWC_GaugeRegen".Translate(rate.ToString("0.##"), core.TicksToNextCharge.ToStringTicksToPeriod()).Resolve());
        if (core.ReloadsPerCharge)
        {
            tip.Append('\n').Append("Stat_Thing_ReloadPerCharge_Name".Translate(core.Props.ChargeNounArgument).Resolve().CapitalizeFirst());
            tip.Append(": ").Append(Command_LoadCore.CostLabel(core.AmmoDef, core.Props.ammoCountPerCharge));
        }
        if (IsDraggable)
        {
            tip.Append('\n').Append(core.TargetCharges > 0
                ? "OLWC_GaugeAutoReload".Translate(core.TargetCharges).Resolve()
                : "OLWC_GaugeNoAutoReload".Translate().Resolve());
        }
        ResurrectMechExtension extension = OLWC_DefOf.OLWC_ResurrectMech.GetModExtension<ResurrectMechExtension>();
        if (extension == null)
            return tip.ToString();
        tip.Append("\n\n").Append("OLWC_GaugeCosts".Translate().Resolve());
        foreach (MechChargeCosts cost in extension.costs)
        {
            if (cost.weightClass != null)
                tip.Append('\n').Append("OLWC_GaugeCost".Translate(cost.weightClass.LabelCap, cost.cost).Resolve());
        }
        tip.Append('\n').Append(extension.LimitsCorpseAge
            ? "OLWC_GaugeCorpseAge".Translate(Ability_ResurrectMech.PeriodText(extension.maxCorpseAgeTicks)).Resolve()
            : "OLWC_GaugeNoCorpseAge".Translate().Resolve());
        return tip.ToString();
    }
}
