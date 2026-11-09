namespace OverlordWarcasket;

// Everything this mod reads off or writes onto the loaded defs at startup: the overlord trait's
// mirror from VFEP's (WorkProfile), then the settings' defaults and overrides. Idempotent, and
// run on every play-data load, not only the first: ModInit's static constructor runs it once per
// process, and the postfix on StaticConstructorOnStartupUtility.CallAll
// (Patches/StaticConstructorOnStartupUtility_CallAll) again after every in-process reload, which
// replaces every def instance and would otherwise leave the mirror and the settings unapplied on
// the fresh ones. Every run sees defs fresh from XML (a reload clears the database before it
// rebuilds), so nothing it reads is a value an earlier run wrote.
public static class Startup
{
    public static void Run()
    {
        WorkProfile.Mirror();
        OverlordWarcasketMod.Settings.LoadDefaultsFromDefs();
        OverlordWarcasketMod.Settings.ApplyToDefs();
    }
}
