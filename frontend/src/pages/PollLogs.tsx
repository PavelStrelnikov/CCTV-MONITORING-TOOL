import { useEffect, useState, useCallback } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Chip from '@mui/material/Chip';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import Alert from '@mui/material/Alert';
import Link from '@mui/material/Link';
import IconButton from '@mui/material/IconButton';
import RefreshIcon from '@mui/icons-material/Refresh';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import { api } from '../api/client.ts';
import { timeAgo } from '../utils/formatTime.ts';
import { getDataGridSx, useThemeMode } from '../theme.ts';
import type { PollLogEntry } from '../types.ts';

export default function PollLogs() {
  const { t } = useTranslation();
  const { mode } = useThemeMode();
  const [logs, setLogs] = useState<PollLogEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [hours, setHours] = useState(24);
  const [search, setSearch] = useState('');

  const fetchLogs = useCallback(async () => {
    try {
      setLoading(true);
      const data = await api.getPollLogs(hours);
      setLogs(data);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : t('pollLogs.failedLoad'));
    } finally {
      setLoading(false);
    }
  }, [hours, t]);

  useEffect(() => {
    fetchLogs();
  }, [fetchLogs]);

  const filtered = search
    ? logs.filter(
        (l) =>
          l.device_name.toLowerCase().includes(search.toLowerCase()) ||
          l.device_id.toLowerCase().includes(search.toLowerCase()),
      )
    : logs;

  const columns: GridColDef<PollLogEntry>[] = [
    {
      field: 'checked_at',
      headerName: t('table.time'),
      width: 140,
      valueGetter: (value: string) => timeAgo(value),
    },
    {
      field: 'device_name',
      headerName: t('table.device'),
      flex: 1,
      minWidth: 180,
      renderCell: (params) => (
        <Link component={RouterLink} to={`/devices/${params.row.device_id}`} underline="hover">
          {params.value}
        </Link>
      ),
    },
    {
      field: 'reachable',
      headerName: t('table.status'),
      width: 110,
      renderCell: (params) => (
        <Chip
          label={params.value ? t('status.reachable') : t('status.unreachable')}
          size="small"
          color={params.value ? 'success' : 'error'}
        />
      ),
    },
    {
      field: 'online_cameras',
      headerName: t('table.cameras'),
      width: 120,
      renderCell: (params) => {
        const row = params.row;
        if (!row.reachable) return <span style={{ color: '#64748B' }}>—</span>;
        const allOnline = row.online_cameras === row.camera_count;
        return (
          <span style={{ color: allOnline ? undefined : '#F59E0B' }}>
            {row.online_cameras}/{row.camera_count}
          </span>
        );
      },
    },
    {
      field: 'disk_ok',
      headerName: t('table.disks'),
      width: 100,
      renderCell: (params) => {
        if (!params.row.reachable) return <span style={{ color: '#64748B' }}>—</span>;
        return (
          <Chip
            label={params.value ? t('status.ok') : t('status.error')}
            size="small"
            color={params.value ? 'success' : 'error'}
          />
        );
      },
    },
    {
      field: 'response_time_ms',
      headerName: t('table.response'),
      width: 110,
      renderCell: (params) => {
        if (!params.row.reachable) return <span style={{ color: '#64748B' }}>—</span>;
        const ms = params.value as number;
        return `${ms.toFixed(0)} ms`;
      },
    },
  ];

  return (
    <Box>
      <Box display="flex" alignItems="center" gap={1} mb={1}>
        <Typography variant="h4">{t('pollLogs.title')}</Typography>
        <IconButton onClick={fetchLogs} size="small" title={t('pollLogs.refresh')}>
          <RefreshIcon />
        </IconButton>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      <Box display="flex" gap={2} mb={2}>
        <TextField
          select
          size="small"
          label={t('table.period')}
          value={hours}
          onChange={(e) => setHours(Number(e.target.value))}
          sx={{ width: 160 }}
        >
          <MenuItem value={6}>{t('pollLogs.last6h')}</MenuItem>
          <MenuItem value={12}>{t('pollLogs.last12h')}</MenuItem>
          <MenuItem value={24}>{t('pollLogs.last24h')}</MenuItem>
          <MenuItem value={48}>{t('pollLogs.last48h')}</MenuItem>
          <MenuItem value={72}>{t('pollLogs.last3d')}</MenuItem>
          <MenuItem value={168}>{t('pollLogs.last7d')}</MenuItem>
        </TextField>
        <TextField
          size="small"
          label={t('pollLogs.searchDevice')}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          sx={{ width: 250 }}
        />
      </Box>

      <DataGrid
        rows={filtered}
        columns={columns}
        loading={loading}
        density="compact"
        pageSizeOptions={[25, 50, 100]}
        initialState={{
          pagination: { paginationModel: { pageSize: 50 } },
          sorting: { sortModel: [{ field: 'checked_at', sort: 'desc' }] },
        }}
        getRowId={(row) => `${row.device_id}_${row.checked_at}`}
        disableRowSelectionOnClick
        autoHeight
        sx={{
          ...getDataGridSx(mode),
          '& .MuiDataGrid-cell': { display: 'flex', alignItems: 'center' },
        }}
      />
    </Box>
  );
}
