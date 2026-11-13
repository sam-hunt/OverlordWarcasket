using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// The mech resurrector's gauge, Toxin's Gizmo_ToxTank pattern over CompMechResurrector: the bar
// is the charges held, ticked at each charge, under Biotech's own "Mech resurrection charges"
// title (a Keyed string of the hard dependency, so every language has it). The fill and the
// count include the part charge the recharge has accrued ("3.4 / 10"), so the bar creeps toward
// the next tick and the count says how far off the next charge is. For the player's
// wearers the drag handle sets TargetCharges, the level the pilot keeps the resurrector at by
// fetching ammo unasked (Patches/CompApparelReloadable_Reload); it starts at zero, the recharge
// being the resurrector's main supply, and an ordered reload ignores it. The colours are red, the
// resurrector's warning light, against the family's canister green (Toxin) and medical blue
// (Medic): the fill, a lighter red on hover and a darker red for the handle, which
// Command_LoadResurrector takes for its background. Gizmo_Slider builds its bar textures and reads
// its target on first draw, so the comp keeps one instance and rebuilds it when the capacity or the
// settings generation changes.
public class Gizmo_MechResurrector : Gizmo_Slider
{
    private static readonly Color ResurrectorFill = new(0.62f, 0.12f, 0.12f);
    private static readonly Color ResurrectorHighlight = new(0.85f, 0.30f, 0.30f);
    internal static readonly Color ResurrectorShade = new(0.40f, 0.07f, 0.07f);

    private static bool draggingBar;

    private readonly CompMechResurrector resurrector;

    public Gizmo_MechResurrector(CompMechResurrector resurrector)
    {
        this.resurrector = resurrector;
    }

    protected override float Target
    {
        get => (float)resurrector.TargetCharges / resurrector.MaxCharges;
        set => resurrector.TargetCharges = Mathf.RoundToInt(value * resurrector.MaxCharges);
    }

    protected override float ValuePercent => Mathf.Clamp01(resurrector.ChargesWithProgress / resurrector.MaxCharges);

    protected override string Title => "MechResurrectionCharges".Translate();

    protected override bool IsDraggable => resurrector.ReloadsPerCharge && resurrector.Wearer?.Faction == Faction.OfPlayer;

    protected override string BarLabel => resurrector.ProgressLabel;

    protected override int Increments => resurrector.MaxCharges;

    protected override Color BarColor => ResurrectorFill;

    protected override Color BarHighlightColor => ResurrectorHighlight;

    protected override Color BarDragColor => ResurrectorShade;

    protected override bool DraggingBar
    {
        get => draggingBar;
        set => draggingBar = value;
    }

    protected override IEnumerable<float> GetBarThresholds()
    {
        int max = resurrector.MaxCharges;
        for (int i = 1; i < max; i++)
            yield return (float)i / max;
    }

    // Laid out as vanilla's hemogen gauge (GeneGizmo_ResourceHemogen.GetTooltip): short "label:
    // value" lines in blocks, a blank line between blocks, sentences last. A tooltip is 260
    // pixels wide, about forty characters of the small font, and a stat line that wraps reads
    // as two lines, so every line here stays short of that and only the closing hint, a
    // paragraph of its own, is left to wrap.
    // - The resurrector: the count under a tinted header, the recharge rate (or that there is
    //   none), what a charge costs to reload and, where the bar drags, the level the handle sets.
    // - The ability, read off its def's extension: how fresh a corpse must be, then what each
    //   weight class costs as a list, bulleted in code as vanilla's tooltip lists are.
    // - Where the bar drags, what dragging it does.
    protected override string GetTooltip()
    {
        var tip = new StringBuilder();
        tip.Append("MechResurrectionCharges".Translate().Resolve().Colorize(ColoredText.TipSectionTitleColor));
        tip.Append(": ").Append(resurrector.ProgressLabel);
        float rate = resurrector.Props.chargesPerDay;
        tip.Append('\n').Append(rate > 0f
            ? "OLWC_GaugeRegen".Translate(rate.ToString("0.##")).Resolve()
            : "OLWC_GaugeNoRegen".Translate().Resolve());
        if (resurrector.ReloadsPerCharge)
            tip.Append('\n').Append("OLWC_GaugeReload".Translate(Command_LoadResurrector.CostLabel(resurrector.AmmoDef, resurrector.Props.ammoCountPerCharge)).Resolve());
        bool draggable = IsDraggable;
        if (draggable)
        {
            tip.Append('\n').Append(resurrector.TargetCharges > 0
                ? "OLWC_GaugeAutoReload".Translate(resurrector.TargetCharges).Resolve()
                : "OLWC_GaugeNoAutoReload".Translate().Resolve());
        }
        ResurrectMechExtension extension = OLWC_DefOf.OLWC_ResurrectMech.GetModExtension<ResurrectMechExtension>();
        if (extension != null)
        {
            tip.Append("\n\n").Append(extension.LimitsCorpseAge
                ? "OLWC_GaugeCorpseAge".Translate(Ability_ResurrectMech.PeriodText(extension.maxCorpseAgeTicks)).Resolve()
                : "OLWC_GaugeNoCorpseAge".Translate().Resolve());
            tip.Append('\n').Append("OLWC_GaugeCosts".Translate().Resolve());
            foreach (MechChargeCosts cost in extension.costs)
            {
                if (cost.weightClass != null)
                    tip.Append("\n  - ").Append("OLWC_GaugeCost".Translate(cost.weightClass.LabelCap, cost.cost).Resolve());
            }
        }
        if (draggable)
            tip.Append("\n\n").Append("OLWC_GaugeDragHint".Translate().Resolve());
        return tip.ToString();
    }
}
