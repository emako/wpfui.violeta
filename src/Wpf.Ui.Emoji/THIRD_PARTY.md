# Third-party components in Wpf.Ui.Emoji

This project is based on [emoji.wpf](https://github.com/samhocevar/emoji.wpf) (WTFPL)
and uses the self-contained engineering approach from
[UI.WPF.Emojis](https://github.com/iNKORE-NET/UI.WPF.Emojis) (no git submodules).

## License of this package

See `COPYING` — **WTFPL** (same as emoji.wpf). Not LGPL.

## Vendored dependencies

| Path | Origin | Notes |
| --- | --- | --- |
| `ThirdParty/Typography/` | Typography (OpenFont / GlyphLayout) | Vendored subset used for COLR/CPAL |
| `ThirdParty/STFU/` | Stfu (Linq + BoolInverter) | Minimal WTFPL utilities used by emoji.wpf |
| `Resources/Text/UnicodeEmoji/emoji-test.txt` | Unicode emoji-test data | Gzipped and embedded at build time |
| `Resources/Shaders/TintEffect.ps` | Precompiled HLSL from emoji.wpf | Packaged as WPF Resource |

UnicodeCLDR is **not** included (runtime only needs `emoji-test.txt`).
