using System;
using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// The armor's mech stun pulse as the settings window offers it: one radio group, what sets the
// pulse off (StunPulseTrigger), and a slider for how far it reaches. Until the player picks,
// each is the def's, the armor's CompProperties_MechStunPulse, read by LoadDefaults on every
// fresh load before Apply writes anything, so the def and its header stay the record and
// another mod's XML patch moves the defaults too. Only an override is saved, the trigger by
// name (so a name this version does not know reads as the default) and the radius as a Tunable
// saves one; both are written back onto the props by Apply through the settings' ApplyToDefs,
// where the comp reads them at the moment a pilot falls.
//
// The radius runs from a cell past touch to the apocriton's own, in whole cells ending in .9 as
// vanilla's radii do, so the shipped 9.9 and the apocriton's 30.9 are both stops. The length of
// the stun is the damage def's and has no row (CompMechStunPulse's header); the summary quotes it.
//
// The group is laid out as the command range's are (CommandRangeSetting, whose label row it
// borrows): a label row, then one radio row per trigger tabbed in under it, the default one
// carrying the "(default)" suffix.
public sealed class StunPulseSetting
{
    private const string ArmorDefName = "OLWC_Warcasket_Overlord";
    private const string Key = "StunPulse";

    // The apocriton's explosiveRadius (Biotech's Races_Mechanoids_SuperHeavy.xml, 1.7.5013).
    public const float MaxRadius = 30.9f;
    public const float MinRadius = 1.9f;

    private const float OptionIndent = 20f;
    private const float GroupGap = 8f;

    private static readonly StunPulseTrigger[] Options = [StunPulseTrigger.Death, StunPulseTrigger.Downed];

    public readonly Tunable radius = new(Key + "Radius", MinRadius, MaxRadius, 1f, cells => cells.ToString("0.#"));

    private string triggerName;
    private CompProperties_MechStunPulse props;

    // False while the armor or its pulse is missing (another mod's patch); the section is skipped.
    public bool Available => props != null;

    public StunPulseTrigger DefaultTrigger { get; private set; }

    public StunPulseTrigger Trigger =>
        triggerName != null && Enum.TryParse(triggerName, out StunPulseTrigger value) ? value : DefaultTrigger;

    // How long the pulse stuns a mech, in ticks, as StunHandler.Notify_DamageApplied works it
    // out: the def's constant length, or thirty ticks a point of its damage.
    public int StunTicks { get; private set; }

    public void Reset()
    {
        triggerName = null;
        radius.Reset();
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref triggerName, Key + "Trigger");
        radius.ExposeData();
    }

    // Off defs fresh from XML: the settings' LoadDefaultsFromDefs, which Startup runs before the
    // first Apply of every load.
    public void LoadDefaults()
    {
        props = DefDatabase<ThingDef>.GetNamedSilentFail(ArmorDefName)?.GetCompProperties<CompProperties_MechStunPulse>();
        if (props == null)
            return;
        DefaultTrigger = props.trigger;
        radius.DefaultValue = props.radius;
        DamageDef damage = props.damageDef;
        StunTicks = damage == null ? 0 : damage.constantStunDurationTicks ?? Mathf.RoundToInt(damage.defaultDamage * 30f);
    }

    public void Apply()
    {
        if (props == null)
            return;
        props.trigger = Trigger;
        props.radius = radius.Value;
    }

    public void DoRows(Listing_Standard listing)
    {
        CommandRangeSetting.GroupLabel(listing, "OLWC_StunPulseTrigger".Translate(), "OLWC_StunPulseTriggerDesc".Translate(), enabled: true);
        foreach (StunPulseTrigger option in Options)
        {
            string label = ("OLWC_StunPulseTrigger_" + option).Translate();
            if (option == DefaultTrigger)
                label += "OLWC_DefaultSuffix".Translate();
            string tooltip = ("OLWC_StunPulseTrigger_" + option + "Desc").Translate();
            if (listing.RadioButton(label, Trigger == option, Tunable.RowIndent + OptionIndent, Tunable.RowEndPad, tooltip, null, false))
                triggerName = option == DefaultTrigger ? null : option.ToString();
        }
        listing.Gap(GroupGap);
        radius.DoRow(listing);
    }
}
