using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OverlordWarcasket;

// The resurrect mech ability's cast range: its rule, the settings slider's stops and the ring.
//
// The rule. The range is the ability def's distanceToTarget, in cells: the click is unlimited
// and the cast job's walk (JobDriver_CastResurrectMech) stops once the pilot is within the range
// of the corpse with line of sight. 0 or less is no limit, as the def's corpse window has it,
// and then the pilot casts from where they stand, wherever on the map the corpse lies and
// whether or not they can see it. Sight goes with the limit: a range that still asked for it
// would walk the pilot toward a corpse behind a wall as any finite range does, so "no limit"
// would promise nothing a player could predict. What limits the ability is the resurrector's
// charges (the ability def's header), not where the pilot stands.
//
// The slider. A stop is a whole number of cells standing for the range a tenth short of it, as
// vanilla writes its radii (10 is the shipped 9.9), because Tunable's banded stops are multiples
// of their step and a range ending in .9 is not. A cell a stop from 2 (1.9: touch, diagonal
// neighbours included, where the walk's path may end) to 30, five cells a stop to 60, ten to
// 120, then one more stop for no limit, written to the def as 0. 119.9 all but spans a 250
// cell map from its centre, and the band's next stop, 129.9, is the first whose ring 1.6 could
// not draw (MaxRingEdges); past it the stop that matters is no limit. A def range above the
// slider reads as its top stop, not as no limit.
//
// The ring. Drawn around the pilot while the ability's gizmo is hovered and while its targeter
// is open (Ability_ResurrectMech.GizmoUpdateOnMouseover and DrawHighlight, both run once a
// frame from the map's update: MapGizmoUtility.MapUIUpdate and Targeter.TargeterUpdate). VEF's
// own rings in those two methods are at GetRangeForPawn, which this ability answers as
// unlimited, so they never draw. The ring marks where a corpse must lie for the pilot to cast
// without walking, sight aside: it is not carved by line of sight, as the apocriton's own ring
// is not (vanilla carves only where a verb sets drawHighlightWithLineOfSight, four short-range
// verbs), since a click outside it or behind a wall is still accepted and only costs a walk,
// and a carved ring would be stale after the pilot's first step. With no limit there is
// nothing to mark and nothing is drawn.
//
// GenDraw.DrawRadiusRing would list every cell of the disc on every call, 45,000 of them at
// 119.9, and refuses a radius above GenRadial's table. Rim lists only the disc's cells with a
// cardinal neighbour outside it, about eight per cell of range, and hands GenDraw.DrawFieldEdges
// the interior cells beside them as its ignoreBorderCells, which drops the edges the rim would
// otherwise draw on its inner side. Membership is the walk's own comparison, IntVec3's distance
// at or below the range, so the ring cannot disagree with the walk and needs no radial table
// (DrawRadiusRing's count includes a cell at exactly the radius too). The cells are rebuilt
// only when the pilot's cell or the range changes. DrawFieldEdges reads Find.CurrentMap, skips
// cells off it and draws no edge that faces off it, so the caller draws for the map on screen
// alone and the rim is not clipped here.
//
// DrawFieldEdges sends all its edges in one Graphics.DrawMeshInstanced call, which 1.6's Unity
// refuses with an ArgumentOutOfRangeException above its instance limit (documented as 1023;
// the 1.7 engine has no such check). A disc's outline has four edges for each of its rows,
// which RingEdges counts, and a range whose ring could exceed the limit is not drawn. Verified
// on 1.7.5013 and the frozen 1.6.4871 assembly, whose GenDraw and GenRadial are identical.
public static class CastRange
{
    public const float SliderMin = 2f;
    public const float SliderMax = 120f;
    public const float SliderNoLimit = SliderMax + 10f;

    // Unity's instance limit for one instanced draw, which bounds the ring's edges.
    public const int MaxRingEdges = 1023;

    // A stop's whole cells less this is the range it stands for.
    private const float StopOffset = 0.1f;

    public static readonly (float upTo, float step)[] SliderBands = [(30f, 1f), (60f, 5f), (SliderNoLimit, 10f)];

    private static readonly List<IntVec3> ringRim = [];
    private static readonly HashSet<IntVec3> ringInside = [];
    private static IntVec3 ringCenter = IntVec3.Invalid;
    private static float ringRange;

    public static bool Limited(float range) => range > 0f;

    // Whether a pilot this far from the corpse is within the range. Pure, for the tests.
    public static bool Within(float range, float distance) => !Limited(range) || distance <= range;

    public static bool IsNoLimitStop(float stop) => stop > SliderMax;

    // The range a slider stop writes to the def, to the hundredth so a def range read through
    // ToStop comes back as it was. Pure, for the tests.
    public static float FromStop(float stop) =>
        IsNoLimitStop(stop) ? 0f : Mathf.Round((stop - StopOffset) * 100f) / 100f;

    // The slider value a def range reads as. Pure, for the tests.
    public static float ToStop(float range) =>
        Limited(range) ? Mathf.Min(range + StopOffset, SliderMax) : SliderNoLimit;

    // The edges of the ring at a range, clear of any map edge: four for each row of the disc.
    // Pure, for the tests.
    public static int RingEdges(float range) => 4 * (2 * Mathf.FloorToInt(range) + 1);

    // The ring's cells around center: into rim the disc's cells with a cardinal neighbour
    // outside it, into inside the disc's cells beside those. Pure, for the tests.
    public static void Rim(IntVec3 center, float range, List<IntVec3> rim, HashSet<IntVec3> inside)
    {
        rim.Clear();
        inside.Clear();
        int top = Mathf.FloorToInt(range);
        for (int z = -top; z <= top; z++)
        {
            // The row is x from -width to width, and the shorter of its two neighbouring rows,
            // the one further out, covers it as far as covered.
            int width = HalfWidth(z, range);
            int covered = HalfWidth(Math.Abs(z) + 1, range);
            for (int x = Math.Min(covered + 1, width); x <= width; x++)
            {
                AddRimCell(center, x, z, range, rim, inside);
                if (x != 0)
                    AddRimCell(center, -x, z, range, rim, inside);
            }
        }
    }

    // Draws the ring around center on the map on screen, in VEF's range ring color for the
    // ability. Nothing for a range with no limit or one whose ring the game could not draw.
    public static void DrawRing(IntVec3 center, float range, Color color)
    {
        if (!Limited(range) || RingEdges(range) > MaxRingEdges)
            return;
        if (center != ringCenter || range != ringRange)
        {
            Rim(center, range, ringRim, ringInside);
            ringCenter = center;
            ringRange = range;
        }
        GenDraw.DrawFieldEdges(ringRim, color, ignoreBorderCells: ringInside);
    }

    // The walk's comparison (IntVec3.DistanceTo is the offset's LengthHorizontal), for a cell
    // this far off the pilot's.
    private static bool InDisc(int x, int z, float range) => new IntVec3(x, 0, z).LengthHorizontal <= range;

    // The furthest x in row z of the disc, or -1 for a row outside it: the square root to
    // start from, then the walk's comparison to settle the last cell.
    private static int HalfWidth(int z, float range)
    {
        if (!InDisc(0, z, range))
            return -1;
        int width = (int)Math.Sqrt((double)range * range - (double)z * z);
        while (width > 0 && !InDisc(width, z, range))
            width--;
        while (InDisc(width + 1, z, range))
            width++;
        return width;
    }

    private static void AddRimCell(IntVec3 center, int x, int z, float range, List<IntVec3> rim, HashSet<IntVec3> inside)
    {
        rim.Add(new IntVec3(center.x + x, 0, center.z + z));
        for (int d = 0; d < 4; d++)
        {
            IntVec3 side = GenAdj.CardinalDirections[d];
            if (InDisc(x + side.x, z + side.z, range))
                inside.Add(new IntVec3(center.x + x + side.x, 0, center.z + z + side.z));
        }
    }
}
