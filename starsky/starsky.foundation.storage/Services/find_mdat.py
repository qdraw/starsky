#!/usr/bin/env python3
"""
find_mdat.py

Locate the first 'mdat' atom in an MP4 file, print header info and optionally hash the first N bytes
of the mdat payload (MD5 + Base32).
"""

import sys
import struct
import argparse
import hashlib
import base64
from pathlib import Path

def base32_no_padding(data: bytes) -> str:
    return base64.b32encode(data).decode('ascii').rstrip('=')

def find_first_mdat(path: Path, hash_bytes: int = 0):
    with path.open('rb') as f:
        file_len = path.stat().st_size
        pos = 0
        while True:
            # Read header (8 bytes)
            header = f.read(8)
            if len(header) < 8:
                print("EOF reached while scanning; no atom header found.")
                return None
            size32, atom_type = struct.unpack('>I4s', header)
            atom_type = atom_type.decode('ascii', errors='replace')
            pos_header = pos
            pos += 8

            if size32 == 0:
                # Atom extends to EOF if seekable
                # calculate atom size as remaining bytes
                try:
                    remaining = file_len - pos_header
                except Exception:
                    remaining = -1
                atom_size = remaining if remaining >= 0 else None
                header_size = 8
                data_offset = pos
            elif size32 == 1:
                # extended size in next 8 bytes
                large = f.read(8)
                if len(large) < 8:
                    print("Unexpected EOF reading extended size.")
                    return None
                size64 = struct.unpack('>Q', large)[0]
                atom_size = size64
                header_size = 16
                data_offset = pos + 8
                pos += 8
            else:
                atom_size = size32
                header_size = 8
                data_offset = pos

            payload_len = (atom_size - header_size) if (atom_size is not None) else None

            if atom_type == 'mdat':
                return {
                    'header_offset': pos_header,
                    'atom_type': atom_type,
                    'atom_size': atom_size,
                    'header_size': header_size,
                    'data_offset': data_offset,
                    'payload_len': payload_len,
                }

            # skip payload to next atom
            if atom_size is None:
                # can't determine next atom, abort
                print("Encountered atom with unknown size; aborting scan.")
                return None

            to_skip = atom_size - header_size
            if to_skip < 0:
                print("Invalid atom size; aborting.")
                return None

            # Use seek if possible
            try:
                f.seek(to_skip, 1)
                pos += to_skip
            except (OSError, IOError):
                # fallback read/skip
                remaining = to_skip
                while remaining > 0:
                    chunk = f.read(min(65536, remaining))
                    if not chunk:
                        break
                    remaining -= len(chunk)
                    pos += len(chunk)

def hash_mdat_payload(path: Path, data_offset: int, payload_len: int, max_bytes: int):
    to_read = min(payload_len if payload_len is not None else max_bytes, max_bytes)
    md5 = hashlib.md5()
    with path.open('rb') as f:
        f.seek(data_offset)
        remaining = to_read
        while remaining > 0:
            chunk = f.read(min(65536, remaining))
            if not chunk:
                break
            md5.update(chunk)
            remaining -= len(chunk)
    digest = md5.digest()
    return md5.hexdigest(), base32_no_padding(digest)

def main():
    p = argparse.ArgumentParser(description='Find first mdat atom in MP4 and optionally hash part of it.')
    p.add_argument('file', type=str)
    p.add_argument('--hash-bytes', type=int, default=0,
                   help='If >0 compute MD5 and base32 of up to N bytes of the mdat payload (default 0)')
    args = p.parse_args()

    path = Path(args.file)
    if not path.exists():
        print("File not found:", path)
        sys.exit(2)

    info = find_first_mdat(path, hash_bytes=args.hash_bytes)
    if not info:
        print("No mdat found.")
        sys.exit(1)

    print("Found first mdat:")
    print(f"  header_offset: {info['header_offset']}")
    print(f"  atom_type: {info['atom_type']}")
    print(f"  atom_size: {info['atom_size']}")
    print(f"  header_size: {info['header_size']}")
    print(f"  data_offset: {info['data_offset']}")
    print(f"  payload_len: {info['payload_len']}")

    if args.hash_bytes and args.hash_bytes > 0:
        payload_len = info['payload_len'] if info['payload_len'] is not None else args.hash_bytes
        md5hex, b32 = hash_mdat_payload(path, info['data_offset'], payload_len, args.hash_bytes)
        print(f"\nMD5 (hex) of first {min(payload_len, args.hash_bytes)} payload bytes: {md5hex}")
        print(f"Base32: {b32}")

if __name__ == '__main__':
    main()
