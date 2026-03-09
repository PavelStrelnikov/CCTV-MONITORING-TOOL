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
