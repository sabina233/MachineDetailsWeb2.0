# Vue Production Dashboard Redesign

## Objective

Refactor `web_app` into a Vite + Vue 3 single-page application. After sign-in, users open a daily production dashboard: a total output summary followed vertically by three category tables for Liandian roll machines, Liandian single-sheet machines, and ZPL. Every category links to its details page.

## UX and navigation

- Keep the existing dark-blue production-system visual language, with responsive cards, tables, and status indicators.
- Dashboard order: top bar (date, refresh, sign out), daily total, Liandian roll, Liandian single-sheet, ZPL.
- Each category area contains its total, machine/user count, compact table, and a details action.
- Routes: `/login`, `/dashboard`, `/liandian/roll`, `/liandian/single`, `/zpl`; single-device and ZPL-user details use `machineName` and `userName` query parameters.
- Route guards clear an absent or expired JWT and return to the login page.

## Frontend

- Add a Vite + Vue 3 project under `web_app`; publish its built assets through the ASP.NET static-file directory.
- Vue Router manages pages; a centralized fetch client reads API base URLs from the existing `wwwroot/config/ApiUrl.json`; Chart.js renders trends.
- Dashboard loads on entry and refreshes every 30 seconds. Detail views keep date filters, tables, pagination, and charts from the legacy `machine-details.html` and `zpl_details.html` workflows.

## Business rules

### Liandian

- `MachineName` matching `^RFID-6` belongs to roll machines; matching `^RFID-8` belongs to single-sheet machines. Other names are excluded from these two categories.
- Read same-day `DataRecords`. Active duration uses each record timestamp to the next record timestamp only when the current record has `State = 1`; `State = 0` time does not count.
- The final record never projects duration forward. To prevent a disconnected device from adding a false duration, each active gap is capped server-side at 10 minutes; response data includes the accumulated seconds.
- Repeated state reports must not duplicate output: group daily records by `(MachineName, TaskName)`, take the maximum `ProductQuantity` of each non-empty task, then sum.
- Return machine output, active duration, latest state, and latest timestamp; the frontend derives roll and single-sheet category displays from these records.

### ZPL

- Count only records whose `PrintStatus` is exactly `Order closure successful`.
- For each same-day `(UserName, OrderNo)`, take only the latest record's `Quantity`; sum it for each user, the ZPL category, and global output.
- Return user daily closed-order output, latest order, region, and update time. The ZPL detail route keeps its paged history and adds a daily closed-order total and date trend.

## API design

- Add `GET /api/Data/dashboard/liandian?date=yyyy-MM-dd&category=roll|single` to `DataController` for Liandian category statistics.
- Add `GET /api/Data/dashboard/summary?date=yyyy-MM-dd` for the dashboard's combined roll, single-sheet, and ZPL totals.
- Add `GET /api/zpl/dashboard/daily-summary?date=yyyy-MM-dd` to return grouped ZPL closed-order output.
- Extract shared computation into `Services/ProductionDashboardService`; controllers validate input and produce HTTP responses only.

## Errors, security, and performance

- Accept only `yyyy-MM-dd`; invalid values return 400. Daily bounds are `[00:00, next day 00:00)`.
- All SQL is parameterized. Empty results return the complete response shape with zero totals.
- Frontend exposes loading, empty, retryable error states; a failed chart does not block other content.

## Verification

- Back-end tests: classifying `RFID-6/8`, active/closed gaps, active-gap cap, task-output de-duplication, and ZPL closed-order latest-value selection.
- Front-end tests: dashboard category display, route arguments, date filtering, and loading/empty/error states.
- Build checks: `npm run build` and `dotnet build web_api2022/web_api2022.csproj`.
- Acceptance: total equals the three category totals; state-0 intervals do not count; ZPL non-closed records do not count; all details support date-filtered tables and charts.
