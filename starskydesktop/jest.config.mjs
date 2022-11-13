import { createRequire } from 'module';
import { pathsToModuleNameMapper } from 'ts-jest';

const require = createRequire(import.meta.url);
const { compilerOptions } = require('./tsconfig.json');

/**
 * Enhance the Jest path mappings map returned from `pathsModuleNameMapper`
 * to support ES modules import syntax in TypeScript.
 *
 * @returns Jest path mappings map.
 */
function pathsToESModuleNameMapper() {
  const map = pathsToModuleNameMapper(
    compilerOptions.paths,
    { prefix: '<rootDir>' },
  );
  const esmMap = {};

  Object.entries(map).forEach((entry) => {
    const [key, val] = entry;

    if (/.*\(\.\*\)\$$/.test(key)) {
      // eslint-disable-next-line prefer-template
      const convertedKey = key.substring(0, key.length - 2)
        + '[^\\.js])(\\.js)?$';
      esmMap[convertedKey] = val;
    }
  });

  // Append the mapping for relative paths without path alias.
  esmMap['^(\\.{1,2}/.*)\\.js$'] = '$1';

  return esmMap;
}

/** @type {import('ts-jest').JestConfigWithTsJest} */
export default {
  testEnvironment: 'jsdom',
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx', 'json'],
  moduleNameMapper: pathsToESModuleNameMapper(),
  modulePathIgnorePatterns: [
    '<rootDir>/dist',
    '<rootDir>/node_modules',
    '<rootDir>/out',
  ],
  transform: {
    '^.+\\.(ts|tsx)$': [
      'ts-jest',
      {
        tsconfig: 'tsconfig.json',
      },
    ],
  },
  testMatch: [
    '**/**/*.(spec|test).([jt]s?(x))',
  ],
  testPathIgnorePatterns: ["/lib/", "/node_modules/"],
  collectCoverage: true,
  collectCoverageFrom: [
    '**/*.{ts,tsx}',
    '!**/node_modules/**',
    '!**/vendor/**',
    '!runtime-starsky-mac-x64/**',
    '!runtime-starsky-win-x64/**',
    '!runtime-starsky-mac-arm64/**',
    '!runtime-starsky-linux-x64/**',
    '!dist/**',
    '!dist-prod/**',
  ],
  errorOnDeprecated: true,
  coverageDirectory: "coverage",
  coverageReporters: ["clover", "json", "lcov", "text"],
  verbose: true,
};
