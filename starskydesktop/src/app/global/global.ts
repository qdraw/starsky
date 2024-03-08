export interface SharedSettings {
  port: number;
}

global.shared = {
  port: -1,
} as SharedSettings;

export default global;
