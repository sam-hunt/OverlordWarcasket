using RimWorld;
using Verse;

namespace OverlordWarcasket;

// Red sparks on the wearer while the armor's resurrection core holds a charge: the apocriton's
// always-on crackle (its CompEffecter playing ApocrionAttached), gated on the charge. The cheapest
// resurrection costs one charge, so "holds a charge" is "can resurrect something". With
// Props.onlyWhileDrafted (a mod setting) a wearer who can be drafted must also be drafted. One
// who cannot is exempt and shows the sparks on the charge alone: PawnComponentsUtility gives a
// Pawn_DraftController only to the player's spawned humanlikes and colony mechs, so a raider or
// any other AI pilot has a null drafter, and Pawn.Drafted would hide its sparks for good.
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
        wearer is { Spawned: true }
        && wearer.Map == Find.CurrentMap
        && (!Props.onlyWhileDrafted || wearer.drafter == null || wearer.Drafted)
        && Core is { Charges: > 0 };

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
