import type { ForgeConfig } from '@electron-forge/shared-types';
import { MakerSquirrel } from '@electron-forge/maker-squirrel';
import { MakerZIP } from '@electron-forge/maker-zip';
import { MakerDeb } from '@electron-forge/maker-deb';
import { MakerRpm } from '@electron-forge/maker-rpm';
import { VitePlugin } from '@electron-forge/plugin-vite';
import { BeforeBuild } from './src/setup/before-build';

function sleep(seconds: number) 
{
  const e = new Date().getTime() + (seconds * 1000);
  while (new Date().getTime() <= e) {
    // wait
  }
}

const config: ForgeConfig = {
  packagerConfig: {
    darwinDarkModeSupport: true,
    // disable other languages
  },
  rebuildConfig: {},
  makers: [new MakerSquirrel({}), new MakerZIP({}, ['darwin']), new MakerRpm({}), new MakerDeb({})],
  plugins: [
    new VitePlugin({
      // `build` can specify multiple entry builds, which can be Main process, Preload scripts, Worker process, etc.
      // If you are familiar with Vite configuration, it will look really familiar.
      build: [
        {
          // `entry` is just an alias for `build.lib.entry` in the corresponding file of `config`.
          entry: 'src/main/main.ts',
          config: 'vite.main.config.ts',
        },
        {
          entry: 'src/preload/preload-main.ts',
          config: 'vite.preload.config.ts',
        },
      ],
      renderer: [
        {
          name: 'main_window',
          config: 'vite.renderer.config.ts',
        },
      ],
    }),
  ],
  hooks: {
    generateAssets: async (config, platform, arch) =>{
      console.log("config.buildIdentifier " + config.buildIdentifier);
      console.log("config " + JSON.stringify(config));

      console.log("generateAssets " +  platform + arch); // darwin -arm64


    },
    packageAfterCopy: async (config, buildPath, electronVersion, platform, arch) => {
      console.log("hid");
      console.log("config " + JSON.stringify(config));
      console.log("buildPath " + buildPath);


    },
    postPackage: async (_, options) => {
      console.log("options " + JSON.stringify(options));

      console.info('Packages built at:', options.outputPaths);

      BeforeBuild(options.platform, options.arch, options.outputPaths[0]);

      sleep(4);
      process.exit(1)
    },
    postStart: async (forgeConfig, appProcess) => {      
      console.log('postStart' + forgeConfig + appProcess);
    }
  }
};

export default config;


