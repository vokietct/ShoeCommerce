// Run only against a disposable, seeded test database. See README.md.
const { chromium } = require('playwright');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const base = process.env.SHOE_BROWSER_BASE_URL;
if (!base || !/^http:\/\/(127\.0\.0\.1|localhost):\d+$/.test(base))
  throw new Error('Set SHOE_BROWSER_BASE_URL to the local disposable test app.');

(async () => {
  const browser = await chromium.launch({ channel: 'msedge', headless: true });
  const errors = [];
  let count = 0;
  const check = (value, label) => { assert(value, label); console.log('PASS: ' + label); count++; };
  const context = await browser.newContext({ viewport: { width: 1440, height: 1000 } });
  const page = await context.newPage();
  page.on('pageerror', e => errors.push(e.message));
  page.on('console', m => { if (m.type() === 'error' && /Unhandled exception|circuit will be terminated/i.test(m.text())) errors.push(m.text()); });
  async function open(path) {
    await page.goto(base + path, { waitUntil: 'domcontentloaded' });
    await page.waitForTimeout(900); // allow the server circuit to attach to prerendered controls
  }
  async function login(email, password, admin = false) {
    await open(admin ? '/admin/login' : '/login');
    await page.locator('[name=email]').fill(email);
    await page.locator('[name=password]').fill(password);
    await Promise.all([page.waitForURL(u => !u.pathname.endsWith('/login')), page.locator('form[action="/account/login"] button').click()]);
    await page.waitForTimeout(700);
  }
  async function logout() {
    // Submit the real antiforgery-protected logout form even when its dropdown is closed.
    await page.locator('form[action="/account/logout"]').evaluate(form => form.requestSubmit());
    await page.waitForURL(base + '/');
    await page.waitForTimeout(700);
  }
  try {
    await open('/');
    check(await page.locator('.product-card').count() > 0, 'Storefront loads products from MySQL');
    await open('/admin/orders');
    check(new URL(page.url()).pathname === '/admin/login', 'Anonymous admin route redirects to internal login');
    await open('/register');
    const email = `browser-${Date.now()}@example.com`;
    await page.locator('[name=fullName]').fill('Browser Customer');
    await page.locator('[name=email]').fill(email);
    await page.locator('[name=phone]').fill('0900000000');
    await page.locator('[name=password]').fill('browser123');
    await page.locator('[name=confirmPassword]').fill('browser123');
    await Promise.all([page.waitForURL(base + '/'), page.locator('form[action="/account/register"] button').click()]);
    await page.reload();
    await page.waitForTimeout(700);
    check((await page.locator('body').innerText()).includes('Browser Customer'), 'Registration signs in and survives reload');
    await open('/product/1');
    await page.getByRole('button', { name: 'Thêm Vào Giỏ Hàng' }).click();
    await page.locator('.alert-success').waitFor();
    check(await page.locator('.cart-badge').innerText() === '1', 'Cart badge updates without navigation');
    await open('/cart');
    check((await page.locator('body').innerText()).includes('Nike Air Force'), 'Cart contains selected shoe');
    await open('/checkout');
    await page.locator('input[placeholder="Nguyễn Văn A"]').fill('Browser Receiver');
    await page.locator('input[placeholder="0901234567"]').fill('0900000000');
    const address = page.locator('textarea').first();
    if (await address.count()) await address.fill('12 Test Street, Ho Chi Minh City');
    else await page.locator('input[placeholder*="địa chỉ" i]').first().fill('12 Test Street, Ho Chi Minh City');
    const checkoutText = await page.locator('body').innerText();
    check(checkoutText.includes('COD'), 'Checkout offers COD payment');
    await page.getByRole('button', { name: /Xác Nhận Đặt Hàng/i }).click();
    await page.waitForURL(/\/order-success\//);
    await page.getByRole('heading', { name: 'Đặt Hàng Thành Công!' }).waitFor();
    check((await page.locator('body').innerText()).includes('Đặt Hàng Thành Công'), 'Customer completes checkout');
    const successUrl = page.url();
    await page.reload();
    await page.getByText('Họ tên: Browser Receiver', { exact: true }).waitFor();
    check((await page.locator('body').innerText()).includes('Browser Receiver'), 'Order success survives reload with customer identity');
    await open('/orders');
    const detailLink = page.locator('a[href^="/order/"]').first();
    await detailLink.click();
    await page.waitForTimeout(700);
    await page.getByRole('button', { name: /Hủy Đơn Hàng/i }).first().click();
    await page.locator('textarea').last().fill('Browser cancellation test');
    await page.getByRole('button', { name: /Xác Nhận Hủy/i }).click();
    await page.locator('.alert-success').waitFor();
    check((await page.locator('body').innerText()).includes('Đã hủy'), 'Customer cancels pending COD order');
    await logout();
    await login('minhanh@example.com', 'customer123');
    await page.goto(successUrl);
    await page.getByRole('heading', { name: 'Không tìm thấy mã đơn hàng' }).waitFor();
    check((await page.locator('body').innerText()).includes('Không tìm thấy mã đơn hàng'), 'Another customer cannot view success details');
    await logout();
    await login('linh@shoestore.com', 'employee123', true);
    await open('/admin/products');
    check((await page.locator('body').innerText()).includes('Nike Air Force'), 'Employee can look up products');
    await open('/admin/employees');
    check(new URL(page.url()).pathname === '/access-denied', 'Employee cannot visit admin-only pages directly');
    await logout();
    await login('admin@shoestore.vn', 'admin123', true);
    const routes = ['/admin', '/admin/products', '/admin/categories-brands', '/admin/inventory', '/admin/stock-documents',
      '/admin/suppliers', '/admin/customers', '/admin/promotions', '/admin/reviews', '/admin/employees', '/admin/orders', '/admin/payments', '/admin/statistics'];
    for (const route of routes) {
      await open(route);
      const body = await page.locator('body').innerText();
      check(!body.includes('Không thể thực hiện thao tác') && !body.includes('An unhandled error') && new URL(page.url()).pathname === route,
        `Admin page renders: ${route}`);
    }
    await page.setViewportSize({ width: 390, height: 844 });
    await open('/admin/products');
    await page.getByRole('button', { name: 'Mở menu quản trị' }).click();
    check(await page.locator('.admin-wrapper.menu-open').count() === 1, 'Mobile admin menu opens');
    await page.locator('.admin-menu-backdrop').click({ position: { x: 350, y: 300 } });
    check(await page.locator('.admin-wrapper.menu-open').count() === 0, 'Mobile admin menu closes');
    fs.mkdirSync('artifacts', { recursive: true });
    await page.screenshot({ path: 'artifacts/admin-mobile.png', fullPage: true });
    await page.setViewportSize({ width: 1440, height: 1000 });
    await open('/admin');
    await page.screenshot({ path: 'artifacts/admin-dashboard.png', fullPage: true });
    await open('/');
    await page.screenshot({ path: 'artifacts/storefront.png', fullPage: true });
    check(errors.length === 0, `No browser application errors: ${errors.join('; ')}`);
    console.log(`PASS: ${count} browser checks.`);
  } finally {
    await browser.close();
  }
})().catch(e => { console.error(e); process.exitCode = 1; });
