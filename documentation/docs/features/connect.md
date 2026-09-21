# Starsky Connect (Syncthing BEP Sync)

Starsky Connect is a built-in sync engine that makes Starsky wire-compatible with [Syncthing](https://syncthing.net). It implements the **BEP v1** (Block Exchange Protocol) so any device running Syncthing can exchange photos and other files with a Starsky instance without requiring a separate Syncthing installation.

---

## How it works

```
Device A (Syncthing)              Device B (Starsky Connect)
        │                                    │
        │── TLS 1.3 (mTLS) ──────────────────│
        │── Hello ──────────────────────────▶│
        │◀─ Hello ───────────────────────────│
        │── ClusterConfig ──────────────────▶│  (lists all shared devices)
        │◀─ ClusterConfig ───────────────────│
        │── Index / IndexUpdate ────────────▶│  (file metadata + block hashes)
        │◀─ Index / IndexUpdate ─────────────│
        │── Request (block) ────────────────▶│  (download individual 128 KiB–16 MiB blocks)
        │◀─ Response (block data) ───────────│
        │   … repeat until file complete …   │
```

Both sides discover which files the other has, then pull only the blocks they are missing. This is exactly how Syncthing works — Starsky Connect is a compatible peer, not a wrapper around the Syncthing binary.

---

## Architecture

The implementation is split across two .NET projects that follow Starsky's layer conventions:

### `starsky.foundation.connect/` — Protocol stack

Pure infrastructure: no knowledge of Starsky's photo model.

| Namespace | What it does |
|---|---|
| `Crypto/` | Self-signed TLS certificate generation, Device ID derivation (SHA-256 → Base32 → Luhn), Luhn-mod-N checksum |
| `Protocol/` | BEP framing (`BepReader` / `BepWriter`), LZ4 compression, generated protobuf types (`bep.proto`) |
| `Transport/` | `TlsDialer`, `TlsListener`, `ConnectionManager` (per-device connection lifecycle, keepalive ping every 90 s, reconnect backoff) |
| `Discovery/` | `LocalDiscovery` (UDP multicast 239.255.68.42:21027), `GlobalDiscovery` (HTTPS against discovery.syncthing.net) |
| `Relay/` | `RelayClient` — relay wire protocol (magic `0x9E79BC40`, XDR payloads, ALPN `bep-relay`) |
| `Storage/` | `ConfigStore` — JSON config (device cert, folder list, peer list) |
| `Sync/` | `ISyncLocalChangeSource`, `VectorClock`, `BlockSizing` |

### `starsky.feature.connect/` — Sync engine

Wires the protocol stack to Starsky's database and filesystem.

| Class | Responsibility |
|---|---|
| `SyncEngine` | Top-level `IHostedService`; BEP state machine (Initial → Ready); message dispatch |
| `FolderModel` | Per-folder index state in EF Core; full/delta index exchange; ClusterConfig Device entries |
| `Scanner` | Periodic full scan + incremental re-scan on `ISyncLocalChangeSource` events; block-level SHA-256 hashing |
| `BlockStore` | Content-addressed temp storage for in-progress downloads |
| `Downloader` | Up to 128 concurrent block Requests; Response correlation via `TaskCompletionSource` |
| `ConflictResolver` | Vector-clock comparison; `.sync-conflict-YYYYMMDD-HHMMSS-XXXXXXX` renaming |

---

## Database schema

Four new EF Core tables live in `ApplicationDbContext` alongside the existing `FileIndex`:

| Table | Purpose |
|---|---|
| `ConnectFileMeta` | BEP overlay on top of `FileIndexItem`: vector clock, sequence number, block size, deleted flag, unix permissions |
| `ConnectBlockInfo` | Per-block SHA-256 hashes for block-level delta transfer |
| `ConnectFolderMeta` | Per-folder `IndexId` + `Sequence` counter for delta index exchange |
| `ConnectDeviceIndex` | Remote device `MaxSequence` + `IndexId` tracking |

`FileIndexItem` (Starsky's own photo metadata table) is **not** replaced — `ConnectFileMeta` soft-joins to it by `(Folder, Name)`. BEP may learn about a file before Starsky's scanner has processed it, so there is intentionally no FK constraint.

---

## File permissions

### Unix / macOS / Linux

The Scanner captures the full `rwxrwxrwx` bits (9 LSB of the Unix file mode) when indexing a local file, stores them in `ConnectFileMeta.Permissions`, and includes them in the outgoing BEP `FileInfo`.

When a file is **downloaded** from a peer, the same permission bits advertised in that peer's `FileInfo` are applied to the assembled file via `File.SetUnixFileMode`. This preserves the original mode across devices.

### Windows

`ConnectFileMeta.Permissions` is stored as `0` on Windows (Unix permission bits are meaningless there). When sending a BEP `FileInfo` to a peer, `Permissions = 0` tells the receiving Syncthing/Starsky instance to fall back to OS-default permissions. Incoming `Permissions` values from Unix peers are silently ignored on Windows — `File.SetUnixFileMode` is never called.

---

## Device identity

Each Starsky Connect instance generates a self-signed X.509 certificate on first start. The Device ID is derived from it:

```
SHA-256(DER-encoded cert)        → 32 bytes
Base32 encode                    → 52 characters
Split into 4 × 13-char groups    → add 1 Luhn check character per group (56 chars)
Group into 8 × 7 chars           → join with hyphens (63 chars, Syncthing display format)
```

The Device ID is used instead of a CA-signed certificate chain. During TLS handshake, Starsky Connect validates the peer's certificate by comparing its computed Device ID against the configured allow-list — the certificate's issuer/subject are irrelevant.

---

## Folder scan algorithm

1. Walk the folder root via `Directory.EnumerateFiles` (or via `FileIndexItem` records in the DB for full-catalog scans).
2. For each file, choose a block size: smallest power-of-two multiple of 128 KiB such that the file fits in fewer than 2 000 blocks, up to 16 MiB max.
3. Compute a SHA-256 hash for each block and upsert `ConnectBlockInfo` rows.
4. Upsert `ConnectFileMeta` with the new sequence number and unix permission bits.
5. Increment `ConnectFolderMeta.Sequence`.

Incremental re-scans are triggered immediately by `ISyncLocalChangeSource.LocalFileChanged` without waiting for the next hourly full scan.

---

## ClusterConfig exchange (BEP spec requirement)

When a BEP connection is established, both sides send a `ClusterConfig` message listing **all devices** that share each folder — not just the sender. Starsky Connect populates this list with:

- The local device's own `IndexId` and `MaxSequence`.
- The remote peer's last-known `IndexId` and `MaxSequence` (from `ConnectDeviceIndex`).

Omitting the remote peer from the list causes Syncthing to not send its `Index` and refuse to sync.

---

## Testing

Integration tests live in [`starskytest/starsky.feature.connect/Sync/SyncEngineFileSyncIntegrationTest.cs`](../../starsky/starskytest/starsky.feature.connect/Sync/SyncEngineFileSyncIntegrationTest.cs). They start a real Syncthing process in a temp directory and exercise both directions:

- **Test A — Syncthing → Starsky**: writes a file into the Syncthing folder; asserts Starsky Connect downloads it within 30 s.
- **Test B — Starsky → Syncthing**: calls `Scanner.ScanFolderDirectAsync` on a file; asserts Syncthing downloads it within 30 s.

Run all connect tests:

```bash
cd starsky && dotnet run --project starskytest/starskytest.csproj -- --filter "FullyQualifiedName~connect"
```

Run only the integration tests:

```bash
cd starsky && dotnet run --project starskytest/starskytest.csproj -- --filter "FullyQualifiedName~SyncEngineFileSyncIntegration"
```

> **Note**: the integration tests require the `syncthing` binary to be on `PATH`. On macOS this is typically installed via `brew install syncthing`.
