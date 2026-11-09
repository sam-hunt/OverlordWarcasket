# Animated Workshop preview from an artist's MP4

Snapshot of the family guide `../PREVIEW_GIF_FROM_MP4.md` (the primary for the next commission
in any mod; update it first and recopy) kept here so this repo's tooling docs stand alone.

Reference implementation: this repo's `Scripts/preview-gif.py`, 2026-10-31. Result:
a 2.4 MB 1920x1080 H.264 delivery became `.steamworkshop/Preview.gif` at 640x360, all 60 frames
at 12 fps, 1,035,622 bytes (98.8% of Steam's 1 MiB limit), with the static art pixel-identical
to `About/Preview.png`. The script's header is the record of the method and the tuning sweep;
this guide is what carries over to the next commission. Budget a session for it: the encode is
seconds, the tuning is a judgement call that needs the user's eye on moving files.

## What the artist delivers

The brief is "animate our About/Preview.png". The delivery so far has been the PNG blown up 3x
to the hero art's source resolution, the hero rigged and animated on top, rendered at 12 fps
padded to 30, audio track attached, badge reading whatever version the PNG read at the time.
Consequences that drive the whole method:

- Only the hero is new information. Everything else is the PNG upscaled, blurred by the scale
  and the codec, tonally shifted (darks crushed, highlights compressed) and noisy frame to
  frame. Downscaling it back loses quality against the PNG we already ship. So the GIF is the
  PNG, with the video's hero composited into the region the animation touches.
- The loop closes cleanly (last frame -> first within noise), but check: the Overlord loop was
  not periodic at any shorter lag, so every unique frame had to be kept.
- The badge in the video does not matter. The GIF's badge comes from the PNG's pixels in
  frame 0, which `gif-patch` bumps in place; never re-export to change a version digit.
- Do not track the MP4. Keep it with the gameplay recordings; the workshop README says so.

## Running it on a new delivery or another mod

For another mod, copy `Scripts/preview-gif.py` across and add the `Scripts/CLAUDE.md` bullet;
the repo rules want the header kept as the record when numbers change. Then check the constants
that encode this delivery's geometry before trusting the output:

- `SIZE` (640x360) must equal the PNG's; the palette is learned at that size (see pitfalls).
- `MASK_LEFT_EDGE` shuts codec noise on the title out of the motion mask; set it to where the
  hero's motion actually starts (the log prints the mask's bbox and coverage, 44% on Overlord).
- `fit_lut` excludes the top-left 80x80 badge corner from the tone fit; move it if the badge
  moves. The log prints the static-region mismatch before and after the fit (13 -> 6 /255 on
  Overlord); if "after" is not a clear improvement, the delivery is not a scaled PNG and the
  composite approach needs rethinking.
- The three-frame palette sample (0, n/3, 2n/3) assumes the colours do not drift across the
  loop; a glow that only appears late in the loop needs a frame from that stretch in the sample.

Then the only real tuning is `GATE`: raise it until the file fits, with a few KB of headroom.
A gif-patch bump on a lossless-LZW file shrinks it slightly (-84 bytes on Overlord); on a file
that went through `gifsicle --lossy` it grows about 4 KB, because frame 0 is rewritten clean.

## The cost model (640x360, 44% hero, 60 frames)

One clean refresh of the hero costs about 35 KB in LZW however it is encoded, so a clean
12 fps encode is 2.1 MiB and nothing conventional closes the gap on its own:

| lever | saving | verdict |
|---|---|---|
| per-pixel gate (keep last colour until truth moves > GATE) | GATE 12: 40%, 16: 51%, 17: 53%, 20: 58% | the one that fits; faint grain in dark shadows |
| Hamming instead of Lanczos for the 3x downscale | 10% | free, Lanczos ringing is noise LZW pays for every frame |
| temporal denoise of quiet pixels (change < 12 to both neighbours) | 3% | free; stronger thresholds cost more, not less |
| refresh the hero every 2nd / 3rd / 4th frame, arcs every frame | 36% / 48% / 55% | rejected by the user as a stutter even at 6 fps |
| `gifsicle --lossy` | 15% at 10, 27% at 80 | streaks flat armour from 30 up |
| 128-colour palette | 18% | not worth the banding |
| luma-relative gate (tighter in darks) | negative | costs more at every setting |
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
  index. Delays 8/8/9 cs repeating make 60 frames exactly 5.00 s.
- Krita's bundled ffmpeg lacks `mpdecimate`; extract every frame (`-fps_mode passthrough
  -pix_fmt rgb24`, Windows path via `wslpath -w`) and drop duplicates in Python by mean change.
- ImageChops and ImageStat cover every statistic needed; nothing here needs numpy.

## Validation before shipping

1. Size strictly under 1,048,576 bytes; the script refuses to write otherwise.
2. `gifsicle --info`: 640x360, global colour table, loop forever; frames' delays sum to the
   loop length.
3. Static region: every stored frame's pixels outside the mask equal frame 0 (max change 0).
4. `~/dev/gif-patch/gif-patch bump Preview.gif --from X --to X --out-dir /tmp/x` reports the
   badge found, nothing repainted outside it, and the result under the limit.
5. A rerun is byte-identical (the build is deterministic; a difference means an input moved).
6. The user plays it in a browser and in the Workshop preview before it is committed.
