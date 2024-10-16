import globals from "globals";
import pluginJs from "@eslint/js";
import tseslint from "typescript-eslint";


export default [
  {files: ["**/*.{js,mjs,cjs,ts}"]},
  {languageOptions: { globals: {...globals.browser, ...globals.node} }},
  pluginJs.configs.recommended,
  ...tseslint.configs.recommended,
  {
    rules: {
      'max-len': ['off'],
      quotes: ['off'],
      '@typescript-eslint/quotes': ['off'],
      'import/extensions': ['off'],
      'arrow-body-style': ['off'],
      '@typescript-eslint/no-unsafe-call': ['off'],
      'import/order': ['off'],
      'import/no-import-module-exports': ['off'],
      'import/no-extraneous-dependencies': ['off'],
      'import/prefer-default-export': ['off'],
      '@typescript-eslint/no-unsafe-return': ['off'],
      'no-console': ['off'],
      '@typescript-eslint/comma-dangle': ['off'],
      'no-unused-vars': ['warn', { argsIgnorePattern: '^_' }],
      '@typescript-eslint/no-explicit-any': ['off'],
      'prefer-promise-reject-errors': ['off'],
      'linebreak-style': ['off'],
    }
  }
];