# Frontend: Theme + i18n/RTL + Responsive — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add light/dark theme toggle, English/Hebrew i18n with full RTL mirror, and responsive layout for phone through desktop.

**Architecture:** Three sequential phases. Phase 1 (Theme) creates the palette/context foundation. Phase 2 (i18n/RTL) adds translations and directional layout. Phase 3 (Responsive) adapts all pages to mobile/tablet breakpoints.

**Tech Stack:** MUI `createTheme`, `react-i18next`, `stylis-plugin-rtl`, `@emotion/cache`, MUI `useMediaQuery` + responsive `sx` props.

---

## Phase 1: Theme (Light/Dark)

### Task 1: Install dependencies and create ThemeContext

**Files:**
- Modify: `frontend/src/theme.ts`

**Step 1: Create the dual-palette theme system with context**

Replace the entire `theme.ts` with a context-based approach:

```typescript
import { createContext, useContext } from 'react';
import { createTheme, type Theme } from '@mui/material/styles';

// ─── Shared tokens ───
const shape = { borderRadius: 12 };
const typography = {
  fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
  h4: { fontWeight: 700 },
  h5: { fontWeight: 600 },
  h6: { fontWeight: 600 },
};

// ─── Component overrides that work for both modes ───
function getComponents(mode: 'light' | 'dark') {
  const isDark = mode === 'dark';
  const border = isDark ? '#1E293B' : '#E2E8F0';
  const paperBg = isDark ? '#111827' : '#FFFFFF';
  const filledBg = isDark ? '#1E293B' : '#F1F5F9';
  const filledHover = isDark ? '#283548' : '#E2E8F0';
  const sidebarBg = '#0F1629'; // sidebar stays dark always
  const appBarBg = '#0F1629';  // appbar stays dark always

  return {
    MuiCssBaseline: {
      styleOverrides: {
        body: { backgroundColor: isDark ? '#0B0F1A' : '#F5F5F5' },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: { backgroundImage: 'none', border: `1px solid ${border}` },
      },
    },
    MuiPaper: {
      styleOverrides: { root: { backgroundImage: 'none' } },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
          backgroundColor: appBarBg,
          borderBottom: `1px solid ${border}`,
          boxShadow: 'none',
        },
      },
    },
    MuiDrawer: {
      styleOverrides: {
        paper: { backgroundColor: sidebarBg, borderRight: `1px solid ${border}` },
      },
    },
    MuiChip: {
      styleOverrides: { root: { fontWeight: 600, fontSize: '0.75rem' } },
    },
    MuiTextField: {
      defaultProps: { variant: 'filled' as const },
    },
    MuiFilledInput: {
      styleOverrides: {
        root: {
          backgroundColor: filledBg,
          '&:hover': { backgroundColor: filledHover },
          '&.Mui-focused': { backgroundColor: filledBg },
        },
      },
    },
    MuiTableCell: {
      styleOverrides: { root: { borderColor: border } },
    },
    MuiDialog: {
      styleOverrides: {
        paper: { backgroundColor: paperBg, border: `1px solid ${border}` },
      },
    },
    MuiTab: {
      styleOverrides: { root: { textTransform: 'none' as const, fontWeight: 500 } },
    },
    MuiButton: {
      styleOverrides: { root: { textTransform: 'none' as const, fontWeight: 600 } },
    },
    MuiListItemButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          margin: '2px 8px',
          '&.active': {
            backgroundColor: 'rgba(59, 130, 246, 0.15)',
            color: '#3B82F6',
            '& .MuiListItemIcon-root': { color: '#3B82F6' },
          },
        },
      },
    },
    MuiListItemIcon: {
      styleOverrides: { root: { color: '#94A3B8', minWidth: 40 } },
    },
  };
}

export function buildTheme(mode: 'light' | 'dark', direction: 'ltr' | 'rtl' = 'ltr'): Theme {
  const isDark = mode === 'dark';
  return createTheme({
    direction,
    palette: {
      mode,
      primary: { main: '#3B82F6' },
      secondary: { main: '#06B6D4' },
      success: { main: '#22C55E' },
      error: { main: '#EF4444' },
      warning: { main: '#F59E0B' },
      info: { main: '#8B5CF6' },
      background: {
        default: isDark ? '#0B0F1A' : '#F5F5F5',
        paper: isDark ? '#111827' : '#FFFFFF',
      },
      text: {
        primary: isDark ? '#F1F5F9' : '#1E293B',
        secondary: isDark ? '#94A3B8' : '#64748B',
      },
      divider: isDark ? '#1E293B' : '#E2E8F0',
    },
    shape,
    typography,
    components: getComponents(mode) as any,
  });
}

/** DataGrid styling — adapts to theme mode */
export function getDataGridSx(mode: 'light' | 'dark') {
  const isDark = mode === 'dark';
  const border = isDark ? '#1E293B' : '#E2E8F0';
  const headerBg = isDark ? '#0F172A' : '#F8FAFC';
  const stripeBg = isDark ? '#0F172A' : '#F8FAFC';
  const hoverBg = isDark ? '#1E293B' : '#F1F5F9';

  return {
    border: `1px solid ${border}`,
    '& .MuiDataGrid-columnHeaders': {
      backgroundColor: headerBg,
      borderBottom: `1px solid ${border}`,
    },
    '& .MuiDataGrid-cell': { borderColor: border },
    '& .MuiDataGrid-row': {
      '&:nth-of-type(even)': { backgroundColor: stripeBg },
      '&:hover': { backgroundColor: hoverBg },
    },
    '& .MuiDataGrid-footerContainer': { borderTop: `1px solid ${border}` },
    '& .MuiDataGrid-columnSeparator': { display: 'none' },
  } as const;
}

// ─── Theme context ───
export type ThemeMode = 'light' | 'dark';

interface ThemeContextValue {
  mode: ThemeMode;
  toggleTheme: () => void;
}

export const ThemeContext = createContext<ThemeContextValue>({
  mode: 'dark',
  toggleTheme: () => {},
});

export const useThemeMode = () => useContext(ThemeContext);
```

**Step 2: Verify the file compiles**

Run: `cd frontend && npx tsc --noEmit`
Expected: No errors (warnings OK)

**Step 3: Commit**

```bash
git add frontend/src/theme.ts
git commit -m "feat(theme): add dual-palette theme system with light/dark support"
```

---

### Task 2: Wire ThemeContext into App.tsx

**Files:**
- Modify: `frontend/src/App.tsx`

**Step 1: Update App.tsx to use ThemeContext**

Replace the entire file:

```typescript
import { useState, useMemo } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { buildTheme, ThemeContext, type ThemeMode } from './theme.ts';
import Layout from './components/Layout.tsx';
import Dashboard from './pages/Dashboard.tsx';
import DeviceList from './pages/DeviceList.tsx';
import AddDevice from './pages/AddDevice.tsx';
import DeviceDetail from './pages/DeviceDetail.tsx';
import EditDevice from './pages/EditDevice.tsx';
import Alerts from './pages/Alerts.tsx';
import PollLogs from './pages/PollLogs.tsx';
import Settings from './pages/Settings.tsx';

function getStoredMode(): ThemeMode {
  const stored = localStorage.getItem('cctv-theme-mode');
  return stored === 'light' ? 'light' : 'dark';
}

export default function App() {
  const [mode, setMode] = useState<ThemeMode>(getStoredMode);

  const toggleTheme = () => {
    setMode((prev) => {
      const next = prev === 'dark' ? 'light' : 'dark';
      localStorage.setItem('cctv-theme-mode', next);
      return next;
    });
  };

  const theme = useMemo(() => buildTheme(mode), [mode]);

  return (
    <ThemeContext.Provider value={{ mode, toggleTheme }}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <BrowserRouter>
          <Routes>
            <Route element={<Layout />}>
              <Route path="/" element={<Dashboard />} />
              <Route path="/devices" element={<DeviceList />} />
              <Route path="/devices/add" element={<AddDevice />} />
              <Route path="/devices/:deviceId" element={<DeviceDetail />} />
              <Route path="/devices/:deviceId/edit" element={<EditDevice />} />
              <Route path="/poll-logs" element={<PollLogs />} />
              <Route path="/alerts" element={<Alerts />} />
              <Route path="/settings" element={<Settings />} />
              <Route path="*" element={<Navigate to="/" replace />} />
            </Route>
          </Routes>
        </BrowserRouter>
      </ThemeProvider>
    </ThemeContext.Provider>
  );
}
```

**Step 2: Verify**

Run: `cd frontend && npx tsc --noEmit`
Expected: No errors

**Step 3: Commit**

```bash
git add frontend/src/App.tsx
git commit -m "feat(theme): wire ThemeContext into App with localStorage persistence"
```

---

### Task 3: Add theme toggle to Layout AppBar

**Files:**
- Modify: `frontend/src/components/Layout.tsx`

**Step 1: Add sun/moon toggle button to the AppBar**

Add imports at the top:
```typescript
import IconButton from '@mui/material/IconButton';
import Brightness4Icon from '@mui/icons-material/Brightness4';
import Brightness7Icon from '@mui/icons-material/Brightness7';
import { useThemeMode } from '../theme.ts';
```

Inside the `Layout` function, before the return:
```typescript
const { mode, toggleTheme } = useThemeMode();
```

Replace the `<Toolbar>` contents:
```tsx
<Toolbar variant="dense" sx={{ justifyContent: 'flex-end' }}>
  <IconButton onClick={toggleTheme} color="inherit" size="small">
    {mode === 'dark' ? <Brightness7Icon /> : <Brightness4Icon />}
  </IconButton>
</Toolbar>
```

**Step 2: Verify the toggle works visually**

Run: `cd frontend && npm run dev`
Open browser, click the sun/moon icon — background and cards should switch between dark and light.

**Step 3: Commit**

```bash
git add frontend/src/components/Layout.tsx
git commit -m "feat(theme): add sun/moon toggle button to AppBar"
```

---

### Task 4: Update all pages to use adaptive DataGrid styles

**Files:**
- Modify: `frontend/src/pages/DeviceList.tsx`
- Modify: `frontend/src/pages/Alerts.tsx`
- Modify: `frontend/src/pages/PollLogs.tsx`
- Modify: `frontend/src/pages/DeviceDetail.tsx`

**Step 1: Update imports in all four files**

In each file, change:
```typescript
import { dataGridDarkSx } from '../theme.ts';
```
to:
```typescript
import { getDataGridSx, useThemeMode } from '../theme.ts';
```

**Step 2: Use dynamic styles in each component**

At the top of each component function, add:
```typescript
const { mode } = useThemeMode();
```

Replace all occurrences of `...dataGridDarkSx` with `...getDataGridSx(mode)`.

Files and exact replacements:

**DeviceList.tsx** (line ~300):
```tsx
sx={{
  ...getDataGridSx(mode),
  '& .MuiDataGrid-cell': { display: 'flex', alignItems: 'center' },
}}
```

**Alerts.tsx** (line ~157):
```tsx
sx={{
  ...getDataGridSx(mode),
  '& .MuiDataGrid-cell': { display: 'flex', alignItems: 'center' },
}}
```

**PollLogs.tsx** (line ~175):
```tsx
sx={{
  ...getDataGridSx(mode),
  '& .MuiDataGrid-cell': { display: 'flex', alignItems: 'center' },
}}
```

**DeviceDetail.tsx** (line ~699 and ~701, disk DataGrid):
```tsx
sx={{
  ...getDataGridSx(mode),
  '& .MuiDataGrid-cell': { display: 'flex', alignItems: 'center' },
}}
```

**Step 3: Update App.css scrollbar to respect theme**

Replace the hardcoded dark scrollbar colors with CSS custom properties. For now, keep dark scrollbar — it works with both themes since sidebar/appbar stay dark.

**Step 4: Verify**

Run: `cd frontend && npx tsc --noEmit`
Expected: No errors

**Step 5: Commit**

```bash
git add frontend/src/pages/DeviceList.tsx frontend/src/pages/Alerts.tsx frontend/src/pages/PollLogs.tsx frontend/src/pages/DeviceDetail.tsx
git commit -m "feat(theme): update all DataGrid pages to use adaptive theme styles"
```

---

## Phase 2: i18n + RTL

### Task 5: Install i18n dependencies

**Step 1: Install packages**

Run:
```bash
cd frontend && npm install i18next react-i18next i18next-browser-languagedetector stylis-plugin-rtl @emotion/cache
```

**Step 2: Commit**

```bash
git add frontend/package.json frontend/package-lock.json
git commit -m "chore: add i18next, stylis-plugin-rtl, emotion-cache dependencies"
```

---

### Task 6: Create i18n config and translation files

**Files:**
- Create: `frontend/src/i18n.ts`
- Create: `frontend/src/locales/en.json`
- Create: `frontend/src/locales/he.json`

**Step 1: Create i18n config**

File `frontend/src/i18n.ts`:
```typescript
import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import en from './locales/en.json';
import he from './locales/he.json';

const storedLang = localStorage.getItem('cctv-lang') || 'en';

i18n.use(initReactI18next).init({
  resources: { en: { translation: en }, he: { translation: he } },
  lng: storedLang,
  fallbackLng: 'en',
  interpolation: { escapeValue: false },
});

export default i18n;
```

**Step 2: Create English translations**

File `frontend/src/locales/en.json`:
```json
{
  "nav.dashboard": "Dashboard",
  "nav.devices": "Devices",
  "nav.pollLogs": "Poll Logs",
  "nav.alerts": "Alerts",
  "nav.settings": "Settings",
  "nav.brand": "CCTV Monitor",

  "dashboard.title": "Dashboard",
  "dashboard.devices": "Devices",
  "dashboard.cameras": "Cameras",
  "dashboard.disks": "Disks",
  "dashboard.recording": "Recording",
  "dashboard.timeSync": "Time Sync",
  "dashboard.alerts": "Alerts",
  "dashboard.allOnline": "all online",
  "dashboard.offline": "{{count}} offline",
  "dashboard.error": "{{count}} error",
  "dashboard.allOk": "all OK",
  "dashboard.noRec": "{{count}} no rec",
  "dashboard.allRecording": "all recording",
  "dashboard.noData": "no data",
  "dashboard.devicesDrifted": "{{count}} devices drifted",
  "dashboard.allSynced": "all synced",
  "dashboard.active": "active",
  "dashboard.noAlerts": "no alerts",
  "dashboard.activeAlerts": "Active Alerts",
  "dashboard.noActiveAlerts": "No active alerts",
  "dashboard.viewAllAlerts": "View all {{count}} alerts",
  "dashboard.noDevices": "No devices configured",
  "dashboard.failedLoad": "Failed to load dashboard",

  "table.name": "Name",
  "table.status": "Status",
  "table.cameras": "Cameras",
  "table.disks": "Disks",
  "table.recording": "Recording",
  "table.time": "Time",
  "table.lastPoll": "Last Poll",
  "table.host": "Host",
  "table.webPort": "Web Port",
  "table.sdkPort": "SDK Port",
  "table.vendor": "Vendor",
  "table.response": "Response",
  "table.actions": "Actions",
  "table.device": "Device",
  "table.type": "Type",
  "table.severity": "Severity",
  "table.message": "Message",
  "table.created": "Created",
  "table.period": "Period",

  "status.online": "Online",
  "status.offline": "Offline",
  "status.unknown": "UNKNOWN",
  "status.reachable": "Reachable",
  "status.unreachable": "Unreachable",
  "status.ok": "OK",
  "status.error": "ERROR",

  "devices.title": "Devices",
  "devices.addDevice": "Add Device",
  "devices.search": "Search",
  "devices.filterByTags": "Filter by tags",
  "devices.failedLoad": "Failed to load devices",
  "devices.deleteConfirm": "Delete device {{id}}?",
  "devices.deleteFailed": "Delete failed",

  "deviceDetail.pollNow": "Poll Now",
  "deviceDetail.edit": "Edit",
  "deviceDetail.delete": "Delete",
  "deviceDetail.deleteConfirm": "Delete this device?",
  "deviceDetail.model": "Model",
  "deviceDetail.serialNumber": "S/N",
  "deviceDetail.address": "Address",
  "deviceDetail.transport": "Transport",
  "deviceDetail.lastPoll": "Last poll",
  "deviceDetail.credentials": "Credentials",
  "deviceDetail.timeSynced": "Time synced",
  "deviceDetail.timeDrift": "Time drift",
  "deviceDetail.cameras": "Cameras",
  "deviceDetail.ignored": "ignored",
  "deviceDetail.disks": "Disks",
  "deviceDetail.history": "History",
  "deviceDetail.alerts": "Alerts",
  "deviceDetail.noCameras": "No cameras found",
  "deviceDetail.noDisks": "No disks found",
  "deviceDetail.noHistory": "No history data yet",
  "deviceDetail.noAlerts": "No alerts",
  "deviceDetail.notFound": "Device not found",
  "deviceDetail.showCreds": "Show credentials",
  "deviceDetail.hideCreds": "Hide credentials",
  "deviceDetail.copyPassword": "Copy password",
  "deviceDetail.includeMonitor": "Include in monitoring",
  "deviceDetail.excludeMonitor": "Exclude from monitoring",
  "deviceDetail.ignore": "Ignore",
  "deviceDetail.addTag": "add tag",
  "deviceDetail.diskId": "Disk ID",
  "deviceDetail.capacity": "Capacity",
  "deviceDetail.freeSpace": "Free Space",
  "deviceDetail.usedPct": "Used %",
  "deviceDetail.health": "Health",
  "deviceDetail.temp": "Temp",
  "deviceDetail.workingTime": "Working Time",
  "deviceDetail.smart": "S.M.A.R.T.",
  "deviceDetail.responseTime": "Response Time (ms)",
  "deviceDetail.onlineCameras": "Online Cameras",
  "deviceDetail.timeAxis": "Time",

  "addDevice.title": "Add Device",
  "addDevice.deviceId": "Device ID",
  "addDevice.name": "Name",
  "addDevice.vendor": "Vendor",
  "addDevice.host": "Host",
  "addDevice.webPort": "Web Port (ISAPI/HTTP)",
  "addDevice.sdkPort": "SDK Port",
  "addDevice.transport": "Transport",
  "addDevice.transportIsapi": "ISAPI (HTTP/HTTPS)",
  "addDevice.transportSdk": "SDK (Device Network)",
  "addDevice.transportAuto": "Auto",
  "addDevice.pollInterval": "Poll Interval (seconds, empty = no auto-poll)",
  "addDevice.pollIntervalHelp": "Leave empty to disable automatic polling",
  "addDevice.username": "Username",
  "addDevice.password": "Password",
  "addDevice.submit": "Add Device",
  "addDevice.submitting": "Adding...",
  "addDevice.cancel": "Cancel",
  "addDevice.failed": "Failed to add device",

  "editDevice.title": "Edit Device",
  "editDevice.save": "Save",
  "editDevice.saving": "Saving...",
  "editDevice.cancel": "Cancel",
  "editDevice.failed": "Failed to update",
  "editDevice.usernameCurrent": "Username (current: {{name}})",
  "editDevice.usernameKeep": "Username (leave empty to keep current)",
  "editDevice.passwordKeep": "Password (leave empty to keep current)",

  "alerts.title": "Alerts",
  "alerts.statusAll": "All",
  "alerts.statusActive": "Active",
  "alerts.statusResolved": "Resolved",
  "alerts.searchMessages": "Search messages",
  "alerts.failedLoad": "Failed to load alerts",

  "pollLogs.title": "Poll Logs",
  "pollLogs.refresh": "Refresh",
  "pollLogs.searchDevice": "Search device",
  "pollLogs.failedLoad": "Failed to load poll logs",
  "pollLogs.last6h": "Last 6 hours",
  "pollLogs.last12h": "Last 12 hours",
  "pollLogs.last24h": "Last 24 hours",
  "pollLogs.last48h": "Last 48 hours",
  "pollLogs.last3d": "Last 3 days",
  "pollLogs.last7d": "Last 7 days",

  "settings.title": "Settings",
  "settings.polling": "Polling",
  "settings.pollingDesc": "Default poll interval for devices that don't have an individual interval set.",
  "settings.defaultPollInterval": "Default Poll Interval",
  "settings.save": "Save",
  "settings.saving": "Saving...",
  "settings.saved": "Settings saved",
  "settings.failedLoad": "Failed to load settings",
  "settings.failedSave": "Failed to save settings",
  "settings.language": "Language",
  "settings.languageDesc": "Choose display language. Hebrew enables right-to-left layout.",
  "settings.langEnglish": "English",
  "settings.langHebrew": "עברית",

  "settings.disabled": "Disabled",
  "settings.1min": "1 minute",
  "settings.5min": "5 minutes",
  "settings.10min": "10 minutes",
  "settings.15min": "15 minutes",
  "settings.30min": "30 minutes",
  "settings.1hour": "1 hour",

  "poll.title": "Device Poll",
  "poll.webPort": "Web Port (TCP)",
  "poll.sdkPort": "SDK Port (TCP)",
  "poll.connect": "Connection",
  "poll.deviceInfo": "Device Info",
  "poll.cameras": "Cameras",
  "poll.disks": "Disks",
  "poll.recording": "Recording",
  "poll.timeCheck": "Time",
  "poll.retry": "Retry",
  "poll.close": "Close",

  "time.justNow": "just now",
  "time.secsAgo": "{{count}}s ago",
  "time.minsAgo": "{{count}} min ago",
  "time.hoursAgo": "{{count}}h ago",
  "time.daysAgo": "{{count}}d ago",
  "time.never": "never",

  "common.loading": "Loading...",
  "common.poll": "Poll",
  "common.edit": "Edit",
  "common.delete": "Delete",
  "common.na": "N/A"
}
```

**Step 3: Create Hebrew translations**

File `frontend/src/locales/he.json`:
```json
{
  "nav.dashboard": "לוח בקרה",
  "nav.devices": "מכשירים",
  "nav.pollLogs": "יומן סקרים",
  "nav.alerts": "התראות",
  "nav.settings": "הגדרות",
  "nav.brand": "CCTV Monitor",

  "dashboard.title": "לוח בקרה",
  "dashboard.devices": "מכשירים",
  "dashboard.cameras": "מצלמות",
  "dashboard.disks": "דיסקים",
  "dashboard.recording": "הקלטה",
  "dashboard.timeSync": "סנכרון שעון",
  "dashboard.alerts": "התראות",
  "dashboard.allOnline": "הכל מחובר",
  "dashboard.offline": "{{count}} לא מחובר",
  "dashboard.error": "{{count}} שגיאה",
  "dashboard.allOk": "הכל תקין",
  "dashboard.noRec": "{{count}} ללא הקלטה",
  "dashboard.allRecording": "הכל מקליט",
  "dashboard.noData": "אין נתונים",
  "dashboard.devicesDrifted": "{{count}} מכשירים עם סטייה",
  "dashboard.allSynced": "הכל מסונכרן",
  "dashboard.active": "פעיל",
  "dashboard.noAlerts": "אין התראות",
  "dashboard.activeAlerts": "התראות פעילות",
  "dashboard.noActiveAlerts": "אין התראות פעילות",
  "dashboard.viewAllAlerts": "צפה בכל {{count}} ההתראות",
  "dashboard.noDevices": "לא הוגדרו מכשירים",
  "dashboard.failedLoad": "טעינת לוח הבקרה נכשלה",

  "table.name": "שם",
  "table.status": "סטטוס",
  "table.cameras": "מצלמות",
  "table.disks": "דיסקים",
  "table.recording": "הקלטה",
  "table.time": "שעון",
  "table.lastPoll": "סקר אחרון",
  "table.host": "כתובת",
  "table.webPort": "פורט Web",
  "table.sdkPort": "פורט SDK",
  "table.vendor": "יצרן",
  "table.response": "תגובה",
  "table.actions": "פעולות",
  "table.device": "מכשיר",
  "table.type": "סוג",
  "table.severity": "חומרה",
  "table.message": "הודעה",
  "table.created": "נוצר",
  "table.period": "תקופה",

  "status.online": "מחובר",
  "status.offline": "מנותק",
  "status.unknown": "לא ידוע",
  "status.reachable": "נגיש",
  "status.unreachable": "לא נגיש",
  "status.ok": "תקין",
  "status.error": "שגיאה",

  "devices.title": "מכשירים",
  "devices.addDevice": "הוסף מכשיר",
  "devices.search": "חיפוש",
  "devices.filterByTags": "סנן לפי תגיות",
  "devices.failedLoad": "טעינת מכשירים נכשלה",
  "devices.deleteConfirm": "למחוק מכשיר {{id}}?",
  "devices.deleteFailed": "המחיקה נכשלה",

  "deviceDetail.pollNow": "סקור עכשיו",
  "deviceDetail.edit": "ערוך",
  "deviceDetail.delete": "מחק",
  "deviceDetail.deleteConfirm": "למחוק מכשיר זה?",
  "deviceDetail.model": "דגם",
  "deviceDetail.serialNumber": "מספר סידורי",
  "deviceDetail.address": "כתובת",
  "deviceDetail.transport": "תעבורה",
  "deviceDetail.lastPoll": "סקר אחרון",
  "deviceDetail.credentials": "פרטי התחברות",
  "deviceDetail.timeSynced": "שעון מסונכרן",
  "deviceDetail.timeDrift": "סטיית שעון",
  "deviceDetail.cameras": "מצלמות",
  "deviceDetail.ignored": "מוחרגות",
  "deviceDetail.disks": "דיסקים",
  "deviceDetail.history": "היסטוריה",
  "deviceDetail.alerts": "התראות",
  "deviceDetail.noCameras": "לא נמצאו מצלמות",
  "deviceDetail.noDisks": "לא נמצאו דיסקים",
  "deviceDetail.noHistory": "אין נתוני היסטוריה עדיין",
  "deviceDetail.noAlerts": "אין התראות",
  "deviceDetail.notFound": "המכשיר לא נמצא",
  "deviceDetail.showCreds": "הצג פרטי התחברות",
  "deviceDetail.hideCreds": "הסתר פרטי התחברות",
  "deviceDetail.copyPassword": "העתק סיסמה",
  "deviceDetail.includeMonitor": "כלול בניטור",
  "deviceDetail.excludeMonitor": "הסר מניטור",
  "deviceDetail.ignore": "התעלם",
  "deviceDetail.addTag": "הוסף תגית",
  "deviceDetail.diskId": "מזהה דיסק",
  "deviceDetail.capacity": "קיבולת",
  "deviceDetail.freeSpace": "מקום פנוי",
  "deviceDetail.usedPct": "% בשימוש",
  "deviceDetail.health": "תקינות",
  "deviceDetail.temp": "טמפ׳",
  "deviceDetail.workingTime": "זמן עבודה",
  "deviceDetail.smart": "S.M.A.R.T.",
  "deviceDetail.responseTime": "זמן תגובה (ms)",
  "deviceDetail.onlineCameras": "מצלמות מחוברות",
  "deviceDetail.timeAxis": "זמן",

  "addDevice.title": "הוסף מכשיר",
  "addDevice.deviceId": "מזהה מכשיר",
  "addDevice.name": "שם",
  "addDevice.vendor": "יצרן",
  "addDevice.host": "כתובת",
  "addDevice.webPort": "פורט Web (ISAPI/HTTP)",
  "addDevice.sdkPort": "פורט SDK",
  "addDevice.transport": "תעבורה",
  "addDevice.transportIsapi": "ISAPI (HTTP/HTTPS)",
  "addDevice.transportSdk": "SDK (Device Network)",
  "addDevice.transportAuto": "אוטומטי",
  "addDevice.pollInterval": "מרווח סקירה (שניות, ריק = ללא סקירה אוטומטית)",
  "addDevice.pollIntervalHelp": "השאר ריק להשבתת סקירה אוטומטית",
  "addDevice.username": "שם משתמש",
  "addDevice.password": "סיסמה",
  "addDevice.submit": "הוסף מכשיר",
  "addDevice.submitting": "מוסיף...",
  "addDevice.cancel": "ביטול",
  "addDevice.failed": "הוספת מכשיר נכשלה",

  "editDevice.title": "עריכת מכשיר",
  "editDevice.save": "שמור",
  "editDevice.saving": "שומר...",
  "editDevice.cancel": "ביטול",
  "editDevice.failed": "העדכון נכשל",
  "editDevice.usernameCurrent": "שם משתמש (נוכחי: {{name}})",
  "editDevice.usernameKeep": "שם משתמש (השאר ריק לשמירה על הקיים)",
  "editDevice.passwordKeep": "סיסמה (השאר ריק לשמירה על הקיימת)",

  "alerts.title": "התראות",
  "alerts.statusAll": "הכל",
  "alerts.statusActive": "פעילות",
  "alerts.statusResolved": "נפתרו",
  "alerts.searchMessages": "חיפוש הודעות",
  "alerts.failedLoad": "טעינת התראות נכשלה",

  "pollLogs.title": "יומן סקרים",
  "pollLogs.refresh": "רענן",
  "pollLogs.searchDevice": "חיפוש מכשיר",
  "pollLogs.failedLoad": "טעינת יומן סקרים נכשלה",
  "pollLogs.last6h": "6 שעות אחרונות",
  "pollLogs.last12h": "12 שעות אחרונות",
  "pollLogs.last24h": "24 שעות אחרונות",
  "pollLogs.last48h": "48 שעות אחרונות",
  "pollLogs.last3d": "3 ימים אחרונים",
  "pollLogs.last7d": "7 ימים אחרונים",

  "settings.title": "הגדרות",
  "settings.polling": "סקירה",
  "settings.pollingDesc": "מרווח סקירה ברירת מחדל למכשירים ללא מרווח אישי.",
  "settings.defaultPollInterval": "מרווח סקירה ברירת מחדל",
  "settings.save": "שמור",
  "settings.saving": "שומר...",
  "settings.saved": "ההגדרות נשמרו",
  "settings.failedLoad": "טעינת הגדרות נכשלה",
  "settings.failedSave": "שמירת הגדרות נכשלה",
  "settings.language": "שפה",
  "settings.languageDesc": "בחר שפת תצוגה. עברית מפעילה תצוגה מימין לשמאל.",
  "settings.langEnglish": "English",
  "settings.langHebrew": "עברית",

  "settings.disabled": "מושבת",
  "settings.1min": "דקה",
  "settings.5min": "5 דקות",
  "settings.10min": "10 דקות",
  "settings.15min": "15 דקות",
  "settings.30min": "30 דקות",
  "settings.1hour": "שעה",

  "poll.title": "סקירת מכשיר",
  "poll.webPort": "פורט Web (TCP)",
  "poll.sdkPort": "פורט SDK (TCP)",
  "poll.connect": "חיבור",
  "poll.deviceInfo": "מידע מכשיר",
  "poll.cameras": "מצלמות",
  "poll.disks": "דיסקים",
  "poll.recording": "הקלטה",
  "poll.timeCheck": "שעון",
  "poll.retry": "נסה שנית",
  "poll.close": "סגור",

  "time.justNow": "עכשיו",
  "time.secsAgo": "לפני {{count}} שניות",
  "time.minsAgo": "לפני {{count}} דקות",
  "time.hoursAgo": "לפני {{count}} שעות",
  "time.daysAgo": "לפני {{count}} ימים",
  "time.never": "אף פעם",

  "common.loading": "טוען...",
  "common.poll": "סקור",
  "common.edit": "ערוך",
  "common.delete": "מחק",
  "common.na": "N/A"
}
```

**Step 4: Verify**

Run: `cd frontend && npx tsc --noEmit`
Expected: No errors

**Step 5: Commit**

```bash
git add frontend/src/i18n.ts frontend/src/locales/en.json frontend/src/locales/he.json
git commit -m "feat(i18n): add i18next config with English and Hebrew translations"
```

---

### Task 7: Wire i18n + RTL into App.tsx

**Files:**
- Modify: `frontend/src/App.tsx`

**Step 1: Add i18n import and RTL cache provider**

Update `App.tsx` to import i18n, create RTL Emotion cache, and rebuild theme on language change:

```typescript
import { useState, useMemo, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { CacheProvider } from '@emotion/react';
import createCache from '@emotion/cache';
import rtlPlugin from 'stylis-plugin-rtl';
import { prefixer } from 'stylis';
import { useTranslation } from 'react-i18next';
import './i18n.ts';
import { buildTheme, ThemeContext, type ThemeMode } from './theme.ts';
import Layout from './components/Layout.tsx';
import Dashboard from './pages/Dashboard.tsx';
import DeviceList from './pages/DeviceList.tsx';
import AddDevice from './pages/AddDevice.tsx';
import DeviceDetail from './pages/DeviceDetail.tsx';
import EditDevice from './pages/EditDevice.tsx';
import Alerts from './pages/Alerts.tsx';
import PollLogs from './pages/PollLogs.tsx';
import Settings from './pages/Settings.tsx';

function getStoredMode(): ThemeMode {
  const stored = localStorage.getItem('cctv-theme-mode');
  return stored === 'light' ? 'light' : 'dark';
}

const ltrCache = createCache({ key: 'mui-ltr' });
const rtlCache = createCache({ key: 'mui-rtl', stylisPlugins: [prefixer, rtlPlugin] });

export default function App() {
  const { i18n } = useTranslation();
  const [mode, setMode] = useState<ThemeMode>(getStoredMode);
  const direction = i18n.language === 'he' ? 'rtl' : 'ltr';

  const toggleTheme = () => {
    setMode((prev) => {
      const next = prev === 'dark' ? 'light' : 'dark';
      localStorage.setItem('cctv-theme-mode', next);
      return next;
    });
  };

  useEffect(() => {
    document.dir = direction;
    document.documentElement.lang = i18n.language;
  }, [direction, i18n.language]);

  const theme = useMemo(() => buildTheme(mode, direction), [mode, direction]);
  const cache = direction === 'rtl' ? rtlCache : ltrCache;

  return (
    <CacheProvider value={cache}>
      <ThemeContext.Provider value={{ mode, toggleTheme }}>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <BrowserRouter>
            <Routes>
              <Route element={<Layout />}>
                <Route path="/" element={<Dashboard />} />
                <Route path="/devices" element={<DeviceList />} />
                <Route path="/devices/add" element={<AddDevice />} />
                <Route path="/devices/:deviceId" element={<DeviceDetail />} />
                <Route path="/devices/:deviceId/edit" element={<EditDevice />} />
                <Route path="/poll-logs" element={<PollLogs />} />
                <Route path="/alerts" element={<Alerts />} />
                <Route path="/settings" element={<Settings />} />
                <Route path="*" element={<Navigate to="/" replace />} />
              </Route>
            </Routes>
          </BrowserRouter>
        </ThemeProvider>
      </ThemeContext.Provider>
    </CacheProvider>
  );
}
```

**Step 2: Verify**

Run: `cd frontend && npx tsc --noEmit`
Expected: No errors

**Step 3: Commit**

```bash
git add frontend/src/App.tsx
git commit -m "feat(i18n): wire i18n + RTL cache into App with direction support"
```

---

### Task 8: Add language switcher to Settings page

**Files:**
- Modify: `frontend/src/pages/Settings.tsx`

**Step 1: Add language section**

Add to the imports:
```typescript
import { useTranslation } from 'react-i18next';
```

Inside the component, add:
```typescript
const { t, i18n } = useTranslation();

const handleLanguageChange = (lang: string) => {
  i18n.changeLanguage(lang);
  localStorage.setItem('cctv-lang', lang);
};
```

After the Polling `<Paper>`, add a Language section:
```tsx
<Paper sx={{ p: 3, maxWidth: 500, mt: 3 }}>
  <Typography variant="h6" gutterBottom>
    {t('settings.language')}
  </Typography>
  <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
    {t('settings.languageDesc')}
  </Typography>
  <TextField
    select
    fullWidth
    label={t('settings.language')}
    value={i18n.language}
    onChange={(e) => handleLanguageChange(e.target.value)}
  >
    <MenuItem value="en">{t('settings.langEnglish')}</MenuItem>
    <MenuItem value="he">{t('settings.langHebrew')}</MenuItem>
  </TextField>
</Paper>
```

Also replace all hardcoded strings in Settings with `t()` calls:
- `"Settings"` → `{t('settings.title')}`
- `"Polling"` → `{t('settings.polling')}`
- description text → `{t('settings.pollingDesc')}`
- `"Default Poll Interval"` → `{t('settings.defaultPollInterval')}`
- `"Save"` / `"Saving..."` → `{t('settings.save')}` / `{t('settings.saving')}`
- `"Settings saved"` → `t('settings.saved')`
- Error messages → use `t()` keys
- Interval option labels → use `t('settings.disabled')`, `t('settings.1min')`, etc.
- `"Loading..."` → `{t('common.loading')}`

**Step 2: Verify language switch works**

Run: `cd frontend && npm run dev`
Go to Settings, switch to עברית — entire page should mirror to RTL.

**Step 3: Commit**

```bash
git add frontend/src/pages/Settings.tsx
git commit -m "feat(i18n): add language switcher to Settings + translate Settings page"
```

---

### Task 9: Translate Layout.tsx

**Files:**
- Modify: `frontend/src/components/Layout.tsx`

**Step 1: Add translations**

Add import:
```typescript
import { useTranslation } from 'react-i18next';
```

Inside `Layout()`:
```typescript
const { t } = useTranslation();
```

Update `navItems` to use translation keys:
```typescript
const navItems = [
  { label: t('nav.dashboard'), icon: <DashboardIcon />, to: '/' },
  { label: t('nav.devices'), icon: <RouterIcon />, to: '/devices' },
  { label: t('nav.pollLogs'), icon: <HistoryIcon />, to: '/poll-logs' },
  { label: t('nav.alerts'), icon: <NotificationsIcon />, to: '/alerts' },
  { label: t('nav.settings'), icon: <SettingsIcon />, to: '/settings' },
];
```

Update brand text: `{t('nav.brand')}`

**NOTE**: `navItems` must move inside the function body (after `useTranslation` hook).

**Step 2: Handle RTL drawer border**

In the MUI theme's `MuiDrawer` override, `borderRight` should become `borderInlineEnd` so it auto-mirrors for RTL. However, since the theme's `direction: 'rtl'` with stylis-plugin-rtl should handle this automatically, no manual change needed.

**Step 3: Commit**

```bash
git add frontend/src/components/Layout.tsx
git commit -m "feat(i18n): translate Layout navigation items"
```

---

### Task 10: Translate Dashboard.tsx

**Files:**
- Modify: `frontend/src/pages/Dashboard.tsx`

**Step 1: Add translations**

Add import:
```typescript
import { useTranslation } from 'react-i18next';
```

At the top of `Dashboard()`:
```typescript
const { t } = useTranslation();
```

Replace all hardcoded strings with `t()` calls. Key replacements:
- `'Dashboard'` → `t('dashboard.title')`
- Stat card titles: `'Devices'` → `t('dashboard.devices')`, `'Cameras'` → `t('dashboard.cameras')`, etc.
- Subtitle logic: `'all online'` → `t('dashboard.allOnline')`, `` `${count} offline` `` → `t('dashboard.offline', { count })`, etc.
- `'Active Alerts'` → `t('dashboard.activeAlerts')`
- `'No active alerts'` → `t('dashboard.noActiveAlerts')`
- `'No devices configured'` → `t('dashboard.noDevices')`
- Table headers: `'Name'` → `t('table.name')`, etc.
- `'Online'` / `'Offline'` → `t('status.online')` / `t('status.offline')`
- `'never'` → `t('time.never')`

**NOTE**: `borderLeft` on stat cards should become `borderInlineStart` for RTL support:
```tsx
sx={{ borderInlineStart: `4px solid ${c.accent}` }}
```

**Step 2: Verify**

Run: `cd frontend && npx tsc --noEmit`

**Step 3: Commit**

```bash
git add frontend/src/pages/Dashboard.tsx
git commit -m "feat(i18n): translate Dashboard page"
```

---

### Task 11: Translate remaining pages (DeviceList, Alerts, PollLogs, AddDevice, EditDevice, DeviceDetail, PollDialog)

**Files:**
- Modify: `frontend/src/pages/DeviceList.tsx`
- Modify: `frontend/src/pages/Alerts.tsx`
- Modify: `frontend/src/pages/PollLogs.tsx`
- Modify: `frontend/src/pages/AddDevice.tsx`
- Modify: `frontend/src/pages/EditDevice.tsx`
- Modify: `frontend/src/pages/DeviceDetail.tsx`
- Modify: `frontend/src/components/PollDialog.tsx`

**Step 1: Apply `useTranslation` pattern to each file**

In every file:
1. Add `import { useTranslation } from 'react-i18next';`
2. Add `const { t } = useTranslation();` at the top of the component
3. Replace every hardcoded English string with the corresponding `t()` key from `en.json`

Key patterns per file:

**DeviceList.tsx:**
- `'Devices'` → `t('devices.title')`, column headers → `t('table.*')`, `'Add Device'` → `t('devices.addDevice')`
- `'Search'` → `t('devices.search')`, `'Filter by tags'` → `t('devices.filterByTags')`
- Status labels use `t('status.*')`

**Alerts.tsx:**
- `'Alerts'` → `t('alerts.title')`, filter labels → `t('alerts.*')`
- Column headers → `t('table.*')`

**PollLogs.tsx:**
- `'Poll Logs'` → `t('pollLogs.title')`, period labels → `t('pollLogs.*')`

**AddDevice.tsx:**
- All form labels → `t('addDevice.*')`

**EditDevice.tsx:**
- All form labels → `t('editDevice.*')`, shared labels with addDevice

**DeviceDetail.tsx:**
- Info labels → `t('deviceDetail.*')`, tab labels → `t('deviceDetail.*')`
- `borderLeft` → `borderInlineStart` on camera cards
- Tooltip texts → `t()`

**PollDialog.tsx:**
- `STEP_LABELS` values → use `t('poll.*')`
- Note: `STEP_LABELS` must be moved inside the component (or made into a function that receives `t`)

**Step 2: Verify all files compile**

Run: `cd frontend && npx tsc --noEmit`

**Step 3: Commit**

```bash
git add frontend/src/pages/ frontend/src/components/PollDialog.tsx
git commit -m "feat(i18n): translate all remaining pages and PollDialog"
```

---

## Phase 3: Responsive Layout

### Task 12: Make Layout responsive with collapsible sidebar

**Files:**
- Modify: `frontend/src/components/Layout.tsx`

**Step 1: Add responsive drawer logic**

Add imports:
```typescript
import useMediaQuery from '@mui/material/useMediaQuery';
import { useTheme } from '@mui/material/styles';
import MenuIcon from '@mui/icons-material/Menu';
```

Inside `Layout()`:
```typescript
const muiTheme = useTheme();
const isMdUp = useMediaQuery(muiTheme.breakpoints.up('md'));
const [mobileOpen, setMobileOpen] = useState(false);
```

Add `useState` to the imports from react.

Replace the `<Drawer>` with:
```tsx
<Drawer
  variant={isMdUp ? 'permanent' : 'temporary'}
  open={isMdUp ? true : mobileOpen}
  onClose={() => setMobileOpen(false)}
  sx={{
    width: DRAWER_WIDTH,
    flexShrink: 0,
    '& .MuiDrawer-paper': {
      width: DRAWER_WIDTH,
      boxSizing: 'border-box',
    },
  }}
>
  {/* same inner content — logo + nav */}
</Drawer>
```

Add hamburger icon to AppBar (only on small screens):
```tsx
<Toolbar variant="dense" sx={{ justifyContent: 'space-between' }}>
  {!isMdUp && (
    <IconButton color="inherit" edge="start" onClick={() => setMobileOpen(true)}>
      <MenuIcon />
    </IconButton>
  )}
  <Box sx={{ flex: 1 }} />
  <IconButton onClick={toggleTheme} color="inherit" size="small">
    {mode === 'dark' ? <Brightness7Icon /> : <Brightness4Icon />}
  </IconButton>
</Toolbar>
```

Update main content area width for mobile:
```tsx
<Box
  component="main"
  sx={{
    flexGrow: 1,
    p: { xs: 1.5, sm: 2, md: 3 },
    width: { md: `calc(100% - ${DRAWER_WIDTH}px)` },
    mt: '48px',
  }}
>
```

**Step 2: Close drawer on nav click (mobile)**

Add `onClick` to each `ListItemButton`:
```tsx
onClick={() => { if (!isMdUp) setMobileOpen(false); }}
```

**Step 3: Verify**

Open dev tools, resize to mobile — hamburger should appear, sidebar should slide in/out.

**Step 4: Commit**

```bash
git add frontend/src/components/Layout.tsx
git commit -m "feat(responsive): add collapsible sidebar with hamburger on mobile"
```

---

### Task 13: Make Dashboard responsive

**Files:**
- Modify: `frontend/src/pages/Dashboard.tsx`

**Step 1: Adjust stat cards grid**

Already uses `repeat(auto-fit, minmax(180px, 1fr))` which is naturally responsive. Reduce minimum width on mobile:
```tsx
gridTemplateColumns: { xs: 'repeat(2, 1fr)', sm: 'repeat(auto-fit, minmax(180px, 1fr))' },
```

**Step 2: Stack device table + alerts vertically on mobile**

The grid already has `gridTemplateColumns: { xs: '1fr', md: '2fr 1fr' }` — this is correct.

**Step 3: Make the device table horizontally scrollable on mobile**

Wrap the `<Table>` in a Box with overflow:
```tsx
<Box sx={{ overflowX: 'auto' }}>
  <Table size="small" sx={{ minWidth: 600 }}>
    {/* existing content */}
  </Table>
</Box>
```

**Step 4: Commit**

```bash
git add frontend/src/pages/Dashboard.tsx
git commit -m "feat(responsive): make Dashboard layout mobile-friendly"
```

---

### Task 14: Make DeviceList responsive

**Files:**
- Modify: `frontend/src/pages/DeviceList.tsx`

**Step 1: Stack header controls on mobile**

```tsx
<Box display="flex" justifyContent="space-between" alignItems="center" mb={2} flexWrap="wrap" gap={1}>
```

**Step 2: Stack search/filter row**

```tsx
<Box display="flex" gap={2} mb={2} flexWrap="wrap">
  <TextField ... sx={{ width: { xs: '100%', sm: 250 } }} />
  <Autocomplete ... sx={{ minWidth: { xs: '100%', sm: 250 } }} />
</Box>
```

**Step 3: Make DataGrid scrollable on mobile**

Wrap DataGrid in:
```tsx
<Box sx={{ width: '100%', overflowX: 'auto' }}>
  <Box sx={{ minWidth: 900 }}>
    <DataGrid ... />
  </Box>
</Box>
```

**Step 4: Commit**

```bash
git add frontend/src/pages/DeviceList.tsx
git commit -m "feat(responsive): make DeviceList mobile-friendly"
```

---

### Task 15: Make remaining pages responsive

**Files:**
- Modify: `frontend/src/pages/Alerts.tsx`
- Modify: `frontend/src/pages/PollLogs.tsx`
- Modify: `frontend/src/pages/AddDevice.tsx`
- Modify: `frontend/src/pages/EditDevice.tsx`
- Modify: `frontend/src/pages/DeviceDetail.tsx`

**Step 1: Alerts and PollLogs — wrap DataGrid in scrollable container**

Same pattern as DeviceList:
```tsx
<Box sx={{ width: '100%', overflowX: 'auto' }}>
  <Box sx={{ minWidth: 700 }}>
    <DataGrid ... />
  </Box>
</Box>
```

Search/filter inputs: `sx={{ width: { xs: '100%', sm: 250 } }}`

**Step 2: AddDevice and EditDevice — full-width on mobile**

```tsx
<Paper sx={{ maxWidth: { xs: '100%', sm: 520 }, p: { xs: 2, sm: 3 } }}>
```

**Step 3: DeviceDetail — responsive header**

The header already uses `flexWrap="wrap"`. Ensure buttons stack on mobile:
```tsx
<Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
```

Camera grid already uses `repeat(auto-fill, minmax(220px, 1fr))` — adjust minimum:
```tsx
gridTemplateColumns: { xs: 'repeat(auto-fill, minmax(160px, 1fr))', sm: 'repeat(auto-fill, minmax(220px, 1fr))' }
```

Disks DataGrid — wrap in scrollable container.

History chart — add responsive height:
```tsx
<LineChart height={window.innerWidth < 600 ? 250 : 350} ... />
```

Actually, avoid `window.innerWidth` in React. Use a simpler approach — the LineChart will auto-scale via its container. Just ensure the container doesn't overflow.

**Step 4: Verify all pages on narrow viewport**

Run: `cd frontend && npm run dev`
Open dev tools, test at 375px, 768px, 1024px, 1440px widths.

**Step 5: Commit**

```bash
git add frontend/src/pages/
git commit -m "feat(responsive): make all remaining pages mobile-friendly"
```

---

### Task 16: Final verification and cleanup

**Step 1: Type check**

Run: `cd frontend && npx tsc --noEmit`
Expected: No errors

**Step 2: Build**

Run: `cd frontend && npm run build`
Expected: Build succeeds

**Step 3: Visual QA checklist**

- [ ] Dark theme: all pages look correct
- [ ] Light theme: all pages look correct, sidebar stays dark
- [ ] English: all strings display correctly
- [ ] Hebrew: all strings display, layout is fully RTL-mirrored
- [ ] Mobile (375px): hamburger menu, stacked layouts, scrollable tables
- [ ] Tablet (768px): sidebar hidden by default, 2-column grids
- [ ] Desktop (1440px): permanent sidebar, full layouts

**Step 4: Commit any fixes**

```bash
git add -A
git commit -m "fix: visual QA fixes for theme, i18n, and responsive"
```

---

Plan complete and saved to `docs/plans/2026-03-09-frontend-theme-i18n-responsive-plan.md`. Two execution options:

**1. Subagent-Driven (this session)** — I dispatch fresh subagent per task, review between tasks, fast iteration

**2. Parallel Session (separate)** — Open new session with executing-plans, batch execution with checkpoints

Which approach?
