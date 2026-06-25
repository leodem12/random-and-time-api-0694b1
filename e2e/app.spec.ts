import { test, expect } from '@playwright/test';

test.describe('Random & Time API — UI acceptance scenarios', () => {

  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('mat-toolbar')).toBeVisible();
  });

  // S1 / happy_path step 1: UI loads with four buttons
  test('UI shows four action buttons on load', async ({ page }) => {
    await expect(page.getByRole('button', { name: 'get-random', exact: true })).toBeVisible();
    await expect(page.getByRole('button', { name: 'get-random-history', exact: true })).toBeVisible();
    await expect(page.getByRole('button', { name: 'get-now', exact: true })).toBeVisible();
    await expect(page.getByRole('button', { name: 'get-now-history', exact: true })).toBeVisible();
    await page.screenshot({ path: 'test-results/01-four-buttons.png' });
  });

  // S1 acceptance: clicking get-random shows a random number
  test('S1 — get-random button displays a random number', async ({ page }) => {
    await page.getByRole('button', { name: 'get-random', exact: true }).click();
    const card = page.locator('mat-card').first();
    await expect(card).toBeVisible({ timeout: 10000 });
    const valueText = await page.locator('mat-card-content p', { hasText: 'Value:' }).textContent();
    expect(valueText).toMatch(/Value:\s*\d+/);
    await page.screenshot({ path: 'test-results/02-get-random-result.png' });
  });

  // S2 acceptance: get-random-history shows history list including just-generated number
  test('S2 — get-random-history shows history after generating a number', async ({ page }) => {
    await page.getByRole('button', { name: 'get-random', exact: true }).click();
    const card = page.locator('mat-card').first();
    await expect(card).toBeVisible({ timeout: 10000 });

    const valueText = await page.locator('mat-card-content p', { hasText: 'Value:' }).textContent();
    const match = valueText?.match(/Value:\s*(\d+)/);
    const generatedValue = match ? match[1] : null;
    expect(generatedValue).toBeTruthy();

    await page.getByRole('button', { name: 'get-random-history', exact: true }).click();
    const historyCard = page.locator('mat-card').first();
    await expect(historyCard).toBeVisible({ timeout: 10000 });

    const historyContent = await page.locator('mat-card-content').textContent();
    expect(historyContent).toContain(generatedValue!);
    await page.screenshot({ path: 'test-results/03-random-history-with-value.png' });
  });

  // S2 acceptance: empty history shows no-records message (not an error)
  test('S2 — get-random-history shows empty state without error when no records exist', async ({ page }) => {
    await page.getByRole('button', { name: 'get-random-history', exact: true }).click();
    const card = page.locator('mat-card').first();
    await expect(card).toBeVisible({ timeout: 10000 });
    const errorCard = page.locator('mat-card.error-card');
    const errorCount = await errorCard.count();
    expect(errorCount).toBe(0);
    await page.screenshot({ path: 'test-results/04-random-history-no-error.png' });
  });

  // S3 acceptance: get-now shows server time (UTC)
  test('S3 — get-now button displays current server time', async ({ page }) => {
    await page.getByRole('button', { name: 'get-now', exact: true }).click();
    const card = page.locator('mat-card').first();
    await expect(card).toBeVisible({ timeout: 10000 });
    const timeText = await page.locator('mat-card-content p', { hasText: 'Server Time (UTC):' }).textContent();
    expect(timeText).toMatch(/Server Time \(UTC\):\s*\d{4}-\d{2}-\d{2}/);
    await page.screenshot({ path: 'test-results/05-get-now-result.png' });
  });

  // S4 acceptance: get-now-history shows history list
  test('S4 — get-now-history shows time history after fetching server time', async ({ page }) => {
    await page.getByRole('button', { name: 'get-now', exact: true }).click();
    const card = page.locator('mat-card').first();
    await expect(card).toBeVisible({ timeout: 10000 });

    const timeText = await page.locator('mat-card-content p', { hasText: 'Server Time (UTC):' }).textContent();
    const dateMatch = timeText?.match(/(\d{4}-\d{2}-\d{2})/);
    const datePart = dateMatch ? dateMatch[1] : null;
    expect(datePart).toBeTruthy();

    await page.getByRole('button', { name: 'get-now-history', exact: true }).click();
    const historyCard = page.locator('mat-card').first();
    await expect(historyCard).toBeVisible({ timeout: 10000 });

    const historyContent = await page.locator('mat-card-content').textContent();
    expect(historyContent).toContain(datePart!);
    await page.screenshot({ path: 'test-results/06-time-history-with-value.png' });
  });

  // S4 acceptance: empty time history shows no-records state without error
  test('S4 — get-now-history shows empty state without error when no records exist', async ({ page }) => {
    await page.getByRole('button', { name: 'get-now-history', exact: true }).click();
    const card = page.locator('mat-card').first();
    await expect(card).toBeVisible({ timeout: 10000 });
    const errorCard = page.locator('mat-card.error-card');
    const errorCount = await errorCard.count();
    expect(errorCount).toBe(0);
    await page.screenshot({ path: 'test-results/07-time-history-no-error.png' });
  });

  // happy_path: end-to-end flow
  test('happy_path — generate random number then verify it appears in history', async ({ page }) => {
    await page.getByRole('button', { name: 'get-random', exact: true }).click();
    await expect(page.locator('mat-card').first()).toBeVisible({ timeout: 10000 });
    const valueText = await page.locator('mat-card-content p', { hasText: 'Value:' }).textContent();
    const match = valueText?.match(/Value:\s*(\d+)/);
    const generatedValue = match ? match[1] : null;
    expect(generatedValue).toBeTruthy();
    await page.screenshot({ path: 'test-results/08-happy-path-random.png' });

    await page.getByRole('button', { name: 'get-random-history', exact: true }).click();
    await expect(page.locator('mat-card').first()).toBeVisible({ timeout: 10000 });
    const historyContent = await page.locator('mat-card-content').textContent();
    expect(historyContent).toContain(generatedValue!);
    await page.screenshot({ path: 'test-results/09-happy-path-history.png' });
  });

});
