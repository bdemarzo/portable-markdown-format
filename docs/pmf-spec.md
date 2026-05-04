# Portable Markdown Format (PMF) 1.0

PMF is a minimalist archive format for transporting a folder hierarchy of Markdown files as a single portable artifact.

## 1. Container format

- A PMF file **MUST** be a standard ZIP archive.
- A PMF file **SHOULD** use the `.pmf` extension.
- ZIP entry names **MUST** use forward slashes (`/`) as path separators.

## 2. Required archive entries

Every PMF archive **MUST** contain:

1. `pmf-manifest.json`
2. One or more Markdown payload files under `content/`

## 3. Manifest

`pmf-manifest.json` **MUST** be UTF-8 JSON with this shape:

```json
{
  "format": "PMF",
  "specificationVersion": "1.0",
  "createdUtc": "2026-05-04T12:00:00+00:00",
  "rootDirectoryName": "docs",
  "files": [
    {
      "relativePath": "guide/intro.md",
      "entryName": "content/guide/intro.md",
      "byteLength": 312,
      "sha256": "b4f1b5d8e8b9cf0d2d8d6cbf0908dd43d89bbf4d815f8880a5b6fa7af8ea9d0d"
    }
  ]
}
```

### Manifest fields

| Field | Type | Meaning |
| --- | --- | --- |
| `format` | string | Literal value `PMF` |
| `specificationVersion` | string | PMF spec version, currently `1.0` |
| `createdUtc` | string | Archive creation timestamp in UTC-compatible ISO 8601 format |
| `rootDirectoryName` | string | Name of the packaged source folder |
| `files` | array | Ordered list of packaged Markdown files |

### File entry fields

| Field | Type | Meaning |
| --- | --- | --- |
| `relativePath` | string | Markdown file path relative to the packaged root folder |
| `entryName` | string | ZIP entry path for the packaged file |
| `byteLength` | number | Payload length in bytes |
| `sha256` | string | Lowercase SHA-256 hash of the payload bytes |

## 4. Path rules

- `relativePath` **MUST NOT** be rooted.
- `relativePath` **MUST NOT** contain empty segments, `.` segments, or `..` segments.
- `entryName` **MUST** equal `content/` plus the normalized `relativePath`.
- Consumers **SHOULD** treat the manifest as authoritative for the package contents.

## 5. Payload rules

- Packaged files are Markdown documents discovered from the source hierarchy.
- Relative folder structure is preserved exactly inside `content/`.
- UTF-8 Markdown text is recommended for maximum portability.

## 6. Reader validation rules

A PMF reader **MUST** reject a package when:

- `pmf-manifest.json` is missing
- `format` is not `PMF`
- `specificationVersion` is unsupported
- a declared payload entry is missing
- a payload hash or byte length does not match the manifest
- a manifest path violates the path rules above

## 7. Writer behavior

A PMF writer **SHOULD**:

- package only Markdown files from the chosen folder hierarchy
- preserve relative paths
- emit deterministic file ordering
- write payload hashes into the manifest
