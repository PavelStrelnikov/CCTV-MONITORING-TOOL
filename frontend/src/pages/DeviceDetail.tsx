import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link as RouterLink } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Tab from '@mui/material/Tab';
import Tabs from '@mui/material/Tabs';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import LinearProgress from '@mui/material/LinearProgress';
import Alert from '@mui/material/Alert';
import CircularProgress from '@mui/material/CircularProgress';
import TextField from '@mui/material/TextField';
import Autocomplete from '@mui/material/Autocomplete';
import IconButton from '@mui/material/IconButton';
import Checkbox from '@mui/material/Checkbox';
import FormControlLabel from '@mui/material/FormControlLabel';
import Tooltip from '@mui/material/Tooltip';
import Dialog from '@mui/material/Dialog';
import Stack from '@mui/material/Stack';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import VisibilityIcon from '@mui/icons-material/Visibility';
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import { LineChart } from '@mui/x-charts/LineChart';
import NetworkCheckIcon from '@mui/icons-material/NetworkCheck';
import { api } from '../api/client.ts';
import { timeAgo, formatBytes } from '../utils/formatTime.ts';
import { getDataGridSx, useThemeMode } from '../theme.ts';
import PollDialog from '../components/PollDialog.tsx';
import type {
  DeviceDetail as DeviceDetailType,
  CameraChannel,
  Disk,
  Alert as AlertType,
  HealthLogEntry,
  Tag,
} from '../types.ts';

// ---------- Tab Panel helper ----------
function TabPanel(props: { children: React.ReactNode; value: number; index: number }) {
  const { children, value, index } = props;
  if (value !== index) return null;
  return <Box sx={{ pt: 2 }}>{children}</Box>;
}

// ---------- Camera card ----------
function CameraCard({ cam, ignored, onToggleIgnore, snapshotUrl }: {
  cam: CameraChannel;
  ignored: boolean;
  onToggleIgnore: (channelId: string) => void;
  snapshotUrl: string;
}) {
  const [hovered, setHovered] = useState(false);
  const [fullscreen, setFullscreen] = useState(false);
  const [imgError, setImgError] = useState(false);

  const borderColor = ignored
    ? '#475569'
    : cam.status.toLowerCase() === 'online'
      ? '#22C55E'
      : cam.status.toLowerCase() === 'offline'
        ? '#EF4444'
        : '#475569';

  const recLabel =
    cam.recording === 'recording'
      ? 'REC'
      : cam.recording === 'not_recording'
        ? 'NO REC'
        : null;
  const recColor =
    cam.recording === 'recording'
      ? 'error'
      : cam.recording === 'not_recording'
        ? 'default'
        : undefined;

  const isOnline = cam.status.toLowerCase() === 'online';

  return (
    <>
      <Card
        onMouseEnter={() => setHovered(true)}
        onMouseLeave={() => setHovered(false)}
        sx={{
          borderLeft: `4px solid ${borderColor}`,
          opacity: ignored ? 0.5 : 1,
          cursor: isOnline && !imgError ? 'pointer' : 'default',
          transition: 'transform 0.2s ease, box-shadow 0.2s ease',
          transform: hovered && isOnline && !imgError ? 'scale(1.02)' : 'scale(1)',
          boxShadow: hovered && isOnline && !imgError ? 6 : 1,
          overflow: 'hidden',
        }}
        onClick={() => {
          if (isOnline && !imgError) setFullscreen(true);
        }}
      >
        {/* Snapshot thumbnail — hidden entirely if image fails to load */}
        {isOnline && !ignored && !imgError && (
          <Box
            sx={{
              position: 'relative',
              width: '100%',
              height: hovered ? 120 : 80,
              transition: 'height 0.2s ease',
              bgcolor: '#0F172A',
              overflow: 'hidden',
            }}
          >
            <Box
              component="img"
              src={snapshotUrl}
              loading="lazy"
              onError={() => setImgError(true)}
              sx={{
                width: '100%',
                height: '100%',
                objectFit: 'cover',
                display: 'block',
              }}
            />
          </Box>
        )}

        <CardContent sx={{ py: 1, '&:last-child': { pb: 1 } }}>
          <Box display="flex" justifyContent="space-between" alignItems="center">
            <Typography variant="subtitle2" noWrap sx={{ flex: 1, mr: 0.5 }}>
              {cam.channel_name || cam.channel_id}
            </Typography>
            <Box display="flex" alignItems="center" gap={0.5}>
              {recLabel && !ignored && (
                <Chip label={recLabel} size="small" color={recColor as any} variant="outlined" sx={{ height: 20, fontSize: 11 }} />
              )}
              {ignored && (
                <Chip label="Ignored" size="small" variant="outlined" sx={{ height: 20, fontSize: 11 }} />
              )}
            </Box>
          </Box>
          <Box display="flex" justifyContent="space-between" alignItems="center">
            <Box>
              {cam.ip_address && (
                <Typography variant="caption" color="text.secondary">
                  {cam.ip_address}
                </Typography>
              )}
              <Typography variant="body2" sx={{ textTransform: 'capitalize', lineHeight: 1.3 }}>
                {cam.status}
              </Typography>
            </Box>
            <Tooltip title={ignored ? 'Include in monitoring' : 'Exclude from monitoring'}>
              <FormControlLabel
                control={
                  <Checkbox
                    size="small"
                    checked={ignored}
                    onChange={(e) => { e.stopPropagation(); onToggleIgnore(cam.channel_id); }}
                    onClick={(e) => e.stopPropagation()}
                    sx={{ p: 0.5 }}
                  />
                }
                label={<Typography variant="caption" color="text.secondary">Ignore</Typography>}
                sx={{ mr: 0 }}
              />
            </Tooltip>
          </Box>
        </CardContent>
      </Card>

      {/* Fullscreen snapshot dialog */}
      <Dialog
        open={fullscreen}
        onClose={() => setFullscreen(false)}
        maxWidth={false}
        PaperProps={{
          sx: {
            bgcolor: '#000',
            maxWidth: '90vw',
            maxHeight: '90vh',
          },
        }}
      >
        <Box
          sx={{ position: 'relative', cursor: 'pointer' }}
          onClick={() => setFullscreen(false)}
        >
          <Box
            component="img"
            src={snapshotUrl}
            sx={{
              display: 'block',
              maxWidth: '90vw',
              maxHeight: '90vh',
              objectFit: 'contain',
            }}
          />
          <Box
            sx={{
              position: 'absolute',
              bottom: 0,
              left: 0,
              right: 0,
              p: 1.5,
              background: 'linear-gradient(transparent, rgba(0,0,0,0.8))',
              color: '#fff',
            }}
          >
            <Typography variant="subtitle1">
              {cam.channel_name || cam.channel_id}
            </Typography>
            {cam.ip_address && (
              <Typography variant="caption" sx={{ opacity: 0.7 }}>
                {cam.ip_address}
              </Typography>
            )}
          </Box>
        </Box>
      </Dialog>
    </>
  );
}

// ---------- Main component ----------
export default function DeviceDetail() {
  const { mode } = useThemeMode();
  const { deviceId } = useParams<{ deviceId: string }>();
  const navigate = useNavigate();

  const [detail, setDetail] = useState<DeviceDetailType | null>(null);
  const [loading, setLoading] = useState(true);
  const [pollOpen, setPollOpen] = useState(false);
  const [error, setError] = useState('');
  const [tab, setTab] = useState(0);
  const [ignoredChannels, setIgnoredChannels] = useState<Set<string>>(new Set());

  // Tags
  const [tagInput, setTagInput] = useState('');
  const [addingTag, setAddingTag] = useState(false);
  const [allTags, setAllTags] = useState<Tag[]>([]);

  // Credentials
  const [credentials, setCredentials] = useState<{ username: string; password: string } | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [loadingCreds, setLoadingCreds] = useState(false);

  // History
  const [history, setHistory] = useState<HealthLogEntry[]>([]);
  const [historyHours, setHistoryHours] = useState(24);
  const [historyLoading, setHistoryLoading] = useState(false);

  // ---------- Fetch detail ----------
  useEffect(() => {
    if (!deviceId) return;
    api
      .getDeviceDetail(deviceId)
      .then((d) => {
        setDetail(d);
        setIgnoredChannels(new Set(d.device.ignored_channels || []));
      })
      .catch((err) => setError(err instanceof Error ? err.message : 'Failed to load'))
      .finally(() => setLoading(false));
    api.getTags().then(setAllTags).catch(() => {});
  }, [deviceId]);

  // ---------- Fetch history when tab changes ----------
  useEffect(() => {
    if (tab !== 2 || !deviceId) return;
    setHistoryLoading(true);
    api
      .getDeviceHistory(deviceId, historyHours)
      .then(setHistory)
      .catch(() => {})
      .finally(() => setHistoryLoading(false));
  }, [tab, historyHours, deviceId]);

  // ---------- Actions ----------
  const handlePollComplete = () => {
    // Refresh detail after poll
    if (deviceId) {
      api.getDeviceDetail(deviceId).then(setDetail).catch(() => {});
    }
  };

  const handleDelete = async () => {
    if (!deviceId || !confirm('Delete this device?')) return;
    try {
      await api.deleteDevice(deviceId);
      navigate('/devices');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Delete failed');
    }
  };

  const handleAddTag = async (value?: string) => {
    const tagName = (value ?? tagInput).trim();
    if (!tagName || !deviceId || addingTag) return;
    if (detail?.device.tags.some((t) => t.name === tagName)) {
      setTagInput('');
      return;
    }
    setAddingTag(true);
    try {
      await api.addTag(deviceId, tagName);
      const existing = allTags.find((t) => t.name === tagName);
      const newTag: Tag = existing || { name: tagName, color: '#6366F1' };
      setDetail((prev) =>
        prev ? { ...prev, device: { ...prev.device, tags: [...prev.device.tags, newTag] } } : prev,
      );
      if (!existing) setAllTags((prev) => [...prev, newTag]);
      setTagInput('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add tag');
    } finally {
      setAddingTag(false);
    }
  };

  const handleRemoveTag = async (tagName: string) => {
    if (!deviceId) return;
    try {
      await api.removeTag(deviceId, tagName);
      setDetail((prev) =>
        prev
          ? { ...prev, device: { ...prev.device, tags: prev.device.tags.filter((t) => t.name !== tagName) } }
          : prev,
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to remove tag');
    }
  };

  const handleShowCredentials = async () => {
    if (credentials) {
      setShowPassword((prev) => !prev);
      return;
    }
    if (!deviceId || loadingCreds) return;
    setLoadingCreds(true);
    try {
      const creds = await api.getCredentials(deviceId);
      setCredentials(creds);
      setShowPassword(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load credentials');
    } finally {
      setLoadingCreds(false);
    }
  };

  const handleCopy = (text: string) => {
    navigator.clipboard.writeText(text).catch(() => {});
  };

  const handleToggleIgnore = async (channelId: string) => {
    if (!deviceId) return;
    const next = new Set(ignoredChannels);
    if (next.has(channelId)) {
      next.delete(channelId);
    } else {
      next.add(channelId);
    }
    setIgnoredChannels(next);
    try {
      await api.setIgnoredChannels(deviceId, [...next]);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update ignored channels');
      setIgnoredChannels(ignoredChannels); // revert
    }
  };

  // ---------- Render guards ----------
  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={4}>
        <CircularProgress />
      </Box>
    );
  }
  if (!detail) return <Alert severity="error">Device not found</Alert>;

  const { device, cameras, disks, alerts } = detail;
  const health = detail.health ?? device.last_health;

  const statusLabel = health ? (health.reachable ? 'ONLINE' : 'OFFLINE') : 'UNKNOWN';
  const statusColor: 'success' | 'error' | 'default' = health
    ? health.reachable
      ? 'success'
      : 'error'
    : 'default';

  // Time drift from cached health data
  const timeCheck = health?.time_check;
  const driftSeconds = timeCheck?.drift_seconds;
  const absDrift = driftSeconds != null ? Math.abs(driftSeconds) : null;

  // ---------- Disk columns ----------
  const diskColumns: GridColDef<Disk>[] = [
    { field: 'disk_id', headerName: 'Disk ID', width: 100 },
    {
      field: 'capacity',
      headerName: 'Capacity',
      width: 120,
      valueGetter: (_v, row) => formatBytes(row.capacity_bytes),
    },
    {
      field: 'free',
      headerName: 'Free Space',
      width: 120,
      valueGetter: (_v, row) => formatBytes(row.free_bytes),
    },
    {
      field: 'used_pct',
      headerName: 'Used %',
      width: 160,
      renderCell: (params) => {
        const cap = params.row.capacity_bytes;
        const free = params.row.free_bytes;
        const pct = cap > 0 ? Math.round(((cap - free) / cap) * 100) : 0;
        return (
          <Box display="flex" alignItems="center" gap={1} width="100%">
            <LinearProgress
              variant="determinate"
              value={pct}
              sx={{ flexGrow: 1, height: 8, borderRadius: 4 }}
              color={pct > 90 ? 'error' : pct > 70 ? 'warning' : 'primary'}
            />
            <Typography variant="body2">{pct}%</Typography>
          </Box>
        );
      },
    },
    {
      field: 'status',
      headerName: 'Status',
      width: 100,
      renderCell: (params) => (
        <Chip
          label={params.row.status}
          size="small"
          color={params.row.status.toLowerCase() === 'ok' ? 'success' : 'error'}
        />
      ),
    },
    { field: 'health_status', headerName: 'Health', width: 120 },
    {
      field: 'temperature',
      headerName: 'Temp',
      width: 80,
      renderCell: (params) => {
        const temp = params.row.temperature;
        if (temp == null) return <Typography variant="caption" color="text.disabled">N/A</Typography>;
        const color = temp > 55 ? 'error.main' : temp > 45 ? 'warning.main' : 'text.primary';
        return <Typography variant="body2" color={color}>{temp}°C</Typography>;
      },
    },
    {
      field: 'power_on_hours',
      headerName: 'Working Time',
      width: 140,
      renderCell: (params) => {
        const hours = params.row.power_on_hours;
        if (hours == null) return <Typography variant="caption" color="text.disabled">N/A</Typography>;
        const days = Math.floor(hours / 24);
        if (days >= 365) {
          const years = (days / 365).toFixed(1);
          return <Typography variant="body2">{years} yrs ({days} d)</Typography>;
        }
        return <Typography variant="body2">{days} days</Typography>;
      },
    },
    {
      field: 'smart_status',
      headerName: 'S.M.A.R.T.',
      width: 100,
      renderCell: (params) => {
        const s = params.row.smart_status;
        if (!s) return <Typography variant="caption" color="text.disabled">N/A</Typography>;
        const color = s === 'ok' ? 'success' : s === 'warning' ? 'warning' : 'error';
        return <Chip label={s.toUpperCase()} size="small" color={color as any} />;
      },
    },
  ];

  // ---------- History chart data ----------
  const historyDates = history.map((h) => new Date(h.checked_at));
  const historyResponseTime = history.map((h) => h.response_time_ms);
  const historyOnlineCameras = history.map((h) => h.online_cameras);

  return (
    <Box>
      {/* ---------- Header ---------- */}
      <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={2} flexWrap="wrap" gap={2}>
        <Box flex={1} minWidth={300}>
          {/* Title row */}
          <Box display="flex" alignItems="center" gap={2} mb={1}>
            <Typography variant="h4">{device.name}</Typography>
            <Chip label={statusLabel} color={statusColor} />
          </Box>

          {/* Info grid */}
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: 'auto 1fr',
              gap: '2px 16px',
              fontSize: 14,
              '& .label': { color: 'text.secondary', fontWeight: 500, whiteSpace: 'nowrap' },
              '& .value': { fontFamily: 'monospace' },
            }}
          >
            {(device.model || device.vendor) && (
              <>
                <Typography variant="body2" className="label">Model</Typography>
                <Typography variant="body2" className="value">
                  {[device.vendor, device.model].filter(Boolean).join(' ')}
                  {device.firmware_version ? ` (v${device.firmware_version})` : ''}
                </Typography>
              </>
            )}
            {device.serial_number && (
              <>
                <Typography variant="body2" className="label">S/N</Typography>
                <Typography variant="body2" className="value">{device.serial_number}</Typography>
              </>
            )}
            <Typography variant="body2" className="label">Address</Typography>
            <Typography variant="body2" className="value">
              {device.host}
              {device.web_port ? `:${device.web_port}` : ''}
              {device.sdk_port ? ` (SDK: ${device.sdk_port})` : ''}
            </Typography>

            <Typography variant="body2" className="label">Transport</Typography>
            <Typography variant="body2" className="value" sx={{ textTransform: 'uppercase' }}>
              {device.transport_mode}
            </Typography>

            {device.last_poll_at && (
              <>
                <Typography variant="body2" className="label">Last poll</Typography>
                <Typography variant="body2" className="value">{timeAgo(device.last_poll_at)}</Typography>
              </>
            )}

            {/* Credentials row */}
            <Typography variant="body2" className="label">Credentials</Typography>
            <Box display="flex" alignItems="center" gap={0.5}>
              {showPassword && credentials ? (
                <>
                  <Typography variant="body2" className="value">
                    {credentials.username} / {credentials.password}
                  </Typography>
                  <Tooltip title="Copy password">
                    <IconButton size="small" onClick={() => handleCopy(credentials.password)} sx={{ p: 0.25 }}>
                      <ContentCopyIcon sx={{ fontSize: 14 }} />
                    </IconButton>
                  </Tooltip>
                </>
              ) : (
                <Typography variant="body2" className="value" color="text.disabled">
                  ••••••••
                </Typography>
              )}
              <Tooltip title={showPassword ? 'Hide credentials' : 'Show credentials'}>
                <IconButton size="small" onClick={handleShowCredentials} disabled={loadingCreds} sx={{ p: 0.25 }}>
                  {loadingCreds ? (
                    <CircularProgress size={14} />
                  ) : showPassword ? (
                    <VisibilityOffIcon sx={{ fontSize: 16 }} />
                  ) : (
                    <VisibilityIcon sx={{ fontSize: 16 }} />
                  )}
                </IconButton>
              </Tooltip>
            </Box>
          </Box>

          {/* Time drift chip */}
          {driftSeconds != null && (
            <Chip
              label={
                absDrift! < 5
                  ? 'Time synced'
                  : absDrift! < 60
                    ? `Time drift: ${driftSeconds > 0 ? '+' : ''}${driftSeconds}s`
                    : absDrift! < 3600
                      ? `Time drift: ${driftSeconds > 0 ? '+' : ''}${Math.round(driftSeconds / 60)}min`
                      : `Time drift: ${driftSeconds > 0 ? '+' : ''}${(driftSeconds / 3600).toFixed(1)}h`
              }
              size="small"
              color={absDrift! < 30 ? 'success' : absDrift! < 300 ? 'warning' : 'error'}
              variant="outlined"
              sx={{ mt: 1 }}
            />
          )}

          {/* Tags */}
          <Box display="flex" alignItems="center" gap={0.5} mt={1} flexWrap="wrap">
            {device.tags.map((t) => (
              <Chip
                key={t.name}
                label={t.name}
                size="small"
                onDelete={() => handleRemoveTag(t.name)}
                sx={{ bgcolor: t.color + '33', borderColor: t.color, color: t.color, fontWeight: 500 }}
                variant="outlined"
              />
            ))}
            <Autocomplete
              freeSolo
              size="small"
              options={allTags.filter((t) => !device.tags.some((dt) => dt.name === t.name)).map((t) => t.name)}
              inputValue={tagInput}
              onInputChange={(_e, v) => setTagInput(v)}
              onChange={(_e, v) => { if (v) { setTagInput(v); handleAddTag(v); } }}
              renderInput={(params) => (
                <TextField
                  {...params}
                  placeholder="add tag"
                  onKeyDown={(e) => { if (e.key === 'Enter') { e.preventDefault(); handleAddTag(); } }}
                  sx={{ width: 150, '& .MuiInputBase-input': { py: 0.5, px: 1, fontSize: 13 } }}
                  disabled={addingTag}
                />
              )}
              sx={{ display: 'inline-flex' }}
            />
          </Box>
        </Box>

        <Stack direction="row" spacing={1}>
          <Button variant="contained" onClick={() => setPollOpen(true)} startIcon={<NetworkCheckIcon />}>
            Poll Now
          </Button>
          <Button variant="outlined" startIcon={<EditIcon />} component={RouterLink} to={`/devices/${deviceId}/edit`}>
            Edit
          </Button>
          <Button variant="outlined" color="error" startIcon={<DeleteIcon />} onClick={handleDelete}>
            Delete
          </Button>
        </Stack>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      {/* ---------- Tabs ---------- */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Tabs value={tab} onChange={(_e, v: number) => setTab(v)}>
          <Tab label={`Cameras (${cameras.length}${ignoredChannels.size > 0 ? `, ${ignoredChannels.size} ignored` : ''})`} />
          <Tab label={`Disks (${disks.length})`} />
          <Tab label="History" />
          <Tab label={`Alerts (${alerts.length})`} />
        </Tabs>
      </Box>

      {/* ----- Tab 0: Cameras ----- */}
      <TabPanel value={tab} index={0}>
        {cameras.length === 0 ? (
          <Typography color="text.secondary">No cameras found</Typography>
        ) : (
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))',
              gap: 2,
            }}
          >
            {cameras.map((c) => (
              <CameraCard key={c.channel_id} cam={c} ignored={ignoredChannels.has(c.channel_id)} onToggleIgnore={handleToggleIgnore} snapshotUrl={api.getSnapshotUrl(deviceId!, c.channel_id)} />
            ))}
          </Box>
        )}
      </TabPanel>

      {/* ----- Tab 1: Disks ----- */}
      <TabPanel value={tab} index={1}>
        {disks.length === 0 ? (
          <Typography color="text.secondary">No disks found</Typography>
        ) : (
          <DataGrid
            rows={disks}
            columns={diskColumns}
            getRowId={(row) => row.disk_id}
            density="compact"
            autoHeight
            disableRowSelectionOnClick
            hideFooter={disks.length <= 25}
            sx={{
              ...getDataGridSx(mode),
              '& .MuiDataGrid-cell': { display: 'flex', alignItems: 'center' },
            }}
          />
        )}
      </TabPanel>

      {/* ----- Tab 2: History ----- */}
      <TabPanel value={tab} index={2}>
        <Box display="flex" gap={1} mb={2}>
          {[1, 6, 24, 168].map((h) => (
            <Button
              key={h}
              variant={historyHours === h ? 'contained' : 'outlined'}
              size="small"
              onClick={() => setHistoryHours(h)}
            >
              {h < 24 ? `${h}h` : `${h / 24}d`}
            </Button>
          ))}
        </Box>
        {historyLoading ? (
          <Box display="flex" justifyContent="center" p={4}>
            <CircularProgress />
          </Box>
        ) : history.length === 0 ? (
          <Typography color="text.secondary">No history data yet</Typography>
        ) : (
          <LineChart
            height={350}
            xAxis={[
              {
                data: historyDates,
                scaleType: 'time',
                label: 'Time',
              },
            ]}
            yAxis={[
              { id: 'time_axis', label: 'Response Time (ms)' },
              { id: 'cam_axis', label: 'Online Cameras' },
            ]}
            series={[
              {
                data: historyResponseTime,
                label: 'Response Time (ms)',
                yAxisId: 'time_axis',
                showMark: false,
              },
              {
                data: historyOnlineCameras,
                label: 'Online Cameras',
                yAxisId: 'cam_axis',
                showMark: false,
              },
            ]}
            {...{ rightAxis: 'cam_axis' } as any}
          />
        )}
      </TabPanel>

      {/* ----- Tab 3: Alerts ----- */}
      <TabPanel value={tab} index={3}>
        {alerts.length === 0 ? (
          <Typography color="text.secondary">No alerts</Typography>
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Type</TableCell>
                <TableCell>Severity</TableCell>
                <TableCell>Message</TableCell>
                <TableCell>Created</TableCell>
                <TableCell>Status</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {alerts.map((a: AlertType) => (
                <TableRow key={a.id}>
                  <TableCell>{a.alert_type.replace(/_/g, ' ')}</TableCell>
                  <TableCell>
                    <Chip
                      label={a.severity}
                      size="small"
                      color={
                        a.severity.toLowerCase() === 'critical'
                          ? 'error'
                          : a.severity.toLowerCase() === 'warning'
                            ? 'warning'
                            : 'info'
                      }
                    />
                  </TableCell>
                  <TableCell>{a.message}</TableCell>
                  <TableCell>{timeAgo(a.created_at)}</TableCell>
                  <TableCell>
                    <Chip
                      label={a.status}
                      size="small"
                      color={a.status === 'active' ? 'error' : 'default'}
                      variant="outlined"
                    />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </TabPanel>

      {deviceId && (
        <PollDialog
          open={pollOpen}
          onClose={() => setPollOpen(false)}
          deviceId={deviceId}
          deviceName={detail?.device.name}
          onPollComplete={handlePollComplete}
        />
      )}
    </Box>
  );
}
