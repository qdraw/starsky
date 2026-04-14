# Folder Picker Bridge Specification

## Overview
This document specifies the native macOS-to-web folder picker implementation in `webView.swift`. The implementation uses a deterministic, id-based Promise bridge that allows the React frontend (`useFolderPicker()`) to request folder selection from the native app and receive the selected path and security-scoped bookmark.

## Architecture

### High-Level Flow
```
React Hook (useFolderPicker)
         ↓
 window.__starskyNative.selectFolder(timeoutMs)
         ↓
 JS Bridge generates unique ID, stores Promise resolver
         ↓
 Posts { action: 'selectFolder', id } via webkit.messageHandlers.filePicker
         ↓
 Native Coordinator receives message, extracts id
         ↓
 FilePickerController opens NSOpenPanel for folder selection
         ↓
 Native evaluates JS: window.__starskyNative._resolveFolderPick(id, path, bookmark)
         ↓
 Promise resolver in _pending[id] is called and deleted
         ↓
 React Hook receives { path, bookmark } result
```

## Components

### 1. Injected JavaScript Bridge (`nativeBridgeJS`)

**Location:** `WebView.makeNSView(context:)` → document-start user script

**Injected at:** Document start (`atDocumentStart`), all frames (`forMainFrameOnly: false`)

**Provides:**
- `window.__starskyNative` namespace
- `window.__starskyNative._pending` object (resolver registry)
- `window.__starskyNative._resolveFolderPick(id, path, bookmark)` function
- `window.__starskyNative.selectFolder(timeoutMs)` Promise API

**Behavior:**

#### `selectFolder(timeoutMs: number = 30000) -> Promise<{path, bookmark}>`
1. Generates unique request ID: `fp_<timestamp>_<random>`
2. Creates a new Promise and stores its `resolve` function in `_pending[id]`
3. Posts message to native handler: `{ action: 'selectFolder', id }`
4. If posting fails or no native handler: immediately resolves with `{ path: null, bookmark: null }`
5. Sets timeout (default 30s): if resolver still in `_pending[id]`, deletes it and resolves with nulls
6. Returns the Promise immediately

**Error Handling:**
- If native handler missing → resolves with nulls immediately
- If postMessage throws → resolves with nulls immediately
- If timeout reached → resolves with nulls (removes resolver)

#### `_resolveFolderPick(id: string, path: string | null, bookmark: string | null) -> boolean`
1. Looks up resolver function in `_pending[id]`
2. If found:
   - Calls resolver with `{ path: path ?? null, bookmark: bookmark ?? null }`
   - Deletes `_pending[id]`
   - Returns `true`
3. If not found or error: returns `false`

### 2. Swift Controller Layer

#### `FilePickerController`

**Method:** `jsForResult(requestId: String, url: URL?) -> String`

**Purpose:** Generate the JavaScript callback to invoke the Promise resolver

**Returns:** A single-line JavaScript call:
```js
window.__starskyNative._resolveFolderPick("<id>","<path-json>","<bookmark-base64-json>");
```

**Encoding:**
- Request ID: JSON-encoded string (handles special chars, quotes)
- Path: JSON-encoded string (or `null` if no selection)
- Bookmark: Base64-encoded security-scoped bookmark data, then JSON-encoded (or `null`)

**Error Handling:**
- If JSONEncoder fails: falls back to `"null"` in JavaScript, which resolver interprets as no selection

**Method:** `performPick(webView: WebViewEvaluating?, requestId: String)`

**Purpose:** Execute the native folder picker and return result to web

**Steps:**
1. Calls `picker.pickFolder { url in ... }`
2. On main thread: generates JavaScript callback via `jsForResult(requestId:url:)`
3. Evaluates JS in webView with completion handler
4. Logs success/error via NSLog

**Logging:**
- `[WebView][FolderPickResult] resolved requestId=<id>` on success
- `[WebView][EvalError] <error>` on JS evaluation failure

#### `NSOpenPanelFilePicker`

**Method:** `pickFolder(completion: @escaping (URL?) -> Void)`

**Purpose:** Native folder picker UI

**Behavior:**
1. Dispatches to main thread
2. Creates NSOpenPanel with:
   - `canChooseFiles = false`
   - `canChooseDirectories = true`
   - `allowsMultipleSelection = false`
   - `prompt = "Select"`
3. Calls `runModal()`
4. If response is `.OK` and URL exists: calls `completion(url)`
5. Otherwise: calls `completion(nil)` (user cancelled or error)

### 3. Swift Coordinator (Message Handler)

**Class:** `Coordinator: NSObject, WKScriptMessageHandler, WKNavigationDelegate`

**Message Handler Name:** `"filePicker"`

**Registered:** Before any bridge scripts are injected

**Incoming Message Format:**
```swift
{
  "action": "selectFolder",
  "id": "fp_<timestamp>_<random>"
}
```

**Handler Logic:**
1. Logs incoming message: `[WebView][ScriptMessage] name=filePicker body=...`
2. Checks for `action == "ping"` (diagnostic) → logs and returns early
3. Extracts `id` from `message.body["id"]`
4. If `id` exists:
   - Logs: `[WebView][ScriptMessage] filePicker request; requestId=<id>`
   - Calls `filePickerController.performPick(webView: webView, requestId: requestId)`
5. If `id` missing:
   - Logs: `[WebView][ScriptMessage] filePicker request but no id found`

**Debug Checks (on page load):**
- Evaluates JS to check bridge presence: `window.__starskyNative`, `window.webkit.messageHandlers.filePicker`
- Logs result: `[WebView][BridgeCheck] {"hasBridge":true,"hasMessageHandler":true}`

### 4. React Frontend Hook

**File:** `src/hooks/use-folder-picker.ts`

**Hook:** `useFolderPicker()`

**Returns:**
```typescript
{
  isNativeApp: () => boolean,
  requestFolderSelection: (timeoutMs?: number) => Promise<{path, bookmark}>
}
```

**`isNativeApp()` Check:**
- Returns `!!window.__starskyNative?.selectFolder`

**`requestFolderSelection(timeoutMs = 30000)`:**
1. Checks for `window.__starskyNative.selectFolder`
2. If available:
   - Calls `window.__starskyNative.selectFolder(timeoutMs)`
   - Awaits Promise
   - Returns `{ path: result?.path ?? null, bookmark: result?.bookmark ?? null }`
3. If not available:
   - Returns `{ path: null, bookmark: null }`
4. On error (catch):
   - Returns `{ path: null, bookmark: null }`

## Data Structures

### Promise Pending Registry

**Structure:** `window.__starskyNative._pending = { [id: string]: (result) => void }`

**Lifecycle:**
1. Created when `selectFolder()` is called
2. Stored with key = unique request ID
3. Looked up and called when native invokes `_resolveFolderPick(id, ...)`
4. Deleted after resolver is invoked or timeout reached

**Cleanup:**
- Automatic on resolution
- Automatic on timeout
- Manual if Promise is never resolved (prevents memory leak after ~30s by default)

### Bookmark Data

**Type:** `URL.bookmarkData(options: .withSecurityScope)`

**Encoding in Result:**
1. Raw bookmark bytes from `url.bookmarkData(...)`
2. Base64-encoded via `data.base64EncodedString()`
3. JSON-encoded as string for safe JS transmission
4. Passed to Promise resolver as `bookmark` field

**Usage on Frontend:**
- Frontend can store/transmit bookmark string
- Native can later resolve bookmark via `URL(bookmarkData:options:)`
- Enables app to re-access folder even after sandbox restrictions

---

## Re-opening bookmarks and token handling (backend behavior)

The mac parent process (native app) is responsible for ensuring security-scoped access for a stored folder token before launching or delegating work to any child process (for example, a bundled .NET backend). The bridge supplies a base64-encoded bookmark to the frontend; the frontend may persist that string (for example in app settings) and later provide it back to the native app. The native app implements robust handling for the token in three modes:

1. Existing bookmark file on disk
   - The manager first checks for an existing bookmark file in Application Support/<bundle>/bookmarks.
   - Supports both legacy raw token filenames (`bookmark_<token>`) and hashed filenames (`bookmark_<sha256(token)>`) for compatibility.
   - If found, the manager resolves and starts access from that file.

2. Token is actually base64 bookmark data
   - If no bookmark file exists, the manager attempts to decode the token as base64 data.
   - Several normalization strategies are tried (trim, strip newlines, URL-safe replacements, percent-decode) to be forgiving of encoding variants
   - If decoding yields valid bookmark data, the manager attempts to resolve the bookmark data (no folder open is required) and, if successful, persists the binary into the canonical bookmark file (hashed filename) and starts access from the resolved URL.

3. No bookmark available — attempt to create one from the folder path
   - If the token is not decodable and no bookmark file exists, the manager will attempt to create a bookmark from the provided `StorageFolder` path via `URL.bookmarkData(...)`.
   - Creating a bookmark requires that the calling process already has permission to open the folder (i.e. the parent must have been granted access). If the parent cannot open the folder, this step will fail with an error such as `NSCocoaErrorDomain Code=256 "Could not open() the item"`.

Notes:
- When a base64 token is provided (mode 2), the parent does not need prior access to the folder; it can resolve directly from the bookmark data and persist a usable bookmark file.
- Persisted bookmark files are saved under a hashed filename to avoid invalid filesystem characters in tokens. Loading supports both legacy and hashed filenames.
- The parent intentionally does not export the raw folder path or token to a child process via environment variables on macOS; the parent must hold the security-scoped access open while the child runs.

### New log messages (diagnostics)
- `[StorageBookmarkManager] token is not decodable base64 (len=<n>)`
- `[StorageBookmarkManager] token decoded to <n> bytes; attempting to resolve bookmark data`
- `[StorageBookmarkManager] Persisting decoded bookmark data to: <path>`
- `[StorageBookmarkManager] Persisted bookmark file size: <n>`
- `[StorageBookmarkManager] Loading bookmark from: <path>`
- `[StorageBookmarkManager] Read bookmark file size: <n>`
- `[StorageBookmarkManager] Resolved bookmark from token data to: <path>`
- `[StorageBookmarkManager] Started access for URL (from token data): <path>`
- `[StorageBookmarkManager] Attempting to create bookmark from folderPath: <path>`
- `[StorageBookmarkManager] Found existing bookmark file: <path>`

These logs help diagnose whether the token was decoded, which file path is being used, file sizes, and whether `startAccessingSecurityScopedResource()` succeeded.

## Timeout Behavior

**Default Timeout:** 30 seconds (configurable via `selectFolder(timeoutMs)`)

**Timeout Mechanism:**
- JavaScript `setTimeout` in bridge
- Checks if resolver still in `_pending[id]`
- If yes: deletes it and calls with `{ path: null, bookmark: null }`
- If no: does nothing (already resolved)

**Why Needed:**
- Prevents indefinite Promise hanging if native app crashes
- Allows frontend to show "cancelled" or "timed out" feedback

## Error Cases

| Scenario | Behavior | Result |
|----------|----------|--------|
| User cancels NSOpenPanel | `completion(nil)` in picker | JS resolves with `{path: null, bookmark: null}` |
| Native handler not registered | Immediate fallback in JS | Resolves with `{path: null, bookmark: null}` |
| postMessage throws exception | Caught in JS bridge | Resolves with `{path: null, bookmark: null}` |
| Timeout reached | Timeout handler fires | Resolves with `{path: null, bookmark: null}` |
| Multiple requests | Each has unique ID | Each resolves independently |
| Promise garbage collected | Timeout cleanup | After ~30s, resolver deleted |
| Token is base64 but invalid bookmark data | Native attempts decode/resolve; if resolve fails, falls back to create-from-path and may throw if parent lacks access | Error logged; JS receives nulls if pick failed earlier or backend logs error on launch |
| Parent lacks permission to open folder when creating a bookmark | `URL.bookmarkData(...)` will throw (e.g. `Could not open()`); native should log this and avoid exporting path to child | Parent logs error; recommendation is to re-create bookmark via UI or provide valid base64 bookmark token |

## Security Considerations

### Security-Scoped Bookmarks
- NSOpenPanel URL is wrapped in a security-scoped bookmark
- Bookmark allows app to re-access folder without UI prompt (if within same session)
- Base64-encoded and transmitted as string to frontend
- Frontend can store and send back later for re-opening

### Sandbox Isolation
- Each request has unique ID (timestamp + random)
- IDs cannot be guessed or reused
- Promise resolvers are function references (not serializable)
- No user input is evaluated as code

### XSS Protection
- All string values (path, bookmark, id) are JSON-encoded before embedding in JS
- No template literals or string concatenation with user input
- JSONEncoder handles escaping

## Testing

### Unit Tests
**File:** `mac/starskyTests/FilePickerControllerTests.swift`

**Coverage:**
- `jsForResult` produces valid JSON-encoded parameters
- Bookmark creation and base64 encoding
- Edge cases: special characters, unicode, empty paths
- Cancel handling (nil path and bookmark)

### Integration Tests
1. Build and run mac app in Xcode (Debug)
2. Open web UI with `useFolderPicker` hook
3. Call `requestFolderSelection()`
4. NSOpenPanel opens → select folder or cancel
5. Promise resolves with `{ path, bookmark }`
6. Frontend receives result correctly

### Backend/Launch Verification
- On app launch the native parent will attempt to ensure bookmark access for a stored token (if present) before starting child services.
- Verify logs show either: resolved-from-token, loaded-from-file, or attempted-create-from-path. If create-from-path is attempted and fails, parent will log `Could not open()` and recommend re-creating bookmark via UI.

### Debug Logging
- `[WebView][BridgeCheck]` → bridge presence on page load
- `[WebView][ScriptMessage]` → incoming filePicker messages with request ID
- `[WebView][FolderPickResult]` → successful JS execution
- `[WebView][EvalError]` → JS evaluation failures
- `[StorageBookmarkManager] *` → bookmark decode/load/save/resolve diagnostics
- `[WebConsole][*]` → forwarded console logs from frontend (Debug only)

## Limitations & Future Work

### Current Limitations
- macOS only (WKWebView native handler)
- Synchronous NSOpenPanel (blocks main thread during selection)
- No progress indication during large folder scans
- Bookmark may fail if app lacks sandbox entitlements

### Future Improvements
1. Async NSOpenPanel via separate thread/queue
2. Cross-platform support (Windows WebView2, Electron)
3. Multi-folder selection (array of URLs)
4. Folder preview / contents inspection before selection
5. Persistent bookmark storage and re-access API
6. Cancellation token support

## Summary

This implementation provides a **reliable, deterministic, and secure** folder picker bridge:
- ✅ Promise-based async API matching modern JavaScript patterns
- ✅ Unique request IDs prevent cross-talk and enable concurrent requests
- ✅ Timeout safety prevents hanging Promises
- ✅ Security-scoped bookmarks enable persistent folder access
- ✅ Minimal frame/event complexity — just direct resolver invocation
- ✅ Full debug visibility via NSLog and bridge logging
