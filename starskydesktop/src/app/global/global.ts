export interface SharedSettings {
  port: number;
  remote: boolean | undefined;
  baseUrl: string;
}

global.shared = {
  port: -1,
  remote: undefined,
  baseUrl: ``,
} as SharedSettings;
export default global;
