# Websockets

Occasionally Starsky functions may have trouble performing when Starsky is not allowed access. This can happen due to issues or limitations applied to your network connection or the environment that you use. The following article lists the most common causes of such issues.

## WebSockets 

The Starsky application requires WebSocket connections. If you're having issues updateing images doen't work as expected this might mean that your connection does not support WebSockets. 

To test your connection, [please open this website.](http://websocketstest.com/)

If the Websockets are identified, you'll see the following message:

wensocket_connection.jpg

If the result is different, it's most likely that there's something in your network that blocks WebSocket connections. If this is the case try the following:

-    Use a different network connection
 -   Use or turn off a VPN
 -   If you use a corporate connection contact your network administrator and ask them to enable the WebSocket connections on port 80 and 443 (SSL). They might be closed or filtered within your corporate network for security reasons. In order to establish a connection, these ports should be open for Starsky addresses to access (see addresses in the "If you use a Firewall" section below)

If the Websockets are identified correctly but there are still issues establishing the connection, you could ask help in the community.

## If you use a Firewall

Please allowlist the domains and subdomains you are using.

## If you use a proxy 

Please ensure that you provide the application with a by-pass. The following specifications will help.

-    The proxy server must support WebSocket connections (HTTP/2). 
-    The proxy HTTP version should be set as 1.1.
-    Source IP/host: see the NAT IPs above (used for Atlassian integrations only).
-    Source port: 80. 80 is used for users that access Starsky through HTTP to direct them to HTTPS (blocking 80 is not recommended).
-    Destination port: 443 (SSL). 443 is used for HTTPS.
-    Protocol: HTTPS
-    TLS: 1.2. (depends on your configuration but this is common).
-    The timeout value on the proxy server should be prolonged. It is most likely that your system waits around 60-90 seconds to connect. It would be best to prolong it to 120-180 seconds.
-    The proxy server should not truncate the request and response headers. Please check if the Upgrade and Connection headers are proxied by the client.

