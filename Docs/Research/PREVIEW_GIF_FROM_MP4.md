# Animated Workshop preview from an artist's MP4

Snapshot of the family guide `../PREVIEW_GIF_FROM_MP4.md` (the primary for the next commission
in any mod; update it first and recopy) kept here so this repo's tooling docs stand alone.
`Preview-GIF-Transcode.md` beside it is the record of this repo's own run.

Four reference implementations, by kind of delivery (see "Which method"). All four fit under
the limit the same way, with the band hold (see the cost model): it is the family's method, and
the per-pixel gate the first three first shipped with is retired.

- **Composite onto the PNG:** OverlordWarcasket `Scripts/preview-gif.py`, 2026-10-31. A 2.4 MB
  1920x1080 H.264 delivery became `.steamworkshop/Preview.gif` at 640x360, all 60 frames at
  12 fps, BAND 9, 1,029,257 bytes (98.2% of Steam's 1 MiB limit), with the static art
  pixel-identical to `About/Preview.png`.
- **The video's own framing:** ToxinWarcasket `Scripts/preview-gif.py`, 2026-11-01. A 2.9 MB
  1920x1080 delivery became a 640x360 GIF, all 72 frames at 12 fps, BAND 10, 1,017,790 bytes
  (97.1%). It replaced the artist's own hand-compressed GIF (ezgif: 544x306, resampled to
  15 fps, 32-64 colour local palettes), and the user preferred ours. When an artist offers a
  GIF, ask for the raw MP4 too.
- **The video's framing, on a scaled PNG with a moving background:** UniqueMeleeWeapons
  `Scripts/preview-gif.py`, 2026-11-02 (Toxin's script, adapted). A 3.1 MB delivery aligned at
  exactly 3x, but the hero held still and the background (rotating rays) moved behind it, so a
  composite would have seamed on every edge. All 60 frames at 640x360, BAND 3, 974,804 bytes
  (93.0%; BAND 2 does not fit). The colour-matrix check (step 0 below) came from this run.
- **Composite onto the PNG, layers nudged:** HemophageWarcasket `Scripts/preview-gif.py`,
  2026-11-18 (Overlord's script, reworked). A 9.5 MB delivery at a true 12 fps (no padding),
  every layer at 3.0x but nudged 1 to 2 PNG px, tones already matching. All 60 frames at
  640x360, BAND 11, 1,033,780 bytes (98.6%). The band hold came from this run, and this is
  the script to start a new composite from: it carries the colour-matrix step and asserts
  what it assumes about the delivery.

Each repo's `Docs/Research/Preview-GIF-Transcode.md` is the record of its run: the delivery's
measurements, the band sweep and what was tried and left out.

Each script's header is the record of its method and tuning sweep; this guide is what carries
over to the next commission. Budget a session for it: the encode is seconds, the tuning is a
judgement call that needs the user's eye on moving files.

## What the artist delivers

The brief is "animate our About/Preview.png". Both deliveries so far were 1920x1080 H.264, the
hero rigged and animated on the PNG's art at 3x, rendered at 12 fps padded to 29.97/30, with an
audio track and a badge reading whatever version the PNG read at the time. Beyond that they
differed, and the difference decides the method:

- **Overlord: the PNG blown up 3x, only the hero moving.** Everything but the hero is the PNG
  upscaled, blurred by the scale and the codec, tonally shifted (darks crushed, highlights
  compressed) and noisy frame to frame. Downscaling it back loses quality against the PNG we
  already ship. So the GIF is the PNG, with the video's hero composited into the region the
  animation touches.
- **Toxin: a re-layout.** Only the hero lined up with the PNG (exactly 3x, offset +5, -6 video
  px). The artist had rescaled and moved the title, hex icon, badge and background, and the
  ground fog animated along with the hero. A composite would have seamed wherever the two
  layouts meet inside the mask. So the GIF keeps the video's own framing. The tones matched the
  PNG (the fitted curve was within 3/255 of identity), so the crushed darks were Overlord's
  encode, not a property of every delivery.

Common to both:

- The loop closes cleanly (last frame -> first within noise), but check: neither loop was
  periodic at any shorter lag, so every unique frame had to be kept.
- The badge never needs re-exporting to change a version digit: `gif-patch` bumps it in place.
  The PNG's badge (Overlord) and the video's own badge (Toxin, rms 0.035 fit) both worked, as
  long as everything outside the motion mask is pinned to frame 0.
- Do not track the MP4. Keep it with the gameplay recordings; the workshop README says so.

## Which method: test the delivery first

Measure before writing anything, and settle the decode first: every later comparison is
against the extracted frames.

0. **Colour matrix: verify it on every delivery, never assume it.** An H.264 stream's YUV
   only becomes RGB through a matrix, and the stream may or may not say which. `ffmpeg -i`
   prints the tags on the video line (`yuv420p(tv, bt709, progressive)` when tagged, bare
   `yuv420p(progressive)` when not). ffmpeg decodes an untagged stream as BT.601, while
   browsers and players assume BT.709 for HD, so a wrong guess produces a GIF that looks
   duller than the MP4 playing beside it, in exactly the saturated warm hues (golds, reds).
   The check: extract frame 0 twice, `-vf scale=in_color_matrix=bt601:in_range=tv:out_range=pc,format=rgb24`
   and the same with `bt709`, downscale both to 640x360 and compare them against the PNG over
   the most saturated region that is static and aligned (mean RGB and HSV saturation of a
   box; a 709-encoded, 601-decoded round trip of the PNG's box predicts the wrong-matrix
   numbers if you need a tiebreak). Use whichever matches the PNG, write it into the script's
   extraction `-vf`, and record the measurement in the header. If neither matches, the
   artist's render changed tones, which is a separate problem (Overlord's tone curve). A
   palette with no saturated colours (Overlord's khaki, Toxin's olive) barely moves between
   the two matrices, so take the reading from the warmest region the art has. UMW, 2026-11-02:
   untagged; hero saturation PNG 113.1, BT.601 109.5, BT.709 113.7, and the BT.601 GIF was
   visibly duller in the browser.

Frame 0 against the PNG, both at 640x360, is then the first signal: Toxin's raw max-channel
mismatch was 29/255, far beyond what a tone shift explains. Then:

1. **Alignment per layer.** Compare edge maps (Gaussian blur 1, `FIND_EDGES`, blur 1.5) in PNG
   space: shrink the video frame by 1/s, paste it at an offset, take the mean difference over a
   box. Run that separately on the hero, the title, a corner icon and a background patch.
   Search coarse to fine: scale in 0.05 steps, then sub-pixel offsets via `Image.transform`
   AFFINE at full resolution. If every layer finds the same scale and offset, it is a scaled
   PNG: composite. If they disagree (Toxin: hero 3.0, title about 3.1, hex 3.45), it is a
   re-layout: keep the video's framing.
   Layers that disagree by only 1 to 2 PNG px are still a scaled PNG if they never move and
   the motion never reaches them (Hemophage): protect their boxes from the mask and assert no
   motion inside them, as that script's `PROTECT` does. A tone curve that does not improve the
   static mismatch can then mean nudged layers, not a failed composite; check tones on a
   blurred static patch instead.
2. **Motion map.** Max change against frame 0 over every frame. Motion outside the hero (fog,
   particles, a drifting background) pushes towards the video's framing even on a scaled PNG,
   because those regions would have to come from the video too.

## Running it on another mod

Copy the reference script whose method the test picked and add the `Scripts/CLAUDE.md` bullet
(or the repo's equivalent); the repo rules want the header kept as the record when numbers
change. The video-framing script has no PNG constants. It takes frames by time slot rather than
deduplicating, and fills the static region with the mean of all frames (`ImageMath` in `F` mode
per channel), which removes the codec's frame-to-frame noise the composite would otherwise have
borrowed the PNG to avoid. For the composite script, check the constants that encode this
delivery's geometry before trusting the output:

- `SIZE` (640x360) must equal the PNG's; the palette is learned at that size (see pitfalls).
- `MASK_LEFT_EDGE` shuts codec noise on the title out of the motion mask; set it to where the
  hero's motion actually starts (the log prints the mask's bbox and coverage, 44% on Overlord).
- `fit_lut` excludes the top-left 80x80 badge corner from the tone fit; move it if the badge
  moves. The log prints the static-region mismatch before and after the fit (13 -> 6 /255 on
  Overlord); if "after" is not a clear improvement, the delivery is not a scaled PNG and the
  composite approach needs rethinking.
- The three-frame palette sample (0, n/3, 2n/3) assumes the colours do not drift across the
  loop; a glow that only appears late in the loop needs a frame from that stretch in the sample.

With either script, the only real tuning is `BAND`: ship the lowest that fits, with a few KB
of headroom. A gif-patch bump on a lossless-LZW file shrinks it slightly (-64 bytes on
Overlord, -201 on Toxin, -255 on UMW); on a file that went through `gifsicle --lossy` it grows
about 4 KB, because frame 0 is rewritten clean.

## The cost model (640x360, 44% hero, 60 frames)

One clean refresh of the hero costs about 35 KB in LZW however it is encoded, so a clean
12 fps encode is two to three times the limit and nothing conventional closes the gap on its
own. Later frames can repaint only some of the moving pixels, so every pixel shows a slightly
stale colour for some frames. The method is the rule for when a pixel repaints and to what.

### The band hold (the method)

Each pixel holds one colour for as long as the true colour stays within BAND of it, then moves
on, so every frame shows every pixel within BAND of the truth. The runs are found by looking
ahead over the whole loop and are the longest possible: a run ends when the truth's spread over
it would exceed 2 x BAND, and it shows its mean, clamped to stay within BAND of every frame in
it. `hold` is the same code in all four scripts; the encode after it only remaps to the
palette, and BAND is the one tuning number.

Error is the decoded GIF against the clean composite frames inside the mask, rms of the max
channel /255. The palette floor is the same error for an unlimited-size encode, which is what
256 undithered colours cost alone.

| mod | frames, mask | clean encode | shipped | error | palette floor | the gate it replaced |
|---|---|---|---|---|---|---|
| Overlord | 60, 44% | 2.45 MiB | BAND 9, 1,029,257 bytes | 6.1 | 4.2 | GATE 17, error 7.7 |
| Toxin | 72, 45% | 2.87 MiB | BAND 10, 1,017,790 bytes | 6.3 | 3.3 | GATE 19, error 8.5 |
| UMW | 60, 53% | 1.50 MiB | BAND 3, 974,804 bytes | 4.65 | 4.4 | GATE 8, error 5.1 |
| Hemophage | 60, 45% | 2.62 MiB | BAND 11, 1,033,780 bytes | 6.1 | 3.5 | GATE 20, error 7.9 |

One band step is 5 to 8% of the file where the hero sways (Overlord, Toxin, Hemophage) and
about 15% near the palette floor (UMW, whose flat rays repaint only along their edges). The
band a delivery needs grows with frames times moving area.

**Near the palette floor, spare budget buys nothing.** UMW ships at 93% of the limit because
BAND 2 is 111%, and its whole gap to an unlimited-size encode is 0.27 rms, so no lever has much
to win. Measured on it: a half step (runs limited to a spread of 5) is 101%; weighting the
palette toward the moving region takes 0.25 rms off it and puts 0.5 onto the dithered hero; a
position-stable ordered dither applied after the hold costs 10% and so a wider band; learning
the palette from six frames instead of three changes about 15 pixels a frame. When the error
is within a few tenths of the palette floor, ship the lowest band that fits and stop.

**The retired gate.** The first three scripts shipped holding each colour until the truth had
moved more than GATE from it, then snapping to the truth. That rule decides frame by frame, so
a pixel is exact when it repaints and a full GATE behind when it next does: a run covers GATE
of drift where a band run covers 2 x BAND, and the same file size costs about twice the worst
error. Do not start a script from it. The band also took the place of the temporal denoise
that ran before the gate (averaging quiet pixels with their neighbouring frames): ahead of the
band it saves 2 to 3%, which lets no tighter band fit on any of the four, and adds error of
its own.

### The other levers

Savings were measured on Overlord under the gate unless a row says otherwise; none of them
changes the choice of method.

| lever | saving | verdict |
|---|---|---|
| Hamming instead of Lanczos for the 3x downscale | 10% | free, Lanczos ringing is noise LZW pays for every frame; bilinear saves 3% more and is softer, box and bicubic cost more (Hemophage) |
| refresh the hero every 2nd / 3rd / 4th frame, arcs every frame | 36% / 48% / 55% | rejected by the user as a stutter even at 6 fps |
| `gifsicle --lossy` | 15% at 10, 27% at 80 | streaks flat armour from 30 up; under the band it buys two band steps, costs more error than they save and degrades the frozen background (Hemophage) |
| a band widened on hard edges, tightened in flat shading | none at equal error | no better than the uniform band on any measure (Hemophage) |
| 128-colour palette | 18% | not worth the banding |
| a hold tighter in the darks (luma-relative) | negative | costs more at every setting |
| row-interleaved refresh phases | negative | more frames with full bounding boxes |
| smaller frame | in proportion to area | last resort, user prefers 640 |
| the electric arcs | about 1% | effectively free, no need to special-case them |

The decision rule the user set: frame rate first. Per-pixel staleness that is visible at 3x
zoom on stills is far less visible than a dropped frame in motion. Offer candidates as files
to view in a browser, with a clean over-limit reference so the motion target is known, rather
than describing them; stills cannot settle this.

## Tool pitfalls (WSL, Pillow 9.0.1, no numpy, no pip)

- Pillow 9.0's `Image.LIBIMAGEQUANT` returns a one-colour palette for any image that is not
  640x360. Learn the palette at 640x360 and only remap at other sizes; assert the colour count.
- `Image.composite` blends the mask into RGB's fourth padding byte and libimagequant reads it
  as alpha, building a visibly worse palette and a smaller, wrong file. Rebuild the buffer with
  `Image.frombytes('RGB', size, im.tobytes())` before quantizing.
- Pillow writes GIF frames interlaced by default; `gif-patch` refuses interlaced frame 0.
  Save with `interlace=False`.
- Pin everything outside the motion mask to frame 0 explicitly. Without that, the first
  refresh repaints the dithered background and gif-patch reports pixels changed outside the
  badge.
- `gifsicle -O3` merges a frame that changed nothing into its predecessor's delay, so frame
  counts in the output can be lower than the input; analyse frames by cumulative delay, not
  index. Delays 8/8/9 cs repeating make 60 frames exactly 5.00 s and 72 exactly 6.00 s.
- Krita's bundled ffmpeg lacks `mpdecimate`; extract every frame (`-fps_mode passthrough`,
  the `scale=in_color_matrix=...,format=rgb24` filter that step 0 picked, Windows path via
  `wslpath -w`) and pick the unique ones in Python. Plain `-pix_fmt rgb24` lets ffmpeg guess
  the matrix; the Overlord and Toxin scripts predate step 0 and still do.
- Not every delivery is padded: Hemophage's was a true 12 fps, 60 frames in the file. Assert
  the frame count and that no frame repeats its neighbour instead of deduplicating.
- 12 fps padded to 29.97 is a 3/2 cadence: the new pictures are at indices 0, 2, 5, 7, ...
  (`i * 5 // 2`), 72 slots in 180 frames. Take frames by slot, not by mean change: Toxin's last
  slot moved only 0.13/255, below the 0.2 dedup threshold, so dedup would have dropped it and
  shortened the loop. Print the frame-to-frame change series once to confirm the cadence, and
  assert it in the script.
- A brute-force alignment search at 1920x1080 in pure Pillow does not finish in two minutes.
  Search at 640x360 in PNG space, coarse to fine, and go to full resolution only for the last
  sub-pixel step.
- ImageChops and ImageStat cover every statistic needed; nothing here needs numpy.

## Validation before shipping

1. Size strictly under 1,048,576 bytes; the script refuses to write otherwise. Error against
   the clean frames is measured, not eyeballed: decode the GIF by cumulative delay and compare
   it with the script's `c640/` frames inside the mask.
2. `gifsicle --info`: 640x360, global colour table, loop forever; frames' delays sum to the
   loop length.
3. Static region: every stored frame's pixels outside the mask equal frame 0 (max change 0).
4. `~/dev/gif-patch/gif-patch bump Preview.gif --from X --to X --out-dir /tmp/x` reports the
   badge found, nothing repainted outside it, and the result under the limit.
5. A rerun is byte-identical (the build is deterministic; a difference means an input moved).
6. Colour against the PNG: over a few boxes inside the hero's flat fills, the GIF's median
   RGB is within a few /255 of the PNG's and the saturation has not dropped. Use medians: the
   video's line art is drawn at a different weight than the PNG's, which moves box means by
   up to 10/255 with no visible colour change (Toxin). A saturation drop in the warm hues
   means step 0 picked the wrong matrix.
7. The user plays it in a browser and in the Workshop preview before it is committed. When the
   output replaces an existing GIF, write it beside the old one (`Preview2.gif`) so the two can
   be compared before either is overwritten.
