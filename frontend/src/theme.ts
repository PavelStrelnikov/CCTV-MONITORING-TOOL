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
  const border = isDark ? '#1E293B' : '#D5D9E2';
  const paperBg = isDark ? '#111827' : '#F0EDE8';
  const filledBg = isDark ? '#1E293B' : '#E8E4DF';
  const filledHover = isDark ? '#283548' : '#DDD9D3';
  const sidebarBg = isDark ? '#0F1629' : '#E8E5E0';
  const appBarBg = isDark ? '#0F1629' : '#E8E5E0';

  return {
    MuiCssBaseline: {
      styleOverrides: {
        body: { backgroundColor: isDark ? '#0B0F1A' : '#E4E0DB' },
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
          color: isDark ? '#F1F5F9' : '#2D3748',
          borderBottom: `1px solid ${border}`,
          boxShadow: 'none',
        },
      },
    },
    MuiDrawer: {
      styleOverrides: {
        paper: {
          backgroundColor: sidebarBg,
          color: isDark ? '#F1F5F9' : '#2D3748',
          borderRight: `1px solid ${border}`,
        },
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
      styleOverrides: { root: { minWidth: 40 } },
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
        default: isDark ? '#0B0F1A' : '#E4E0DB',
        paper: isDark ? '#111827' : '#F0EDE8',
      },
      text: {
        primary: isDark ? '#F1F5F9' : '#2D3748',
        secondary: isDark ? '#94A3B8' : '#6B7280',
      },
      divider: isDark ? '#1E293B' : '#D5D9E2',
    },
    shape,
    typography,
    components: getComponents(mode) as any,
  });
}

/** DataGrid styling — adapts to theme mode */
export function getDataGridSx(mode: 'light' | 'dark') {
  const isDark = mode === 'dark';
  const border = isDark ? '#1E293B' : '#D5D9E2';
  const headerBg = isDark ? '#0F172A' : '#E8E5E0';
  const stripeBg = isDark ? '#0F172A' : '#EBE8E3';
  const hoverBg = isDark ? '#1E293B' : '#E0DDD7';

  const headerColor = isDark ? '#E2E8F0' : '#1E293B';

  return {
    border: `1px solid ${border}`,
    '& .MuiDataGrid-columnHeaders': {
      backgroundColor: headerBg,
      borderBottom: `2px solid ${border}`,
    },
    '& .MuiDataGrid-columnHeaderTitle': {
      fontWeight: 700,
      fontSize: '0.85rem',
      color: headerColor,
      textTransform: 'uppercase' as const,
      letterSpacing: '0.03em',
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
