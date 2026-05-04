# Portable Markdown Format

Portable Markdown Format (PMF) is a minimalist ZIP-based packaging format for Markdown collections, plus a cross-platform .NET toolset for building and viewing `.pmf` archives.

## Solution layout

- `src/PortableMarkdownFormat.Core` - PMF archive model, ZIP read/write services, and discovery helpers
- `src/PortableMarkdownFormat.Builder` - Avalonia desktop app for packaging a folder of Markdown files into a `.pmf`
- `src/PortableMarkdownFormat.Viewer` - Avalonia desktop app for opening a `.pmf`, browsing packaged files, and rendering Markdown
- `tests/PortableMarkdownFormat.Core.Tests` - XUnit 3 coverage for core PMF behavior
- `docs/pmf-spec.md` - the PMF 1.0 specification

## PMF summary

A PMF file is a standard ZIP archive with:

1. A required UTF-8 JSON manifest at `pmf-manifest.json`
2. Packaged Markdown files stored under `content/`
3. Relative paths preserved with forward-slash (`/`) separators

See [`docs/pmf-spec.md`](docs/pmf-spec.md) for the full format.

## Running the toolset

```bash
dotnet restore PortableMarkdownFormat.slnx
dotnet build PortableMarkdownFormat.slnx
dotnet test PortableMarkdownFormat.slnx
dotnet run --project src/PortableMarkdownFormat.Builder
dotnet run --project src/PortableMarkdownFormat.Viewer
```

## Builder workflow

1. Choose a source folder that already contains Markdown files.
2. Choose the destination `.pmf` file.
3. Build the package. The original folder hierarchy is preserved in the archive.

## Viewer workflow

1. Open an existing `.pmf` file.
2. Browse the packaged Markdown files by relative path.
3. Select a file to render it in the preview pane.
