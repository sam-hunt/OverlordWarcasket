using System;
using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// The mechanitor command range as the settings window offers it: two radio groups, who it
// applies to (CommandRangeAppliesTo) and how far it reaches (CommandRangeScope), the Radius
// scope's rung a slider under its own radio. Until the player picks, each is the def's: the
// piece the CommandRangeExtension sits on in XML (the helmet), its scope and its radius, read by
// LoadDefaults on every fresh load before Apply moves anything, so the def and its header stay
// the record and another mod's XML patch moves the defaults too. Only an override is saved, the
// choices by name (so a name this version does not know reads as the default) and the radius as
// a stored float cleared when it lands on the default's rung; written onto the defs and the
// patches' state by CommandRange.Apply through the settings' ApplyToDefs.
//
// Layout (Unique Weapons Unbound's skill-check prerequisite is the model): each group is a
// label row, indented as a Tunable row is and carrying the group's tooltip, then one radio row
// per option under it, tabbed in further, the radio at the row's end as the family's checkboxes
// sit; the default option carries the "(default)" suffix. While the range applies to no one the
// second group is inert: grey, non-interactive, its tooltips the one note that it has no effect,
// its stored choices kept for when the first group turns it back on. The slider runs over the
// rungs of CommandRange.Ladder, each as wide as the next, is live whenever the group is, and
// pressing or moving it also selects the Radius scope so the player need not click the radio
// first. Its radio's label doubles as its value, "Within 24.9 cells", with vanilla's rung named
// as such.
public sealed class CommandRangeSetting
{
    private const string Key = "CommandRange";
    private const float Unset = -1f;

    // The option rows tab in past the group label; the slider past the radio label's start.
    private const float OptionIndent = 20f;
    private const float SliderIndent = 12f;
    private const float SliderHeight = 22f;
    private const float GroupGap = 8f;
    private const float LabelGap = 4f;

    private static readonly CommandRangeAppliesTo[] AppliesToOptions =
    [
        CommandRangeAppliesTo.Helmet, CommandRangeAppliesTo.Shoulders,
        CommandRangeAppliesTo.AllMechanitors, CommandRangeAppliesTo.None,
    ];

    private string appliesToName;
    private string scopeName;
    private float radius = Unset;
    private float[] ladder;
    private ThingDef helmet;
    private ThingDef shoulders;

    // False while a piece is missing or neither carries the extension; the section is skipped.
    public bool Available { get; private set; }

    public CommandRangeAppliesTo DefaultAppliesTo { get; private set; }
    public CommandRangeScope DefaultScope { get; private set; }
    public float DefaultRadius { get; private set; }

    public CommandRangeAppliesTo AppliesTo =>
        appliesToName != null && Enum.TryParse(appliesToName, out CommandRangeAppliesTo value) ? value : DefaultAppliesTo;

    public CommandRangeScope Scope =>
        scopeName != null && Enum.TryParse(scopeName, out CommandRangeScope value) ? value : DefaultScope;

    public float Radius => radius < 0f ? DefaultRadius : radius;

    // The section header's icon: the piece the range rides on, the helmet for the piece-free
    // choices, read off the def at draw time so a play-data reload's fresh texture is the one
    // drawn.
    public Texture2D Icon => (AppliesTo == CommandRangeAppliesTo.Shoulders ? shoulders : helmet)?.uiIcon;

    public void Reset()
    {
        appliesToName = null;
        scopeName = null;
        radius = Unset;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref appliesToName, Key + "AppliesTo");
        Scribe_Values.Look(ref scopeName, Key + "Scope");
        Scribe_Values.Look(ref radius, Key + "Radius", Unset);
    }

    // Off defs fresh from XML: the settings' LoadDefaultsFromDefs, which Startup runs before the
    // first Apply of every load.
    public void LoadDefaults()
    {
        Available = CommandRange.TryReadDefs(out helmet, out shoulders, out CommandRangeAppliesTo bearer, out CommandRangeExtension extension);
        if (!Available)
            return;
        DefaultAppliesTo = bearer;
        DefaultScope = extension.scope;
        DefaultRadius = extension.radius;
    }

    public void Apply()
    {
        if (Available)
            CommandRange.Apply(AppliesTo, Scope, Radius);
        else
            CommandRange.Clear();
    }

    // What the groups amount to: who may order their mechs how far, or that vanilla's range
    // stands.
    public string Summary()
    {
        string vanilla = CellsText(CommandRange.VanillaRadius);
        CommandRangeAppliesTo appliesTo = AppliesTo;
        if (appliesTo == CommandRangeAppliesTo.None)
            return "OLWC_CommandRangeSummaryNone".Translate(vanilla);
        string reach = Scope switch
        {
            CommandRangeScope.Anywhere => "OLWC_CommandRangeReachAnywhere".Translate(),
            CommandRangeScope.Radius => "OLWC_CommandRangeReachRadius".Translate(CellsText(Radius)),
            _ => "OLWC_CommandRangeReachMap".Translate(),
        };
        if (appliesTo == CommandRangeAppliesTo.AllMechanitors)
            return "OLWC_CommandRangeSummaryAll".Translate(reach);
        return "OLWC_CommandRangeSummaryWearing".Translate(PieceLabel(appliesTo), reach);
    }

    public void DoRows(Listing_Standard listing)
    {
        ladder ??= CommandRange.Ladder(CommandRange.RingCap);

        GroupLabel(listing, "OLWC_CommandRangeAppliesTo".Translate(), "OLWC_CommandRangeAppliesToDesc".Translate(), enabled: true);
        foreach (CommandRangeAppliesTo option in AppliesToOptions)
        {
            if (Option(listing, AppliesToLabel(option), AppliesToDesc(option), AppliesTo == option, option == DefaultAppliesTo, enabled: true))
                appliesToName = option == DefaultAppliesTo ? null : option.ToString();
        }

        bool enabled = AppliesTo != CommandRangeAppliesTo.None;
        string inert = "OLWC_CommandRangeInert".Translate();
        listing.Gap(GroupGap);
        GroupLabel(listing, "OLWC_CommandRangeScope".Translate(), enabled ? "OLWC_CommandRangeScopeDesc".Translate() : inert, enabled);
        ScopeOption(listing, CommandRangeScope.CurrentMap, "OLWC_CommandRangeCurrentMap".Translate(),
            enabled ? "OLWC_CommandRangeCurrentMapDesc".Translate() : inert, enabled);
        ScopeOption(listing, CommandRangeScope.Anywhere, "OLWC_CommandRangeAnywhere".Translate(),
            enabled ? "OLWC_CommandRangeAnywhereDesc".Translate() : inert, enabled);

        string vanillaSuffix = Mathf.Abs(Radius - CommandRange.VanillaRadius) < 0.05f ? "OLWC_CommandRangeVanillaSuffix".Translate() : "";
        string radiusLabel = "OLWC_CommandRangeRadius".Translate(CellsText(Radius), vanillaSuffix);
        string radiusDesc = enabled
            ? "OLWC_CommandRangeRadiusDesc".Translate(CellsText(CommandRange.VanillaRadius), CellsText(DefaultRadius))
            : inert;
        ScopeOption(listing, CommandRangeScope.Radius, radiusLabel, radiusDesc, enabled);
        Slider(listing, enabled, inert);
        listing.Gap(Tunable.RowGap);
    }

    private void ScopeOption(Listing_Standard listing, CommandRangeScope option, string label, string tooltip, bool enabled)
    {
        if (Option(listing, label, tooltip, Scope == option, option == DefaultScope, enabled))
            SelectScope(option);
    }

    private void SelectScope(CommandRangeScope option) => scopeName = option == DefaultScope ? null : option.ToString();

    // The slider runs over the ladder's rungs; a radius off them (an XML default between rungs,
    // or an old save) sits on its nearest until moved. A press over the track is read before
    // the slider consumes the event, so clicking the current rung selects the scope too.
    private void Slider(Listing_Standard listing, bool enabled, string inert)
    {
        Rect rect = listing.GetRect(SliderHeight);
        rect.xMin += Tunable.RowIndent + OptionIndent + SliderIndent;
        rect.xMax -= Tunable.RowEndPad + Widgets.RadioButtonSize + SliderIndent;
        int current = Tunable.Nearest(ladder, Radius);
        if (!enabled)
        {
            Color color = GUI.color;
            GUI.color = ColoredText.SubtleGrayColor;
            Widgets.HorizontalSlider(rect, current, 0f, ladder.Length - 1, middleAlignment: true);
            GUI.color = color;
            TooltipHandler.TipRegion(rect, inert);
            return;
        }
        bool pressed = Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(rect);
        int index = Mathf.RoundToInt(Widgets.HorizontalSlider(rect, current, 0f, ladder.Length - 1, middleAlignment: true));
        if (pressed || index != current)
        {
            radius = index == Tunable.Nearest(ladder, DefaultRadius) ? Unset : ladder[index];
            SelectScope(CommandRangeScope.Radius);
        }
        if (Scope != CommandRangeScope.Radius)
            TooltipHandler.TipRegion(rect, "OLWC_CommandRangeSliderSelects".Translate());
    }

    // A group's label row, indented as a Tunable row is, grey while its group is inert.
    private static void GroupLabel(Listing_Standard listing, string label, string tooltip, bool enabled)
    {
        Rect rect = listing.GetRect(Text.LineHeight);
        rect.xMin += Tunable.RowIndent;
        rect.xMax -= Tunable.RowEndPad;
        Color color = GUI.color;
        if (!enabled)
            GUI.color = ColoredText.SubtleGrayColor;
        Widgets.Label(rect, label);
        GUI.color = color;
        TooltipHandler.TipRegion(rect, tooltip);
        listing.Gap(LabelGap);
    }

    // One radio row; true when the player picked it. A disabled row draws grey and ignores the
    // click.
    private static bool Option(Listing_Standard listing, string label, string tooltip, bool active, bool isDefault, bool enabled)
    {
        if (isDefault)
            label += "OLWC_DefaultSuffix".Translate();
        bool clicked = listing.RadioButton(label, active, Tunable.RowIndent + OptionIndent, Tunable.RowEndPad, tooltip, null, !enabled);
        return clicked && enabled;
    }

    private string AppliesToLabel(CommandRangeAppliesTo option) => option switch
    {
        CommandRangeAppliesTo.Helmet or CommandRangeAppliesTo.Shoulders => "OLWC_CommandRangeWearing".Translate(PieceLabel(option)),
        CommandRangeAppliesTo.AllMechanitors => "OLWC_CommandRangeAllMechanitors".Translate(),
        _ => "OLWC_CommandRangeNone".Translate(),
    };

    private string AppliesToDesc(CommandRangeAppliesTo option) => option switch
    {
        CommandRangeAppliesTo.Helmet or CommandRangeAppliesTo.Shoulders => "OLWC_CommandRangeWearingDesc".Translate(PieceLabel(option)),
        CommandRangeAppliesTo.AllMechanitors => "OLWC_CommandRangeAllMechanitorsDesc".Translate(),
        _ => "OLWC_CommandRangeNoneDesc".Translate(CellsText(CommandRange.VanillaRadius)),
    };

    // The piece's own label, so a rename or a translation reaches the rows.
    private string PieceLabel(CommandRangeAppliesTo option) =>
        (option == CommandRangeAppliesTo.Shoulders ? shoulders : helmet).label;

    // A radius as the rows show one: a tenth where there is one, no trailing zero.
    private static string CellsText(float cells) => cells.ToString("0.#");
}
