# Portable Markdown Format (PMF) v1

Portable Markdown Format is a minimalist ZIP-based container for distributing a folder hierarchy of Markdown files as a single file.

## Goals

- Preserve an existing Markdown folder hierarchy
- Remain inspectable with standard ZIP tooling
- Require no manifest file for v1
- Keep viewer behavior deterministic across platforms

## Container

- **Extension:** `.pmf`
- **Physical format:** ZIP archive
- **Encoding:** ZIP entry names are interpreted as UTF-8 text paths

## Allowed archive entries

PMF v1 allows only Markdown file entries. A valid archive contains one or more file entries whose extensions are one of:

- `.md`
- `.markdown`
- `.mdown`
- `.mkd`
- `.mkdn`

Explicit directory entries are optional and ignored by readers. Non-Markdown file entries are invalid in PMF v1.

## Path rules

Each Markdown file is stored at its relative path inside the ZIP archive and must follow these rules:

- Use `/` as the logical path separator
- Do not start with `/`
- Do not end with `/`
- Do not contain empty path segments
- Do not contain `.` or `..` path segments
- Do not contain `:`
- Paths are treated as case-insensitively unique for PMF validation

## Reserved namespace

The top-level path segment `_pmf/` is reserved for future PMF metadata and extensions. PMF v1 writers must not create entries under `_pmf/`, and PMF v1 readers reject them.

## Source of truth

PMF v1 has no required manifest file. Readers derive:

- the packaged file list from ZIP file entries
- the folder tree from ZIP entry paths
- the document display names from the final path segment of each entry

## Writer behavior

A compliant PMF v1 writer:

1. Recursively scans an input directory
2. Selects only supported Markdown files
3. Normalizes each relative path according to the PMF path rules
4. Rejects duplicate normalized paths
5. Stores the files in a ZIP archive using the normalized relative paths

## Reader behavior

A compliant PMF v1 reader:

1. Opens the ZIP archive
2. Ignores explicit directory entries
3. Validates each file entry against the PMF v1 rules
4. Rejects empty archives
5. Builds the display hierarchy from entry paths
6. Renders the selected Markdown document as Markdown

## Compatibility

- PMF v1 readers should reject archives that use the reserved `_pmf/` namespace because that signals a future format extension they do not understand.
- Future PMF revisions may define metadata under `_pmf/` without changing the ZIP container itself.
