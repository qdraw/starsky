export interface ITemplateContentTypeProperties {
  sourceMaxWidth: boolean;
  overlayMaxWidth: boolean;
  overlayFullPath: boolean;
  path: boolean;
  prepend: boolean;
  template: boolean;
  copy: boolean;
  optimizers: boolean;
  folder: boolean;
  append: boolean;
  metaData: boolean;
}

export interface ITemplateContentType {
  id: number;
  type: string;
  properties: ITemplateContentTypeProperties;
}

export interface IAppSettingsPublishProfileOptimizer {
  imageFormats?: string[];
  id?: string;
  enabled?: boolean;
  options?: Record<string, unknown>;
}

export interface IAppSettingsPublishProfileItem {
  contentType: string;
  sourceMaxWidth?: number;
  overlayMaxWidth?: number;
  overlayFullPath?: string;
  path?: string;
  prepend?: string;
  template?: string;
  copy?: boolean;
  optimizers?: IAppSettingsPublishProfileOptimizer[];
  folder?: string;
  append?: string;
  metaData?: boolean;
}

export type IAppSettingsPublishProfiles = Record<string, IAppSettingsPublishProfileItem[]>;
