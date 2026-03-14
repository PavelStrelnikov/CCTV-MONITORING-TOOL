import { useEffect, useState, useCallback, useMemo, useRef } from 'react';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
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
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import FormControlLabel from '@mui/material/FormControlLabel';
import Checkbox from '@mui/material/Checkbox';
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
import FileDownloadIcon from '@mui/icons-material/FileDownload';
import DragIndicatorIcon from '@mui/icons-material/DragIndicator';
import {
  DndContext,
  closestCenter,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
  DragOverlay,
  type DragStartEvent,
} from '@dnd-kit/core';
import {
  SortableContext,
  horizontalListSortingStrategy,
  verticalListSortingStrategy,
  useSortable,
  arrayMove,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { api } from '../api/client.ts';
import { timeAgo } from '../utils/formatTime.ts';
import { useThemeMode } from '../theme.ts';
import type { Device, Tag, FolderTree, Folder } from '../types.ts';
import PollDialog from '../components/PollDialog.tsx';
import FolderManagementDialog, { getFolderIcon } from '../components/FolderManagementDialog.tsx';

const TAB_KEY = 'deviceList.activeTab';

// ---------- Sortable Tab (custom box-based, not MUI Tab) ----------
function SortableTabButton({ item, counts, onClick, selected, isDark }: {
  item: { id: 'all' | 'none' | number; label: string; color: string; icon: string | null; folder?: FolderTree };
  counts: { total: number; online: number } | undefined;
  onClick: () => void;
  selected: boolean;
  isDark: boolean;
}) {
  const isFolderTab = typeof item.id === 'number';
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: String(item.id), disabled: !isFolderTab });

  const style: React.CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  };

  const FIcon = item.icon ? getFolderIcon(item.icon) : item.id === 'all' ? undefined : FolderIcon;

  return (
    <Box
      ref={setNodeRef}
      style={style}
      onClick={onClick}
      {...(isFolderTab ? { ...attributes, ...listeners } : {})}
      sx={{
        display: 'flex',
        alignItems: 'center',
        gap: 0.75,
        px: 2,
        py: 1,
        cursor: isFolderTab ? 'grab' : 'pointer',
        userSelect: 'none',
        borderBottom: selected ? '2px solid' : '2px solid transparent',
        borderColor: selected ? 'primary.main' : 'transparent',
        color: selected ? 'primary.main' : 'text.secondary',
        fontWeight: selected ? 700 : 400,
        fontSize: '0.875rem',
        whiteSpace: 'nowrap',
        transition: 'all 0.15s',
        '&:hover': { color: 'text.primary', bgcolor: isDark ? '#1E293B' : '#F1EFE9' },
      }}
    >
      {FIcon && <FIcon sx={{ fontSize: 16, color: item.color || 'text.secondary' }} />}
      <span>{item.label}</span>
      {counts && (
        <Chip
          size="small"
          label={`${counts.online}/${counts.total}`}
          color={counts.total > 0 && counts.online === counts.total ? 'success' : counts.total > 0 && counts.online === 0 ? 'error' : 'default'}
          sx={{ height: 18, fontSize: '0.65rem', ml: 0.25 }}
        />
      )}
    </Box>
  );
}

// ---------- Sortable Device Row ----------
function SortableDeviceRow({ d, children }: { d: Device; children: (dragHandleProps: object) => React.ReactNode }) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: d.device_id });

  const style: React.CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.4 : 1,
    position: 'relative' as const,
    zIndex: isDragging ? 100 : undefined,
  };

  return (
    <TableRow ref={setNodeRef} style={style} sx={{ '& td': { py: 0.75 } }}>
      {children({ ...attributes, ...listeners })}
    </TableRow>
  );
}

export default function DeviceList() {
  const { t } = useTranslation();
  const { mode } = useThemeMode();
  const navigate = useNavigate();
  const [devices, setDevices] = useState<Device[]>([]);
  const [folders, setFolders] = useState<FolderTree[]>([]);
  const [allTags, setAllTags] = useState<Tag[]>([]);
  const [selectedTags, setSelectedTags] = useState<string[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [pollDialogDevice, setPollDialogDevice] = useState<Device | null>(null);
  const [folderDialogOpen, setFolderDialogOpen] = useState(false);
  const [exportDialogOpen, setExportDialogOpen] = useState(false);
  const [exportFolderIds, setExportFolderIds] = useState<Set<number>>(new Set());
  const [activeTab, setActiveTab] = useState<number>(() => {
    try {
      const saved = localStorage.getItem(TAB_KEY);
      return saved ? parseInt(saved, 10) : 0;
    } catch { return 0; }
  });
  const [draggingDeviceId, setDraggingDeviceId] = useState<string | null>(null);

  // Pause auto-refresh during drag
  const isDraggingRef = useRef(false);

  const fetchData = useCallback(async () => {
    if (isDraggingRef.current) return;
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

  const handleTabChange = (_e: unknown, newValue: number) => {
    setActiveTab(newValue);
    localStorage.setItem(TAB_KEY, String(newValue));
  };

  // DnD sensors — require a small movement before starting drag
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
  );

  // Filtered by search + tags
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
      return true;
    });
  }, [devices, search, selectedTags]);

  // Tab structure: [All, ...topFolders, No folder]
  const tabItems = useMemo(() => {
    const items: { id: 'all' | 'none' | number; label: string; color: string; icon: string | null; folder?: FolderTree }[] = [];
    items.push({ id: 'all', label: t('devices.allDevices', 'All'), color: '', icon: null });
    for (const f of folders) {
      items.push({ id: f.id, label: f.name, color: f.color || '#3B82F6', icon: f.icon, folder: f });
    }
    const hasNoFolder = filtered.some((d) => d.folder_id === null);
    if (hasNoFolder) {
      items.push({ id: 'none', label: t('folders.noFolder', 'No folder'), color: '#9CA3AF', icon: null });
    }
    return items;
  }, [folders, filtered, t]);

  // IDs for sortable tabs (only folder tabs are draggable)
  const sortableTabIds = useMemo(() => tabItems.map((item) => String(item.id)), [tabItems]);

  const safeTab = Math.min(activeTab, tabItems.length - 1);

  // Devices for active tab
  const devicesByFolder = useMemo(() => {
    const map = new Map<number | null, Device[]>();
    for (const d of filtered) {
      const key = d.folder_id;
      if (!map.has(key)) map.set(key, []);
      map.get(key)!.push(d);
    }
    // Sort within each folder group by display_order
    for (const [, devs] of map) {
      devs.sort((a, b) => a.display_order - b.display_order);
    }
    return map;
  }, [filtered]);

  const getFolderIds = (folder: FolderTree): number[] => {
    return [folder.id, ...(folder.children || []).map((c) => c.id)];
  };

  type Group = { subfolder: Folder | null; devices: Device[]; color: string; folderId: number | null };

  const { visibleGroups } = useMemo(() => {
    const current = tabItems[safeTab];
    if (!current) return { visibleGroups: [] as Group[] };

    const groups: Group[] = [];

    if (current.id === 'all') {
      for (const f of folders) {
        for (const child of (f.children || [])) {
          const devs = devicesByFolder.get(child.id) || [];
          if (devs.length > 0) {
            groups.push({ subfolder: child, devices: devs, color: child.color || f.color || '#3B82F6', folderId: child.id });
          }
        }
        const directDevs = devicesByFolder.get(f.id) || [];
        if (directDevs.length > 0) {
          groups.push({ subfolder: f as Folder, devices: directDevs, color: f.color || '#3B82F6', folderId: f.id });
        }
      }
      const noFolder = devicesByFolder.get(null) || [];
      if (noFolder.length > 0) {
        groups.push({ subfolder: null, devices: noFolder, color: '#9CA3AF', folderId: null });
      }
    } else if (current.id === 'none') {
      const noFolder = devicesByFolder.get(null) || [];
      if (noFolder.length > 0) {
        groups.push({ subfolder: null, devices: noFolder, color: '#9CA3AF', folderId: null });
      }
    } else {
      const folder = current.folder!;
      const directDevs = devicesByFolder.get(folder.id) || [];
      if (directDevs.length > 0) {
        groups.push({ subfolder: null, devices: directDevs, color: folder.color || '#3B82F6', folderId: folder.id });
      }
      for (const child of (folder.children || [])) {
        const devs = devicesByFolder.get(child.id) || [];
        if (devs.length > 0) {
          groups.push({ subfolder: child, devices: devs, color: child.color || folder.color || '#3B82F6', folderId: child.id });
        }
      }
    }

    return { visibleGroups: groups };
  }, [tabItems, safeTab, folders, devicesByFolder]);

  // All device IDs in current view for sortable context
  const allVisibleDeviceIds = useMemo(() => {
    return visibleGroups.flatMap((g) => g.devices.map((d) => d.device_id));
  }, [visibleGroups]);

  // Tab counts
  const tabCounts = useMemo(() => {
    return tabItems.map((item) => {
      if (item.id === 'all') {
        const online = filtered.filter((d) => d.last_health?.reachable).length;
        return { total: filtered.length, online };
      }
      if (item.id === 'none') {
        const devs = filtered.filter((d) => d.folder_id === null);
        const online = devs.filter((d) => d.last_health?.reachable).length;
        return { total: devs.length, online };
      }
      const folder = item.folder!;
      const ids = new Set(getFolderIds(folder));
      const devs = filtered.filter((d) => d.folder_id !== null && ids.has(d.folder_id));
      const online = devs.filter((d) => d.last_health?.reachable).length;
      return { total: devs.length, online };
    });
  }, [tabItems, filtered]);

  // Folder color map for device name coloring
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

  // --- Tab drag handlers ---
  const handleTabDragEnd = async (event: DragEndEvent) => {
    const { active, over } = event;
    if (!over || active.id === over.id) return;

    const folderTabs = tabItems.filter((item) => typeof item.id === 'number');
    const oldIndex = folderTabs.findIndex((t) => String(t.id) === String(active.id));
    const newIndex = folderTabs.findIndex((t) => String(t.id) === String(over.id));
    if (oldIndex === -1 || newIndex === -1) return;

    const newOrder = arrayMove(folderTabs, oldIndex, newIndex);
    const newFolders = newOrder.map((tab, i) => {
      const f = folders.find((f) => f.id === tab.id)!;
      return { ...f, sort_order: i };
    });
    setFolders(newFolders);

    await api.reorderFolders(newOrder.map((tab, i) => ({ id: tab.id as number, sort_order: i })));
    fetchData();
  };

  // --- Device drag handlers ---
  const handleDeviceDragStart = (event: DragStartEvent) => {
    isDraggingRef.current = true;
    setDraggingDeviceId(event.active.id as string);
  };

  const handleDeviceDragEnd = async (event: DragEndEvent) => {
    isDraggingRef.current = false;
    setDraggingDeviceId(null);
    const { active, over } = event;
    if (!over || active.id === over.id) return;

    // Find which group the active and over devices belong to
    const activeId = active.id as string;
    const overId = over.id as string;

    // Build flat list of devices in current view order
    const flatDevices = visibleGroups.flatMap((g) =>
      g.devices.map((d) => ({ ...d, groupFolderId: g.folderId }))
    );

    const oldIndex = flatDevices.findIndex((d) => d.device_id === activeId);
    const newIndex = flatDevices.findIndex((d) => d.device_id === overId);
    if (oldIndex === -1 || newIndex === -1) return;

    const activeDevice = flatDevices[oldIndex];
    const overDevice = flatDevices[newIndex];

    // Move device in the flat list
    const reordered = arrayMove(flatDevices, oldIndex, newIndex);

    // Determine new folder for the moved device (folder of the target position)
    const newFolderId = overDevice.groupFolderId;
    const folderChanged = activeDevice.groupFolderId !== newFolderId;

    // Update local state optimistically
    setDevices((prev) => {
      const updated = [...prev];
      // Update display_order for all visible devices
      reordered.forEach((rd, i) => {
        const idx = updated.findIndex((d) => d.device_id === rd.device_id);
        if (idx >= 0) {
          updated[idx] = { ...updated[idx], display_order: i };
          if (rd.device_id === activeId && folderChanged) {
            updated[idx] = { ...updated[idx], folder_id: newFolderId };
          }
        }
      });
      return updated;
    });

    // Persist to backend
    const items = reordered.map((rd, i) => ({
      device_id: rd.device_id,
      display_order: i,
      ...(rd.device_id === activeId && folderChanged ? { folder_id: newFolderId } : {}),
    }));
    await api.reorderDevices(items);
    fetchData();
  };

  // Currently active folder for "Add device" link
  const currentFolderId = useMemo(() => {
    const current = tabItems[safeTab];
    if (!current || current.id === 'all' || current.id === 'none') return null;
    return current.id as number;
  }, [tabItems, safeTab]);

  const addDeviceUrl = currentFolderId ? `/devices/add?folder=${currentFolderId}` : '/devices/add';

  function renderGroupHeader(subfolder: Folder | null, color: string, deviceCount: number, onlineCount: number) {
    const FIcon = subfolder ? getFolderIcon(subfolder.icon) : FolderIcon;
    const label = subfolder ? subfolder.name : t('folders.noFolder', 'No folder');
    const alpha = isDark ? '18' : '15';
    return (
      <TableRow
        key={`group-${subfolder?.id ?? 'none'}`}
        sx={{
          bgcolor: color + alpha,
          '& td': { borderColor, py: 0.5, borderBottom: `2px solid ${color}40` },
        }}
      >
        <TableCell colSpan={11}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <FIcon sx={{ color, fontSize: 18 }} />
            <Typography fontWeight={700} fontSize="0.85rem" sx={{ color }}>
              {label}
            </Typography>
            <Chip
              size="small"
              label={`${onlineCount}/${deviceCount}`}
              sx={{
                height: 18,
                fontSize: '0.65rem',
                fontWeight: 700,
                bgcolor: color + '22',
                color,
                border: `1px solid ${color}44`,
              }}
            />
          </Box>
        </TableCell>
      </TableRow>
    );
  }

  const draggingDevice = draggingDeviceId ? devices.find((d) => d.device_id === draggingDeviceId) : null;

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={2} flexWrap="wrap" gap={1}>
        <Typography variant="h4">{`${t('devices.title')} (${devices.length})`}</Typography>
        <Box display="flex" gap={1}>
          <Button variant="outlined" startIcon={<FileDownloadIcon />} onClick={() => {
            setExportFolderIds(new Set());
            setExportDialogOpen(true);
          }}>
            Excel
          </Button>
          <Button variant="outlined" startIcon={<SettingsIcon />} onClick={() => setFolderDialogOpen(true)}>
            {t('folders.manage')}
          </Button>
          <Button variant="contained" startIcon={<AddIcon />} component={RouterLink} to={addDeviceUrl}>
            {t('devices.addDevice')}
          </Button>
        </Box>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>{error}</Alert>
      )}

      {/* Filters */}
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

      {/* Folder tabs — draggable */}
      {tabItems.length > 1 && (
        <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 0 }}>
          <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleTabDragEnd}>
            <SortableContext items={sortableTabIds} strategy={horizontalListSortingStrategy}>
              <Box sx={{ display: 'flex', overflowX: 'auto', minHeight: 40 }}>
                {tabItems.map((item, idx) => (
                  <SortableTabButton
                    key={String(item.id)}
                    item={item}
                    counts={tabCounts[idx]}
                    onClick={() => handleTabChange(null, idx)}
                    selected={safeTab === idx}
                    isDark={isDark}
                  />
                ))}
              </Box>
            </SortableContext>
          </DndContext>
        </Box>
      )}

      {/* Table with draggable rows */}
      {loading ? (
        <Typography sx={{ mt: 2 }}>{t('common.loading')}</Typography>
      ) : (
        <DndContext
          sensors={sensors}
          collisionDetection={closestCenter}
          onDragStart={handleDeviceDragStart}
          onDragEnd={handleDeviceDragEnd}
        >
          <SortableContext items={allVisibleDeviceIds} strategy={verticalListSortingStrategy}>
            <TableContainer sx={{ border: 1, borderColor, borderRadius: '0 0 8px 8px', borderTop: 0, maxHeight: 'calc(100vh - 280px)', overflow: 'auto' }}>
              <Table size="small" stickyHeader>
                <TableHead>
                  <TableRow sx={{
                    '& th': {
                      fontWeight: 700, fontSize: '0.8rem', textTransform: 'uppercase',
                      letterSpacing: '0.03em', borderColor, py: 1,
                      bgcolor: headerBg,
                    },
                  }}>
                    <TableCell sx={{ width: 32 }} />
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
                  {visibleGroups.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={11} align="center" sx={{ py: 4 }}>
                        <Typography color="text.secondary">{t('devices.noDevices')}</Typography>
                      </TableCell>
                    </TableRow>
                  )}
                  {visibleGroups.map((group, gi) => {
                    const onlineCount = group.devices.filter((d) => d.last_health?.reachable).length;
                    const currentTab = tabItems[safeTab];
                    const isAllTab = currentTab?.id === 'all';
                    const isNoneTab = currentTab?.id === 'none';
                    const showHeader = isAllTab
                      || (isNoneTab && group.subfolder === null)
                      || (!isAllTab && !isNoneTab && group.subfolder !== null);
                    return [
                      showHeader && renderGroupHeader(group.subfolder, group.color, group.devices.length, onlineCount),
                      ...group.devices.map((d, di) => {
                        const h = d.last_health;
                        return (
                          <SortableDeviceRow key={d.device_id} d={d}>
                            {(dragHandleProps) => (
                              <>
                                <TableCell sx={{ width: 32, px: 0.5, borderColor, cursor: 'grab' }} {...dragHandleProps}>
                                  <DragIndicatorIcon sx={{ fontSize: 16, color: 'text.disabled' }} />
                                </TableCell>
                                <TableCell sx={{ borderColor }}>
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
                                </TableCell>
                                <TableCell sx={{ borderColor }}>{statusChip(d)}</TableCell>
                                <TableCell sx={{ borderColor }}>
                                  <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.9rem' }}>{d.host}</Typography>
                                </TableCell>
                                <TableCell sx={{ borderColor }}>{portChip(d.web_port, h?.web_port_open)}</TableCell>
                                <TableCell sx={{ borderColor }}>{portChip(d.sdk_port, h?.sdk_port_open)}</TableCell>
                                <TableCell sx={{ borderColor }}>{!h ? '—' : !h.reachable ? <Typography variant="caption" color="text.disabled">?</Typography> : `${h.online_cameras}/${h.camera_count}`}</TableCell>
                                <TableCell sx={{ borderColor }}>
                                  {!h ? '—' : !h.reachable ? <Typography variant="caption" color="text.disabled">?</Typography> : <Chip label={h.disk_ok ? t('status.ok') : t('status.error')} color={h.disk_ok ? 'success' : 'error'} size="small" />}
                                </TableCell>
                                <TableCell sx={{ borderColor }}>
                                  <Typography variant="caption">{h ? `${Math.round(h.response_time_ms)}ms` : '—'}</Typography>
                                </TableCell>
                                <TableCell sx={{ borderColor }}>
                                  <Typography variant="caption">{timeAgo(d.last_poll_at)}</Typography>
                                </TableCell>
                                <TableCell sx={{ whiteSpace: 'nowrap', borderColor }}>
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
                              </>
                            )}
                          </SortableDeviceRow>
                        );
                      }),
                    ];
                  })}
                </TableBody>
              </Table>
            </TableContainer>
          </SortableContext>
          <DragOverlay>
            {draggingDevice && (
              <Table size="small" sx={{ width: '100%', bgcolor: 'background.paper', boxShadow: 4, borderRadius: 1, opacity: 0.9 }}>
                <TableBody>
                  <TableRow>
                    <TableCell sx={{ width: 32, px: 0.5 }}>
                      <DragIndicatorIcon sx={{ fontSize: 16 }} />
                    </TableCell>
                    <TableCell>
                      <Typography fontWeight={700} fontSize="0.95rem">{draggingDevice.name}</Typography>
                    </TableCell>
                    <TableCell>{statusChip(draggingDevice)}</TableCell>
                    <TableCell>
                      <Typography variant="body2" sx={{ fontFamily: 'monospace', fontSize: '0.9rem' }}>{draggingDevice.host}</Typography>
                    </TableCell>
                    <TableCell colSpan={7} />
                  </TableRow>
                </TableBody>
              </Table>
            )}
          </DragOverlay>
        </DndContext>
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

      {/* Export dialog */}
      <Dialog open={exportDialogOpen} onClose={() => setExportDialogOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>ייצוא ל-Excel</DialogTitle>
        <DialogContent>
          <FormControlLabel
            control={
              <Checkbox
                checked={exportFolderIds.size === 0}
                onChange={() => setExportFolderIds(new Set())}
              />
            }
            label="הכל"
          />
          {folders.map((f) => {
            const FIcon = getFolderIcon(f.icon);
            return (
              <Box key={f.id} sx={{ ml: 1 }}>
                <FormControlLabel
                  control={
                    <Checkbox
                      checked={exportFolderIds.has(f.id)}
                      onChange={() => {
                        setExportFolderIds((prev) => {
                          const next = new Set(prev);
                          if (next.has(f.id)) next.delete(f.id);
                          else next.add(f.id);
                          return next;
                        });
                      }}
                    />
                  }
                  label={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                      <FIcon sx={{ fontSize: 16, color: f.color || '#3B82F6' }} />
                      <span>{f.name}</span>
                    </Box>
                  }
                />
              </Box>
            );
          })}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setExportDialogOpen(false)}>ביטול</Button>
          <Button
            variant="contained"
            startIcon={<FileDownloadIcon />}
            onClick={() => {
              const ids = [...exportFolderIds];
              const url = api.exportDevicesUrl(ids.length > 0 ? ids : undefined);
              window.open(url, '_blank');
              setExportDialogOpen(false);
            }}
          >
            הורדה
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
