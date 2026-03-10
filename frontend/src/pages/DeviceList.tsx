import { useEffect, useState, useCallback, useMemo } from 'react';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import Autocomplete from '@mui/material/Autocomplete';
import Chip from '@mui/material/Chip';
import Checkbox from '@mui/material/Checkbox';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import Alert from '@mui/material/Alert';
import Link from '@mui/material/Link';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import AddIcon from '@mui/icons-material/Add';
import RefreshIcon from '@mui/icons-material/Refresh';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import FolderIcon from '@mui/icons-material/Folder';
import SettingsIcon from '@mui/icons-material/Settings';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowRightIcon from '@mui/icons-material/KeyboardArrowRight';
import { api } from '../api/client.ts';
import { timeAgo } from '../utils/formatTime.ts';
import { useThemeMode } from '../theme.ts';
import type { Device, Tag, FolderTree } from '../types.ts';
import PollDialog from '../components/PollDialog.tsx';
import FolderManagementDialog, { getFolderIcon } from '../components/FolderManagementDialog.tsx';

const EXPANDED_KEY = 'deviceList.expandedFolders';

function loadExpanded(): string[] {
  try {
    const raw = localStorage.getItem(EXPANDED_KEY);
    return raw ? JSON.parse(raw) : [];
  } catch {
    return [];
  }
}

function saveExpanded(ids: string[]) {
  localStorage.setItem(EXPANDED_KEY, JSON.stringify(ids));
}

export default function DeviceList() {
  const { t } = useTranslation();
  const { mode } = useThemeMode();
  const navigate = useNavigate();
  const [devices, setDevices] = useState<Device[]>([]);
  const [folders, setFolders] = useState<FolderTree[]>([]);
  const [allTags, setAllTags] = useState<Tag[]>([]);
  const [selectedTags, setSelectedTags] = useState<string[]>([]);
  const [selectedFolders, setSelectedFolders] = useState<number[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [pollDialogDevice, setPollDialogDevice] = useState<Device | null>(null);
  const [expanded, setExpanded] = useState<string[]>(loadExpanded);
  const [folderDialogOpen, setFolderDialogOpen] = useState(false);

  const fetchData = useCallback(async () => {
    try {
      const [devicesData, foldersData] = await Promise.all([
        api.getDevices(),
        api.getFolders(),
      ]);
      setDevices(devicesData);
      setFolders(foldersData);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : t('devices.failedLoad'));
    } finally {
      setLoading(false);
    }
  }, [t]);

  useEffect(() => {
    fetchData();
    api.getTags().then(setAllTags).catch(() => {});
    const interval = setInterval(fetchData, 15000);
    return () => clearInterval(interval);
  }, [fetchData]);

  const handlePoll = (device: Device) => setPollDialogDevice(device);
  const handlePollComplete = () => fetchData();

  const handleDelete = async (deviceId: string) => {
    if (!confirm(t('devices.deleteConfirm', { id: deviceId }))) return;
    try {
      await api.deleteDevice(deviceId);
      setDevices((prev) => prev.filter((d) => d.device_id !== deviceId));
    } catch (err) {
      setError(err instanceof Error ? err.message : t('devices.deleteFailed'));
    }
  };

  const toggleExpand = (panelId: string) => {
    const next = expanded.includes(panelId)
      ? expanded.filter((id) => id !== panelId)
      : [...expanded, panelId];
    setExpanded(next);
    saveExpanded(next);
  };

  // Build set of all folder IDs that match the folder filter (including children)
  const visibleFolderIds = useMemo(() => {
    if (selectedFolders.length === 0) return null; // null = show all
    const ids = new Set<number>();
    for (const fid of selectedFolders) {
      ids.add(fid);
      // Also include children of selected folders
      const folder = folders.find((f) => f.id === fid);
      if (folder) folder.children?.forEach((c) => ids.add(c.id));
      // Check if fid is a child — include parent too for display
      for (const f of folders) {
        if (f.children?.some((c) => c.id === fid)) ids.add(f.id);
      }
    }
    return ids;
  }, [selectedFolders, folders]);

  const filtered = useMemo(() => {
    return devices.filter((d) => {
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
        if (!selectedTags.some((st) => d.tags.some((tg) => tg.name === st))) return false;
      }
      if (visibleFolderIds !== null) {
        if (d.folder_id === null || !visibleFolderIds.has(d.folder_id)) return false;
      }
      return true;
    });
  }, [devices, search, selectedTags, visibleFolderIds]);

  const devicesByFolder = useMemo(() => {
    const map = new Map<number | null, Device[]>();
    for (const d of filtered) {
      const key = d.folder_id;
      if (!map.has(key)) map.set(key, []);
      map.get(key)!.push(d);
    }
    return map;
  }, [filtered]);

  const rootDevices = devicesByFolder.get(null) || [];

  // All folder options for the filter (flat list with path names)
  const folderOptions = useMemo(() => {
    const opts: { id: number; label: string; color: string | null; icon: string | null; parentId: number | null; childIds: number[] }[] = [];
    for (const f of folders) {
      const childIds = (f.children || []).map((c) => c.id);
      opts.push({ id: f.id, label: f.name, color: f.color, icon: f.icon, parentId: null, childIds });
      for (const c of (f.children || [])) {
        opts.push({ id: c.id, label: `${f.name} / ${c.name}`, color: c.color || f.color, icon: c.icon || f.icon, parentId: f.id, childIds: [] });
      }
    }
    return opts;
  }, [folders]);

  const handleFolderFilterChange = useCallback((_e: unknown, newValue: typeof folderOptions) => {
    const newIds = new Set(newValue.map((o) => o.id));
    const prevIds = new Set(selectedFolders);

    // Find newly added IDs
    for (const opt of newValue) {
      if (!prevIds.has(opt.id) && opt.childIds.length > 0) {
        // Parent was just selected — auto-select its children
        for (const cid of opt.childIds) newIds.add(cid);
      }
    }

    // Find newly removed IDs
    for (const pid of prevIds) {
      if (!newIds.has(pid)) {
        const opt = folderOptions.find((o) => o.id === pid);
        if (opt && opt.childIds.length > 0) {
          // Parent was deselected — auto-deselect children
          for (const cid of opt.childIds) newIds.delete(cid);
        }
      }
    }

    setSelectedFolders([...newIds]);
  }, [selectedFolders, folderOptions]);

  // Filter which top-level folders to render
  const visibleFolders = useMemo(() => {
    if (visibleFolderIds === null) return folders;
    return folders.filter((f) => visibleFolderIds.has(f.id));
  }, [folders, visibleFolderIds]);
  // Map folder_id → folder color for device name coloring
  // Children inherit parent color if they don't have their own
  const folderColorMap = useMemo(() => {
    const map = new Map<number, string>();
    for (const f of folders) {
      if (f.color) map.set(f.id, f.color);
      for (const c of (f.children || [])) {
        const color = c.color || f.color;
        if (color) map.set(c.id, color);
      }
    }
    return map;
  }, [folders]);

  const isDark = mode === 'dark';
  const borderColor = isDark ? '#1E293B' : '#D5D9E2';
  const headerBg = isDark ? '#0F172A' : '#E8E5E0';
  const stripeBg = isDark ? '#0F172A' : '#EBE8E3';
  const hoverBg = isDark ? '#1E293B' : '#E0DDD7';

  function statusChip(d: Device) {
    if (!d.last_health) return <Chip label={t('status.unknown')} size="small" />;
    return d.last_health.reachable
      ? <Chip label={t('status.online').toUpperCase()} color="success" size="small" />
      : <Chip label={t('status.offline').toUpperCase()} color="error" size="small" />;
  }

  function portChip(port: number | null, isOpen: boolean | null | undefined) {
    if (!port) return <Typography variant="caption" color="text.secondary">—</Typography>;
    if (isOpen === null || isOpen === undefined) return <Chip label={port} size="small" color="warning" />;
    return <Chip label={port} size="small" color={isOpen ? 'success' : 'error'} />;
  }

  function folderSummary(devs: Device[]) {
    const total = devs.length;
    const online = devs.filter((d) => d.last_health?.reachable).length;
    return { total, online };
  }

  function renderDeviceRow(d: Device, indent = 0) {
    const h = d.last_health;
    return (
      <TableRow
        key={d.device_id}
        sx={{
          '&:nth-of-type(even)': { bgcolor: stripeBg },
          '&:hover': { bgcolor: hoverBg },
          '& td': { py: 0.75, borderColor },
        }}
      >
        <TableCell>
          <Box sx={{ pl: indent }}>
            <Link
              component={RouterLink}
              to={`/devices/${d.device_id}`}
              underline="hover"
              fontWeight={700}
              fontSize="0.95rem"
              sx={d.folder_id && folderColorMap.has(d.folder_id) ? { color: folderColorMap.get(d.folder_id) } : undefined}
            >
              {d.name}
            </Link>
          </Box>
        </TableCell>
        <TableCell>{statusChip(d)}</TableCell>
        <TableCell>
          <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>{d.host}</Typography>
        </TableCell>
        <TableCell>{portChip(d.web_port, h?.web_port_open)}</TableCell>
        <TableCell>{portChip(d.sdk_port, h?.sdk_port_open)}</TableCell>
        <TableCell>{h ? `${h.online_cameras}/${h.camera_count}` : '—'}</TableCell>
        <TableCell>
          {h ? <Chip label={h.disk_ok ? t('status.ok') : t('status.error')} color={h.disk_ok ? 'success' : 'error'} size="small" /> : '—'}
        </TableCell>
        <TableCell>
          <Typography variant="caption">{h ? `${Math.round(h.response_time_ms)}ms` : '—'}</Typography>
        </TableCell>
        <TableCell>
          <Typography variant="caption">{timeAgo(d.last_poll_at)}</Typography>
        </TableCell>
        <TableCell sx={{ whiteSpace: 'nowrap' }}>
          <Tooltip title={t('common.poll')}>
            <IconButton size="small" onClick={() => handlePoll(d)}><RefreshIcon sx={{ fontSize: 18 }} /></IconButton>
          </Tooltip>
          <Tooltip title={t('common.edit')}>
            <IconButton size="small" onClick={() => navigate(`/devices/${d.device_id}/edit`)}><EditIcon sx={{ fontSize: 18 }} /></IconButton>
          </Tooltip>
          <Tooltip title={t('common.delete')}>
            <IconButton size="small" onClick={() => handleDelete(d.device_id)}><DeleteIcon sx={{ fontSize: 18 }} /></IconButton>
          </Tooltip>
        </TableCell>
      </TableRow>
    );
  }

  function renderFolderRows(folder: FolderTree, level = 0, parentColor?: string): React.ReactNode[] {
    const children = folder.children || [];
    const panelId = `folder-${folder.id}`;
    const directDevices = devicesByFolder.get(folder.id) || [];
    const childFolderIds = children.map((c) => c.id);
    const childDevices = childFolderIds.flatMap((cid) => devicesByFolder.get(cid) || []);
    const allDevicesInFolder = [...directDevices, ...childDevices];
    const { total, online } = folderSummary(allDevicesInFolder);
    const isExpanded = expanded.includes(panelId);
    const folderColor = folder.color || parentColor || '#3B82F6';
    const FIcon = getFolderIcon(folder.icon);
    const indent = level * 3;
    const rows: React.ReactNode[] = [];

    rows.push(
      <TableRow
        key={panelId}
        onClick={() => toggleExpand(panelId)}
        sx={{
          cursor: 'pointer',
          bgcolor: isDark ? `rgba(59,130,246,${0.06 - level * 0.02})` : `rgba(59,130,246,${0.04 - level * 0.01})`,
          '&:hover': { bgcolor: isDark ? 'rgba(59,130,246,0.12)' : 'rgba(59,130,246,0.08)' },
          '& td': { borderColor, py: 0.75 },
        }}
      >
        <TableCell colSpan={7}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, pl: indent }}>
            {isExpanded
              ? <KeyboardArrowDownIcon sx={{ fontSize: 18, color: 'text.secondary' }} />
              : <KeyboardArrowRightIcon sx={{ fontSize: 18, color: 'text.secondary' }} />}
            <FIcon sx={{ color: folderColor, fontSize: level === 0 ? 22 : 19 }} />
            <Typography fontWeight={level === 0 ? 700 : 600} fontSize={level === 0 ? '0.95rem' : '0.875rem'}>{folder.name}</Typography>
            <Chip
              size="small"
              label={`${online}/${total}`}
              color={total > 0 && online === total ? 'success' : total > 0 && online === 0 ? 'error' : 'default'}
              sx={{ height: level === 0 ? 20 : 18, fontSize: level === 0 ? '0.7rem' : '0.65rem' }}
            />
          </Box>
        </TableCell>
        <TableCell colSpan={3} />
      </TableRow>
    );

    if (isExpanded) {
      for (const child of children) {
        rows.push(...renderFolderRows(child as unknown as FolderTree, level + 1, folderColor));
      }
      for (const d of directDevices) {
        rows.push(renderDeviceRow(d, indent + 3));
      }
      if (directDevices.length === 0 && children.length === 0) {
        rows.push(
          <TableRow key={`${panelId}-empty`} sx={{ '& td': { borderColor } }}>
            <TableCell colSpan={10}>
              <Typography variant="body2" color="text.secondary" sx={{ pl: indent + 3, py: 0.5 }}>
                {t('folders.empty')}
              </Typography>
            </TableCell>
          </TableRow>
        );
      }
    }

    return rows;
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2} flexWrap="wrap" gap={1}>
        <Typography variant="h4">{`${t('devices.title')} (${devices.length})`}</Typography>
        <Box display="flex" gap={1}>
          <Button variant="outlined" startIcon={<SettingsIcon />} onClick={() => setFolderDialogOpen(true)}>
            {t('folders.manage')}
          </Button>
          <Button variant="contained" startIcon={<AddIcon />} component={RouterLink} to="/devices/add">
            {t('devices.addDevice')}
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>{error}</Alert>
      )}

      <Box display="flex" gap={2} mb={2} flexWrap="wrap">
        <TextField
          size="small"
          label={t('devices.search')}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          sx={{ width: { xs: '100%', sm: 250 } }}
        />
        <Autocomplete
          multiple
          size="small"
          disableCloseOnSelect
          options={folderOptions}
          getOptionLabel={(opt) => opt.label}
          value={folderOptions.filter((o) => selectedFolders.includes(o.id))}
          onChange={handleFolderFilterChange}
          isOptionEqualToValue={(opt, val) => opt.id === val.id}
          renderInput={(params) => <TextField {...params} label={t('devices.filterByFolders')} />}
          renderOption={(props, option) => {
            const { key, ...rest } = props as { key: string } & typeof props;
            const OptIcon = getFolderIcon(option.icon);
            return (
              <li key={key} {...rest}>
                <Checkbox size="small" checked={selectedFolders.includes(option.id)} sx={{ mr: 0.5, p: 0.25 }} />
                <OptIcon sx={{ fontSize: 16, color: option.color || 'primary.main', mr: 0.75 }} />
                {option.label}
              </li>
            );
          }}
          renderTags={(value, getTagProps) =>
            value.map((opt, idx) => {
              const TagIcon = getFolderIcon(opt.icon);
              return (
                <Chip
                  size="small" label={opt.label} {...getTagProps({ index: idx })} key={opt.id}
                  icon={<TagIcon sx={{ fontSize: 16, color: opt.color || 'primary.main' }} />}
                  variant="outlined"
                />
              );
            })
          }
          sx={{ minWidth: { xs: '100%', sm: 250 } }}
        />
        <Autocomplete
          multiple
          size="small"
          options={allTags.map((tg) => tg.name)}
          value={selectedTags}
          onChange={(_e, v) => setSelectedTags(v)}
          renderInput={(params) => <TextField {...params} label={t('devices.filterByTags')} />}
          renderTags={(value, getTagProps) =>
            value.map((opt, idx) => {
              const tagDef = allTags.find((tg) => tg.name === opt);
              const color = tagDef?.color || '#6366F1';
              return (
                <Chip
                  size="small" label={opt} {...getTagProps({ index: idx })} key={opt}
                  sx={{ bgcolor: color + '33', borderColor: color, color }} variant="outlined"
                />
              );
            })
          }
          sx={{ minWidth: { xs: '100%', sm: 250 } }}
        />
      </Box>

      {loading ? (
        <Typography>{t('common.loading')}</Typography>
      ) : (
        <TableContainer sx={{ border: 1, borderColor, borderRadius: 2, maxHeight: 'calc(100vh - 220px)', overflow: 'auto' }}>
          <Table size="small" stickyHeader>
            <TableHead>
              <TableRow sx={{
                '& th': {
                  fontWeight: 700, fontSize: '0.75rem', textTransform: 'uppercase',
                  letterSpacing: '0.03em', borderColor, py: 1,
                  bgcolor: headerBg,
                },
              }}>
                <TableCell>{t('table.name')}</TableCell>
                <TableCell>{t('table.status')}</TableCell>
                <TableCell>{t('table.host')}</TableCell>
                <TableCell>Web</TableCell>
                <TableCell>SDK</TableCell>
                <TableCell>{t('table.cameras')}</TableCell>
                <TableCell>{t('table.disks')}</TableCell>
                <TableCell>{t('table.response')}</TableCell>
                <TableCell>{t('table.lastPoll')}</TableCell>
                <TableCell>{t('table.actions')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {visibleFolders.flatMap((folder) => renderFolderRows(folder))}

              {rootDevices.length > 0 && visibleFolders.length > 0 && visibleFolderIds === null && (
                <TableRow sx={{
                  bgcolor: isDark ? 'rgba(100,100,100,0.06)' : 'rgba(100,100,100,0.04)',
                  '& td': { borderColor, py: 0.75 },
                }}>
                  <TableCell colSpan={10}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
                      <FolderIcon sx={{ color: 'text.secondary', fontSize: 20 }} />
                      <Typography fontWeight={500} variant="body2" color="text.secondary">
                        {t('folders.noFolder')}
                      </Typography>
                    </Box>
                  </TableCell>
                </TableRow>
              )}
              {rootDevices.map((d) => renderDeviceRow(d))}

              {filtered.length === 0 && (
                <TableRow>
                  <TableCell colSpan={10} align="center" sx={{ py: 4 }}>
                    <Typography color="text.secondary">{t('devices.noDevices')}</Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {pollDialogDevice && (
        <PollDialog
          open={!!pollDialogDevice}
          onClose={() => setPollDialogDevice(null)}
          deviceId={pollDialogDevice.device_id}
          deviceName={pollDialogDevice.name}
          onPollComplete={handlePollComplete}
        />
      )}

      <FolderManagementDialog
        open={folderDialogOpen}
        onClose={() => setFolderDialogOpen(false)}
        folders={folders}
        onFoldersChanged={fetchData}
      />
    </Box>
  );
}
