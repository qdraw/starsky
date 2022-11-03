/* eslint-disable class-methods-use-this */
export default class UrlQuery {
  public HealthApi(): string {
    return "/api/health";
  }

  public HealthShouldContain(): string {
    return "ealth"; // allow Unhealthy or Healthy (case sensitive)
  }

  public HealthVersionApi(version: string): string {
    return `/api/health/version?version=${version}`;
  }

  public HealthCheckForUpdates(version: string) {
    return `/api/health/check-for-updates?currentVersion=${version}`;
  }

  public Index(filePath: string) {
    return `/starsky/api/index?f=${filePath}`;
  }

  public DownloadSidecarFile(sideCarSubPath: string) {
    return `/starsky/api/download-sidecar?f=${sideCarSubPath}`;
  }

  public DownloadPhoto(subPath: string) {
    return `/starsky/api/download-photo?isThumbnail=false&f=${subPath}&cache=false`;
  }
}
