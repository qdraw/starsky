export interface SharedSettings {
  port: number;
}

global.shared = {
  port: 9609,
} as SharedSettings;

export default global;
