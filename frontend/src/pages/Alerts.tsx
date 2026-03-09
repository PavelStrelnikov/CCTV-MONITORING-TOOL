import { useEffect, useState, useCallback } from 'react';
import { Link as RouterLink } from 'react-router-dom';
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
      setError(err instanceof Error ? err.message : 'Failed to load alerts');
    } finally {
      setLoading(false);
    }
  }, [statusFilter]);

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
      headerName: 'Device',
      width: 220,
      renderCell: (params) => (
        <Link component={RouterLink} to={`/devices/${params.row.device_id}`} underline="hover" color="inherit">
          {params.value || params.row.device_id}
        </Link>
      ),
    },
    {
      field: 'alert_type',
      headerName: 'Type',
      width: 160,
      valueFormatter: (value: string) => value?.replace(/_/g, ' ') ?? '',
    },
    {
      field: 'severity',
      headerName: 'Severity',
      width: 120,
      renderCell: (params) => (
        <Chip label={params.value} size="small" color={severityColor(params.value as string)} />
      ),
    },
    {
      field: 'message',
      headerName: 'Message',
      flex: 1,
      minWidth: 200,
    },
    {
      field: 'status',
      headerName: 'Status',
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
      headerName: 'Created',
      width: 130,
      valueGetter: (value: string) => timeAgo(value),
    },
  ];

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Alerts
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      <Box display="flex" gap={2} mb={2}>
        <TextField
          select
          size="small"
          label="Status"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          sx={{ width: 160 }}
        >
          <MenuItem value="all">All</MenuItem>
          <MenuItem value="active">Active</MenuItem>
          <MenuItem value="resolved">Resolved</MenuItem>
        </TextField>
        <TextField
          size="small"
          label="Search messages"
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
        initialState={{ pagination: { paginationModel: { pageSize: 25 } } }}
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
