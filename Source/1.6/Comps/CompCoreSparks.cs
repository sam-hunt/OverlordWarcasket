using System.Collections.Generic;
using RimWorld;
using Verse;
using AbilityDef = VEF.Abilities.AbilityDef;
using CompProperties_AbilitiesApparel = VEF.Abilities.CompProperties_AbilitiesApparel;

namespace OverlordWarcasket;

// Red sparks on the wearer while the armor's resurrection core holds a charge: the apocriton's
// always-on crackle (its CompEffecter playing ApocrionAttached), gated on the charge. The cheapest
// resurrection costs one charge, so "holds a charge" is "can resurrect something", and the
// wearer must be able to act on it too: alive, not downed and awake, the states in which a pilot
// can cast. The apocriton's own effecter checks only that it is spawned, so it crackles while
// downed; the core is a pilot's readied tool, not a mech's hum. A dead wearer is already
// unspawned (Pawn.Kill despawns the pawn before spawning its corpse, 1.7.4990 decompile), so the
// Dead check only states the rule, and Notify_WearerDied (Apparel.Notify_PawnKilled) tears the
// effecter down at the death itself rather than at the armor's next tick. With
// Props.onlyWhileDrafted (a mod setting) a wearer who can be drafted must also be drafted, and
// be able to cast: a mechanitor while the ability's requireMechlink holds (the ability's own
// IsEnabledForPawn rule, read off the ability def this armor names, so one setting governs
// both), since the drafted crackle is the core readied for battle. One who cannot be drafted
// is exempt from both and shows the sparks on the charge alone: PawnComponentsUtility gives a
// Pawn_DraftController only to the player's spawned humanlikes and colony mechs, so a raider or
// any other AI pilot has a null drafter, and Pawn.Drafted would hide its sparks for good.
// Without onlyWhileDrafted the sparks are the apocriton's hum and play for any pilot.
//
// Vanilla's CompEffecter cannot be reused: it plays on its parent and only while the parent is
// spawned, and worn apparel is never spawned. This is its logic (1.7.4990 decompile:
// CompEffecter.CompTick, ShouldShowEffecter) with the wearer as the effecter's target. Worn
// apparel ticks every tick on the armor's Normal tickerType (Thing.DoTick recurses into the
// pawn's child holders, Pawn_ApparelTracker's wornApparel among them), and SubEffecter_
// SprayerChance rolls its chance once per EffectTick, so the effecter must tick every tick for
// the def's rate to hold. Like vanilla, it plays only on the map being viewed and is torn down
// whenever it stops, so a wearer changing hands or maps starts a fresh one.
public class CompCoreSparks : ThingComp
{
    [Unsaved]
    private Effecter effecter;

    [Unsaved]
    private CompResurrectionCore core;

    public CompProperties_CoreSparks Props => (CompProperties_CoreSparks)props;

    private CompResurrectionCore Core => core ??= parent.GetComp<CompResurrectionCore>();

    private Pawn Wearer => (parent as Apparel)?.Wearer;

    private bool ShouldShow(Pawn wearer) =>
        wearer is { Spawned: true, Dead: false, Downed: false }
        && wearer.Map == Find.CurrentMap
        && wearer.Awake()
        && (!Props.onlyWhileDrafted || wearer.drafter == null || (wearer.Drafted && CanCast(wearer)))
        && Core is { Charges: > 0 };

    // The ability's mechlink rule. The extension is looked up on the live def each time, and only
    // for a drafted non-mechanitor, so neither a settings write nor a play-data reload leaves a
    // stale copy; none found reads as required, as the ability reads it.
    private bool CanCast(Pawn wearer) =>
        MechanitorUtility.IsMechanitor(wearer) || !RequiresMechlink();

    private bool RequiresMechlink()
    {
        List<AbilityDef> abilities = parent.def.GetCompProperties<CompProperties_AbilitiesApparel>()?.abilities;
        if (abilities != null)
        {
            foreach (AbilityDef ability in abilities)
            {
                if (ability.GetModExtension<ResurrectMechExtension>() is { } extension)
                    return extension.requireMechlink;
            }
        }
        return true;
    }

    public override void CompTick()
    {
        base.CompTick();
        Pawn wearer = Wearer;
        if (!ShouldShow(wearer))
        {
            Stop();
            return;
        }
        effecter ??= Props.effecterDef.SpawnAttached(wearer, wearer.Map);
        effecter.EffectTick(wearer, wearer);
    }

    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        Stop();
    }

    public override void Notify_WearerDied()
    {
        base.Notify_WearerDied();
        Stop();
    }

    public override void Notify_Unequipped(Pawn pawn)
    {
        base.Notify_Unequipped(pawn);
        Stop();
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);
        Stop();
    }

    private void Stop()
    {
        effecter?.Cleanup();
        effecter = null;
    }
}
