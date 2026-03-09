import { useEffect, useState, useCallback } from 'react';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import Autocomplete from '@mui/material/Autocomplete';
import Chip from '@mui/material/Chip';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import Alert from '@mui/material/Alert';
import Link from '@mui/material/Link';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import AddIcon from '@mui/icons-material/Add';
import RefreshIcon from '@mui/icons-material/Refresh';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import { api } from '../api/client.ts';
import { timeAgo } from '../utils/formatTime.ts';
import { getDataGridSx, useThemeMode } from '../theme.ts';
import type { Device, Tag } from '../types.ts';
import PollDialog from '../components/PollDialog.tsx';

export default function DeviceList() {
  const { mode } = useThemeMode();
  const navigate = useNavigate();
  const [devices, setDevices] = useState<Device[]>([]);
  const [allTags, setAllTags] = useState<Tag[]>([]);
  const [selectedTags, setSelectedTags] = useState<string[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [pollDialogDevice, setPollDialogDevice] = useState<Device | null>(null);

  const fetchDevices = useCallback(async () => {
    try {
      const data = await api.getDevices();
      setDevices(data);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load devices');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchDevices();
    api.getTags().then(setAllTags).catch(() => {});
    const interval = setInterval(fetchDevices, 15000);
    return () => clearInterval(interval);
  }, [fetchDevices]);

  const handlePoll = (device: Device) => {
    setPollDialogDevice(device);
  };

  const handlePollComplete = () => {
    fetchDevices();
  };

  const handleDelete = async (deviceId: string) => {
    if (!confirm(`Delete device ${deviceId}?`)) return;
    try {
      await api.deleteDevice(deviceId);
      setDevices((prev) => prev.filter((d) => d.device_id !== deviceId));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Delete failed');
    }
  };

  const statusLabel = (d: Device) => {
    if (!d.last_health) return 'UNKNOWN';
    return d.last_health.reachable ? 'ONLINE' : 'OFFLINE';
  };

  const statusColor = (d: Device): 'success' | 'error' | 'default' => {
    if (!d.last_health) return 'default';
    return d.last_health.reachable ? 'success' : 'error';
  };

  const filtered = devices.filter((d) => {
    if (search) {
      const q = search.toLowerCase();
      if (
        !d.name.toLowerCase().includes(q) &&
        !d.host.toLowerCase().includes(q) &&
        !d.device_id.toLowerCase().includes(q)
      )
        return false;
    }
    if (selectedTags.length > 0) {
      if (!selectedTags.some((st) => d.tags.some((t) => t.name === st))) return false;
    }
    return true;
  });

  const columns: GridColDef<Device>[] = [
    {
      field: 'name',
      headerName: 'Name',
      flex: 1,
      minWidth: 150,
      renderCell: (params) => (
        <Link
          component={RouterLink}
          to={`/devices/${params.row.device_id}`}
          underline="hover"
        >
          {params.value}
        </Link>
      ),
    },
    { field: 'host', headerName: 'Host', width: 150 },
    {
      field: 'web_port',
      headerName: 'Web Port',
      width: 100,
      renderCell: (params) => {
        const port = params.row.web_port;
        if (!port) return <Chip label="N/A" size="small" />;
        const h = params.row.last_health;
        if (!h) return <Chip label={port} size="small" color="warning" />;
        const webOpen = h.web_port_open;
        if (webOpen === null || webOpen === undefined) return <Chip label={port} size="small" color="warning" />;
        const color: 'success' | 'error' = webOpen ? 'success' : 'error';
        return <Chip label={port} size="small" color={color} />;
      },
    },
    {
      field: 'sdk_port',
      headerName: 'SDK Port',
      width: 100,
      renderCell: (params) => {
        const port = params.row.sdk_port;
        if (!port) return <Chip label="N/A" size="small" />;
        const h = params.row.last_health;
        if (!h) return <Chip label={port} size="small" color="warning" />;
        const sdkOpen = h.sdk_port_open;
        if (sdkOpen === null || sdkOpen === undefined) return <Chip label={port} size="small" color="warning" />;
        const color: 'success' | 'error' = sdkOpen ? 'success' : 'error';
        return <Chip label={port} size="small" color={color} />;
      },
    },
    { field: 'vendor', headerName: 'Vendor', width: 100 },
    {
      field: 'status',
      headerName: 'Status',
      width: 120,
      sortComparator: (a: string, b: string) => a.localeCompare(b),
      valueGetter: (_value, row) => statusLabel(row),
      renderCell: (params) => {
        const d = params.row;
        return <Chip label={statusLabel(d)} color={statusColor(d)} size="small" />;
      },
    },
    {
      field: 'cameras',
      headerName: 'Cameras',
      width: 100,
      valueGetter: (_value, row) =>
        row.last_health
          ? `${row.last_health.online_cameras}/${row.last_health.camera_count}`
          : '\u2014',
    },
    {
      field: 'disks',
      headerName: 'Disks',
      width: 100,
      renderCell: (params) => {
        const h = params.row.last_health;
        if (!h) return '\u2014';
        return (
          <Chip
            label={h.disk_ok ? 'OK' : 'ERROR'}
            color={h.disk_ok ? 'success' : 'error'}
            size="small"
          />
        );
      },
    },
    {
      field: 'response_time',
      headerName: 'Response',
      width: 100,
      valueGetter: (_value, row) =>
        row.last_health ? `${Math.round(row.last_health.response_time_ms)}ms` : '\u2014',
    },
    {
      field: 'last_poll',
      headerName: 'Last Poll',
      width: 120,
      valueGetter: (_value, row) => timeAgo(row.last_poll_at),
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 150,
      sortable: false,
      filterable: false,
      renderCell: (params) => {
        const id = params.row.device_id;
        return (
          <Box>
            <Tooltip title="Poll">
              <span>
                <IconButton
                  size="small"
                  onClick={() => handlePoll(params.row)}
                >
                  <RefreshIcon fontSize="small" />
                </IconButton>
              </span>
            </Tooltip>
            <Tooltip title="Edit">
              <IconButton
                size="small"
                onClick={() => navigate(`/devices/${id}/edit`)}
              >
                <EditIcon fontSize="small" />
              </IconButton>
            </Tooltip>
            <Tooltip title="Delete">
              <IconButton size="small" onClick={() => handleDelete(id)}>
                <DeleteIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </Box>
        );
      },
    },
  ];

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
        <Typography variant="h4">Devices ({devices.length})</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          component={RouterLink}
          to="/devices/add"
        >
          Add Device
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      <Box display="flex" gap={2} mb={2}>
        <TextField
          size="small"
          label="Search"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          sx={{ width: 250 }}
        />
        <Autocomplete
          multiple
          size="small"
          options={allTags.map((t) => t.name)}
          value={selectedTags}
          onChange={(_e, v) => setSelectedTags(v)}
          renderInput={(params) => (
            <TextField {...params} label="Filter by tags" />
          )}
          renderTags={(value, getTagProps) =>
            value.map((opt, idx) => {
              const tagDef = allTags.find((t) => t.name === opt);
              const color = tagDef?.color || '#6366F1';
              return (
                <Chip
                  size="small"
                  label={opt}
                  {...getTagProps({ index: idx })}
                  key={opt}
                  sx={{ bgcolor: color + '33', borderColor: color, color }}
                  variant="outlined"
                />
              );
            })
          }
          sx={{ minWidth: 250 }}
        />
      </Box>

      <DataGrid
        rows={filtered}
        columns={columns}
        getRowId={(row) => row.device_id}
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

      {pollDialogDevice && (
        <PollDialog
          open={!!pollDialogDevice}
          onClose={() => setPollDialogDevice(null)}
          deviceId={pollDialogDevice.device_id}
          deviceName={pollDialogDevice.name}
          onPollComplete={handlePollComplete}
        />
      )}
    </Box>
  );
}
