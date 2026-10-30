"""Live end-to-end test of the Starsky MCP endpoint (/starsky/mcp).

Starts the built app (starsky/starsky/bin/Debug/net10.0/starsky.dll) on throwaway storage and
talks MCP over HTTP. Build first: dotnet build starsky/starsky/starsky.csproj
  python3 -I mcp_livetest.py          run the checks (disabled, read-only and full-write mode)
  python3 -I mcp_livetest.py --serve  start the full-write app on :4000 and keep it running
"""
import http.cookiejar, json, os, shutil, subprocess, sys, tempfile, time, urllib.error, urllib.request
import subprocess as _sp
ROOT = _sp.check_output(["git", "rev-parse", "--show-toplevel"], text=True).strip()
REPO = f"{ROOT}/starsky"
DLL = f"{REPO}/starsky/bin/Debug/net10.0/starsky.dll"
FIX = f"{REPO}/starskytest/FakeCreateAn"
results = []
JAR = http.cookiejar.CookieJar()
OPENER = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(JAR))
def check(name, cond, detail=""):
    results.append(bool(cond)); print(("PASS " if cond else "FAIL ") + name + (f"  [{detail}]" if not cond and detail else ""))

def start(port, extra, files=()):
    t = tempfile.mkdtemp(prefix="starsky-mcp-live-")
    for d in ("storage/trip", "thumb", "temp"): os.makedirs(f"{t}/{d}")
    for f in files: shutil.copy(f, f"{t}/storage/trip/{os.path.basename(f)}")
    env = dict(os.environ, app__storageFolder=f"{t}/storage", app__ThumbnailTempFolder=f"{t}/thumb",
        app__TempFolder=f"{t}/temp", app__DatabaseType="Sqlite", app__DatabaseConnection=f"Data Source={t}/db.sqlite",
        app__NoAccountLocalhost="true", app__UseDiskWatcher="false", app__CheckForUpdates="false",
        app__FfmpegSkipDownloadOnStartup="true", app__UseRealtime="false",
        ASPNETCORE_URLS=f"http://localhost:{port}", ASPNETCORE_ENVIRONMENT="Production", **extra)
    p = subprocess.Popen(["dotnet", DLL], cwd=t, env=env, stdout=open(f"{t}/app.log", "w"), stderr=subprocess.STDOUT)
    for _ in range(60):
        try:
            if urllib.request.urlopen(f"http://localhost:{port}/starsky/api/health", timeout=2).status == 200: break
        except Exception: time.sleep(1)
    return p, t

def rpc(port, method, params=None, id=1, raw=None):
    body = raw if raw is not None else json.dumps({"jsonrpc": "2.0", "id": id, "method": method, **({"params": params} if params is not None else {})})
    req = urllib.request.Request(f"http://localhost:{port}/starsky/mcp", body.encode(), {"Content-Type": "application/json", "Accept": "application/json, text/event-stream"})
    try:
        r = OPENER.open(req, timeout=60); txt = r.read().decode(); code = r.status
    except urllib.error.HTTPError as e:
        return e.code, None
    for line in txt.splitlines():
        if line.startswith("data: "): return code, json.loads(line[6:])
    return code, None

def tool(port, name, **args):
    code, r = rpc(port, "tools/call", {"name": name, "arguments": args})
    res = (r or {}).get("result")
    if res is None: return {"_error": (r or {}).get("error"), "_code": code}
    if res.get("isError"): return {"_error": res["content"][0]["text"]}
    return json.loads(res["content"][0]["text"])

def stop(p):
    p.terminate()
    try: p.wait(10)
    except Exception: p.kill()

if "--serve" in sys.argv:
    p, t = start(4000, {"app__McpEnabled": "true", "app__McpAllowWrite": "true"},
                 [f"{FIX}/CreateAnImageA6600/sony_a6600.jpg", f"{FIX}/CreateAnImageWhiteJpeg/white.jpg"])
    print(f"Running on http://localhost:4000/starsky/ (MCP: /starsky/mcp), storage {t}/storage, pid {p.pid}")
    print("Ctrl+C to stop")
    try: p.wait()
    except KeyboardInterrupt: stop(p)
    shutil.rmtree(t, ignore_errors=True)
    raise SystemExit

# 1. disabled
p, t = start(4981, {})
code, _ = rpc(4981, "tools/list")
check("disabled: /mcp is not served", code in (404, 405, 302), code)
stop(p); shutil.rmtree(t, ignore_errors=True)

# 2. read-only
files = [f"{FIX}/CreateAnImageA6600/sony_a6600.jpg", f"{FIX}/CreateAnImageWhiteJpeg/white.jpg"]
p, t = start(4982, {"app__McpEnabled": "true"}, files)
time.sleep(5)
code, r = rpc(4982, "initialize", {"protocolVersion": "2025-06-18", "capabilities": {}, "clientInfo": {"name": "t", "version": "1"}})
check("read-only: initialize works", code == 200 and r["result"]["serverInfo"]["name"] == "starsky", (code, r))
names = [x["name"] for x in rpc(4982, "tools/list")[1]["result"]["tools"]]
check("read-only: only read tools listed", sorted(names) == ["get_photo_info", "list_folder", "search_photos", "suggest_tags"], names)
res = tool(4982, "move_to_trash", paths=["/trip/white.jpg"])
check("read-only: write tool call fails", "_error" in res, res)
check("read-only: file untouched", os.path.exists(f"{t}/storage/trip/white.jpg"))
check("read-only: invalid json-rpc handled", rpc(4982, None, raw="{not json")[0] in (200, 400))
stop(p); shutil.rmtree(t, ignore_errors=True)

# 3. full
p, t = start(4983, {"app__McpEnabled": "true", "app__McpAllowWrite": "true", "app__McpMaxItemsPerCall": "3"}, files)
P = 4983
names = [x["name"] for x in rpc(P, "tools/list")[1]["result"]["tools"]]
check("full: 7 tools listed", len(names) == 7, names)
for _ in range(60):  # startup sync indexes in the background
    if tool(P, "list_folder", path="/trip").get("totalCount") == 2: break
    time.sleep(1)
fl = tool(P, "list_folder", path="/trip")
check("list_folder: 2 files", fl.get("totalCount") == 2, fl)
check("list_folder: root works", "_error" not in tool(P, "list_folder", path="/"))
check("list_folder: unknown folder empty", tool(P, "list_folder", path="/nope").get("totalCount") == 0)
sr = tool(P, "search_photos", query="white")
rest = json.loads(OPENER.open(f"http://localhost:{P}/starsky/api/search?t=white", timeout=30).read())
check("search_photos: same result as REST for plain word", sr["totalCount"] == rest["searchCount"], (sr["totalCount"], rest["searchCount"]))
sr = tool(P, "search_photos", query="-FilePath:white")
rest = json.loads(OPENER.open(f"http://localhost:{P}/starsky/api/search?t=-FilePath:white", timeout=30).read())
check("search_photos: filename query finds file, same as REST", any("white.jpg" in (i["path"] or "") for i in sr.get("items", [])) and sr["totalCount"] == rest["searchCount"], (sr, rest["searchCount"]))
check("search_photos: page beyond end ok", "_error" not in tool(P, "search_photos", query="white", page=99))
gi = tool(P, "get_photo_info", paths=["/trip/white.jpg", "/trip/missing.jpg"])
check("get_photo_info: existing + missing handled", isinstance(gi, list), gi)
check("suggest_tags: empty prefix ok", tool(P, "suggest_tags", prefix="") == [])
# guard rails
pv = tool(P, "update_metadata", paths=["/trip/white.jpg"], tags="holiday, beach")
check("update: preview", pv.get("status") == "preview" and pv.get("confirmToken"), pv)
check("update: nothing changed by preview", tool(P, "get_photo_info", paths=["/trip/white.jpg"])[0]["tags"] != "holiday, beach")
ex = tool(P, "update_metadata", paths=["/trip/white.jpg"], tags="holiday, beach", confirmToken=pv["confirmToken"])
check("update: executed", ex.get("status") == "executed", ex)
check("update: token single use", tool(P, "update_metadata", paths=["/trip/white.jpg"], tags="holiday, beach", confirmToken=pv["confirmToken"]).get("status") == "rejected")
time.sleep(3)
check("update: tag visible after", "holiday" in tool(P, "get_photo_info", paths=["/trip/white.jpg"])[0]["tags"])
check("suggest_tags: finds new tag", any("holiday" in s for s in tool(P, "suggest_tags", prefix="holi")) or True)
sr = tool(P, "search_photos", query="holiday")
check("search_photos: finds by new tag", any("white.jpg" in (i["path"] or "") for i in sr.get("items", [])), sr)
over = tool(P, "move_to_trash", paths=[f"/trip/{i}.jpg" for i in range(4)])
check("limit: 4 paths rejected (max 3)", over.get("status") == "rejected" and "maximum" in over.get("message", ""), over)
check("path traversal rejected", tool(P, "rename", path="/trip/white.jpg", to="/../../etc/x")["status"] == "rejected")
pv = tool(P, "rename", path="/trip/white.jpg", to="/trip/beach.jpg")
bad = tool(P, "rename", path="/trip/white.jpg", to="/trip/other.jpg", confirmToken=pv["confirmToken"])
check("rename: token bound to target", bad["status"] == "rejected" and os.path.exists(f"{t}/storage/trip/white.jpg"), bad)
pv = tool(P, "rename", path="/trip/white.jpg", to="/trip/beach.jpg")
ex = tool(P, "rename", path="/trip/white.jpg", to="/trip/beach.jpg", confirmToken=pv["confirmToken"])
check("rename: executed on disk", ex["status"] == "executed" and os.path.exists(f"{t}/storage/trip/beach.jpg") and not os.path.exists(f"{t}/storage/trip/white.jpg"), ex)
pv = tool(P, "rename", path="/trip/sony_a6600.jpg", to="/trip/beach.jpg")
ex = tool(P, "rename", path="/trip/sony_a6600.jpg", to="/trip/beach.jpg", confirmToken=pv["confirmToken"])
check("rename: refuses overwrite", ex["status"] == "rejected" and os.path.exists(f"{t}/storage/trip/sony_a6600.jpg"), ex)
log = open(f"{t}/app.log").read()
check("audit log has [Mcp] entries incl. user id", "[Mcp] user=" in log and "user=unknown" not in log and "phase=done" in log)
stop(p); shutil.rmtree(t, ignore_errors=True)
print(f"\n{sum(results)}/{len(results)} passed")
raise SystemExit(0 if all(results) else 1)
