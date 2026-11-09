#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Xunit;

namespace OverlordWarcasket.Tests;

// The gated compat roots under 1.6/Mods/<Mod>/Patches/ only ever run with their mod active, and
// 1.6/Patches/SOS2Patch.xml sits behind a PatchOperationFindMod gate of its own, so a renamed node
// in a def would break any of them silently for every player who has that mod and nobody else.
// Every operation's xpath must resolve in the defs it patches, merged as the game merges them, to
// one node per def it names: the main tree's for the roots that patch our own defs (the SOS2
// patch names its two defs in one xpath, as VFEP's does), joined by the DLC's own defs from the
// local install for a root that patches a DLC def the main tree never carries (DlcRoots). A
// PatchOperationConditional's own xpath is the exception, a probe for a node that should be
// absent.
public class CompatPatchTests
{
    private static readonly string RepoRoot = FindRepoRoot();

    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "OverlordWarcasket.sln")))
                return dir.FullName;
        }
        throw new InvalidOperationException("OverlordWarcasket.sln not found above the test output folder");
    }

    // A gated root whose patches target a DLC's defs, with the DLC's folder under the install's
    // Data/. The install is RIMWORLD_PATH or a stock Steam path, as the csproj resolves it;
    // without one the root's files fail here and say so.
    private static readonly (string root, string dlc)[] DlcRoots = [("1.6/Mods/Odyssey/", "Odyssey")];

    private static string? RimWorldPath()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string?[] candidates =
        [
            Environment.GetEnvironmentVariable("RIMWORLD_PATH"),
            @"C:\Program Files (x86)\Steam\steamapps\common\RimWorld",
            "/mnt/c/Program Files (x86)/Steam/steamapps/common/RimWorld",
            Path.Combine(home, ".local/share/Steam/steamapps/common/RimWorld"),
            Path.Combine(home, "Library/Application Support/Steam/steamapps/common/RimWorld"),
        ];
        return candidates.FirstOrDefault(c => !string.IsNullOrEmpty(c) && Directory.Exists(Path.Combine(c, "Data")));
    }

    private static XDocument MergedDefs(string relativePath)
    {
        var folders = new List<string> { Path.Combine(RepoRoot, "1.6/Defs") };
        foreach (var (root, dlc) in DlcRoots)
        {
            if (!relativePath.Replace('\\', '/').StartsWith(root, StringComparison.Ordinal))
                continue;
            var install = RimWorldPath();
            Assert.True(install != null, $"{relativePath} patches {dlc}'s defs, but no RimWorld install was found: set RIMWORLD_PATH");
            folders.Add(Path.Combine(install!, "Data", dlc, "Defs"));
        }
        var defs = new XElement("Defs");
        foreach (var folder in folders)
        {
            foreach (var file in Directory.EnumerateFiles(folder, "*.xml", SearchOption.AllDirectories))
                defs.Add(XDocument.Load(file).Root!.Elements());
        }
        return new XDocument(defs);
    }

    public static TheoryData<string> PatchFiles()
    {
        var data = new TheoryData<string>();
        foreach (var file in Directory.EnumerateFiles(Path.Combine(RepoRoot, "1.6"), "*.xml", SearchOption.AllDirectories)
                     .Where(f => f.Replace('\\', '/').Contains("/Patches/"))
                     .OrderBy(f => f, StringComparer.Ordinal))
            data.Add(file.Substring(RepoRoot.Length + 1));
        return data;
    }

    [Theory]
    [MemberData(nameof(PatchFiles))]
    public void EveryXPath_ResolvesToOneNodePerNamedDef(string relativePath)
    {
        var defs = MergedDefs(relativePath);
        var patch = XDocument.Load(Path.Combine(RepoRoot, relativePath));
        var operations = patch.Descendants().Where(e => e.Attribute("Class") != null && e.Element("xpath") != null);
        Assert.NotEmpty(operations);
        foreach (var op in operations)
        {
            var xpath = op.Element("xpath")!.Value;
            var hits = defs.XPathSelectElements(xpath).Count();
            var expected = (string)op.Attribute("Class") == "PatchOperationConditional"
                ? 0
                : Regex.Matches(xpath, "defName\\s*=\\s*\"").Count;
            Assert.True(hits == expected, $"{relativePath}: {xpath} resolved {hits} node(s), expected {expected}");
        }
    }
}
