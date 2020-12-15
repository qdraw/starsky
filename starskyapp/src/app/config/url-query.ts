export default class UrlQuery {
  public HealthApi(): string {
    return "/api/health";
  }
  public HealthShouldContain(): string {
    return "Health";
  }
  public HealthVersionApi(): string {
    return "/api/health/version";
  }

  public HealthCheckForUpdates(version: string) {
    return "/api/health/check-for-updates?currentVersion=" + version;
  }

  public Index(filePath: string) {
    return "/starsky/api/index?f=" + filePath;
  }
}
