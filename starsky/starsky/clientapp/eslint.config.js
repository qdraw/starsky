import pluginJs from "@eslint/js";
import jestReactPlugin from "eslint-plugin-jest-react";
import prettierPlugin from "eslint-plugin-prettier";
import pluginReact from "eslint-plugin-react";
import hooksPlugin from "eslint-plugin-react-hooks";
import storybookPlugin from "eslint-plugin-storybook";
import testingLibPlugin from 'eslint-plugin-testing-library';
import globals from "globals";
import tseslint from "typescript-eslint";

/** @type {import('eslint').Linter.Config[]} */
export default [
  { files: ["**/*.{js,mjs,cjs,ts,jsx,tsx}"] },
  { languageOptions: { globals: globals.browser } },
  pluginJs.configs.recommended,
  ...tseslint.configs.recommended,
  {
    ...pluginReact.configs.flat.recommended,
    settings: {
      react: {
        version: "detect",
      },
    },
    files: ['**/*.{js,jsx,mjs,cjs,ts,tsx}'],
    plugins: {
      react: pluginReact,
      hooks: hooksPlugin,
      prettier: prettierPlugin,
      jestReact: jestReactPlugin,
      storybook: storybookPlugin,
      testing: testingLibPlugin
    },
    languageOptions: {
      parserOptions: {
        ecmaFeatures: {
          jsx: true,
        },
      },
      globals: {
        ...globals.browser,
      },
    },
    rules: {
      'react/jsx-uses-react': 'error',
      'react/jsx-uses-vars': 'error',
      '@typescript-eslint/no-explicit-any': 'warn',
      '@typescript-eslint/no-unsafe-function-type': 'warn',
      // wait for https://github.com/facebook/react/pull/30774
      //  'react-hooks/rules-of-hooks': 'error',
      //'react-hooks/exhaustive-deps': 'error'
    }
  },
  {
    ignores: [
      "**/build/**",
      "**/.storybook/middleware.js",
      'tsconfig.json',
    ]
  }
];