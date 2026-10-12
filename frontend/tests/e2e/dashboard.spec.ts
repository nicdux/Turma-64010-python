import { expect, test } from "@playwright/test";

test("shows the demonstrative dashboard and switches revenue chart type", async ({ page }) => {
  await page.goto("/");
  await expect(page.getByRole("heading", { name: /Bom dia, Marcos/ })).toBeVisible();
  await expect(page.getByText(/Visão demonstrativa/)).toBeVisible();

  const barButton = page.getByRole("button", { name: "Barras" });
  await barButton.click();
  await expect(barButton).toHaveAttribute("aria-pressed", "true");
  await expect(page.getByRole("button", { name: "Área" })).toHaveAttribute("aria-pressed", "false");
});
