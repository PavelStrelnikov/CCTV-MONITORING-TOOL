import { useState, useEffect, memo } from 'react';
import { NavLink, Outlet } from 'react-router-dom';
import AppBar from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import Drawer from '@mui/material/Drawer';
import IconButton from '@mui/material/IconButton';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Toolbar from '@mui/material/Toolbar';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import useMediaQuery from '@mui/material/useMediaQuery';
import { useTheme } from '@mui/material/styles';
import Brightness4Icon from '@mui/icons-material/Brightness4';
import Brightness7Icon from '@mui/icons-material/Brightness7';
import ExitToAppIcon from '@mui/icons-material/ExitToApp';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import DashboardIcon from '@mui/icons-material/Dashboard';
import MenuIcon from '@mui/icons-material/Menu';
import RouterIcon from '@mui/icons-material/Router';
import HistoryIcon from '@mui/icons-material/History';
import NotificationsIcon from '@mui/icons-material/Notifications';
import SettingsIcon from '@mui/icons-material/Settings';
import VideocamIcon from '@mui/icons-material/Videocam';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { useThemeMode } from '../theme.ts';
import { clearAuthToken } from '../api/client';

const DRAWER_WIDTH = 220;
const DRAWER_COLLAPSED = 64;

/** Isolated clock component — ticks every second without re-rendering the rest of Layout */
const AppBarClock = memo(function AppBarClock({ locale }: { locale: string }) {
  const [now, setNow] = useState(new Date());
  useEffect(() => {
    const timer = setInterval(() => setNow(new Date()), 1000);
    return () => clearInterval(timer);
  }, []);
  const loc = locale === 'he' ? 'he-IL' : 'en-GB';
  return (
    <>
      <Typography variant="body2" sx={{ opacity: 0.85, fontVariantNumeric: 'tabular-nums', display: { xs: 'none', sm: 'block' } }}>
        {now.toLocaleDateString(loc, { weekday: 'short', day: 'numeric', month: 'short', year: 'numeric' })}
      </Typography>
      <Typography variant="body2" sx={{ fontWeight: 600, fontVariantNumeric: 'tabular-nums', minWidth: 56 }}>
        {now.toLocaleTimeString(loc, { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
      </Typography>
    </>
  );
});

export default function Layout() {
  const { mode, toggleTheme } = useThemeMode();
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const muiTheme = useTheme();
  const isMdUp = useMediaQuery(muiTheme.breakpoints.up('md'));
  const [mobileOpen, setMobileOpen] = useState(false);
  const [collapsed, setCollapsed] = useState(false);
  const isRtl = i18n.language === 'he';

  const drawerWidth = isMdUp && collapsed ? DRAWER_COLLAPSED : DRAWER_WIDTH;

  const navItems = [
    { label: t('nav.dashboard'), icon: <DashboardIcon />, to: '/', color: '#3B82F6' },
    { label: t('nav.devices'), icon: <RouterIcon />, to: '/devices', color: '#22C55E' },
    { label: t('nav.pollLogs'), icon: <HistoryIcon />, to: '/poll-logs', color: '#8B5CF6' },
    { label: t('nav.alerts'), icon: <NotificationsIcon />, to: '/alerts', color: '#F59E0B' },
    { label: t('nav.settings'), icon: <SettingsIcon />, to: '/settings', color: '#6B7280' },
  ];

  const drawerContent = (
    <>
      {/* Logo / brand */}
      <Box sx={{ px: collapsed ? 0 : 2, py: 2, display: 'flex', alignItems: 'center', justifyContent: collapsed ? 'center' : 'flex-start', gap: 1.5 }}>
        <VideocamIcon sx={{ color: 'primary.main', fontSize: 28 }} />
        {!collapsed && (
          <Typography variant="h6" sx={{ fontWeight: 700, letterSpacing: '-0.02em', whiteSpace: 'nowrap' }}>
            {t('nav.brand')}
          </Typography>
        )}
      </Box>

      <List sx={{ px: 0.5 }}>
        {navItems.map((item) => (
          <Tooltip key={item.to} title={collapsed ? item.label : ''} placement={isRtl ? 'left' : 'right'}>
            <ListItemButton
              component={NavLink}
              to={item.to}
              end={item.to === '/'}
              onClick={() => { if (!isMdUp) setMobileOpen(false); }}
              sx={{
                justifyContent: collapsed ? 'center' : 'flex-start',
                px: collapsed ? 1 : undefined,
              }}
            >
              <ListItemIcon sx={{
                color: item.color,
                minWidth: collapsed ? 0 : 40,
                justifyContent: 'center',
              }}>
                {item.icon}
              </ListItemIcon>
              {!collapsed && (
                <ListItemText
                  primary={item.label}
                  primaryTypographyProps={{ fontSize: '0.875rem', fontWeight: 500 }}
                />
              )}
            </ListItemButton>
          </Tooltip>
        ))}
      </List>

      {/* Collapse toggle — desktop only */}
      {isMdUp && (
        <Box sx={{ mt: 'auto', p: 1, display: 'flex', justifyContent: 'center' }}>
          <IconButton size="small" onClick={() => setCollapsed((p) => !p)}>
            {(collapsed !== isRtl) ? <ChevronRightIcon /> : <ChevronLeftIcon />}
          </IconButton>
        </Box>
      )}
    </>
  );

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <AppBar
        position="fixed"
        sx={{ zIndex: (th) => th.zIndex.drawer + 1 }}
      >
        <Toolbar variant="dense" sx={{ gap: 1 }}>
          {!isMdUp && (
            <IconButton color="inherit" edge="start" onClick={() => setMobileOpen(true)}>
              <MenuIcon />
            </IconButton>
          )}
          <Typography variant="subtitle1" sx={{ fontWeight: 700, letterSpacing: '0.04em' }}>
            {t('nav.brand')}
          </Typography>
          <Box sx={{ flex: 1 }} />
          <AppBarClock locale={i18n.language} />
          <IconButton onClick={toggleTheme} color="inherit" size="small" sx={{ ml: 0.5 }}>
            {mode === 'dark' ? <Brightness7Icon /> : <Brightness4Icon />}
          </IconButton>
          <Tooltip title={t('nav.logout', 'Logout')}>
            <IconButton
              color="inherit"
              size="small"
              onClick={() => { clearAuthToken(); navigate('/login', { replace: true }); }}
            >
              <ExitToAppIcon />
            </IconButton>
          </Tooltip>
        </Toolbar>
      </AppBar>

      <Drawer
        variant={isMdUp ? 'permanent' : 'temporary'}
        open={isMdUp ? true : mobileOpen}
        onClose={() => setMobileOpen(false)}
        sx={{
          width: drawerWidth,
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width: drawerWidth,
            boxSizing: 'border-box',
            transition: 'width 0.2s ease',
            overflowX: 'hidden',
            display: 'flex',
            flexDirection: 'column',
          },
        }}
      >
        {drawerContent}
      </Drawer>

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: { xs: 1.5, sm: 2, md: 3 },
          width: { md: `calc(100% - ${drawerWidth}px)` },
          transition: 'width 0.2s ease',
          mt: '48px', // dense toolbar height
        }}
      >
        <Outlet />
      </Box>
    </Box>
  );
}
