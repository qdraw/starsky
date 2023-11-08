# Security Header Guidance

## Summary

The HTTP protocol supports a set of security headers to control certain aspects of (browser) behaviour. Below is a summary of the most important security headers and their suggested/required value. 

There are sites like https://securityheaders.io/ and https://internet.nl/ where you can scan your site to see how well it scores. More details on scanning/monitoring below.

The links in the table below also provide examples on how to set these headers in popular web servers like Apache and nginx .

This page is inspired by the [OWASP Secure Headers Project](https://owasp.org/www-project-secure-headers/#configuration-proposal) and their [Configuration Proposal](https://owasp.org/www-project-secure-headers/#configuration-proposal). We will make more of those mandatory for iO over time, so please up to speed with the below headers.

Please make sure headers are not set multiple times, as this can lead to unpredictable results.
## **Mandatory headers**

|**Purpose**|**Header**|**Explanation**|**Recommended value**|
| :-: | :-: | :-: | :-: |
|**Enforce HTTPS/TLS**|Strict-Transport-Security|[HTTP Strict Transport Security](https://scotthelme.co.uk/hsts-the-missing-link-in-tls/) forces browsers to always and only access you domain vita HTTPS/TLS.|<p>max-age=31536000 </p><p></p><p>If possible, use:</p><p>max-age=31536000; includeSubDomains</p>|
|<p>**Prevent Clickjacking**</p><p></p>|<p>X-Frame-Options</p><p></p>|<p>[X-Frame-Options](https://scotthelme.co.uk/hardening-your-http-response-headers/#x-frame-options) tells the browser whether you want to allow your site to be framed or not. By preventing a browser from framing your site you can defend against attacks like clickjacking. It's common to only allow framing of your site from your own site.</p><p>This header will be deprecated at some point, so please set content-security-policy: frame-ancestors 'self'  as mentioned next.</p>|SAMEORIGIN|
||Content-Security-Policy|<p>[Content-Security-Policy: frame-ancestors](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy/frame-ancestors) header is the successor of the (deprecated) X-Frame-Options header.</p><p>For now only the **frame-ancestors** directive is mandatory, the rest of CSP is not easily implemented because it's whitelist based and easily bypassable.</p><p>In the future we may advise on implementing strict-dynamic , which is not yet easily supported by all webservers/webframeworks.</p><p></p>|frame-ancestors self|
|**Prevent XSS filter abuse**|X-XSS-Protection|The XSS Protection filters implemented in browsers turned out to provide an extra attack vector for attackers due to bugs in their implementation. The recommended value is now to turn off XSS Protection, or to not set the header at all.|<p>do not set, or </p><p>0 </p>|
|**Prevent content type abuse**|X-Content-Type-Options|[X-Content-Type-Options](https://scotthelme.co.uk/hardening-your-http-response-headers/#x-content-type-options) stops a browser from trying to MIME-sniff the content type and forces it to stick with the declared content-type.|nosniff |
|**Prevent information leakage**|Referrer-Policy|[Referrer Policy](https://scotthelme.co.uk/a-new-security-header-referrer-policy/) is a header that allows a site to control how much information the browser includes with navigations away from a document and should be set by all sites.|strict-origin-when-cross-origin|
## **Optional headers**

|**Purpose**|**Header**|**Explanation**|**Recommended value**|
| :-: | :-: | :-: | :-: |
|**Limit impact of XSS**|Content-Security-Policy|[Content Security Policy](https://scotthelme.co.uk/content-security-policy-an-introduction/) is an effective measure to protect your site from XSS attacks. By whitelisting sources of approved content, you can prevent the browser from loading malicious assets.||
||Permissions-Policy|<p>The [Permissions Policy](https://scotthelme.co.uk/goodbye-feature-policy-and-hello-permissions-policy/) controls which browser features are allowed to be used by your site. For example:</p><p>- Battery status</p><p>- Client Hints</p><p>- Encrypted-media decoding</p><p>- Fullscreen</p><p>- Geolocation</p><p>- Picture-in-picture</p><p>- Sensors: Accelerometer, Ambient Light Sensor, Gyroscope, Magnetometer</p><p>- User media: Camera, Microphone</p><p>- Video Autoplay</p><p>- Web Payment Request</p><p>- WebMIDI</p><p>- WebUSB</p><p>- WebXR</p><p>There is no single valid recommended value as it differs per web application which features are needed. Of course the safest value is none  to block access to all features, but that would prevent things like fullscreen  and picture in picture .</p>|choose appropiately|
## **Deprecated headers**

|**Purpose**|**Header**|**Explanation**|**Recommended value**|
| :-: | :-: | :-: | :-: |
|**Prevent Man-in-the-middle attacks**|Public-Key-Pins|[HTTP Public Key Pinning](https://scotthelme.co.uk/hpkp-http-public-key-pinning/) protects your site from MiTM attacks using rogue X.509 certificates. By whitelisting only the identities that the browser should trust, your users are protected in the event a certificate authority is compromised. Browser have stopped supporting this header as the focus shifted to Certificate Transparancy.|deprecated in browsers, but still required by some clients in mobile apps|
|**Validate HTTPS/TLS certificats**|Expect-CT|The [Expect-CT](https://owasp.org/www-project-secure-headers/#expect-ct)  header is used by a server to indicate that browsers should evaluate connections to the host for [Certificate Transparency](https://certificate.transparency.dev/) compliance. This headers is deprecated as since May 2018 new certificates are expected to support SCTs by default, which is checked by modern browsers.|deprecated, do not use|

Sites:

- https://securityheaders.com/
- https://internet.nl/
- https://dev.ssllabs.com/ssltest/analyze.html
- https://chrome.google.com/webstore/detail/security-header-extension/nggplilppojikmgpmlecpcikpoiffinp

Scripts:

- https://github.com/santoru/shcheck

For the Content-Security-Policy (CSP) it is a lot more complicated to check. You can use these to check the CSP:

- https://csp-evaluator.withgoogle.com/
