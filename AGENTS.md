# Agent guidelines

## Commits

Commit messages follow [`doc/CommitConvention.md`](doc/CommitConvention.md): English imperative, verb-first (Add / Refactor / Fix / …), **no trailing period**.

## Bump version

When asked to **Bump version**, do both in the same change set:

1. Bump package/assembly versions (`AssemblyVersion` / `FileVersion` / `Version` in the relevant `.csproj` files, and `Version` in `makekits.json`)
2. Add a Keep a Changelog section at the top of [`CHANGELOG.md`](CHANGELOG.md) for the new version (`## [x.y.z.w] - YYYY-MM-DD` plus bullet entries for commits since the previous version)

Tag/release notes are built from that CHANGELOG section; tagging without a matching section will fail CI.

## WPF-UI source

When you need WPF-UI source, check `../wpfui` first (sibling of this repo). If that directory exists, read it from there. If it does not, find the source another way. Reference only: do not modify that tree. Changes belong in this repo.

