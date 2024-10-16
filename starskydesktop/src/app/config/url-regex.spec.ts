import { ipRegex } from "./url-regex";

describe("ipRegex", () => {
  test("Should match valid URLs with IP addresses", () => {
    expect(ipRegex.test("https://255.255.255.255")).toBe(true);
  });

  test("Should not match invalid URLs with IP addresses", () => {
    expect(ipRegex.test("http://192.168.0.1")).toBe(false);
    expect(ipRegex.test("http://123.45.67.89:8080")).toBe(true);
    expect(ipRegex.test("http://0.0.0.0:12345/path")).toBe(false);

    expect(ipRegex.test("http://300.168.0.1")).toBe(false); // Out of range octet
    expect(ipRegex.test("https://192.168.1")).toBe(false); // Incomplete IP address
    expect(ipRegex.test("http://192.168.0.1.")).toBe(false); // Extra dot at the end
    expect(ipRegex.test("http://192.168.0.1:")).toBe(true); // Colon without port number
    expect(ipRegex.test("http://192.168.0.1/path?query")).toBe(false); // Path and query string
  });

  test("Should match valid URLs with domain names", () => {
    expect(ipRegex.test("http://example.com")).toBe(false);
    expect(ipRegex.test("https://sub.domain.com")).toBe(false);
    expect(ipRegex.test("http://www.example123.net:8080")).toBe(false);
    expect(ipRegex.test("http://www.example.com/path")).toBe(false);
  });

  test("Should not match invalid URLs with domain names", () => {
    expect(ipRegex.test("http://example")).toBe(false); // No top-level domain
    expect(ipRegex.test("https://.example.com")).toBe(false); // Empty subdomain
    expect(ipRegex.test("http://example..com")).toBe(false); // Double dot in domain name
    expect(ipRegex.test("http://example.com:")).toBe(false); // Colon without port number
    expect(ipRegex.test("http://example.com/path?query")).toBe(false); // Path and query string
  });
});
