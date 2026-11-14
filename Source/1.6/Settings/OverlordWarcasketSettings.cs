using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using AbilityDef = VEF.Abilities.AbilityDef;

namespace OverlordWarcasket;

// The set's mod settings. Each setting overrides a def field, written onto the def by
// ApplyToDefs (at startup, then whenever the window closes), so every reader of those fields,
// vanilla's and VEF's included, sees the setting with no code of its own. The defaults, and the
// rationale for them, are the defs'. Nothing here is saved into a game: a save loads the same
// with any settings, and with none.
//
// On the armor's CompProperties_MechResurrector entry, the charge capacity overrides maxCharges,
// the recharge rate chargesPerDay and the reload cost ammoCountPerCharge, vanilla's reload field;
// that row is skipped, and the field left alone, while the def does not reload by the charge
// (another mod's patch). On the OLWC_ResurrectMech AbilityDef, the casting time and cooldown
// override castTime (in seconds, ticks on the def) and cooldownTime (stored in game hours, shown
// in seconds, hours or days by its size; ticks on the def), and on its ResurrectMechExtension the
// corpse window overrides maxCorpseAgeTicks (stored and shown in game hours, days from one day
// up; ticks on the def, 0 for the slider's last stop, no limit) and the mechlink switch
// requireMechlink. On the armor's
// CompProperties_ResurrectorSparks entry, the sparks switch overrides onlyWhileDrafted, and the
// spark frequency overrides chancePerTick on its effecter's chance sprayer, stored as a percent
// of the same child's chance on Biotech's ApocrionAttached, the apocriton's crackle the sparks
// are cut from (same bursts, same mote), so 100% reads as "as often as an apocriton" and 0% is
// none; SubEffecter_SprayerChance reads the def's chance every tick (EffectiveChancePerTick,
// 1.7.4990 decompile), so a write reaches running effecters at once. That row is skipped, and
// the field left alone, while either def lacks a chance sprayer (another mod's patch). Both
// rows sit in the ability's section, the resurrector being the ability's, the slider after the
// other sliders and the switch after the other switch. The weight class costs are
// def-only. A line of what the rows amount to opens the section, under its header, which carries
// the ability's icon and tooltip, read off the def at draw time: VEF resolves def.icon from
// iconPath in a deferred step at the end of loading, and a play-data reload replaces the def and
// its texture together.
//
// The command range section is two radio groups and a slider (CommandRangeSetting), who the
// set's extended mechanitor command range applies to and how far it reaches, written through
// CommandRange.Apply onto the set's CommandRangeExtension (its scope and radius), onto the piece
// that carries it (moved between the helmet and the shoulders) and into the state the two
// patches read; the defaults are the extension's as the XML places it, on the shoulders. Its
// header carries that piece's icon, read off the def at draw time, and the section's own text.
// It needs only the two pieces, so it is skipped when either is missing or neither carries the
// extension, and shown without the ability's defs.
//
// The work section is one checkbox per work type the trait mirror leaves to the player
// (WorkSetting over WorkProfile.Toggleable), written onto the overlord trait's disabledWorkTypes
// through WorkProfile.Apply, which also refreshes every entombed pilot; the mirror's own rule is
// each box's default. Its header's icon is vanilla's mining work site, a Core texture, so it
// survives a play-data reload as a plain static. The section is skipped while the mirror found
// nothing to toggle (a broken install).
//
// The cerebrex node section is one switch that writes no def: the XML patch
// 1.6/Mods/Odyssey/Patches/CerebrexNodeArt.xml reads it by name while the patches apply
// (PatchOperationSetting), so its row says a change takes a restart and the summary reads which
// art is live off the def (CerebrexNode.UsesOurArt) rather than off the box. Its header carries
// the node's own label and description and our art as its icon, a mod-shipped texture that
// CerebrexNode.Icon re-resolves after a reload. The section needs none of the set's defs and is
// skipped without Odyssey.
//
// What a write leaves on live instances is caught up by them: a running cooldown on the next
// gizmo request, keyed on Generation (Ability_ResurrectMech.ReconcileSettings); charges above a
// lowered capacity are trimmed off the worn resurrectors at once (TrimWornResurrectors), and a
// changed rate reaches a resurrector on its next regen step, its part charge kept.
public class OverlordWarcasketSettings : ModSettings
{
    private const string AbilityDefName = "OLWC_ResurrectMech";
    private const string ArmorDefName = "OLWC_Warcasket_Overlord";

    // The whitespace above every section header but the first, and below the last row.
    private const float SectionGap = 28f;

    // Bumped by every ApplyToDefs, including the one at startup.
    public static int Generation { get; private set; }

    public readonly BoolSetting requireMechlink = new("RequireMechlink");
    public readonly BoolSetting sparksOnlyDrafted = new("SparksOnlyDrafted");
    public readonly BoolSetting cerebrexArt = new("CerebrexArt") { RequiresRestart = true };
    public readonly WorkSetting work = new();
    public readonly CommandRangeSetting commandRange = new();

    // The cooldown is game hours up to ten days, banded so one slider spans a gap between casts
    // in a fight and one between raids: real seconds at normal speed up to ten (quarter, then
    // half, then whole seconds), then half hours to 3 h, hours to 12 h, quarter days to a day,
    // half days to three, and whole days. The slider gives every stop the same width.
    private const float CooldownMax = 10f * GenDate.HoursPerDay;
    private static readonly (float upTo, float step)[] CooldownBands =
    [
        (Seconds(3f), Seconds(0.25f)), (Seconds(5f), Seconds(0.5f)), (Seconds(10f), Seconds(1f)),
        (3f, 0.5f), (12f, 1f), (24f, 6f), (72f, 12f), (CooldownMax, GenDate.HoursPerDay),
    ];

    // The corpse window is game hours from half an hour to ten days, the unit the player's clock
    // runs in at any speed: half hours to 3 h, hours to 12 h, quarter days to a day, half days to
    // three, and whole days, the cooldown's bands above its seconds. One more stop a day past
    // the longest window is no limit, shown as the word and written to the def as 0.
    private const float CorpseAgeMax = 10f * GenDate.HoursPerDay;
    private const float CorpseAgeNoLimit = CorpseAgeMax + GenDate.HoursPerDay;

    // The capacity in charges; the recharge in charges per day, fine at the slow end around the
    // shipped rate, coarser up to the shipped resurrector in a day; the reload cost in items of the
    // def's ammo per charge; the casting time banded as the Medic's is.
    public readonly Tunable charges = new("Charges", 1f, 30f, 1f, Whole);
    public readonly Tunable recharge = new("ChargesPerDay", 0f, 10f, 0.25f, Decimal)
    {
        StepBands = [(2f, 0.25f), (5f, 0.5f), (10f, 1f)],
    };
    public readonly Tunable reloadCost = new("ReloadCost", 1f, 10f, 1f, Whole);
    public readonly Tunable corpseAge = new("CorpseAge", 0.5f, CorpseAgeNoLimit, 0.5f, CorpseWindow)
    {
        StepBands = [(3f, 0.5f), (12f, 1f), (24f, 6f), (72f, 12f), (CorpseAgeNoLimit, GenDate.HoursPerDay)],
    };
    public readonly Tunable castTime = new("CastTime", 0f, 10f, 0.25f, RealSeconds)
    {
        StepBands = [(3f, 0.25f), (6f, 0.5f), (10f, 1f)],
    };
    public readonly Tunable cooldown = new("Cooldown", 0f, CooldownMax, 0.25f, Period) { StepBands = CooldownBands };

    // The spark frequency in percent of the apocriton's rate, up to twice it; see the header.
    public readonly Tunable sparkFrequency = new("SparkFrequency", 0f, 200f, 5f, Percent);

    private bool defaultsLoaded;
    private Vector2 scrollPosition;
    private float contentHeight;

    // The tooltip over the ability section's header: the ability, its name as VEF titles an
    // ability's tooltip over the description its command shows in game.
    private string abilityTip;

    // The ability def, for its header icon; see the class header.
    private AbilityDef abilityDef;

    // The cost lines the summary quotes, read when the defaults are.
    private string costs;

    // The resurrector's reload item, while the def reloads by the charge; null hides the reload
    // cost row and the summary's reload clause.
    private ThingDef reloadItem;

    // The sparks effecter's chance sprayer, which the frequency writes, and the apocriton's
    // chance per tick it is a percent of; a null sprayer hides the frequency row.
    private SubEffecterDef sparkSprayer;
    private float apocritonSparkChance;

    // The work section's header icon, vanilla's mining work site. Its art is in the base game's
    // Resources, not Ideology's, so it loads whatever DLCs are active and survives a reload.
    private Texture2D workIcon;

    // Odyssey's cerebrex node, whether it carries our art this session and its header tooltip;
    // null without Odyssey, when the section is skipped.
    private ThingDef cerebrexNode;
    private bool cerebrexLive;
    private string cerebrexTip;

    private IEnumerable<Tunable> All => [charges, recharge, reloadCost, corpseAge, castTime, cooldown, sparkFrequency];

    private static string Whole(float v) => Mathf.RoundToInt(v).ToString();

    private static string Percent(float v) => (v / 100f).ToStringPercent();

    // An effecter's first chance sprayer child, the one ApocrionAttached and OLWC_ResurrectorSparks
    // both open on.
    private static SubEffecterDef ChanceSprayer(EffecterDef effecter) =>
        effecter?.children?.FirstOrDefault(child => child.subEffecterClass == typeof(SubEffecter_SprayerChance));

    private static string Decimal(float v) => v.ToString("0.##");

    // Game hours from real seconds at normal speed.
    private static float Seconds(float s) => s * GenTicks.TicksPerRealSecond / GenDate.TicksPerHour;

    // Every time a row or summary shows carries its unit as vanilla's short period form does
    // (GenDate.ToStringTicksToPeriod with shortForm): the number, then the unit letter, no space.
    private static string RealSeconds(float s) => s.ToString("0.##") + "LetterSecond".Translate();

    // A period in game hours, in the unit of the cooldown band it falls in: seconds below the
    // half hour, hours below a day, days from there.
    private static string Period(float hours)
    {
        if (hours < 0.5f)
            return RealSeconds(hours * GenDate.TicksPerHour / GenTicks.TicksPerRealSecond);
        if (hours < GenDate.HoursPerDay)
            return hours.ToString("0.##") + "LetterHour".Translate();
        return (hours / GenDate.HoursPerDay).ToString("0.##") + "LetterDay".Translate();
    }

    // The corpse window's value: a period, or the word for the stop past the longest one.
    private static bool NoCorpseLimit(float hours) => hours > CorpseAgeMax;

    private static string CorpseWindow(float hours) =>
        NoCorpseLimit(hours) ? "OLWC_Unlimited".Translate().Resolve() : Period(hours);

    public override void ExposeData()
    {
        base.ExposeData();
        requireMechlink.ExposeData();
        sparksOnlyDrafted.ExposeData();
        cerebrexArt.ExposeData();
        work.ExposeData();
        commandRange.ExposeData();
        foreach (Tunable tunable in All)
            tunable.ExposeData();
    }

    // Reads the def-backed defaults before the first ApplyToDefs overwrites them; defs must be
    // loaded, so ModInit's static constructor calls this, never the Mod constructor. The cerebrex
    // node's and the command range's come first, needing none of the ability's defs.
    public void LoadDefaultsFromDefs()
    {
        cerebrexNode = CerebrexNode.Def;
        if (cerebrexNode != null)
        {
            cerebrexLive = CerebrexNode.UsesOurArt(cerebrexNode);
            cerebrexTip = HeaderTip(cerebrexNode.LabelCap, cerebrexNode.description);
        }
        commandRange.LoadDefaults();
        if (!TryGetDefs(out Defs defs))
            return;
        charges.DefaultValue = defs.resurrector.maxCharges;
        recharge.DefaultValue = defs.resurrector.chargesPerDay;
        if (defs.resurrector.ammoDef != null && defs.resurrector.ammoCountPerCharge > 0 && defs.resurrector.ammoCountToRefill == 0)
        {
            reloadItem = defs.resurrector.ammoDef;
            reloadCost.DefaultValue = defs.resurrector.ammoCountPerCharge;
            reloadCost.DescArgs = [Find.ActiveLanguageWorker.Pluralize(reloadItem.label)];
        }
        else
        {
            reloadItem = null;
        }
        corpseAge.DefaultValue = defs.extension.LimitsCorpseAge
            ? (float)defs.extension.maxCorpseAgeTicks / GenDate.TicksPerHour
            : CorpseAgeNoLimit;
        castTime.DefaultValue = (float)defs.ability.castTime / GenTicks.TicksPerRealSecond;
        cooldown.DefaultValue = (float)defs.ability.cooldownTime / GenDate.TicksPerHour;
        requireMechlink.DefaultValue = defs.extension.requireMechlink;
        sparksOnlyDrafted.DefaultValue = defs.sparks.onlyWhileDrafted;
        sparkSprayer = ChanceSprayer(defs.sparks.effecterDef);
        apocritonSparkChance = ChanceSprayer(DefDatabase<EffecterDef>.GetNamedSilentFail("ApocrionAttached"))?.chancePerTick ?? 0f;
        if (apocritonSparkChance > 0f && sparkSprayer != null)
            sparkFrequency.DefaultValue = sparkSprayer.chancePerTick / apocritonSparkChance * 100f;
        else
            sparkSprayer = null;
        abilityTip = HeaderTip(defs.ability.LabelCap, defs.ability.description);
        abilityDef = defs.ability;
        costs = defs.extension.costs
            .Where(cost => cost.weightClass != null)
            .Select(cost => "OLWC_SummaryCost".Translate(cost.weightClass, cost.cost).Resolve())
            .ToCommaList(useAnd: true);
        workIcon = ContentFinder<Texture2D>.Get("World/WorldObjects/Expanding/Sites/MiningWorkSite");
        defaultsLoaded = true;
    }

    // The work profile and the command range go first: they need none of the ability's defs, so
    // a saved choice still holds while they are missing, though the page is unavailable to
    // change it. The cerebrex art switch writes nothing here; its patch read it at startup.
    public void ApplyToDefs()
    {
        WorkProfile.Apply(work.IsOpen);
        commandRange.Apply();
        if (!defaultsLoaded || !TryGetDefs(out Defs defs))
            return;
        defs.resurrector.maxCharges = Mathf.Max(1, charges.IntValue);
        defs.resurrector.chargesPerDay = Mathf.Max(0f, recharge.Value);
        if (reloadItem != null)
            defs.resurrector.ammoCountPerCharge = Mathf.Max(1, reloadCost.IntValue);
        defs.extension.maxCorpseAgeTicks = NoCorpseLimit(corpseAge.Value)
            ? 0
            : Mathf.Max(1, Mathf.RoundToInt(corpseAge.Value * GenDate.TicksPerHour));
        defs.extension.requireMechlink = requireMechlink.Value;
        defs.sparks.onlyWhileDrafted = sparksOnlyDrafted.Value;
        if (sparkSprayer != null)
            sparkSprayer.chancePerTick = Mathf.Max(0f, sparkFrequency.Value) / 100f * apocritonSparkChance;
        defs.ability.castTime = Mathf.RoundToInt(castTime.Value * GenTicks.TicksPerRealSecond);
        defs.ability.cooldownTime = Mathf.RoundToInt(cooldown.Value * GenDate.TicksPerHour);
        Generation++;
        TrimWornResurrectors();
    }

    // A lowered capacity leaves worn resurrectors above it; every live pawn, since a caravan or
    // transporter pawn spawns with no hook of its own.
    private static void TrimWornResurrectors()
    {
        if (Current.ProgramState != ProgramState.Playing)
            return;
        foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive)
        {
            if (pawn.apparel == null)
                continue;
            foreach (Apparel apparel in pawn.apparel.WornApparel)
                apparel.TryGetComp<CompMechResurrector>()?.Trim();
        }
    }

    // Each section opens on its summary under its header, then one row per setting, and a reset
    // button sits below the list. The section is headed by whitespace and a medium-font header
    // rather than a rule, which reads as another slider, and the rows sit indented under it; it
    // closes on the same gap so the list does not end flush on its last row. The list scrolls
    // once it outgrows the window's height, sized from the previous frame's listing height; the
    // scrollbar only takes its width once it is needed. maxOneColumn keeps the listing growing
    // downward. While the defs were not found there is nothing to tune, so the page says so
    // instead of drawing rows around the -1 the defaults never got.
    public void DoWindowContents(Rect inRect)
    {
        if (!defaultsLoaded)
        {
            Widgets.Label(inRect, "OLWC_SettingsUnavailable".Translate());
            return;
        }

        const float buttonHeight = 30f;
        const float buttonWidth = 200f;
        const float scrollbarWidth = 16f;
        var outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - buttonHeight - Listing.DefaultGap);
        float viewWidth = contentHeight > outRect.height ? outRect.width - scrollbarWidth : outRect.width;
        var viewRect = new Rect(0f, 0f, viewWidth, Mathf.Max(contentHeight, outRect.height));
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        var listing = new Listing_Standard { maxOneColumn = true };
        listing.Begin(viewRect);
        SectionHeader(listing, "OLWC_SectionResurrect", abilityDef.icon, abilityTip, first: true);
        SummaryRow(listing, ResurrectSummary);
        charges.DoRow(listing);
        recharge.DoRow(listing);
        if (reloadItem != null)
            reloadCost.DoRow(listing);
        corpseAge.DoRow(listing);
        castTime.DoRow(listing);
        cooldown.DoRow(listing);
        if (sparkSprayer != null)
            sparkFrequency.DoRow(listing);
        requireMechlink.DoRow(listing);
        sparksOnlyDrafted.DoRow(listing);
        if (commandRange.Available)
        {
            SectionHeader(listing, "OLWC_SectionCommandRange", commandRange.Icon,
                HeaderTip("OLWC_SectionCommandRange".Translate(), "OLWC_SectionCommandRangeDesc".Translate(commandRange.NormalRadius.ToString("0.#"))));
            SummaryRow(listing, commandRange.Summary);
            commandRange.DoRows(listing);
        }
        if (WorkProfile.Toggleable.Count > 0)
        {
            SectionHeader(listing, "OLWC_SectionWork", workIcon, HeaderTip("OLWC_SectionWork".Translate(), "OLWC_SectionWorkDesc".Translate()));
            SummaryRow(listing, WorkSummary);
            work.DoRows(listing);
        }
        if (cerebrexNode != null)
        {
            SectionHeader(listing, "OLWC_SectionCerebrex", CerebrexNode.Icon, cerebrexTip);
            SummaryRow(listing, CerebrexSummary);
            cerebrexArt.DoRow(listing);
        }
        listing.Gap(SectionGap);
        contentHeight = listing.CurHeight;
        listing.End();
        Widgets.EndScrollView();

        if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - buttonHeight, buttonWidth, buttonHeight),
                "RestoreToDefaultSettings".Translate()))
        {
            requireMechlink.Reset();
            sparksOnlyDrafted.Reset();
            cerebrexArt.Reset();
            work.Reset();
            commandRange.Reset();
            foreach (Tunable tunable in All)
                tunable.Reset();
        }
    }

    // What the section's rows amount to: the resurrector's capacity and rate (a rate of 0 ends the
    // clause on never) with what a charge reloads for, the costs, the corpse window (its own
    // clause, or that there is none) and the cast's timings, in the formats their rows show; a
    // cooldown of 0 ends the line on its own clause.
    private string ResurrectSummary()
    {
        string rate = recharge.Value > 0f
            ? "OLWC_SummaryRegen".Translate(Decimal(recharge.Value)).Resolve()
            : "OLWC_SummaryNoRegen".Translate().Resolve();
        if (reloadItem != null)
            rate = "OLWC_SummaryReload".Translate(rate, Gizmo_MechResurrector.CostLabel(reloadItem, reloadCost.IntValue)).Resolve();
        string window = NoCorpseLimit(corpseAge.Value)
            ? "OLWC_SummaryNoWindow".Translate().Resolve()
            : "OLWC_SummaryWindow".Translate(Period(corpseAge.Value)).Resolve();
        string casting = RealSeconds(castTime.Value);
        if (cooldown.Value <= 0f)
            return "OLWC_ResurrectSummaryNoCooldown".Translate(charges.IntValue, rate, costs, window, casting);
        return "OLWC_ResurrectSummary".Translate(charges.IntValue, rate, costs, window, casting, Period(cooldown.Value));
    }

    // What the work section's boxes amount to: the work the pilot may take on, named as the
    // work tab names it, or that there is none; the rest is VFEP's rule either way.
    private string WorkSummary()
    {
        List<string> open = work.Open.Select(WorkSetting.Name).ToList();
        if (open.Count == 0)
            return "OLWC_WorkSummaryNone".Translate();
        return "OLWC_WorkSummary".Translate(open.ToCommaList(useAnd: true));
    }

    // Whose art the node wears this session, off the def, then that the box's choice waits for a
    // restart while it differs.
    private string CerebrexSummary()
    {
        string live = cerebrexLive ? "OLWC_CerebrexSummaryOurs".Translate() : "OLWC_CerebrexSummaryVanilla".Translate();
        if (cerebrexArt.Value == cerebrexLive)
            return live;
        return live + " " + "OLWC_CerebrexSummaryRestart".Translate();
    }

    // A muted line in a section, indented and padded as its rows are, as tall as its text, with
    // a row's gap below it.
    private static void SummaryRow(Listing_Standard listing, Func<string> summary)
    {
        string text = summary();
        float width = listing.ColumnWidth - Tunable.RowIndent - Tunable.RowEndPad;
        Rect rect = listing.GetRect(Text.CalcHeight(text, width));
        rect.xMin += Tunable.RowIndent;
        rect.xMax -= Tunable.RowEndPad;
        GUI.color = ColoredText.SubtleGrayColor;
        Widgets.Label(rect, text);
        GUI.color = Color.white;
        listing.Gap(Tunable.RowGap);
    }

    // The header's text in the medium font after its icon, a square as tall as the header; a
    // header whose texture is missing starts its text at the left edge rather than after an
    // empty slot. The tooltip covers the icon and the text only, not the empty width after them.
    private static void SectionHeader(Listing_Standard listing, string key, Texture2D icon, string tooltip, bool first = false)
    {
        const float headerGap = 4f;
        const float headerHeight = 32f;
        const float iconPad = 8f;
        if (!first)
            listing.Gap(SectionGap);
        Rect rect = listing.GetRect(headerHeight);
        float left = rect.xMin;
        if (icon != null && icon != BaseContent.BadTex)
        {
            GUI.DrawTexture(rect.LeftPartPixels(headerHeight), icon);
            rect.xMin += headerHeight + iconPad;
        }
        GameFont font = Text.Font;
        TextAnchor anchor = Text.Anchor;
        Text.Font = GameFont.Medium;
        Text.Anchor = TextAnchor.MiddleLeft;
        string label = key.Translate();
        Widgets.Label(rect, label);
        if (!tooltip.NullOrEmpty())
            TooltipHandler.TipRegion(new Rect(left, rect.y, rect.xMin - left + Text.CalcSize(label).x, rect.height), tooltip);
        Text.Anchor = anchor;
        Text.Font = font;
        listing.Gap(headerGap);
    }

    // An ability's tooltip as VEF opens one: its name in vanilla's tip title colour over its
    // description. VEF's stat lines are left out; the rows above the summary quote those numbers.
    private static string HeaderTip(string label, string description) =>
        description.NullOrEmpty() ? null : label.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + description;

    private struct Defs
    {
        public AbilityDef ability;
        public ResurrectMechExtension extension;
        public CompProperties_MechResurrector resurrector;
        public CompProperties_ResurrectorSparks sparks;
    }

    // All or nothing: with any target missing (another mod patched it away) the settings leave
    // every def at its XML values and say so once, as a warning, since the set itself still
    // works.
    private static bool warned;

    private static bool TryGetDefs(out Defs defs)
    {
        AbilityDef ability = DefDatabase<AbilityDef>.GetNamedSilentFail(AbilityDefName);
        ThingDef armor = DefDatabase<ThingDef>.GetNamedSilentFail(ArmorDefName);
        defs = new Defs
        {
            ability = ability,
            extension = ability?.GetModExtension<ResurrectMechExtension>(),
            resurrector = armor?.GetCompProperties<CompProperties_MechResurrector>(),
            sparks = armor?.GetCompProperties<CompProperties_ResurrectorSparks>(),
        };
        if (defs.ability != null && defs.extension != null && defs.resurrector != null && defs.sparks != null)
            return true;
        if (!warned)
            Log.Warning("[Overlord Warcasket] Settings could not find the resurrect mech ability, its extension, or the armor's mech resurrector or resurrector sparks; the set keeps its XML values.");
        warned = true;
        return false;
    }
}
