# Animated Workshop preview: the Overlord run

The record of turning the artist's animated `Preview.mp4` into `.steamworkshop/Preview.gif`:
first built 2026-10-31 with a per-pixel gate, rebuilt 2026-11-18 from the same MP4 with the
band hold. `Scripts/preview-gif.py`'s header is the compact record of the method;
`PREVIEW_GIF_FROM_MP4.md` beside this file is the family guide. This file keeps what was
measured on this delivery.

## Inputs

| item | value |
|---|---|
| delivery | `Preview.mp4`, 2,453,978 bytes, never tracked, kept with the gameplay recordings |
| sha256 | `dc2bbee287424bfb33ecffdeefee5b5eefb95328d60eccc3b5a8ab15755ae6e9` |
| stream | H.264, 1920x1080, 12 fps padded to 30: 150 frames, 60 unique, 5.00 s |
| PNG | `About/Preview.png`, 640x360, badge "1.7" |

A rebuild needs the MP4 at `.steamworkshop/Preview.mp4` (or `--mp4`). With the same MP4 and
PNG the script's output is byte-identical to the committed GIF.

## What the script measures on it

- **Tone curve:** the static-region mismatch against the PNG falls from 13.1 to 5.9 /255.
- **Mask:** 44% of the frame, bbox (251, 6)-(640, 360), 5,941 hole pixels filled.
- **Decode:** ffmpeg's default matrix (`-pix_fmt rgb24`). The palette is too unsaturated for
  the matrix to show (the hero's fill medians are within 2/255 of the PNG), so it was left.

## The hold: band 9 in place of gate 17

Decoded GIF against the clean composite frames inside the mask, max channel /255.

| build | bytes | of limit | error mean / rms / worst |
|---|---|---|---|
| clean (band 0) | 2,572,876 | 245.4% | 3.44 / 4.18 / 39 |
| gate 17 with the temporal denoise (shipped until 2026-11-18) | 1,035,622 | 98.8% | 6.10 / 7.73 / 39 |
| band 7 | 1,192,889 | 113.8% | 4.51 / 5.46 / 39 |
| band 8 | 1,105,817 | 105.5% | 4.78 / 5.81 / 39 |
| **band 9 (shipped)** | 1,029,257 | 98.2% | 5.02 / 6.13 / 39 |
| band 10 | 962,336 | 91.8% | 5.26 / 6.48 / 39 |
| band 8 after the temporal denoise | 1,082,255 | 103.2% | 4.87 / 5.96 / 37 |
| band 9 after the temporal denoise | 1,007,337 | 96.1% | 5.11 / 6.28 / 37 |

The clean row is the palette's own cost. The gate added 3.5 rms to it and the band adds 1.9.
The temporal denoise that ran before the gate is gone: ahead of the band it saves 2%, which
does not bring band 8 under the limit, and it moves band 9 further from the clean frames.

Before the swap, the unchanged script reproduced the then-shipped GIF byte for byte from this
MP4, so the two builds differ in the hold and the denoise only.

## Validation

- 1,029,257 bytes; `gifsicle --info`: 640x360, global colour table of 256, loop forever, 60
  frames, 5.00 s.
- Outside the mask, every frame equals frame 0 (max change 0).
- `gif-patch bump --from 1.7 --to 1.7` finds the badge (rms 0.020), changes nothing outside
  it, and writes 64 bytes fewer.
- The author compared the gate and band builds side by side in a browser and kept the band.
