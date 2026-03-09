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
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import { api } from '../api/client.ts';
import { timeAgo } from '../utils/formatTime.ts';
import { getDataGridSx, useThemeMode } from '../theme.ts';
import type { Alert as AlertType } from '../types.ts';

export default function Alerts() {
  const { t } = useTranslation();
  const { mode } = useThemeMode();
  const [alerts, setAlerts] = useState<AlertType[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [search, setSearch] = useState('');

  const fetchAlerts = useCallback(async () => {
    try {
      const params: { status?: string } = {};
      if (statusFilter !== 'all') params.status = statusFilter;
      const data = await api.getAlerts(params);
      setAlerts(data);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : t('alerts.failedLoad'));
    } finally {
      setLoading(false);
    }
  }, [statusFilter, t]);

  useEffect(() => {
    setLoading(true);
    fetchAlerts();
  }, [fetchAlerts]);

  const filtered = search
    ? alerts.filter((a) => {
        const q = search.toLowerCase();
        return a.message.toLowerCase().includes(q) || a.device_name.toLowerCase().includes(q);
      })
    : alerts;

  const severityColor = (sev: string): 'error' | 'warning' | 'info' | 'default' => {
    switch (sev.toLowerCase()) {
      case 'critical':
        return 'error';
      case 'warning':
        return 'warning';
      case 'info':
        return 'info';
      default:
        return 'default';
    }
  };

  const columns: GridColDef<AlertType>[] = [
    {
      field: 'device_name',
      headerName: t('table.device'),
      width: 220,
      renderCell: (params) => (
        <Link component={RouterLink} to={`/devices/${params.row.device_id}`} underline="hover" color="inherit">
          {params.value || params.row.device_id}
        </Link>
      ),
    },
    {
      field: 'alert_type',
      headerName: t('table.type'),
      width: 160,
      valueFormatter: (value: string) => value?.replace(/_/g, ' ') ?? '',
    },
    {
      field: 'severity',
      headerName: t('table.severity'),
      width: 120,
      renderCell: (params) => (
        <Chip label={params.value} size="small" color={severityColor(params.value as string)} />
      ),
    },
    {
      field: 'message',
      headerName: t('table.message'),
      flex: 1,
      minWidth: 200,
    },
    {
      field: 'status',
      headerName: t('table.status'),
      width: 110,
      renderCell: (params) => (
        <Chip
          label={params.value}
          size="small"
          color={params.value === 'active' ? 'error' : 'default'}
          variant="outlined"
        />
      ),
    },
    {
      field: 'created_at',
      headerName: t('table.created'),
      width: 130,
      valueGetter: (value: string) => timeAgo(value),
    },
  ];

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        {t('alerts.title')}
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      <Box display="flex" gap={2} mb={2} flexWrap="wrap">
        <TextField
          select
          size="small"
          label={t('table.status')}
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          sx={{ width: { xs: '100%', sm: 160 } }}
        >
          <MenuItem value="all">{t('alerts.statusAll')}</MenuItem>
          <MenuItem value="active">{t('alerts.statusActive')}</MenuItem>
          <MenuItem value="resolved">{t('alerts.statusResolved')}</MenuItem>
        </TextField>
        <TextField
          size="small"
          label={t('alerts.searchMessages')}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          sx={{ width: { xs: '100%', sm: 250 } }}
        />
      </Box>

      <Box sx={{ width: '100%', overflowX: 'auto' }}>
        <Box sx={{ minWidth: 700 }}>
          <DataGrid
            rows={filtered}
            columns={columns}
            loading={loading}
            density="compact"
            pageSizeOptions={[10, 25, 50]}
            initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
            disableRowSelectionOnClick
            autoHeight
            sx={{
              ...getDataGridSx(mode),
              '& .MuiDataGrid-cell': { display: 'flex', alignItems: 'center' },
            }}
          />
        </Box>
      </Box>
    </Box>
  );
}
