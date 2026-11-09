using HarmonyLib;
using Verse;

namespace OverlordWarcasket.Patches;

// Re-runs Startup.Run on every play-data load. A static constructor runs once per process, but
// a mid-session language switch (LanguageDatabase.SelectLanguage: ClearAllPlayData then
// LoadAllPlayData) rebuilds every def, so def-derived startup state must be re-applied; this
// hook fires at the moment the startup static constructors run, after defs, cross-refs, DefOf
// rebinding and DefInjected, on every load. The UniqueMeleeWeapons repo's copy of this patch
// carries the full decompile-verified rationale. On the first load the patch is applied from
// inside CallAll itself (ModInit is one of the constructors it runs), so that invocation never
// reaches the postfix; ModInit runs Startup.Run directly for it.
[HarmonyPatch(typeof(StaticConstructorOnStartupUtility), nameof(StaticConstructorOnStartupUtility.CallAll))]
public static class StaticConstructorOnStartupUtility_CallAll
{
    public static void Postfix() => Startup.Run();
}
