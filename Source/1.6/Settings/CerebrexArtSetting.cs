using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace OverlordWarcasket;

// The cerebrex node's art as the settings window offers it: one tile per CerebrexArt, side by
// side, each the worn pack seen from behind as that choice draws it, the facing whose red core
// the choices differ around. The tile is the control: a click picks it, and the picked one
// takes vanilla's selected-option fill and outline (Widgets.DrawOptionBackground, the look of
// the scenario and storyteller pickers). Nothing here writes a def: the XML patch
// 1.6/Mods/Odyssey/Patches/CerebrexNodeArt.xml reads the choice by name while the patches apply
// (PatchOperationSetting, through TryMatch), so the group's label says a change takes a restart
// and the tile whose art is live this session, read off the def by the caller, carries an "in
// use" tag until it does. The default is Odyssey's own art, the XML's state with no patch run;
// only an override is saved, by name, so a name this version does not know reads as the default.
//
// Under the tiles a row of swatches dyes the previews, since what tells the choices apart is
// what a dye paints: the first is undyed, white, what an undyed node draws through any mask,
// then five of Core's paint colors by def name, the closest it has to white, dark grey, red,
// green and blue (Core carries structure, hair and planning colors only; the dyes a styling
// station offers are Ideology's). The window opens on the dark grey, since undyed the masked
// choices draw alike. The swatch is view state, never saved, and dyes nothing.
//
// A tile draws its art as the game draws the worn pack. Without a mask that is the texture
// times the dye, which the GUI's own color does. With one it is vanilla's icon path for a
// masked thing (BuildableDef.ResolveIcon and Widgets.ThingIcon, 1.6 and 1.7.5000 decompiles):
// the UI twin of CutoutComplex from ShaderDatabase.TryGetUIShader, a pooled material carrying
// the dye and the mask, drawn by GenUI.DrawTextureWithMaterial. MaterialPool keeps a material
// per texture, mask and dye, a dozen at most.
public sealed class CerebrexArtSetting : IPatchSetting
{
    public const CerebrexArt Default = CerebrexArt.Odyssey;

    private const string Key = "CerebrexArt";

    // A tile is a square of art over its label, as wide as the row allows up to MaxTileSide,
    // the selected tile's outline (3 px outside the tile) clear of its neighbours in the gap.
    private const float MaxTileSide = 168f;
    private const float TileGap = 10f;
    private const float TilePad = 8f;
    private const float GroupGap = 8f;

    // A swatch as vanilla's color boxes are (Widgets.ColorBox): the color inside its padding.
    private const int SwatchSize = 22;
    private const int SwatchPad = 2;
    private const float SwatchLabelGap = 10f;

    private static readonly CerebrexArt[] Options =
    [
        CerebrexArt.Odyssey, CerebrexArt.DyeAll, CerebrexArt.DyeFrame, CerebrexArt.DyeFrameAndRing,
    ];

    private const string OpeningDye = "Structure_Slate";

    private static readonly string[] DyeDefNames =
    [
        "Structure_White", OpeningDye, "Structure_Red", "Structure_Green", "Structure_Blue",
    ];

    private readonly List<ColorDef> dyes = [];
    private string name;
    private Color dye = Color.white;

    public CerebrexArt Value => TryParse(name, out CerebrexArt value) ? value : Default;

    public void Reset() => name = null;

    public void ExposeData() => Scribe_Values.Look(ref name, Key);

    public bool TryMatch(string choice, out bool matches)
    {
        bool known = TryParse(choice, out CerebrexArt named);
        matches = known && named == Value;
        return known;
    }

    // The swatches' color defs, on every fresh load: a play-data reload replaces them. The
    // preview starts on the opening dye, or undyed when another mod removed that color.
    public void LoadDefaults()
    {
        dyes.Clear();
        dye = Color.white;
        foreach (string defName in DyeDefNames)
        {
            ColorDef def = DefDatabase<ColorDef>.GetNamedSilentFail(defName);
            if (def == null)
                continue;
            dyes.Add(def);
            if (defName == OpeningDye)
                dye = def.color;
        }
    }

    public void DoRows(Listing_Standard listing, CerebrexArt live)
    {
        CommandRangeSetting.GroupLabel(listing, "OLWC_CerebrexArt".Translate() + "OLWC_RequiresRestartSuffix".Translate(),
            "OLWC_CerebrexArtDesc".Translate(Label(Default)), enabled: true);
        Tiles(listing, live);
        listing.Gap(GroupGap);
        Swatches(listing);
        listing.Gap(Tunable.RowGap);
    }

    private void Tiles(Listing_Standard listing, CerebrexArt live)
    {
        float width = listing.ColumnWidth - Tunable.RowIndent - Tunable.RowEndPad;
        float side = Mathf.Min(MaxTileSide, Mathf.Floor((width - TileGap * (Options.Length - 1)) / Options.Length));
        float labelHeight = 0f;
        foreach (CerebrexArt option in Options)
            labelHeight = Mathf.Max(labelHeight, Text.CalcHeight(TileLabel(option), side - 2f * TilePad));

        Rect row = listing.GetRect(side + labelHeight + TilePad);
        CerebrexArt current = Value;
        for (int i = 0; i < Options.Length; i++)
        {
            CerebrexArt option = Options[i];
            var tile = new Rect(row.x + Tunable.RowIndent + i * (side + TileGap), row.y, side, row.height);
            if (Tile(tile, option, option == current, option == live))
                name = option == Default ? null : option.ToString();
        }
    }

    // One tile; true when the player clicked it. The tick is the radio buttons'.
    private bool Tile(Rect tile, CerebrexArt option, bool selected, bool live)
    {
        Widgets.DrawOptionBackground(tile, selected);
        var art = new Rect(tile.x + TilePad, tile.y + TilePad, tile.width - 2f * TilePad, tile.width - 2f * TilePad);
        DrawArt(art, option);

        GameFont font = Text.Font;
        TextAnchor anchor = Text.Anchor;
        if (live)
        {
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = ColoredText.SubtleGrayColor;
            Widgets.Label(new Rect(art.x, tile.y + TilePad / 2f, art.width, Text.LineHeight), "OLWC_CerebrexInUse".Translate());
            GUI.color = Color.white;
            Text.Font = font;
        }
        Text.Anchor = TextAnchor.UpperCenter;
        Widgets.Label(new Rect(art.x, art.yMax, art.width, tile.yMax - art.yMax), TileLabel(option));
        Text.Anchor = anchor;

        TooltipHandler.TipRegion(tile, ("OLWC_CerebrexArt_" + option + "Desc").Translate());
        if (!Widgets.ButtonInvisible(tile))
            return false;
        if (!selected)
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        return true;
    }

    // The pack from behind in the preview dye; see the class header. A tile whose texture is
    // missing stays empty.
    private void DrawArt(Rect rect, CerebrexArt option)
    {
        Texture2D texture = CerebrexNode.North(option);
        if (texture == null)
            return;
        Texture2D mask = CerebrexNode.NorthMask(option);
        if (mask == null || !ShaderDatabase.TryGetUIShader(ShaderDatabase.CutoutComplex, out Shader shader))
        {
            GUI.color = dye;
            GUI.DrawTexture(rect, texture);
            GUI.color = Color.white;
            return;
        }
        Material material = MaterialPool.MatFrom(new MaterialRequest(texture, shader, dye) { maskTex = mask });
        GenUI.DrawTextureWithMaterial(rect, texture, material);
    }

    // The swatch row: its label, then the undyed swatch and one per color def, each a vanilla
    // color box, which outlines the picked one and ticks on a click.
    private void Swatches(Listing_Standard listing)
    {
        float box = SwatchSize + 2 * SwatchPad;
        Rect row = listing.GetRect(box);
        row.xMin += Tunable.RowIndent;
        row.xMax -= Tunable.RowEndPad;

        string label = "OLWC_CerebrexDye".Translate();
        var labelRect = new Rect(row.x, row.y, Text.CalcSize(label).x, row.height);
        TextAnchor anchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(labelRect, label);
        Text.Anchor = anchor;
        TooltipHandler.TipRegion(labelRect, "OLWC_CerebrexDyeDesc".Translate());

        var swatch = new Rect(labelRect.xMax + SwatchLabelGap, row.y, box, box);
        Swatch(swatch, Color.white, "OLWC_CerebrexDyeNone".Translate());
        foreach (ColorDef def in dyes)
        {
            swatch.x += box + SwatchPad;
            Swatch(swatch, def.color, def.LabelCap);
        }
    }

    private void Swatch(Rect rect, Color color, string tooltip)
    {
        Widgets.ColorBox(rect, ref dye, color, SwatchSize, SwatchPad);
        TooltipHandler.TipRegion(rect, tooltip);
    }

    private static string TileLabel(CerebrexArt option) =>
        option == Default ? Label(option) + "OLWC_DefaultSuffix".Translate() : Label(option);

    private static string Label(CerebrexArt option) => ("OLWC_CerebrexArt_" + option).Translate();

    // By name only: Enum.TryParse alone would take a number too.
    private static bool TryParse(string name, out CerebrexArt value)
    {
        value = Default;
        return name != null && Enum.IsDefined(typeof(CerebrexArt), name) && Enum.TryParse(name, out value);
    }
}
