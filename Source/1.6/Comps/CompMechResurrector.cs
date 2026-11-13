using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// The armor's mech resurrector: the charge store behind the resurrect mech ability
// (Ability_ResurrectMech), the apocriton's resurrectCharges on a piece of apparel. Vanilla's
// CompApparelReloadable, as every gauge in the warcasket family is, with a recharge of its own on
// top: remainingCharges is the charges held, a charge reloads from the props' ammoDef, and the
// resurrector also gains Props.chargesPerDay by itself. The def declares no verbs, so vanilla draws
// no command for it; the readouts are ours.
//
// The recharge is the main supply and the reload the shortcut (the armor def's header has the
// why), so the two never compete:
// - Recharge. regenProgress is the fraction of the next charge, accrued every RegenInterval
//   ticks and banked whole, so a lowered capacity or rate never strands a part charge: a full
//   resurrector holds no progress. The armor's tickerType is Normal (VFEP_WarcasketArmorBase) and
//   worn apparel ticks through Pawn_ApparelTracker, so CompTick runs wherever the wearer is.
// - Automatic reload. A colonist fetches ammo on their own (vanilla's JobGiver_Reload) only
//   while the resurrector sits below TargetCharges, the gauge's drag handle, which starts at zero:
//   a pilot never spends the colony's ammo unasked. Patches/CompApparelReloadable_Reload has how
//   vanilla's queries are bent to that, and Patches/JobGiver_Reload_TryGiveJob holds the job's
//   count to the handle.
// - Ordered reload. Right-clicking the ammo (vanilla's float menu, drafted or not) fills the
//   resurrector, whatever the handle says: vanilla gathers stacks outward from the clicked one
//   until the whole need is met.
// Both are vanilla's own reload and take ammo lying on the map (ReloadableUtility.FindEnoughAmmo);
// nothing here reads the pilot's inventory. Combat Extended's "Reload worn armor" button, which
// it adds to every worn reloadable, is yielded as it comes: the reload from the inventory is
// CE's to give (Patches/JobGiver_Reload_TryGiveJob has what it does on the resurrector).
//
// The resurrector starts full (vanilla's PostPostMake), as the apocriton starts with its thirty. A
// capacity lowered by the settings trims a resurrector above it on the next tick, on equip and on
// load (Trim), and OverlordWarcasketSettings.TrimWornResurrectors does it at once for every worn
// one; vanilla's reload arithmetic assumes the count never exceeds the capacity.
//
// Readouts: the gauge (Gizmo_MechResurrector, rebuilt whenever the capacity or the settings
// generation moves under it, since Gizmo_Slider builds its bar textures and reads its target
// once), which counts the part charge the recharge has accrued (FormatProgress), and the inspect
// line and the ability command's top-right count (Command_ResurrectMech), which count whole
// charges, the ones a cast can spend.
// Dev mode adds charge buttons beside the gauge: vanilla's "DEV: Reload to full" and two more.
public class CompMechResurrector : CompApparelReloadable
{
    // Ticks between regen steps; a charge takes a day, so finer steps buy nothing.
    private const int RegenInterval = 150;

    private float regenProgress;

    // The level an automatic reload fills to; zero never fetches ammo.
    private int targetCharges;

    [Unsaved]
    private Gizmo_MechResurrector gauge;

    [Unsaved]
    private int gaugeMaxCharges = -1;

    [Unsaved]
    private int gaugeGeneration = -1;

    public new CompProperties_MechResurrector Props => (CompProperties_MechResurrector)props;

    public int Charges => remainingCharges;

    public bool Full => remainingCharges >= MaxCharges;

    // The fraction of the next charge accrued, 0 while full.
    public float RegenProgress => regenProgress;

    // The charges held with the part charge the recharge has accrued: what the gauge shows.
    public float ChargesWithProgress => Mathf.Min(remainingCharges + regenProgress, MaxCharges);

    public string ProgressLabel => FormatProgress(remainingCharges, regenProgress, MaxCharges);

    public int TargetCharges
    {
        get => Mathf.Clamp(targetCharges, 0, MaxCharges);
        set => targetCharges = Mathf.Clamp(value, 0, MaxCharges);
    }

    // Vanilla's two reload modes: ours is a count of ammo per charge. A patch that moved the def
    // to ammoCountToRefill, or took its ammo away, gets vanilla's rules untouched.
    public bool ReloadsPerCharge => AmmoDef != null && Props.ammoCountPerCharge > 0 && Props.ammoCountToRefill == 0;

    // The gauge's count: the charges held to one decimal, the part charge cut rather than
    // rounded so the whole number never reads a charge the resurrector does not hold yet ("3.9",
    // never "4", at 3.96), and a whole count without the decimal ("3 / 10").
    public static string FormatProgress(int charges, float progress, int maxCharges)
    {
        int tenths = charges >= maxCharges ? 0 : Mathf.Clamp(Mathf.FloorToInt(progress * 10f), 0, 9);
        return (Mathf.Min(charges, maxCharges) + tenths / 10f).ToString("0.#") + " / " + maxCharges;
    }

    // The resurrector worn by pawn, if any: the armor's.
    public static CompMechResurrector WornBy(Pawn pawn)
    {
        if (pawn?.apparel == null)
            return null;
        foreach (Apparel apparel in pawn.apparel.WornApparel)
        {
            CompMechResurrector resurrector = apparel.TryGetComp<CompMechResurrector>();
            if (resurrector != null)
                return resurrector;
        }
        return null;
    }

    // The charges a reload may add: an ordered one fills the resurrector, an automatic one stops at
    // the target.
    public static int ChargesWanted(int charges, int maxCharges, int target, bool ordered) =>
        Mathf.Max(0, (ordered ? maxCharges : Mathf.Min(target, maxCharges)) - charges);

    public int ChargesWanted(bool ordered) => ChargesWanted(remainingCharges, MaxCharges, targetCharges, ordered);

    public bool TryConsume(int cost)
    {
        if (cost < 0 || cost > remainingCharges)
            return false;
        remainingCharges -= cost;
        return true;
    }

    public void Add(int count)
    {
        remainingCharges = Mathf.Clamp(remainingCharges + count, 0, MaxCharges);
        if (Full)
            regenProgress = 0f;
    }

    public void Trim()
    {
        remainingCharges = Mathf.Clamp(remainingCharges, 0, MaxCharges);
        if (Full || float.IsNaN(regenProgress) || regenProgress < 0f)
            regenProgress = 0f;
    }

    // Accrues ticks' worth of recharge and banks every whole charge it completes.
    public void Regen(int ticks)
    {
        float perDay = Props.chargesPerDay;
        if (Full || perDay <= 0f)
        {
            regenProgress = 0f;
            return;
        }
        regenProgress += perDay * ticks / GenDate.TicksPerDay;
        while (regenProgress >= 1f && !Full)
        {
            regenProgress -= 1f;
            remainingCharges++;
        }
        if (Full)
            regenProgress = 0f;
    }

    public override void CompTick()
    {
        base.CompTick();
        if (!parent.IsHashIntervalTick(RegenInterval))
            return;
        if (remainingCharges > MaxCharges)
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
        Scribe_Values.Look(ref regenProgress, "regenProgress");
        Scribe_Values.Look(ref targetCharges, "targetCharges");
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            Trim();
    }

    public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
            yield return gizmo;
        if (gauge == null || gaugeMaxCharges != MaxCharges || gaugeGeneration != OverlordWarcasketSettings.Generation)
        {
            gauge = new Gizmo_MechResurrector(this);
            gaugeMaxCharges = MaxCharges;
            gaugeGeneration = OverlordWarcasketSettings.Generation;
        }
        yield return gauge;
        if (!DebugSettings.ShowDevGizmos)
            yield break;
        yield return new Command_Action { defaultLabel = "DEV: Add charge", action = () => Add(1) };
        yield return new Command_Action { defaultLabel = "DEV: Empty charges", action = () => Add(-MaxCharges) };
    }

    public override string CompInspectStringExtra() =>
        "MechResurrectionCharges".Translate() + ": " + LabelRemaining;
}
