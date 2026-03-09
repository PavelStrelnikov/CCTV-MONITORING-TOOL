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
