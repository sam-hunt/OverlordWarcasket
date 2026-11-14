namespace OverlordWarcasket;

// A setting an XML patch can branch on (PatchOperationSetting): a public field of the settings
// whose current value the patch compares with the choice it names. False when the choice is
// not one the setting has, which the operation reports as an error, so a mistyped choice never
// reads as a branch that quietly does not run.
public interface IPatchSetting
{
    bool TryMatch(string choice, out bool matches);
}
