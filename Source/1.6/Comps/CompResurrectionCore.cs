using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OverlordWarcasket;

// The armor's resurrection core: the charge store behind the resurrect mech ability
// (Ability_ResurrectMech), the apocriton's resurrectCharges on a piece of apparel. Vanilla's
// CompApparelReloadable, as every gauge in the warcasket family is, with a recharge of its own on
// top: remainingCharges is the charges held, a charge reloads from the props' ammoDef, and the
// core also gains Props.chargesPerDay by itself. The def declares no verbs, so vanilla draws no
// command for it; the readouts are ours.
//
// The recharge is the main supply and the reload the shortcut (the armor def's header has the
// why), so the two never compete:
// - Recharge. regenProgress is the fraction of the next charge, accrued every RegenInterval
//   ticks and banked whole, so a lowered capacity or rate never strands a part charge: a full
//   core holds no progress. The armor's tickerType is Normal (VFEP_WarcasketArmorBase) and worn
//   apparel ticks through Pawn_ApparelTracker, so CompTick runs wherever the wearer is.
// - Automatic reload. A colonist fetches ammo on their own (vanilla's JobGiver_Reload) only
//   while the core sits below TargetCharges, the gauge's drag handle, which starts at zero: a
//   pilot never spends the colony's ammo unasked. Patches/CompApparelReloadable_Reload has how
//   vanilla's queries are bent to that. With Props.autoReloadFromInventory it loads the ammo
//   the pilot carries first, up to the handle and by the carried reload's job, and fetches
//   only for what is still missing (Patches/JobGiver_Reload_TryGiveJob).
// - Ordered reload. Right-clicking the ammo (vanilla's float menu, drafted or not) reloads as
//   far as the clicked stack goes, whatever the handle says.
// - Carried reload. Vanilla's reload only looks for ammo lying on the map, so ammo in the
//   pilot's own inventory gets a command beside the gauge (Command_LoadCore), yielded only
//   while the pilot carries some, which orders JobDriver_LoadCore; ordered, it fills the core
//   past the handle as the float menu's does. Combat Extended adds a reload
//   button of its own to every worn reloadable (ForeignReloadCommand), dropped here: the
//   carried reload is ours.
//
// The core starts full (vanilla's PostPostMake), as the apocriton starts with its thirty. A
// capacity lowered by the settings trims a core above it on the next tick, on equip and on load
// (Trim), and OverlordWarcasketSettings.TrimWornCores does it at once for every worn one;
// vanilla's reload arithmetic assumes the count never exceeds the capacity.
//
// Readouts: the gauge (Gizmo_ResurrectionCore, rebuilt whenever the capacity or the settings
// generation moves under it, since Gizmo_Slider builds its bar textures and reads its target
// once), the inspect line and the ability command's top-right count (Command_ResurrectMech).
// Dev mode adds charge buttons beside the gauge: vanilla's "DEV: Reload to full" and two more.
public class CompResurrectionCore : CompApparelReloadable
{
    // Ticks between regen steps; a charge takes a day, so finer steps buy nothing.
    private const int RegenInterval = 150;

    // Combat Extended's "Reload worn armor" (CE 16.7.4), which its postfix on
    // CompApparelVerbOwner_Charged.CompGetWornGizmosExtra appends for every player pawn. It
    // runs CE's own automatic reload (Patches/JobGiver_Reload_TryGiveJob has what that does), so
    // on the core it is a button that does nothing until the handle is raised and then drops
    // the carried ammo to reload from the floor. Matched by name: CE is not a reference.
    private const string ForeignReloadCommand = "CombatExtended.Command_ReloadArmor";

    private float regenProgress;

    // The level an automatic reload fills to; zero never fetches ammo.
    private int targetCharges;

    [Unsaved]
    private Gizmo_ResurrectionCore gauge;

    [Unsaved]
    private int gaugeMaxCharges = -1;

    [Unsaved]
    private int gaugeGeneration = -1;

    [Unsaved]
    private Command_LoadCore loadCommand;

    public new CompProperties_ResurrectionCore Props => (CompProperties_ResurrectionCore)props;

    public int Charges => remainingCharges;

    public bool Full => remainingCharges >= MaxCharges;

    // The fraction of the next charge accrued, 0 while full.
    public float RegenProgress => regenProgress;

    public int TargetCharges
    {
        get => Mathf.Clamp(targetCharges, 0, MaxCharges);
        set => targetCharges = Mathf.Clamp(value, 0, MaxCharges);
    }

    // Vanilla's two reload modes: ours is a count of ammo per charge. A patch that moved the def
    // to ammoCountToRefill, or took its ammo away, gets vanilla's rules untouched.
    public bool ReloadsPerCharge => AmmoDef != null && Props.ammoCountPerCharge > 0 && Props.ammoCountToRefill == 0;

    // Ticks until the next charge at the current rate, or -1 while full or at a rate of zero.
    public int TicksToNextCharge
    {
        get
        {
            float perDay = Props.chargesPerDay;
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

    // The charges a reload may add: an ordered one fills the core, an automatic one stops at the
    // target.
    public static int ChargesWanted(int charges, int maxCharges, int target, bool ordered) =>
        Mathf.Max(0, (ordered ? maxCharges : Mathf.Min(target, maxCharges)) - charges);

    public int ChargesWanted(bool ordered) => ChargesWanted(remainingCharges, MaxCharges, targetCharges, ordered);

    // The ammo of the core's kind in pawn's inventory. Indexed rather than enumerated: the gizmo
    // pass asks every frame the wearer is selected.
    public int CarriedAmmo(Pawn pawn)
    {
        ThingDef ammo = AmmoDef;
        ThingOwner<Thing> carried = pawn?.inventory?.innerContainer;
        if (ammo == null || carried == null)
            return 0;
        int count = 0;
        for (int i = 0; i < carried.Count; i++)
        {
            if (carried[i].def == ammo)
                count += carried[i].stackCount;
        }
        return count;
    }

    // Whether pawn carries a stack that holds a charge's worth: what a carried reload can load,
    // since ReloadFrom takes one stack at a time. The automatic reload asks before it gives the
    // job, so a job that could load nothing is never given over and over.
    public bool CarriesACharge(Pawn pawn)
    {
        ThingDef ammo = AmmoDef;
        ThingOwner<Thing> carried = pawn?.inventory?.innerContainer;
        if (ammo == null || carried == null)
            return false;
        for (int i = 0; i < carried.Count; i++)
        {
            if (carried[i].def == ammo && carried[i].stackCount >= Props.ammoCountPerCharge)
                return true;
        }
        return false;
    }

    // Reloads from the ammo in pawn's inventory, stack by stack, until the charges wanted are
    // loaded (the whole core for an ordered reload, up to the target for an automatic one) or
    // the ammo runs out. ReloadFrom loads all a stack can give the core, so a stack holding more
    // than is wanted has that much split off first, and whatever ReloadFrom leaves of the split
    // goes back. It splits what it loads off the stack, which takes a spent stack out of the
    // inventory itself, hence the walk from the end.
    public void ReloadFromInventory(Pawn pawn, bool ordered)
    {
        ThingDef ammo = AmmoDef;
        ThingOwner<Thing> carried = pawn?.inventory?.innerContainer;
        if (ammo == null || carried == null)
            return;
        for (int i = carried.Count - 1; i >= 0; i--)
        {
            int wanted = ChargesWanted(ordered);
            if (wanted <= 0)
                return;
            Thing stack = carried[i];
            if (stack.def != ammo)
                continue;
            int most = wanted * Props.ammoCountPerCharge;
            Thing part = stack.stackCount > most ? stack.SplitOff(most) : stack;
            ReloadFrom(part);
            if (part != stack && !part.Destroyed)
                carried.TryAdd(part);
        }
    }

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
        {
            if (gizmo.GetType().FullName != ForeignReloadCommand)
                yield return gizmo;
        }
        if (gauge == null || gaugeMaxCharges != MaxCharges || gaugeGeneration != OverlordWarcasketSettings.Generation)
        {
            gauge = new Gizmo_ResurrectionCore(this);
            gaugeMaxCharges = MaxCharges;
            gaugeGeneration = OverlordWarcasketSettings.Generation;
        }
        yield return gauge;
        Command_LoadCore load = LoadCommand();
        if (load != null)
            yield return load;
        if (!DebugSettings.ShowDevGizmos)
            yield break;
        yield return new Command_Action { defaultLabel = "DEV: Add charge", action = () => Add(1) };
        yield return new Command_Action { defaultLabel = "DEV: Empty charges", action = () => Add(-MaxCharges) };
    }

    // The carried reload's command, for a player's pilot with ammo in the inventory and no one
    // else; one cached instance, refreshed on every request.
    private Command_LoadCore LoadCommand()
    {
        Pawn wearer = Wearer;
        if (!ReloadsPerCharge || wearer is not { IsColonistPlayerControlled: true })
            return null;
        int carried = CarriedAmmo(wearer);
        if (carried <= 0)
            return null;
        loadCommand ??= new Command_LoadCore(this);
        loadCommand.Refresh(wearer, carried);
        return loadCommand;
    }

    // Orders the carried reload; a drafted pilot takes it as any ordered job.
    public void OrderReloadFromInventory(Pawn pawn) =>
        pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(OLWC_DefOf.OLWC_LoadCore, parent), JobTag.Misc);

    public override string CompInspectStringExtra() =>
        "MechResurrectionCharges".Translate() + ": " + LabelRemaining;
}
