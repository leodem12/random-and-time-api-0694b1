import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: '.',
  retries: 1,
  use: {
    baseURL: 'http://app:5000',
    screenshot: 'on',
    actionTimeout: 15000,
  },
  projects: [
    {
      name: 'chromium',
      use: { browserName: 'chromium' },
    },
  ],
});
