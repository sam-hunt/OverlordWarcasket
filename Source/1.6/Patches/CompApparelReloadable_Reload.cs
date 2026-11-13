using HarmonyLib;
using RimWorld;

namespace OverlordWarcasket.Patches;

// The mech resurrector reloads on its own only up to its gauge's target
// (CompMechResurrector.TargetCharges, zero until the player drags the handle), while an ordered
// reload fills it. Vanilla's reload queries are not virtual, so the comp cannot override them;
// every other reloadable passes through, and so does a resurrector whose def no longer reloads by
// the charge.
//
// Vanilla already tells the two reloads apart (verified on 1.6 and 1.7.5000): JobGiver_Reload,
// the automatic one, asks FindSomeReloadableComponent and FindEnoughAmmo with allowForcedReload
// false, and the float menu (FloatMenuOptionProvider_Reload) and JobDriver_Reload ask with true.
// NeedsReload gates all of them, MinAmmoNeeded and ReloadFrom; MaxAmmoNeeded bounds the ammo
// FindEnoughAmmo gathers. So with false the two answer for the target, and with true for the
// whole resurrector, which is vanilla's own answer but for the trimmed count never exceeding the
// capacity.
//
// One call ignores the flag: JobGiver_Reload.MakeReloadJob, shared by both reloads, sets the
// job's count from MaxAmmoNeeded(true), and ReloadFrom loads all that was carried. The automatic
// job's count is capped to the target by Patches/JobGiver_Reload_TryGiveJob.
[HarmonyPatch(typeof(CompApparelReloadable))]
public static class CompApparelReloadable_Reload
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CompApparelReloadable.NeedsReload))]
    public static bool NeedsReload(CompApparelReloadable __instance, bool allowForcedReload, ref bool __result)
    {
        if (__instance is not CompMechResurrector { ReloadsPerCharge: true } resurrector)
            return true;
        __result = resurrector.ChargesWanted(allowForcedReload) > 0;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CompApparelReloadable.MaxAmmoNeeded))]
    public static bool MaxAmmoNeeded(CompApparelReloadable __instance, bool allowForcedReload, ref int __result)
    {
        if (__instance is not CompMechResurrector { ReloadsPerCharge: true } resurrector)
            return true;
        __result = resurrector.Props.ammoCountPerCharge * resurrector.ChargesWanted(allowForcedReload);
        return false;
    }
}
