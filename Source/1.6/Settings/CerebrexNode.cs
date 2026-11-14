using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// The art Odyssey's cerebrex node is drawn with, by what a dye paints: Odyssey's own, untouched;
// ours with no mask, so a dye tints the whole piece as it does Odyssey's; ours with the masks
// that leave the ring and the core their own colors, the dye on the frame alone; ours with the
// masks that leave only the core, the dye on the frame and the ring. The names are saved in
// the settings file and named by the patch's choices, so a rename is a change to both.
public enum CerebrexArt
{
    Odyssey,
    DyeAll,
    DyeFrame,
    DyeFrameAndRing,
}

// Odyssey's cerebrex node (Apparel_CerebrexNode) as the settings window sees it. Its art is
// swapped for ours by the XML patch 1.6/Mods/Odyssey/Patches/CerebrexNodeArt.xml, one branch
// per CerebrexArt but Odyssey's, picked by the cerebrex art setting while the patches apply;
// nothing here writes the def. TexPath and RingTexPath are the two paths that patch writes,
// both under Mods/Odyssey/Textures: the worn pack reads its masks beside its art by name alone
// (ApparelGraphicRecordGetter.TryGetGraphicApparel hands Graphic_Multi no mask path, so it looks
// for <path>_northm and its like; 1.6 and 1.7.5000 decompiles), so the second mask set sits in
// a folder of its own beside a copy of the art. OdysseyTexPath is the path Odyssey's def ships
// with, for the picker's first tile, which must draw Odyssey's art in a session whose def
// carries ours. Live reads off the def which branch ran this session.
//
// Every texture the window draws resolves through Texture: ours are mod-shipped, so they
// re-resolve on a Unity-null check after a play-data reload destroys them
// (Source/1.6/CLAUDE.md), and Odyssey's takes the same road. The startup attribute only
// satisfies the game's check on static texture fields, which would otherwise warn at every boot.
[StaticConstructorOnStartup]
public static class CerebrexNode
{
    public const string DefName = "Apparel_CerebrexNode";
    public const string OdysseyTexPath = "Things/Pawn/Humanlike/Apparel/CerebrexNode/CerebrexNodePack";
    public const string TexPath = "Things/Pawn/Humanlike/Apparel/CerebrexNodeOverlord/CerebrexNodePack";
    public const string RingTexPath = "Things/Pawn/Humanlike/Apparel/CerebrexNodeOverlordRing/CerebrexNodePack";

    // The worn pack's facing the picker shows, and its mask: the back, where the core sits.
    private const string NorthSuffix = "_north";
    private const string NorthMaskSuffix = "_northm";

    private static readonly Dictionary<string, Texture2D> Textures = [];

    // The section header's icon: our item art.
    public static Texture2D Icon => Texture(TexPath);

    // Odyssey's def, or null without Odyssey or when another mod removed it.
    public static ThingDef Def => ModsConfig.OdysseyActive ? DefDatabase<ThingDef>.GetNamedSilentFail(DefName) : null;

    public static string PathFor(CerebrexArt art) => art switch
    {
        CerebrexArt.Odyssey => OdysseyTexPath,
        CerebrexArt.DyeFrameAndRing => RingTexPath,
        _ => TexPath,
    };

    public static bool Masked(CerebrexArt art) => art is CerebrexArt.DyeFrame or CerebrexArt.DyeFrameAndRing;

    // The pack seen from behind as the choice draws it, and the mask it draws through; no mask
    // for a choice that reads none.
    public static Texture2D North(CerebrexArt art) => Texture(PathFor(art) + NorthSuffix);

    public static Texture2D NorthMask(CerebrexArt art) => Masked(art) ? Texture(PathFor(art) + NorthMaskSuffix) : null;

    // Which branch of the patch ran this session: the def carries one of our paths, and the
    // worn mask flag the masked branches add.
    public static CerebrexArt Live(ThingDef def)
    {
        string path = def.graphicData?.texPath;
        if (path == RingTexPath)
            return CerebrexArt.DyeFrameAndRing;
        if (path != TexPath)
            return CerebrexArt.Odyssey;
        return def.apparel?.useWornGraphicMask == true ? CerebrexArt.DyeFrame : CerebrexArt.DyeAll;
    }

    private static Texture2D Texture(string path)
    {
        if (!Textures.TryGetValue(path, out Texture2D texture) || texture == null)
            Textures[path] = texture = ContentFinder<Texture2D>.Get(path, reportFailure: false);
        return texture;
    }
}
