using RimWorld;
using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// The armor's mech stun pulse: the apocriton's "mech stun pulse on death" on a piece of apparel.
// When the pilot dies, or is downed where the settings say so, an explosion of Props.damageDef
// (Biotech's MechBandShockwave) goes out from the pilot's cell and stuns every mechanoid it
// reaches, whoever it belongs to. That is the apocriton's rule and kept on purpose: the pulse
// costs a pilot standing among their own mechs and pays for one who falls among an enemy's.
//
// Vanilla, verified on 1.7.5013 and the frozen 1.6 assembly, every method named here the same
// on both:
// - The apocriton's is a stock CompExplosive with explodeOnKilled, radius 30.9, propagationSpeed
//   0.5. That comp fires from its own parent's PostDestroy, and worn armor outlives its pilot,
//   so this comp calls GenExplosion.DoExplosion itself, with the same arguments left at their
//   defaults but for the display (below). Calling it directly also keeps the pulse off Combat
//   Extended's transpiler on CompExplosive.Detonate, which swaps in its own explosion.
// - Death: Apparel.Notify_PawnKilled calls Notify_WearerDied on every comp, from Pawn.Kill's
//   PreDeathPawnModifications, before DropBeforeDying takes the pawn out of a carrier's arms
//   and before it despawns, so the wearer's held position and map are still the place of death.
// - Downed: worn apparel hears nothing (Pawn.Notify_Downed tells the pawn's own comps only), so
//   Patches/Pawn_Notify_Downed passes it on. Pawn_HealthTracker.MakeDowned calls it after
//   setting the pawn Downed, and AddHediff adds a hediff to the set before it checks for the
//   state change that downs the pawn.
// - Who is stunned: StunHandler.CanBeStunnedByDamage admits three defs for a mechanoid, each
//   by comparing the def itself, so a def of our own could not stun a mech at all. Stun stuns
//   every pawn and EMP everything that is not flesh; MechBandShockwave is the one that stuns
//   mechanoids and nothing else, which is the pulse. The length of the stun is therefore that
//   def's constantStunDurationTicks and not ours to tune. The rule also refuses a pawn that is
//   downed or dead. Buildings answer through their CompStunnable's list, which names this def
//   on none of vanilla's. StunHandler.StunFor keeps the longer of the stun running and the new
//   one, so pulses that overlap do not add up.
// - Where it reaches: DamageWorker.ExplosionCellsToHit takes the cells within the radius that
//   the centre has line of sight to (GenSight.LineOfSight, GenGrid.CanBeSeenOver: a building
//   of full fillage blocks, an open door does not), fixed when the explosion starts. The wave
//   then reaches a cell at distance d after d * 1.5 / propagationSpeed ticks and stuns what
//   stands there then.
//
// The display is the one thing not left to the explosion. Its flash, smoke and camera shake
// are sized by the radius, but the damage def's explosionCenterEffecter, the red ring and the
// distortion wave, is one size, the apocriton's: DamageWorker.ExplosionVisualEffectCenter
// spawns it at scale 1 whatever the radius. Props.centerEffecter is that effecter over again
// with sizes the settings shrink to the pulse's radius (DisplayScale; the effecter def's header
// has the rule). An explosion offers no way to leave out the centre effecter alone, and
// swapping ours onto Biotech's def for the call would be a write to a def other mods read, so
// the explosion is asked for no display at all (doVisualEffects false) and PlayDisplay plays
// what that turns off, with ours at the centre. The switch is read in three places:
// - DamageWorker.ExplosionStart: the white flash, the camera shake, and
//   ExplosionVisualEffectCenter's four puffs of smoke and centre effecter. PlayDisplay repeats
//   each with vanilla's own arithmetic.
// - Explosion.StartExplosion: the splash on water, repeated too.
// - DamageWorker.ExplosionAffectCell: the def's explosionCellFleck or explosionCellMote, of
//   which MechBandShockwave has neither, as it has no centre or interior fleck or mote for
//   ExplosionVisualEffectCenter to play; one another mod's patch gave it would not show here.
// The electric arcs, the def's explosionCellEffecter, are not behind the switch and nor is the
// sound, so both stay the explosion's. With no centerEffecter on the props the explosion keeps
// its whole display and PlayDisplay is not called.
//
// The instigator is left null, where the apocriton's is the apocriton. A pilot named as the
// instigator would be blamed for the pulse: Faction.Notify_MemberTookDamage costs the player
// goodwill for a stunned mech of any faction not hostile to them, this def being external
// violence for a mechanoid, and returns at once on a null instigator. StunFor's battle log
// entry takes a null initiator.
//
// Any wearer pulses, a mechanitor or not, as the apocriton's comp asks nothing of it either, and
// the pulse spends no charge: it comes once per pilot where it comes on death, and a pilot who
// falls with an empty resurrector still pulses.
//
// A wearer with no map (in a caravan, a pod in flight) does not pulse. One carried in another
// pawn's arms pulses from where they are held, as the apocriton's does (Detonate's
// ignoreUnspawned). One who dies shut in a casket or another enclosing container does not:
// Pawn.Kill takes the pawn out of it before the callback, leaving no map to pulse on.
//
// A downing counts only as a fall. A pilot who goes under where they already lie, in a bed
// (RestUtility.InBed; MakeDowned leaves a pawn lying in bed in its job, so the answer still
// holds in the postfix) or under anesthetic, does not pulse: surgery and a deathrest casket,
// which is a bed, would otherwise be ways to pulse at will, and a patient going under would
// stun the mechs tending them. Toxin Warcasket's tank makes the anesthetic exception too, and
// for a death as well; here a death always pulses. A pilot of another faction who is killed by
// the roll for death on downing was never downed, so under the Downed trigger they do not
// pulse either.
//
// Nothing is saved: the pulse is an event, and the downed one comes from the patch rather than
// from a watched flag, so a save made before the comp existed loads with nothing to reconcile.
public class CompMechStunPulse : ThingComp
{
    public CompProperties_MechStunPulse Props => (CompProperties_MechStunPulse)props;

    private Pawn Wearer => (parent as Apparel)?.Wearer;

    // Whether what happened to the pilot sets the pulse off under the setting; atRest is a
    // pilot in a bed or under anesthetic. Pure, for the tests.
    public static bool Fires(StunPulseTrigger setting, StunPulseTrigger happened, bool atRest) =>
        setting == happened && !(happened == StunPulseTrigger.Downed && atRest);

    // How large the pulse's display is drawn against the sizes in its def, which are drawn for
    // drawnForRadius: a pulse of a third the reach is a third the size. Pure, for the tests.
    public static float DisplayScale(float radius, float drawnForRadius) =>
        drawnForRadius > 0f ? radius / drawnForRadius : 1f;

    public override void Notify_WearerDied()
    {
        base.Notify_WearerDied();
        TryPulse(StunPulseTrigger.Death);
    }

    // From Patches/Pawn_Notify_Downed.
    public void Notify_WearerDowned() => TryPulse(StunPulseTrigger.Downed);

    private void TryPulse(StunPulseTrigger happened)
    {
        Pawn wearer = Wearer;
        Map map = wearer?.MapHeld;
        if (map == null || Props.damageDef == null || Props.radius <= 0f)
            return;
        bool atRest = wearer.InBed() || wearer.health.hediffSet.HasHediff(HediffDefOf.Anesthetic);
        if (!Fires(Props.trigger, happened, atRest))
            return;
        IntVec3 cell = wearer.PositionHeld;
        bool ownDisplay = Props.centerEffecter != null;
        if (ownDisplay)
            PlayDisplay(cell, map);
        GenExplosion.DoExplosion(cell, map, Props.radius, Props.damageDef, instigator: null,
            doVisualEffects: !ownDisplay, propagationSpeed: Props.propagationSpeed);
    }

    // What doVisualEffects false takes from the explosion (the header has the three reads), as
    // vanilla plays it, with Props.centerEffecter where the damage def's centre effecter was.
    private void PlayDisplay(IntVec3 cell, Map map)
    {
        float radius = Props.radius;
        Vector3 center = cell.ToVector3Shifted();
        FleckMaker.Static(cell, map, FleckDefOf.ExplosionFlash, radius * 6f);
        if (map == Find.CurrentMap)
            Find.CameraDriver.shaker.DoShake(4f * radius / (center - Find.Camera.transform.position).magnitude);
        for (int i = 0; i < 4; i++)
            FleckMaker.ThrowSmoke(center + Gen.RandomHorizontalVector(radius * 0.7f), map, radius * 0.6f);
        Props.centerEffecter.Spawn(cell, map, Vector3.zero);
        FleckMaker.WaterSplash(center, map, radius * 6f, 20f);
    }
}
