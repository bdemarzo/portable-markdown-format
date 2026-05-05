# Portable Markdown Format

Portable Markdown Format packages a Markdown folder tree into a single `.pmf` archive and provides a desktop viewer for browsing the packaged documents.

## Projects

- `src/PortableMarkdownFormat.Core` - shared PMF ZIP parsing, validation, hierarchy, and packaging logic
- `src/PortableMarkdownFormat.Builder` - cross-platform .NET CLI that builds `.pmf` archives
- `src/PortableMarkdownFormat.Viewer` - Avalonia desktop app that opens and renders `.pmf` archives
- `tests/PortableMarkdownFormat.Core.Tests` - xUnit 3 test suite with a 90% line coverage threshold

## Build

```powershell
dotnet build .\PortableMarkdownFormat.slnx
```

## Test

```powershell
dotnet test .\PortableMarkdownFormat.slnx
```

The test project enforces a minimum **90% total line coverage** with Coverlet.

## Build a PMF archive

```powershell
dotnet run --project .\src\PortableMarkdownFormat.Builder -- build --source .\docs --output .\sample.pmf
```

Add `--overwrite` to replace an existing archive.

## Open the viewer

```powershell
dotnet run --project .\src\PortableMarkdownFormat.Viewer -- .\sample.pmf
```

You can also start the viewer without arguments and open a `.pmf` file from the UI.

## PMF v1 summary

- `.pmf` files are ZIP archives
- PMF v1 contains Markdown files only
- PMF v1 has no required manifest file
- the `_pmf/` top-level path is reserved for future format extensions

See `docs/pmf-spec.md` for the format contract.
