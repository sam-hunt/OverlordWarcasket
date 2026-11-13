using RimWorld;
using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// The carried reload's command: loads the mech resurrector from the ammo in the pilot's own
// inventory, which vanilla's reload never looks in (ReloadableUtility.FindEnoughAmmo searches
// the map). CompMechResurrector yields it only while the pilot carries some, so it appears with
// the ammo and goes with it.
//
// The icon is the ammo's own, set as Designator_Build sets a buildable's (uiIcon with its angle,
// offset and colour), over a background in the resurrector gauge's red rather than vanilla's grey,
// so the button reads as the gauge's. The count in the corner is the ammo carried.
//
// The comp keeps one instance and refreshes it on every gizmo request, each frame the pilot is
// selected, so Refresh compares before it builds: the icon and the texts are rebuilt only when
// the reload item or its cost changes (the def's to change, and a play-data reload replaces the
// def and its icon together), the disabled reason only when the state or the pilot does, and
// the count's text only with the count.
//
// The background is a solid colour made once, as Gizmo_Slider makes its bar textures; it belongs
// to no mod content, so a play-data reload leaves it alone.
[StaticConstructorOnStartup]
public class Command_LoadResurrector : Command_Action
{
    private static readonly Texture2D Background =
        SolidColorMaterials.NewSolidColorTexture(Gizmo_MechResurrector.ResurrectorShade);

    private readonly CompMechResurrector resurrector;

    private Pawn pilot;
    private ThingDef ammo;
    private int perCharge = -1;
    private int carried = -1;
    private string carriedLabel;
    private State state = State.Unset;

    private enum State
    {
        Unset,
        Ready,
        Full,
        Short,
    }

    public Command_LoadResurrector(CompMechResurrector resurrector)
    {
        this.resurrector = resurrector;
        action = () => this.resurrector.OrderReloadFromInventory(pilot);
    }

    public override Texture2D BGTexture => Background;

    public override Texture2D BGTextureShrunk => Background;

    public override string TopRightLabel => carriedLabel;

    public void Refresh(Pawn wearer, int carriedAmmo)
    {
        bool restate = wearer != pilot;
        pilot = wearer;
        if (resurrector.AmmoDef != ammo || resurrector.Props.ammoCountPerCharge != perCharge)
        {
            ammo = resurrector.AmmoDef;
            perCharge = resurrector.Props.ammoCountPerCharge;
            string plural = Find.ActiveLanguageWorker.Pluralize(ammo.label);
            icon = ammo.uiIcon;
            iconAngle = ammo.uiIconAngle;
            iconOffset = ammo.uiIconOffset;
            defaultIconColor = ammo.uiIconColor;
            defaultLabel = "OLWC_LoadCarried".Translate(plural);
            defaultDesc = "OLWC_LoadCarriedDesc".Translate(plural, CostLabel(ammo, perCharge));
            restate = true;
        }
        if (carriedAmmo != carried)
        {
            carried = carriedAmmo;
            carriedLabel = carriedAmmo.ToString();
        }
        State now = resurrector.Full ? State.Full : carriedAmmo < perCharge ? State.Short : State.Ready;
        if (now == state && !restate)
            return;
        state = now;
        Disabled = now != State.Ready;
        disabledReason = now switch
        {
            State.Full => "OLWC_ResurrectorFull".Translate(),
            State.Short => "OLWC_NotEnoughCarried".Translate(wearer.LabelShortCap, CostLabel(ammo, perCharge)),
            _ => null,
        };
    }

    // "1 advanced component", "2 advanced components": what a charge costs to reload.
    internal static string CostLabel(ThingDef ammo, int count) =>
        count + " " + Find.ActiveLanguageWorker.Pluralize(ammo.label, count);
}
