import { useCallback, useEffect, useMemo, useRef, useState, memo } from 'react';
import { useParams, useNavigate, Link as RouterLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
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
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import VisibilityIcon from '@mui/icons-material/Visibility';
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import { LineChart } from '@mui/x-charts/LineChart';
import NetworkCheckIcon from '@mui/icons-material/NetworkCheck';
import RefreshIcon from '@mui/icons-material/Refresh';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import StorageIcon from '@mui/icons-material/Storage';
import RouterIcon from '@mui/icons-material/Router';
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

// ---------- Channel display name helper ----------
const DEFAULT_CH_NAME = /^(channel|camera|ipcamera|ip camera)\s*\d+$/i;

function buildChannelDisplayNames(cameras: CameraChannel[]): Map<string, string> {
  const sorted = [...cameras].sort((a, b) => {
    const na = parseInt(a.channel_id, 10);
    const nb = parseInt(b.channel_id, 10);
    if (!isNaN(na) && !isNaN(nb)) return na - nb;
    return a.channel_id.localeCompare(b.channel_id);
  });
  const map = new Map<string, string>();
  sorted.forEach((cam, idx) => {
    const name = cam.channel_name || '';
    if (!name || DEFAULT_CH_NAME.test(name.trim())) {
      map.set(cam.channel_id, `Channel ${idx + 1}`);
    } else {
      map.set(cam.channel_id, name);
    }
  });
  return map;
}

// ---------- Camera card ----------
const CameraCard = memo(function CameraCard({ cam, displayLabel, ignored, onToggleIgnore, snapshotUrl, cachedUrl, onLoaded, t, lazySnapshot = false }: {
  cam: CameraChannel;
  displayLabel: string;
  ignored: boolean;
  onToggleIgnore: (channelId: string) => void;
  snapshotUrl: string;
  cachedUrl?: string;
  onLoaded?: (channelId: string, blobUrl: string) => void;
  t: (key: string) => string;
  lazySnapshot?: boolean;
}) {
  const [hovered, setHovered] = useState(false);
  const [fullscreen, setFullscreen] = useState(false);
  const [imgError, setImgError] = useState(false);
  const [snapshotEnabled, setSnapshotEnabled] = useState(!lazySnapshot);

  const displayUrl = cachedUrl || snapshotUrl;
  const imgRef = useRef<HTMLImageElement>(null);

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
          borderInlineStart: `4px solid ${borderColor}`,
          opacity: ignored ? 0.5 : 1,
          cursor: isOnline && !imgError ? 'pointer' : 'default',
          transition: 'transform 0.2s ease, box-shadow 0.2s ease',
          transform: hovered && isOnline && !imgError ? 'scale(1.02)' : 'scale(1)',
          boxShadow: hovered && isOnline && !imgError ? 6 : 1,
          overflow: 'hidden',
        }}
        onClick={() => {
          if (!isOnline || imgError || ignored) return;
          if (lazySnapshot && !snapshotEnabled) {
            setSnapshotEnabled(true);
            return;
          }
          setFullscreen(true);
        }}
      >
        {/* Snapshot thumbnail — hidden entirely if image fails to load */}
        {isOnline && !ignored && !imgError && snapshotEnabled && (
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
              ref={imgRef}
              src={displayUrl}
              loading="lazy"
              onLoad={() => {
                if (!cachedUrl && onLoaded && imgRef.current) {
                  // Cache as blob URL on first network load
                  const img = imgRef.current;
                  try {
                    const canvas = document.createElement('canvas');
                    canvas.width = img.naturalWidth;
                    canvas.height = img.naturalHeight;
                    const ctx = canvas.getContext('2d');
                    if (ctx) {
                      ctx.drawImage(img, 0, 0);
                      canvas.toBlob((blob) => {
                        if (blob) {
                          const url = URL.createObjectURL(blob);
                          onLoaded(cam.channel_id, url);
                        }
                      }, 'image/jpeg');
                    }
                  } catch { /* cross-origin — skip caching */ }
                }
              }}
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
        {isOnline && !ignored && !imgError && !snapshotEnabled && (
          <Box
            sx={{
              position: 'relative',
              width: '100%',
              height: 80,
              bgcolor: '#0F172A',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
            }}
          >
            <Button
              size="small"
              variant="outlined"
              onClick={(e) => {
                e.stopPropagation();
                setSnapshotEnabled(true);
              }}
            >
              Load snapshot
            </Button>
          </Box>
        )}

        <CardContent sx={{ py: 1, '&:last-child': { pb: 1 } }}>
          <Box display="flex" justifyContent="space-between" alignItems="center">
            <Typography variant="subtitle2" noWrap sx={{ flex: 1, mr: 0.5 }}>
              {displayLabel}
            </Typography>
            <Box display="flex" alignItems="center" gap={0.5}>
              {recLabel && !ignored && (
                <Chip label={recLabel} size="small" color={recColor as any} variant="outlined" sx={{ height: 20, fontSize: 11 }} />
              )}
              {ignored && (
                <Chip label={t('deviceDetail.ignore')} size="small" variant="outlined" sx={{ height: 20, fontSize: 11 }} />
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
            <Tooltip title={ignored ? t('deviceDetail.includeMonitor') : t('deviceDetail.excludeMonitor')}>
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
                label={<Typography variant="caption" color="text.secondary">{t('deviceDetail.ignore')}</Typography>}
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
            src={displayUrl}
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
              {displayLabel}
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
});

// ---------- Main component ----------
export default function DeviceDetail() {
  const { t } = useTranslation();
  const { mode } = useThemeMode();
  const { deviceId } = useParams<{ deviceId: string }>();
  const navigate = useNavigate();

  const [detail, setDetail] = useState<DeviceDetailType | null>(null);
  const [loading, setLoading] = useState(true);
  const [pollOpen, setPollOpen] = useState(false);
  const [error, setError] = useState('');
  const [tab, setTab] = useState(0);
  const [ignoredChannels, setIgnoredChannels] = useState<Set<string>>(new Set());
  const [showIgnored, setShowIgnored] = useState(false);

  // Time sync
  const [syncing, setSyncing] = useState(false);
  const [syncResult, setSyncResult] = useState<{ success: boolean; message: string } | null>(null);

  // Network info
  const [networkInfo, setNetworkInfo] = useState<{ interfaces: any[]; ports: any[] } | null>(null);
  const [networkLoading, setNetworkLoading] = useState(false);

  // Snapshot pagination & cache
  const SNAPSHOT_PAGE_SIZE = 16;
  const [snapshotPage, setSnapshotPage] = useState(1);
  const [snapshotEpoch, setSnapshotEpoch] = useState(0);
  const snapshotCacheRef = useRef<Map<string, string>>(new Map());

  const handleSnapshotLoaded = useCallback((channelId: string, blobUrl: string) => {
    const old = snapshotCacheRef.current.get(channelId);
    if (old) URL.revokeObjectURL(old);
    snapshotCacheRef.current.set(channelId, blobUrl);
  }, []);

  const handleRefreshSnapshots = useCallback(() => {
    // Revoke all cached blob URLs and bump epoch to force re-fetch
    snapshotCacheRef.current.forEach((url) => URL.revokeObjectURL(url));
    snapshotCacheRef.current.clear();
    setSnapshotEpoch(e => e + 1);
  }, []);

  // Cleanup blob URLs on unmount
  useEffect(() => {
    const cache = snapshotCacheRef.current;
    return () => { cache.forEach((url) => URL.revokeObjectURL(url)); };
  }, []);

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
    setSnapshotPage(1);
    snapshotCacheRef.current.forEach((url) => URL.revokeObjectURL(url));
    snapshotCacheRef.current.clear();
    setSnapshotEpoch(0);
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

  // ---------- Auto-load network info when System tab opens ----------
  useEffect(() => {
    if (tab !== 1 || !deviceId || networkInfo) return;
    const d = detail?.device;
    if (!d || (!d.web_port && !d.sdk_port)) return;
    setNetworkLoading(true);
    api.getDeviceNetwork(deviceId)
      .then(setNetworkInfo)
      .catch(() => setNetworkInfo({ interfaces: [], ports: [] }))
      .finally(() => setNetworkLoading(false));
  }, [tab, deviceId, detail, networkInfo]);

  // ---------- Actions ----------
  const handlePollComplete = () => {
    // Refresh detail after poll
    if (deviceId) {
      api.getDeviceDetail(deviceId).then(setDetail).catch(() => {});
    }
  };

  const handleDelete = async () => {
    if (!deviceId || !confirm(t('deviceDetail.deleteConfirm'))) return;
    try {
      await api.deleteDevice(deviceId);
      navigate('/devices');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Delete failed');
    }
  };

  const handleSyncTime = async () => {
    if (!deviceId || !confirm('Sync device time to server time (Israel timezone)?')) return;
    setSyncing(true);
    setSyncResult(null);
    try {
      const result = await api.syncDeviceTime(deviceId);
      setSyncResult({
        success: result.success,
        message: result.success ? `Time set to ${result.time_set}` : `Failed (HTTP ${result.status_code})`,
      });
      // Re-poll device to refresh drift value
      if (result.success) {
        try {
          await api.pollDevice(deviceId);
          const updated = await api.getDeviceDetail(deviceId);
          setDetail(updated);
        } catch { /* ignore poll errors */ }
      }
    } catch (err) {
      setSyncResult({ success: false, message: err instanceof Error ? err.message : 'Sync failed' });
    } finally {
      setSyncing(false);
    }
  };

  const handleLoadNetwork = async () => {
    if (!deviceId) return;
    setNetworkLoading(true);
    try {
      const result = await api.getDeviceNetwork(deviceId);
      setNetworkInfo(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load network info');
    } finally {
      setNetworkLoading(false);
    }
  };

  const handleAddTag = async (value?: string) => {
    const tagName = (value ?? tagInput).trim();
    if (!tagName || !deviceId || addingTag) return;
    if (detail?.device.tags.some((tg) => tg.name === tagName)) {
      setTagInput('');
      return;
    }
    setAddingTag(true);
    try {
      await api.addTag(deviceId, tagName);
      const existing = allTags.find((tg) => tg.name === tagName);
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
          ? { ...prev, device: { ...prev.device, tags: prev.device.tags.filter((tg) => tg.name !== tagName) } }
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

  // Memoize channel names and visible cameras (must be before render guards)
  const cameras = detail?.cameras ?? [];
  const channelNames = useMemo(() => buildChannelDisplayNames(cameras), [cameras]);
  const visibleCameras = useMemo(() => showIgnored
    ? cameras
    : cameras.filter(c => !ignoredChannels.has(c.channel_id)),
    [cameras, showIgnored, ignoredChannels]);

  // History chart data
  const historyDates = useMemo(() => history.map((h) => new Date(h.checked_at)), [history]);
  const historyResponseTime = useMemo(() => history.map((h) => h.response_time_ms), [history]);
  const historyOnlineCameras = useMemo(() => history.map((h) => h.online_cameras), [history]);

  // ---------- Render guards ----------
  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={4}>
        <CircularProgress />
      </Box>
    );
  }
  if (!detail) return <Alert severity="error">{t('deviceDetail.notFound')}</Alert>;

  const { device, disks, alerts } = detail;
  const health = detail.health ?? device.last_health;
  const isLegacySnapshotModel =
    device.vendor === 'hikvision' &&
    (device.model || '').toUpperCase().includes('DS-7616NI-E2/A');

  const statusLabel = health ? (health.reachable ? t('status.online').toUpperCase() : t('status.offline').toUpperCase()) : t('status.unknown');
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
    { field: 'disk_id', headerName: t('deviceDetail.diskId'), width: 100 },
    {
      field: 'capacity',
      headerName: t('deviceDetail.capacity'),
      width: 120,
      valueGetter: (_v, row) => formatBytes(row.capacity_bytes),
    },
    {
      field: 'free',
      headerName: t('deviceDetail.freeSpace'),
      width: 120,
      valueGetter: (_v, row) => formatBytes(row.free_bytes),
    },
    {
      field: 'used_pct',
      headerName: t('deviceDetail.usedPct'),
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
      headerName: t('table.status'),
      width: 100,
      renderCell: (params) => (
        <Chip
          label={params.row.status}
          size="small"
          color={params.row.status.toLowerCase() === 'ok' ? 'success' : 'error'}
        />
      ),
    },
    { field: 'health_status', headerName: t('deviceDetail.health'), width: 120 },
    {
      field: 'temperature',
      headerName: t('deviceDetail.temp'),
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
      headerName: t('deviceDetail.workingTime'),
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
      headerName: t('deviceDetail.smart'),
      width: 100,
      renderCell: (params) => {
        const s = params.row.smart_status;
        if (!s) return <Typography variant="caption" color="text.disabled">N/A</Typography>;
        const color = s === 'ok' ? 'success' : s === 'warning' ? 'warning' : 'error';
        return <Chip label={s.toUpperCase()} size="small" color={color as any} />;
      },
    },
  ];

  return (
    <Box>
      {/* ---------- Header ---------- */}
      <Box display="flex" justifyContent="space-between" alignItems="flex-start" mb={2} flexWrap="wrap" gap={2}>
        <Box flex={1} minWidth={300}>
          {/* Title row with tags */}
          <Box display="flex" alignItems="center" gap={1.5} mb={1} flexWrap="wrap">
            <Typography variant="h4">{device.name}</Typography>
            <Chip label={statusLabel} color={statusColor} />
            {device.tags.map((tg) => (
              <Chip
                key={tg.name}
                label={tg.name}
                size="small"
                onDelete={() => handleRemoveTag(tg.name)}
                sx={{ bgcolor: tg.color + '33', borderColor: tg.color, color: tg.color, fontWeight: 500 }}
                variant="outlined"
              />
            ))}
            <Autocomplete
              freeSolo
              size="small"
              options={allTags.filter((tg) => !device.tags.some((dt) => dt.name === tg.name)).map((tg) => tg.name)}
              inputValue={tagInput}
              onInputChange={(_e, v) => setTagInput(v)}
              onChange={(_e, v) => { if (v) { setTagInput(v); handleAddTag(v); } }}
              renderInput={(params) => (
                <TextField
                  {...params}
                  placeholder={t('deviceDetail.addTag')}
                  onKeyDown={(e) => { if (e.key === 'Enter') { e.preventDefault(); handleAddTag(); } }}
                  sx={{ width: 130, '& .MuiInputBase-input': { py: 0.25, px: 1, fontSize: 12 } }}
                  disabled={addingTag}
                />
              )}
              sx={{ display: 'inline-flex' }}
            />
          </Box>

          {/* Info grid — two columns on wider screens */}
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' },
              gap: { xs: 0, md: 4 },
            }}
          >
            {/* Left column */}
            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: 'auto 1fr',
                gap: '2px 16px',
                fontSize: 14,
                alignContent: 'start',
                '& .label': { color: 'text.secondary', fontWeight: 500, whiteSpace: 'nowrap' },
                '& .value': { fontFamily: 'monospace' },
              }}
            >
              {(device.model || device.vendor) && (
                <>
                  <Typography variant="body2" className="label">{t('deviceDetail.model')}</Typography>
                  <Typography variant="body2" className="value">
                    {[device.vendor, device.model].filter(Boolean).join(' ')}
                    {device.firmware_version ? ` (v${device.firmware_version})` : ''}
                  </Typography>
                </>
              )}
              {device.serial_number && (
                <>
                  <Typography variant="body2" className="label">{t('deviceDetail.serialNumber')}</Typography>
                  <Typography variant="body2" className="value">{device.serial_number}</Typography>
                </>
              )}
              <Typography variant="body2" className="label">{t('deviceDetail.address')}</Typography>
              <Box className="value" display="flex" alignItems="center" gap={0.5}>
                {device.web_port ? (
                  <Typography
                    variant="body2"
                    component="a"
                    href={`${device.web_protocol || 'http'}://${device.host}:${device.web_port}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    sx={{ fontFamily: 'monospace', color: 'primary.main', textDecoration: 'none', '&:hover': { textDecoration: 'underline' } }}
                  >
                    {device.host}:{device.web_port}
                  </Typography>
                ) : (
                  <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                    {device.host}
                  </Typography>
                )}
                {device.sdk_port && (
                  <Typography variant="body2" color="text.secondary" sx={{ fontFamily: 'monospace' }}>
                    (SDK: {device.sdk_port})
                  </Typography>
                )}
              </Box>
            </Box>

            {/* Right column */}
            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: 'auto 1fr',
                gap: '2px 16px',
                fontSize: 14,
                alignContent: 'start',
                '& .label': { color: 'text.secondary', fontWeight: 500, whiteSpace: 'nowrap' },
                '& .value': { fontFamily: 'monospace' },
              }}
            >
              <Typography variant="body2" className="label">{t('deviceDetail.transport')}</Typography>
              <Typography variant="body2" className="value" sx={{ textTransform: 'uppercase' }}>
                {device.transport_mode}
              </Typography>

              {device.last_poll_at && (
                <>
                  <Typography variant="body2" className="label">{t('deviceDetail.lastPoll')}</Typography>
                  <Typography variant="body2" className="value">{timeAgo(device.last_poll_at)}</Typography>
                </>
              )}

              {/* Credentials row */}
              <Typography variant="body2" className="label">{t('deviceDetail.credentials')}</Typography>
              <Box display="flex" alignItems="center" gap={0.5}>
                {showPassword && credentials ? (
                  <>
                    <Typography variant="body2" className="value">
                      {credentials.username} / {credentials.password}
                    </Typography>
                    <Tooltip title={t('deviceDetail.copyPassword')}>
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
                <Tooltip title={showPassword ? t('deviceDetail.hideCreds') : t('deviceDetail.showCreds')}>
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
          </Box>

          {/* Time drift chip */}
          {driftSeconds != null && (
            <Chip
              label={
                absDrift! < 5
                  ? t('deviceDetail.timeSynced')
                  : absDrift! < 60
                    ? `${t('deviceDetail.timeDrift')}: ${driftSeconds > 0 ? '+' : ''}${driftSeconds}s`
                    : absDrift! < 3600
                      ? `${t('deviceDetail.timeDrift')}: ${driftSeconds > 0 ? '+' : ''}${Math.round(driftSeconds / 60)}min`
                      : `${t('deviceDetail.timeDrift')}: ${driftSeconds > 0 ? '+' : ''}${(driftSeconds / 3600).toFixed(1)}h`
              }
              size="small"
              color={absDrift! < 30 ? 'success' : absDrift! < 300 ? 'warning' : 'error'}
              variant="outlined"
              sx={{ mt: 1 }}
            />
          )}

          {/* Tags moved to title row */}
        </Box>

        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
          <Button variant="contained" onClick={() => setPollOpen(true)} startIcon={<NetworkCheckIcon />}>
            {t('deviceDetail.pollNow')}
          </Button>
          <Button variant="outlined" startIcon={<EditIcon />} component={RouterLink} to={`/devices/${deviceId}/edit`}>
            {t('deviceDetail.edit')}
          </Button>
          <Button variant="outlined" color="error" startIcon={<DeleteIcon />} onClick={handleDelete}>
            {t('deviceDetail.delete')}
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
          <Tab label={`${t('deviceDetail.cameras')} (${cameras.length}${ignoredChannels.size > 0 ? `, ${ignoredChannels.size} ${t('deviceDetail.ignored')}` : ''})`} />
          <Tab label={t('deviceDetail.system', 'System')} />
          <Tab label={t('deviceDetail.history')} />
          <Tab label={`${t('deviceDetail.alerts')} (${alerts.length})`} />
        </Tabs>
      </Box>

      {/* ----- Tab 0: Cameras ----- */}
      <TabPanel value={tab} index={0}>
        {cameras.length === 0 ? (
          <Typography color="text.secondary">{t('deviceDetail.noCameras')}</Typography>
        ) : (
          <>
            {isLegacySnapshotModel && (
              <Alert severity="info" sx={{ mb: 2 }}>
                Snapshot loading is on-demand for this legacy NVR model.
              </Alert>
            )}
            {(() => {
              const totalSnapshotPages = Math.max(1, Math.ceil(visibleCameras.length / SNAPSHOT_PAGE_SIZE));
              const effectivePage = Math.min(snapshotPage, totalSnapshotPages);
              const pagedCameras = visibleCameras.slice(
                (effectivePage - 1) * SNAPSHOT_PAGE_SIZE,
                effectivePage * SNAPSHOT_PAGE_SIZE,
              );
              return (
                <>
                  <Box
                    sx={{
                      display: 'grid',
                      gridTemplateColumns: { xs: 'repeat(auto-fill, minmax(160px, 1fr))', sm: 'repeat(auto-fill, minmax(220px, 1fr))' },
                      gap: 2,
                    }}
                  >
                    {pagedCameras.map((c) => (
                      <CameraCard
                        key={`${c.channel_id}-${snapshotEpoch}`}
                        cam={c}
                        displayLabel={channelNames.get(c.channel_id) || c.channel_name || c.channel_id}
                        ignored={ignoredChannels.has(c.channel_id)}
                        onToggleIgnore={handleToggleIgnore}
                        snapshotUrl={api.getSnapshotUrl(deviceId!, c.channel_id)}
                        cachedUrl={snapshotCacheRef.current.get(c.channel_id)}
                        onLoaded={handleSnapshotLoaded}
                        t={t}
                        lazySnapshot={isLegacySnapshotModel}
                      />
                    ))}
                  </Box>
                  <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 2, mt: 2, flexWrap: 'wrap' }}>
                    {totalSnapshotPages > 1 && (
                      <Button
                        size="small"
                        disabled={effectivePage <= 1}
                        onClick={() => setSnapshotPage(p => p - 1)}
                      >
                        {t('common.back', '← Back')}
                      </Button>
                    )}
                    {totalSnapshotPages > 1 && (
                      <Typography variant="body2">
                        {effectivePage} / {totalSnapshotPages}
                      </Typography>
                    )}
                    {totalSnapshotPages > 1 && (
                      <Button
                        size="small"
                        disabled={effectivePage >= totalSnapshotPages}
                        onClick={() => setSnapshotPage(p => p + 1)}
                      >
                        {t('common.next', 'Next →')}
                      </Button>
                    )}
                    <Tooltip title={t('deviceDetail.refreshSnapshots', 'Refresh snapshots')}>
                      <IconButton size="small" onClick={handleRefreshSnapshots}>
                        <RefreshIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    {ignoredChannels.size > 0 && (
                      <FormControlLabel
                        control={
                          <Checkbox
                            size="small"
                            checked={showIgnored}
                            onChange={(_e, v) => { setShowIgnored(v); setSnapshotPage(1); }}
                          />
                        }
                        label={
                          <Typography variant="body2" color="text.secondary">
                            {t('deviceDetail.showIgnored', 'Show ignored')} ({ignoredChannels.size})
                          </Typography>
                        }
                        sx={{ ml: 1 }}
                      />
                    )}
                  </Box>
                </>
              );
            })()}
          </>
        )}
      </TabPanel>

      {/* ----- Tab 1: System ----- */}
      <TabPanel value={tab} index={1}>
        <Stack spacing={2}>
          {/* --- Time --- */}
          <Card variant="outlined" sx={{ borderRadius: 2 }}>
            <Box sx={{
              px: 2, py: 1.5,
              display: 'flex', alignItems: 'center', justifyContent: 'space-between',
              borderBottom: 1, borderColor: 'divider',
              bgcolor: 'action.hover',
            }}>
              <Box display="flex" alignItems="center" gap={1}>
                <AccessTimeIcon fontSize="small" color="primary" />
                <Typography variant="subtitle1" fontWeight={600}>{t('deviceDetail.time', 'Time')}</Typography>
              </Box>
              <Box display="flex" alignItems="center" gap={1}>
                {syncResult && (
                  <Chip
                    label={syncResult.message}
                    size="small"
                    color={syncResult.success ? 'success' : 'error'}
                    variant="filled"
                  />
                )}
                <Tooltip title={!device?.web_port && !device?.sdk_port ? t('deviceDetail.noPort', 'No web or SDK port configured') : ''}>
                  <span>
                    <Button
                      variant="contained"
                      size="small"
                      onClick={handleSyncTime}
                      disabled={syncing || (!device?.web_port && !device?.sdk_port)}
                      startIcon={syncing ? <CircularProgress size={14} /> : <AccessTimeIcon fontSize="small" />}
                      sx={{ textTransform: 'none', minWidth: 100 }}
                    >
                      {t('deviceDetail.syncTime', 'Sync Time')}
                    </Button>
                  </span>
                </Tooltip>
              </Box>
            </Box>
            <CardContent sx={{ py: 1.5, '&:last-child': { pb: 1.5 } }}>
              {(() => {
                const fmtTime = (iso: string | undefined) => {
                  if (!iso) return '-';
                  try {
                    const d = new Date(iso);
                    return d.toLocaleString('he-IL', {
                      day: '2-digit', month: '2-digit', year: 'numeric',
                      hour: '2-digit', minute: '2-digit', second: '2-digit',
                      hour12: false,
                    });
                  } catch { return iso; }
                };
                const fmtDrift = (s: number | null | undefined) => {
                  if (s == null) return '-';
                  const abs = Math.abs(s);
                  const sign = s > 0 ? '+' : s < 0 ? '-' : '';
                  if (abs < 60) return `${sign}${abs}s`;
                  if (abs < 3600) return `${sign}${Math.floor(abs / 60)}m ${abs % 60}s`;
                  return `${sign}${Math.floor(abs / 3600)}h ${Math.floor((abs % 3600) / 60)}m`;
                };
                const fmtTz = (tz: string | undefined) => {
                  if (!tz) return null;
                  if (tz.startsWith('CST-2')) return 'Israel (UTC+2)';
                  if (tz.startsWith('CST-3')) return 'UTC+3';
                  return tz.length > 20 ? tz.slice(0, 20) + '...' : tz;
                };
                return (
                  <Box display="flex" alignItems="center" gap={3} flexWrap="wrap">
                    <Box>
                      <Typography variant="caption" color="text.secondary">{t('deviceDetail.deviceTime', 'Device Time')}</Typography>
                      <Typography variant="body2" fontWeight={500} sx={{ fontFamily: 'monospace' }}>{fmtTime(timeCheck?.device_time)}</Typography>
                    </Box>
                    <Box>
                      <Typography variant="caption" color="text.secondary">Drift</Typography>
                      <Box mt={0.25}>
                        <Chip
                          label={fmtDrift(driftSeconds)}
                          size="small"
                          sx={{
                            fontWeight: 700, height: 22, fontSize: '0.75rem',
                            bgcolor: absDrift == null ? 'action.selected'
                              : absDrift < 30 ? 'success.main'
                              : absDrift < 300 ? 'warning.main'
                              : 'error.main',
                            color: absDrift != null && absDrift < 300 ? 'success.contrastText' : undefined,
                          }}
                        />
                      </Box>
                    </Box>
                    {timeCheck?.time_mode && (
                      <Box>
                        <Typography variant="caption" color="text.secondary">Mode</Typography>
                        <Box mt={0.25}>
                          <Chip label={timeCheck.time_mode.toUpperCase()} size="small" variant="outlined" sx={{ height: 22, fontSize: '0.75rem' }} />
                        </Box>
                      </Box>
                    )}
                    {timeCheck?.timezone && (
                      <Box>
                        <Typography variant="caption" color="text.secondary">TZ</Typography>
                        <Typography variant="body2" fontWeight={500}>{fmtTz(timeCheck.timezone)}</Typography>
                      </Box>
                    )}
                  </Box>
                );
              })()}
            </CardContent>
          </Card>

          {/* --- Disks --- */}
          <Card variant="outlined" sx={{ borderRadius: 2 }}>
            <Box sx={{
              px: 2, py: 1.5,
              display: 'flex', alignItems: 'center', gap: 1,
              borderBottom: 1, borderColor: 'divider',
              bgcolor: 'action.hover',
            }}>
              <StorageIcon fontSize="small" color="primary" />
              <Typography variant="subtitle1" fontWeight={600}>{t('deviceDetail.disks')} ({disks.length})</Typography>
            </Box>
            <CardContent sx={{ py: disks.length === 0 ? 2 : 0, px: disks.length === 0 ? 2 : 0, '&:last-child': { pb: disks.length === 0 ? 2 : 0 } }}>
              {disks.length === 0 ? (
                <Typography color="text.secondary">{t('deviceDetail.noDisks')}</Typography>
              ) : (
                <Box sx={{ width: '100%', overflowX: 'auto' }}>
                  <Box sx={{ minWidth: 800 }}>
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
                        border: 'none',
                        '& .MuiDataGrid-cell': { display: 'flex', alignItems: 'center' },
                      }}
                    />
                  </Box>
                </Box>
              )}
            </CardContent>
          </Card>

          {/* --- Network --- */}
          <Card variant="outlined" sx={{ borderRadius: 2 }}>
            <Box sx={{
              px: 2, py: 1.5,
              display: 'flex', alignItems: 'center', justifyContent: 'space-between',
              borderBottom: 1, borderColor: 'divider',
              bgcolor: 'action.hover',
            }}>
              <Box display="flex" alignItems="center" gap={1}>
                <NetworkCheckIcon fontSize="small" color="primary" />
                <Typography variant="subtitle1" fontWeight={600}>{t('deviceDetail.network', 'Network')}</Typography>
              </Box>
              <Tooltip title={!device?.web_port && !device?.sdk_port ? t('deviceDetail.noPort', 'No web or SDK port configured') : ''}>
                <span>
                  <IconButton
                    size="small"
                    onClick={handleLoadNetwork}
                    disabled={networkLoading || (!device?.web_port && !device?.sdk_port)}
                  >
                    {networkLoading ? <CircularProgress size={18} /> : <RefreshIcon fontSize="small" />}
                  </IconButton>
                </span>
              </Tooltip>
            </Box>
            <CardContent sx={{ py: 2, '&:last-child': { pb: 2 } }}>
              {networkInfo ? (
                <Stack spacing={2}>
                  {networkInfo.interfaces.length > 0 && (
                    <Box>
                      <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: 0.5, mb: 1, display: 'block' }}>
                        Interfaces
                      </Typography>
                      <Stack spacing={1}>
                        {networkInfo.interfaces.map((iface) => (
                          <Box key={iface.id} sx={{
                            px: 2, py: 1.5, borderRadius: 1.5, bgcolor: 'action.hover',
                            display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap',
                          }}>
                            <Chip
                              icon={<RouterIcon sx={{ fontSize: 16 }} />}
                              label={`LAN ${iface.id}`}
                              size="small"
                              color="primary"
                              variant="outlined"
                              sx={{ fontWeight: 600 }}
                            />
                            <Box sx={{
                              display: 'grid',
                              gridTemplateColumns: 'auto 1fr',
                              gap: '2px 12px',
                              flex: 1,
                              minWidth: 200,
                            }}>
                              {iface.ip && (
                                <>
                                  <Typography variant="caption" color="text.secondary">IP</Typography>
                                  <Typography variant="body2" fontWeight={600} sx={{ fontFamily: 'monospace' }}>{iface.ip}</Typography>
                                </>
                              )}
                              {iface.mask && (
                                <>
                                  <Typography variant="caption" color="text.secondary">Mask</Typography>
                                  <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>{iface.mask}</Typography>
                                </>
                              )}
                              {iface.gateway && (
                                <>
                                  <Typography variant="caption" color="text.secondary">Gateway</Typography>
                                  <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>{iface.gateway}</Typography>
                                </>
                              )}
                              {(iface.dns1 || iface.dns2) && (
                                <>
                                  <Typography variant="caption" color="text.secondary">DNS</Typography>
                                  <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                                    {[iface.dns1, iface.dns2].filter(Boolean).join(', ')}
                                  </Typography>
                                </>
                              )}
                            </Box>
                            {iface.mac && (
                              <Typography variant="caption" color="text.secondary" sx={{ fontFamily: 'monospace', ml: 'auto' }}>
                                {iface.mac}
                              </Typography>
                            )}
                          </Box>
                        ))}
                      </Stack>
                    </Box>
                  )}
                  {networkInfo.ports.length > 0 && (
                    <Box>
                      <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: 0.5, mb: 1, display: 'block' }}>
                        Ports
                      </Typography>
                      <Box display="flex" gap={1} flexWrap="wrap">
                        {networkInfo.ports.map((p) => (
                          <Chip
                            key={`${p.protocol}-${p.port}`}
                            label={`${p.protocol}: ${p.port}`}
                            size="small"
                            variant={p.enabled ? 'filled' : 'outlined'}
                            color={p.enabled ? 'default' : 'error'}
                          />
                        ))}
                      </Box>
                    </Box>
                  )}
                  {networkInfo.interfaces.length === 0 && networkInfo.ports.length === 0 && (
                    <Box>
                      <Typography color="text.secondary" sx={{ mb: 1 }}>
                        {t('deviceDetail.networkNotSupported', 'Device does not support network info query')}
                      </Typography>
                      <Box display="flex" gap={1} flexWrap="wrap">
                        <Chip label={`IP: ${device.host}`} size="small" variant="outlined" />
                        {device.web_port && <Chip label={`Web: ${device.web_port}`} size="small" variant="outlined" />}
                        {device.sdk_port && <Chip label={`SDK: ${device.sdk_port}`} size="small" variant="outlined" />}
                      </Box>
                    </Box>
                  )}
                </Stack>
              ) : (
                <Typography variant="body2" color="text.secondary">
                  {t('deviceDetail.clickLoad', 'Click Load to fetch network configuration from device')}
                </Typography>
              )}
            </CardContent>
          </Card>
        </Stack>
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
          <Typography color="text.secondary">{t('deviceDetail.noHistory')}</Typography>
        ) : (
          <LineChart
            height={350}
            xAxis={[
              {
                data: historyDates,
                scaleType: 'time',
                label: t('deviceDetail.timeAxis'),
              },
            ]}
            yAxis={[
              { id: 'time_axis', label: t('deviceDetail.responseTime') },
              { id: 'cam_axis', label: t('deviceDetail.onlineCameras') },
            ]}
            series={[
              {
                data: historyResponseTime,
                label: t('deviceDetail.responseTime'),
                yAxisId: 'time_axis',
                showMark: false,
              },
              {
                data: historyOnlineCameras,
                label: t('deviceDetail.onlineCameras'),
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
          <Typography color="text.secondary">{t('deviceDetail.noAlerts')}</Typography>
        ) : (
          <Box sx={{ overflowX: 'auto' }}>
            <Table size="small" sx={{ minWidth: 500 }}>
              <TableHead>
                <TableRow>
                  <TableCell>{t('table.type')}</TableCell>
                  <TableCell>{t('table.severity')}</TableCell>
                  <TableCell>{t('table.message')}</TableCell>
                  <TableCell>{t('table.created')}</TableCell>
                  <TableCell>{t('table.status')}</TableCell>
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
          </Box>
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
