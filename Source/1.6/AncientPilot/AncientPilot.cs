using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OverlordWarcasket;

// Dresses a dead mechanitor Biotech generates in the whole set, by the chance the armor's
// AncientPilotExtension gives its source: flavour, the corpse of a pilot the player comes too
// late for. Called by the two postfixes on the code that makes those corpses
// (Patches/QuestNode_Root_MechanitorShip_RunInt and
// Patches/LayoutWorkerComplex_Mechanitor_TryPlaceDeceasedMechanitor, whose headers have the call
// sites), never from pawn generation: PawnGenerator.GeneratePawn has returned by then, so VFEP's
// postfix on it (the one Patches/PawnGenerator_GeneratePawn_Patch_Postfix masks) found no
// warcasket to complete with parts of its own, and the generator's one death notice for a pawn
// generated dead (pawn.apparel.Notify_PawnKilled, TryGenerateNewPawnInternal) has passed. The
// set's comps therefore never hear of this death, which matters for the mech stun pulse: the
// pod's corpse already lies on the map being generated.
//
// What the corpse is left as is what a pilot who died in the set leaves, piece by piece:
// - The set alone. Everything else the pawn wore is destroyed first, as VFEP's postfix strips
//   a generated pilot, and each piece goes on locked, as the foundry and that postfix lock
//   theirs (WarcasketProject.ApplyOn, Wear with locked true).
// - Worn by a corpse: Apparel.Notify_PawnKilled's one write for a death with no damage,
//   WornByCorpse where the def cares, set here by hand in place of the notice.
// - The overlord trait on the dead pawn, through VEF's hook on Pawn_ApparelTracker.
//   Notify_ApparelAdded (ApparelExtensionUtilities.EquipGear, the armor's traitsOnEquip).
//   TraitSet.GainTrait skips its mood refresh for a dead pawn.
// - Each piece in a colour of its own from the def's colorGenerator (CompColorable.Initialize
//   draws it as the thing is made), as VFEP's generated pilots are.
//
// All or nothing: a pawn who cannot wear every piece (ApparelUtility.HasPartsToWear and
// ApparelProperties.PawnCanWear, the two refusals Pawn_ApparelTracker.Wear only warns of) keeps
// the clothes vanilla gave them, so no corpse wears part of a set.
//
// Only a dead pawn is dressed. The set on a living generated pawn is what the armor def's
// header rules out, so a mod that makes either source hand over a living mechanitor gets no set.
//
// A failure here is logged and goes no further: the callers run inside quest generation and map
// generation, where an exception from a postfix would cost the player the quest or the map.
//
// Vanilla as read on 1.7.5014; the calls made here compile against 1.6.
public static class AncientPilot
{
    private const string ArmorDefName = "OLWC_Warcasket_Overlord";

    // Null while the armor or its extension is missing (another mod's patch).
    public static AncientPilotExtension Extension =>
        DefDatabase<ThingDef>.GetNamedSilentFail(ArmorDefName)?.GetModExtension<AncientPilotExtension>();

    public static void TryDress(Pawn pawn, AncientPilotSource source)
    {
        try
        {
            ThingDef armor = DefDatabase<ThingDef>.GetNamedSilentFail(ArmorDefName);
            AncientPilotExtension extension = armor?.GetModExtension<AncientPilotExtension>();
            if (extension == null || pawn?.apparel == null || !pawn.Dead)
                return;
            if (!Rand.Chance(extension.ChanceFor(source)))
                return;
            List<ThingDef> set = [armor, .. extension.otherPieces];
            if (!set.TrueForAll(piece => Fits(pawn, piece)))
                return;
            pawn.apparel.DestroyAll();
            foreach (ThingDef piece in set)
            {
                var apparel = (Apparel)ThingMaker.MakeThing(piece, GenStuff.RandomStuffFor(piece));
                pawn.apparel.Wear(apparel, dropReplacedApparel: false, locked: true);
                if (piece.apparel.careIfWornByCorpse)
                    apparel.WornByCorpse = true;
            }
        }
        catch (Exception exception)
        {
            Log.Error($"[Overlord Warcasket] Could not dress the dead mechanitor {pawn} in the overlord warcasket: {exception}");
        }
    }

    private static bool Fits(Pawn pawn, ThingDef piece) =>
        piece?.apparel != null
        && ApparelUtility.HasPartsToWear(pawn, piece)
        && piece.apparel.PawnCanWear(pawn, ignoreGender: true);
}
