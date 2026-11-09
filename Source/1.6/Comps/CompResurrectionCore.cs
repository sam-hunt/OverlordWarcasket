using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// The armor's resurrection core: the charge store behind the resurrect mech ability
// (Ability_ResurrectMech), the apocriton's resurrectCharges on a piece of apparel. A plain
// ThingComp rather than a CompApparelReloadable: nothing reloads it, it recharges on its own. The
// armor's tickerType is Normal (VFEP_WarcasketArmorBase), and worn apparel ticks through
// Pawn_ApparelTracker, so CompTick runs wherever the wearer is, every RegenInterval ticks.
//
// Charges are whole; regenProgress is the fraction of the next one, accrued at Props.regenPerDay
// of the capacity per day and spent whole, so a lowered capacity or rate never strands a part
// charge: a full core holds no progress. The core starts full (PostPostMake), as the apocriton
// starts with its thirty. A capacity lowered by the settings trims a core above it on the next
// tick, on equip and on load (Trim), and OverlordWarcasketSettings.TrimWornCores does it at once
// for every worn one.
//
// Readouts: the gauge (Gizmo_ResurrectionCore, rebuilt whenever the capacity or the settings
// generation moves under it, since Gizmo_Slider builds its bar textures once), the inspect line
// and the ability command's top-right count (Command_ResurrectMech). Dev mode adds charge
// buttons beside the gauge, the apocriton's "DEV: Add charge" and two more.
public class CompResurrectionCore : ThingComp
{
    // Ticks between regen steps; the rate is slow enough that finer steps buy nothing.
    private const int RegenInterval = 150;

    private int charges;
    private float regenProgress;

    [Unsaved]
    private Gizmo_ResurrectionCore gauge;

    [Unsaved]
    private int gaugeMaxCharges = -1;

    [Unsaved]
    private int gaugeGeneration = -1;

    public CompProperties_ResurrectionCore Props => (CompProperties_ResurrectionCore)props;

    public int MaxCharges => Props.maxCharges;

    public int Charges => charges;

    public bool Full => charges >= MaxCharges;

    // The fraction of the next charge accrued, 0 while full.
    public float RegenProgress => regenProgress;

    // Charges accrued per day at the current capacity and rate.
    public float ChargesPerDay => MaxCharges * Props.regenPerDay;

    // "3 / 10", the gauge's bar label, the inspect line and the command's count.
    public string LabelCharges => $"{charges} / {MaxCharges}";

    public Pawn Wearer => (parent as Apparel)?.Wearer;

    // Ticks until the next charge at the current rate, or -1 while full or at a rate of zero.
    public int TicksToNextCharge
    {
        get
        {
            float perDay = ChargesPerDay;
            if (Full || perDay <= 0f)
                return -1;
            return Mathf.CeilToInt((1f - regenProgress) / perDay * GenDate.TicksPerDay);
        }
    }

    // The core worn by pawn, if any: the armor's.
    public static CompResurrectionCore WornBy(Pawn pawn)
    {
        if (pawn?.apparel == null)
            return null;
        foreach (Apparel apparel in pawn.apparel.WornApparel)
        {
            CompResurrectionCore core = apparel.TryGetComp<CompResurrectionCore>();
            if (core != null)
                return core;
        }
        return null;
    }

    public bool TryConsume(int cost)
    {
        if (cost < 0 || cost > charges)
            return false;
        charges -= cost;
        return true;
    }

    public void Add(int count)
    {
        charges = Mathf.Clamp(charges + count, 0, MaxCharges);
        if (Full)
            regenProgress = 0f;
    }

    public void Trim()
    {
        charges = Mathf.Clamp(charges, 0, MaxCharges);
        if (Full || float.IsNaN(regenProgress) || regenProgress < 0f)
            regenProgress = 0f;
    }

    // Accrues ticks' worth of recharge and banks every whole charge it completes.
    public void Regen(int ticks)
    {
        float perDay = ChargesPerDay;
        if (Full || perDay <= 0f)
        {
            regenProgress = 0f;
            return;
        }
        regenProgress += perDay * ticks / GenDate.TicksPerDay;
        while (regenProgress >= 1f && !Full)
        {
            regenProgress -= 1f;
            charges++;
        }
        if (Full)
            regenProgress = 0f;
    }

    public override void PostPostMake()
    {
        base.PostPostMake();
        charges = MaxCharges;
    }

    public override void CompTick()
    {
        base.CompTick();
        if (!parent.IsHashIntervalTick(RegenInterval))
            return;
        if (charges > MaxCharges)
            Trim();
        Regen(RegenInterval);
    }

    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        Trim();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref charges, "charges", MaxCharges);
        Scribe_Values.Look(ref regenProgress, "regenProgress");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            Trim();
    }

    public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
            yield return gizmo;
        if (gauge == null || gaugeMaxCharges != MaxCharges || gaugeGeneration != OverlordWarcasketSettings.Generation)
        {
            gauge = new Gizmo_ResurrectionCore(this);
            gaugeMaxCharges = MaxCharges;
            gaugeGeneration = OverlordWarcasketSettings.Generation;
        }
        yield return gauge;
        if (!DebugSettings.ShowDevGizmos)
            yield break;
        yield return new Command_Action { defaultLabel = "DEV: Add charge", action = () => Add(1) };
        yield return new Command_Action { defaultLabel = "DEV: Fill charges", action = () => Add(MaxCharges) };
        yield return new Command_Action { defaultLabel = "DEV: Empty charges", action = () => Add(-MaxCharges) };
    }

    public override string CompInspectStringExtra() =>
        "MechResurrectionCharges".Translate() + ": " + LabelCharges;
}
