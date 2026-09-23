# Commit Convention

This repository uses **English imperative** commit messages that briefly state what changed and why. **Do not end with a period.**

## Format

```
<Verb> <concise description>
```

- Start with a capitalized verb (or verb phrase)
- Rest of the line is plain English, spaced as one readable sentence
- **Do not** end with `.`
- Conventional Commits `type(scope):` prefixes are not required; start with a verb
- Prefer a single line; if a body is needed, leave a blank line then continue—still no trailing period

### Good examples

```
Add window-hole breathing island indicator
Refactor SMTC UI to shared MVVM state
Fix island hover flicker during morphs
Improve SMTC metadata display
```

### Bad examples

```
Added window hole feature.          ← past tense + period
fix: hover bug                      ← lowercase verb; this repo does not require type: prefixes
Refactor SMTC UI.                   ← trailing period
update stuff                        ← vague, verb not capitalized
```

## Common verbs

Based on recent habit in this repo; prefer these for consistent style.

| Verb | When to use | Example |
|------|-------------|---------|
| **Add** | New feature, asset, setting, or module | `Add SMTC media controls and settings` |
| **Refactor** | Restructure or extract shared logic; behavior mostly unchanged | `Refactor island modules to region slots` |
| **Fix** | Bug fix or incorrect behavior | `Fix island collapse on rapid hover` |
| **Improve** | Better experience or quality on existing capability | `Improve SMTC metadata display` |
| **Refine** | Smaller tweak than Improve | `Refine window hole minimize behavior` |
| **Update** | Existing assets, dependencies, copy, or config | `Update app icon assets` |
| **Rename** | Rename APIs, setting keys, styles, etc. | `Rename auto-collapse settings and migrate keys` |
| **Remove** / **Drop** | Delete feature, dead code, or deprecated items | `Remove unused island test stub` |
| **Harden** | Edge cases, lifecycle, nulls, etc. | `Harden SMTC session lifecycle handling` |
| **Align** | Match system / existing UI or behavior | `Align SMTC transport controls with system flyout styling` |
| **Document** | Docs / conventions only | `Document Fluent System Icons convention` |
| **Clean up** | Reduce XAML/code noise with no feature intent | `Clean up SettingsWindow XAML attributes` |
| **Redesign** | Larger UI / interaction redo | `Redesign SMTC card and trim publish behavior` |
| **Animate** | Dedicated animation or motion | `Animate settings section navigation` |
| **Stabilize** | Remove flicker, races, unstable behavior | `Stabilize island hover during morphs` |
| **Prevent** | Block a specific bad outcome | `Prevent initial Settings window white flash` |
| **Gate** | Conditionally limit a capability (e.g. debug-only) | `Gate Island Test section to debug builds` |
| **Skip** | Intentionally skip a target / path | `Skip own windows in hole targeting` |
| **Keep** | Maintain an invariant or constraint | `Keep detached island perfectly circular` |
| **Inline** | Inline implementation; remove needless indirection | `Inline CompactMinimal island rendering` |
| **Modularize** | Split into composable modules | `Modularize island content and add clock style` |
| **Adjust** | Size, layout, or numeric tweaks | `Adjust SMTC expanded seek layout height` |
| **Smooth** | Smoother transitions / shutdown / animation | `Smooth SMTC equalizer shutdown` |
| **Invert** | Flip polarity, order, or a boolean default | `Invert island auto-collapse default` |
| **Pause** / **Always**, etc. | When the verb *is* the behavior | `Pause idle timers while context menu is open` |

When unsure: **Add** for new capability, **Refactor** for structure, **Fix** for bugs, **Improve** / **Refine** for better UX.

## Writing tips

1. **Focus on intent**: describe what users or maintainers notice; avoid filename laundry lists
2. **One theme per commit**: split large changes
3. **Scannable**: the verb should show the category at a glance (Add / Fix / Refactor…)
4. **No period**: neither subject nor body paragraphs end with `.`
5. **Skip i18n / icon notes** unless this change touches strings or icon conventions (e.g. `Add localization for…`)

## Agent notes

When drafting a commit message:

- Use English imperative + verbs from the table above
- **Do not** put a period at the end of the message
- Do not use Conventional Commits `feat:` / `fix:` prefixes unless the user explicitly asks
