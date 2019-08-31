export class BreadcrumbConvert {

  public getParent = (breadcrumb: string[]): string => {

    if (breadcrumb.length >= 1) return "" + breadcrumb[breadcrumb.length - 1];
    return "";
  }
}