import js from '@eslint/js';
import nextPlugin from '@next/eslint-plugin-next';
import boundaries from 'eslint-plugin-boundaries';
import checkFile from 'eslint-plugin-check-file';
import jsxA11y from 'eslint-plugin-jsx-a11y';
import react from 'eslint-plugin-react';
import { defineConfig } from 'eslint/config';
import tseslint from 'typescript-eslint';

export default defineConfig(
  {
    ignores: [
      '**/node_modules/**',
      '**/dist/**',
      '**/.next/**',
      '**/.turbo/**',
      'backend/**',
      'docs/**',
      '**/coverage/**',
      'packages/api-client/src/client/**',
    ],
  },
  js.configs.recommended,
  ...tseslint.configs.strictTypeChecked,
  jsxA11y.flatConfigs.recommended,
  {
    languageOptions: {
      parserOptions: {
        projectService: { allowDefaultProject: ['eslint.config.js'] },
        tsconfigRootDir: import.meta.dirname,
      },
    },
    plugins: { '@next/next': nextPlugin, 'check-file': checkFile, boundaries, react },
    settings: {
      react: { version: 'detect' },
      'boundaries/elements': [
        { type: 'feature', pattern: 'apps/*/src/features/*', capture: ['feature'] },
      ],
    },
    rules: {
      ...nextPlugin.configs['core-web-vitals'].rules,
      'no-console': 'error',
      'no-debugger': 'error',
      eqeqeq: 'error',
      '@typescript-eslint/no-explicit-any': 'error',
      '@typescript-eslint/ban-ts-comment': [
        'error',
        { 'ts-ignore': true, 'ts-expect-error': 'allow-with-description' },
      ],
      '@typescript-eslint/no-floating-promises': 'error',
      '@typescript-eslint/naming-convention': [
        'error',
        { selector: 'typeLike', format: ['PascalCase'] },
        {
          selector: 'variable',
          types: ['boolean'],
          format: ['PascalCase'],
          prefix: ['is', 'has', 'can'],
        },
      ],
      'no-restricted-globals': ['error', 'localStorage', 'sessionStorage'],
      'no-restricted-imports': ['error', { paths: ['axios', 'pg', 'dapper'] }],
      'boundaries/element-types': [
        'error',
        {
          default: 'allow',
          rules: [
            {
              from: 'feature',
              disallow: [['feature', { feature: '!${from.feature}' }]],
              message: 'features/* không import chéo features khác',
            },
          ],
        },
      ],
      'react/no-danger': 'error',
      'check-file/filename-naming-convention': [
        'error',
        { '**/*.{ts,tsx}': 'KEBAB_CASE' },
        { ignoreMiddleExtensions: true },
      ],
      'no-warning-comments': ['error', { terms: ['todo', 'fixme'], location: 'start' }],
    },
  },
  { files: ['**/*.js'], extends: [tseslint.configs.disableTypeChecked] },
);
