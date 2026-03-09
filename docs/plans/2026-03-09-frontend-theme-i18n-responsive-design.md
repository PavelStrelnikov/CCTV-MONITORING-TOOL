# Frontend: Theme + i18n/RTL + Responsive Design

**Date:** 2026-03-09
**Approach:** Sequential — Theme → i18n/RTL → Responsive

## 1. Theme (Light/Dark)

**Architecture:**
- Extend `theme.ts` with two palettes (`darkPalette`, `lightPalette`)
- `createTheme()` called dynamically based on `mode: 'light' | 'dark'`
- State in `localStorage` key `cctv-theme-mode`
- React context `ThemeContext` with `toggleTheme()` wrapping `App.tsx`

**Components:**
- AppBar: sun/moon `IconButton` (`Brightness4`/`Brightness7` MUI icons)
- Click toggles mode, writes localStorage, theme updates instantly

**Light palette:**
- Background: `#f5f5f5` (paper: `#ffffff`)
- Primary: keep current blue
- Sidebar: stays dark in both themes
- DataGrid, cards, alerts — all via MUI theme tokens

**Files changed:**
- `theme.ts` — palettes + context
- `App.tsx` — `ThemeContext.Provider`
- `Layout.tsx` — toggle icon in AppBar
- `dataGridDarkSx` → `dataGridSx(mode)` — adaptive table styles

## 2. i18n + RTL

**Stack:**
- `react-i18next` + `i18next`
- `stylis-plugin-rtl` + `@emotion/cache`
- MUI `direction: 'rtl'` support

**Translation files:**
- `frontend/src/locales/en.json`, `frontend/src/locales/he.json`
- Flat structure with page namespaces: `"dashboard.title": "Dashboard"`
- ~100-150 keys per language

**RTL mechanics:**
- Language stored in `localStorage` key `cctv-lang` (default: `en`)
- Hebrew: `document.dir = 'rtl'`, theme `direction: 'rtl'`, Emotion cache with RTL plugin
- Sidebar moves to right, margins/paddings auto-mirrored via stylis
- Arrow icons manually mirrored (`transform: scaleX(-1)`)

**Language switcher:**
- Settings page — Select with English / עברית
- Changes `i18n.changeLanguage()`, direction, localStorage

**Files changed:**
- New: `i18n.ts`, `locales/en.json`, `locales/he.json`
- `App.tsx` — `I18nextProvider`, dynamic `CacheProvider`
- `theme.ts` — `direction` parameter
- All 8 pages + `Layout.tsx` — `useTranslation()`
- `Settings.tsx` — language picker

## 3. Responsive

**Breakpoints (MUI defaults):**
- `xs` (0-599): phone — hamburger menu, single column, compact cards
- `sm` (600-899): tablet portrait — sidebar collapsed, 2 columns
- `md` (900-1199): tablet landscape / laptop — sidebar visible, 2-3 columns
- `lg` (1200+): desktop — current layout

**Layout:**
- Sidebar: `Drawer` `variant="permanent"` on md+, `variant="temporary"` on xs-sm
- Hamburger icon in AppBar on xs-sm
- AppBar: hide "CCTV Monitor" text on xs, icons only

**Pages:**
- Dashboard: stat cards Grid 1col(xs), 2col(sm), 4col(md+). Device table → cards on xs.
- DeviceList: DataGrid on md+, card list on xs-sm
- DeviceDetail: tabs stack vertically on xs
- AddDevice/EditDevice: full-width on xs, max-width 600px on md+
- Alerts: DataGrid on md+, card list on xs-sm
- PollLogs: DataGrid on md+, simplified list on xs-sm
- Settings: single column always, max-width 600px

**Files changed:**
- `Layout.tsx` — responsive Drawer, hamburger
- All 8 pages — `sx` breakpoints, Grid containers
- Possibly new `MobileCard.tsx` for card-view tables
