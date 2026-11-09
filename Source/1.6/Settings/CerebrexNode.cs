using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// Odyssey's cerebrex node (Apparel_CerebrexNode) as the settings window sees it. Its art is
// swapped for ours by the XML patch 1.6/Mods/Odyssey/Patches/CerebrexNodeArt.xml, behind the
// cerebrex art switch read while the patches apply; nothing here writes the def. TexPath is the
// path that patch writes, under Mods/Odyssey/Textures, so UsesOurArt reads off the def which art
// is live this session. The header icon is our item art, a mod-shipped texture, so it re-resolves
// on a Unity-null check after a play-data reload destroys it (Source/1.6/CLAUDE.md); the startup
// attribute only satisfies the game's check on static texture fields, which would otherwise warn
// about it at every boot.
[StaticConstructorOnStartup]
public static class CerebrexNode
{
    public const string DefName = "Apparel_CerebrexNode";
    public const string TexPath = "Things/Pawn/Humanlike/Apparel/CerebrexNodeOverlord/CerebrexNodePack";

    private static Texture2D icon;

    public static Texture2D Icon
    {
        get
        {
            if (icon == null)
                icon = ContentFinder<Texture2D>.Get(TexPath, reportFailure: false);
            return icon;
        }
    }

    // Odyssey's def, or null without Odyssey or when another mod removed it.
    public static ThingDef Def => ModsConfig.OdysseyActive ? DefDatabase<ThingDef>.GetNamedSilentFail(DefName) : null;

    // Whether the patch ran this session: the def carries our path.
    public static bool UsesOurArt(ThingDef def) => def.graphicData?.texPath == TexPath;
}
