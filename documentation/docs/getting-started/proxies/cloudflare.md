# Using Cloudflare as Reverse Proxy

> **Info** <br/>
    Starsky only trusts the `X-Forwarded-For`/`X-Forwarded-Proto` headers from proxies you
    explicitly configure (see [`TrustedProxiesCloudflare`](#trustedproxiescloudflare-setting-recommended)
    below). Without this,
    putting Starsky behind Cloudflare without any config at all is still safe — Starsky simply
    falls back to trusting loopback only, so forwarded headers from Cloudflare are ignored and
    rate limiting keys on Cloudflare's own edge IP instead of the real visitor. The downside is
    every visitor then shares one rate-limit bucket (Cloudflare's edge IP), so set
    `TrustedProxiesCloudflare` (or `TrustedProxies`) to get accurate per-visitor rate limiting and `NoAccountLocalhost` behavior.

## `TrustedProxiesCloudflare` setting (recommended)

Instead of copying Cloudflare's ranges into config by hand, set a single flag and Starsky adds
them for you (see `CloudflareTrustedProxiesHelper` in `starsky.foundation.platform`):

```json
{
  "app": {
    "TrustedProxiesCloudflare": "true"
  }
}
```

Or as an environment variable: `app__TrustedProxiesCloudflare=true`.

This has the same effect as listing every range below under `TrustedProxies` manually, but it's
kept up to date in code instead of your config, and you can still add your own entries to
`TrustedProxies` alongside it (e.g. an internal load balancer in front of Cloudflare).

> **Warning** <br/>
    As soon as `TrustedProxiesCloudflare` is enabled or `TrustedProxies` contains any entry, **only**
    the listed addresses are trusted. The default trust of loopback (`127.0.0.1`/`::1`) is dropped.
    If a `cloudflared` tunnel or a reverse proxy runs on the same host as Starsky, add `127.0.0.1`
    and `::1` to `TrustedProxies` yourself (`app__TrustedProxies__0=127.0.0.1`,
    `app__TrustedProxies__1=::1`).
    Entries must be IP addresses or CIDR ranges; hostnames are not supported. Invalid entries are
    skipped and only logged to the console (`app__TrustedProxies: ignoring invalid entry`).

## `TrustedProxies` setting (manual list)

If you'd rather manage the list yourself, add Cloudflare's published edge IP ranges to
`TrustedProxies` directly so Starsky trusts the `X-Forwarded-For` header Cloudflare sets, and
uses the real visitor IP for rate limiting (`app__UseRateLimiting`) instead of Cloudflare's own
address.

`appsettings.json`:

```json
{
  "app": {
    "TrustedProxies": [
      "173.245.48.0/20",
      "103.21.244.0/22",
      "103.22.200.0/22",
      "103.31.4.0/22",
      "141.101.64.0/18",
      "108.162.192.0/18",
      "190.93.240.0/20",
      "188.114.96.0/20",
      "197.234.240.0/22",
      "198.41.128.0/17",
      "162.158.0.0/15",
      "104.16.0.0/13",
      "104.24.0.0/14",
      "172.64.0.0/13",
      "131.0.72.0/22",
      "2400:cb00::/32",
      "2606:4700::/32",
      "2803:f800::/32",
      "2405:b500::/32",
      "2405:8100::/32",
      "2a06:98c0::/29",
      "2c0f:f248::/32"
    ]
  }
}
```

Or as environment variables, one per range:

```
app__TrustedProxies__0=173.245.48.0/20
app__TrustedProxies__1=103.21.244.0/22
app__TrustedProxies__2=103.22.200.0/22
app__TrustedProxies__3=103.31.4.0/22
app__TrustedProxies__4=141.101.64.0/18
app__TrustedProxies__5=108.162.192.0/18
app__TrustedProxies__6=190.93.240.0/20
app__TrustedProxies__7=188.114.96.0/20
app__TrustedProxies__8=197.234.240.0/22
app__TrustedProxies__9=198.41.128.0/17
app__TrustedProxies__10=162.158.0.0/15
app__TrustedProxies__11=104.16.0.0/13
app__TrustedProxies__12=104.24.0.0/14
app__TrustedProxies__13=172.64.0.0/13
app__TrustedProxies__14=131.0.72.0/22
app__TrustedProxies__15=2400:cb00::/32
app__TrustedProxies__16=2606:4700::/32
app__TrustedProxies__17=2803:f800::/32
app__TrustedProxies__18=2405:b500::/32
app__TrustedProxies__19=2405:8100::/32
app__TrustedProxies__20=2a06:98c0::/29
app__TrustedProxies__21=2c0f:f248::/32
```

**These ranges change occasionally.** Don't treat the list above as permanent — check it
against [cloudflare.com/ips-v4](https://www.cloudflare.com/ips-v4) and
[cloudflare.com/ips-v6](https://www.cloudflare.com/ips-v6) and update your config when they do.

## Lock down the origin

Trusting Cloudflare's IPs for the forwarded header is not enough on its own. If your origin
server is also reachable directly on the internet by its real IP, anyone can skip Cloudflare
entirely and send a request straight to the origin with a forged `X-Forwarded-For` header —
bypassing the rate limiter the same way the trusted-proxy check was meant to prevent.

Pick one of:

- **Firewall the origin** so only Cloudflare's IP ranges (the same list above) may reach the
  port Starsky listens on. Most hosting providers and cloud firewalls support this.
- **Cloudflare Tunnel** (`cloudflared`) — the origin makes an outbound connection to Cloudflare,
  so it never has a publicly reachable address at all.
- **Authenticated Origin Pulls** — Cloudflare presents a client certificate on every request to
  the origin; reject connections that don't present it.

### Why Use a Proxy? ###

If you install Starsky on a public server outside your home network, **always run it behind a
secure HTTPS reverse proxy**. Your files and passwords will otherwise be transmitted in clear
text and can be intercepted by anyone, including your provider, hackers, and governments. Backup
tools and file sync apps may refuse to connect as well.
