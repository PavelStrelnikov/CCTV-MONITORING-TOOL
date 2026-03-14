import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import IconButton from '@mui/material/IconButton';
import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';
import Collapse from '@mui/material/Collapse';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import Tooltip from '@mui/material/Tooltip';
import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import FolderIcon from '@mui/icons-material/Folder';
import AddIcon from '@mui/icons-material/Add';
import CheckIcon from '@mui/icons-material/Check';
import CloseIcon from '@mui/icons-material/Close';
import BusinessIcon from '@mui/icons-material/Business';
import ApartmentIcon from '@mui/icons-material/Apartment';
import HomeWorkIcon from '@mui/icons-material/HomeWork';
import LocationCityIcon from '@mui/icons-material/LocationCity';
import StoreIcon from '@mui/icons-material/Store';
import WarehouseIcon from '@mui/icons-material/Warehouse';
import CorporateFareIcon from '@mui/icons-material/CorporateFare';
import DomainIcon from '@mui/icons-material/Domain';
import SchoolIcon from '@mui/icons-material/School';
import LocalHospitalIcon from '@mui/icons-material/LocalHospital';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';
import FactoryIcon from '@mui/icons-material/Factory';
import ChurchIcon from '@mui/icons-material/Church';
import MosqueIcon from '@mui/icons-material/Mosque';
import HolidayVillageIcon from '@mui/icons-material/HolidayVillage';
import VideocamIcon from '@mui/icons-material/Videocam';
import SecurityIcon from '@mui/icons-material/Security';
import ShieldIcon from '@mui/icons-material/Shield';
import type { SvgIconProps } from '@mui/material/SvgIcon';
import { api } from '../api/client.ts';
import type { FolderTree, Folder } from '../types.ts';

const FOLDER_COLORS = [
  null,
  '#3B82F6', '#2563EB', '#1D4ED8',   // blues
  '#22C55E', '#16A34A', '#15803D',   // greens
  '#EF4444', '#DC2626', '#B91C1C',   // reds
  '#F59E0B', '#D97706', '#B45309',   // ambers
  '#8B5CF6', '#7C3AED', '#6D28D9',   // purples
  '#EC4899', '#DB2777', '#BE185D',   // pinks
  '#06B6D4', '#0891B2', '#0E7490',   // cyans
  '#F97316', '#EA580C', '#C2410C',   // oranges
  '#6366F1', '#4F46E5', '#4338CA',   // indigos
  '#14B8A6', '#0D9488', '#0F766E',   // teals
  '#78716C', '#57534E', '#44403C',   // stones
  '#64748B', '#475569', '#334155',   // slates
];

type IconDef = { key: string; label: string; Icon: React.ComponentType<SvgIconProps> };

const FOLDER_ICONS: IconDef[] = [
  { key: 'folder', label: 'Folder', Icon: FolderIcon },
  { key: 'business', label: 'Business', Icon: BusinessIcon },
  { key: 'apartment', label: 'Apartment', Icon: ApartmentIcon },
  { key: 'homework', label: 'Home Work', Icon: HomeWorkIcon },
  { key: 'city', label: 'City', Icon: LocationCityIcon },
  { key: 'store', label: 'Store', Icon: StoreIcon },
  { key: 'warehouse', label: 'Warehouse', Icon: WarehouseIcon },
  { key: 'corporate', label: 'Corporate', Icon: CorporateFareIcon },
  { key: 'domain', label: 'Domain', Icon: DomainIcon },
  { key: 'school', label: 'School', Icon: SchoolIcon },
  { key: 'hospital', label: 'Hospital', Icon: LocalHospitalIcon },
  { key: 'bank', label: 'Bank', Icon: AccountBalanceIcon },
  { key: 'factory', label: 'Factory', Icon: FactoryIcon },
  { key: 'church', label: 'Church', Icon: ChurchIcon },
  { key: 'mosque', label: 'Mosque', Icon: MosqueIcon },
  { key: 'village', label: 'Village', Icon: HolidayVillageIcon },
  { key: 'camera', label: 'Camera', Icon: VideocamIcon },
  { key: 'security', label: 'Security', Icon: SecurityIcon },
  { key: 'shield', label: 'Shield', Icon: ShieldIcon },
];

export function getFolderIcon(iconKey: string | null | undefined): React.ComponentType<SvgIconProps> {
  if (!iconKey) return FolderIcon;
  const found = FOLDER_ICONS.find((i) => i.key === iconKey);
  return found ? found.Icon : FolderIcon;
}

interface Props {
  open: boolean;
  onClose: () => void;
  folders: FolderTree[];
  onFoldersChanged: () => void;
}

export default function FolderManagementDialog({ open, onClose, folders, onFoldersChanged }: Props) {
  const { t } = useTranslation();
  const [newName, setNewName] = useState('');
  const [newParentId, setNewParentId] = useState<number | ''>('');
  const [newColor, setNewColor] = useState<string | null>(null);
  const [newIcon, setNewIcon] = useState<string | null>(null);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editName, setEditName] = useState('');
  const [editParentId, setEditParentId] = useState<number | null>(null);
  const [editColor, setEditColor] = useState<string | null>(null);
  const [editIcon, setEditIcon] = useState<string | null>(null);
  const [error, setError] = useState('');
  const [showAddForm, setShowAddForm] = useState(false);
  const [expandedFolders, setExpandedFolders] = useState<Set<number>>(() => new Set(folders.map((f) => f.id)));

  const toggleExpanded = (folderId: number) => {
    setExpandedFolders((prev) => {
      const next = new Set(prev);
      if (next.has(folderId)) next.delete(folderId);
      else next.add(folderId);
      return next;
    });
  };

  const handleCreate = async () => {
    if (!newName.trim()) return;
    try {
      await api.createFolder({
        name: newName.trim(),
        parent_id: newParentId === '' ? null : newParentId,
        color: newColor,
        icon: newIcon,
      });
      setNewName('');
      setNewParentId('');
      setNewColor(null);
      setNewIcon(null);
      setShowAddForm(false);
      setError('');
      onFoldersChanged();
    } catch (err) {
      setError(err instanceof Error ? err.message : t('folders.createError'));
    }
  };

  const handleUpdate = async (folderId: number) => {
    if (!editName.trim()) return;
    try {
      const payload = {
        name: editName.trim(),
        parent_id: editParentId,
        color: editColor,
        icon: editIcon,
      };
      console.log('[FolderUpdate] id=%d payload=%o', folderId, payload);
      await api.updateFolder(folderId, payload);
      setEditingId(null);
      setError('');
      onFoldersChanged();
    } catch (err) {
      console.error('[FolderUpdate] error:', err);
      setError(err instanceof Error ? err.message : t('folders.updateError'));
    }
  };

  const handleDelete = async (folderId: number, folderName: string) => {
    if (!confirm(t('folders.deleteConfirm', { name: folderName }))) return;
    try {
      await api.deleteFolder(folderId);
      setError('');
      onFoldersChanged();
    } catch (err) {
      setError(err instanceof Error ? err.message : t('folders.deleteError'));
    }
  };

  const startEdit = (folder: Folder) => {
    setEditingId(folder.id);
    setEditName(folder.name);
    setEditParentId(folder.parent_id);
    setEditColor(folder.color);
    setEditIcon(folder.icon);
  };

  const cancelEdit = () => {
    setEditingId(null);
    setEditName('');
    setEditParentId(null);
    setEditColor(null);
    setEditIcon(null);
  };

  const topLevelFolders = folders.filter((f) => f.id !== editingId);

  function renderColorPicker(value: string | null, onChange: (c: string | null) => void) {
    return (
      <Box sx={{ display: 'flex', gap: 0.4, flexWrap: 'wrap', alignItems: 'center', maxWidth: 320 }}>
        {FOLDER_COLORS.map((c, i) => (
          <Box
            key={i}
            onClick={() => onChange(c)}
            sx={{
              width: 18,
              height: 18,
              borderRadius: '50%',
              bgcolor: c || 'action.disabled',
              border: value === c ? '2px solid' : '2px solid transparent',
              borderColor: value === c ? 'text.primary' : 'transparent',
              cursor: 'pointer',
              '&:hover': { opacity: 0.8, transform: 'scale(1.2)' },
              transition: 'transform 0.1s',
            }}
          />
        ))}
      </Box>
    );
  }

  function renderIconPicker(value: string | null, onChange: (icon: string | null) => void, color: string | null) {
    const iconColor = color || '#3B82F6';
    return (
      <Box sx={{ display: 'flex', gap: 0.25, flexWrap: 'wrap', alignItems: 'center', maxWidth: 320 }}>
        {FOLDER_ICONS.map((item) => {
          const isSelected = (value || 'folder') === item.key;
          return (
            <Tooltip key={item.key} title={item.label} arrow>
              <IconButton
                size="small"
                onClick={() => onChange(item.key === 'folder' ? null : item.key)}
                sx={{
                  p: 0.4,
                  border: isSelected ? '2px solid' : '2px solid transparent',
                  borderColor: isSelected ? 'text.primary' : 'transparent',
                  borderRadius: 1,
                }}
              >
                <item.Icon sx={{ fontSize: 20, color: iconColor }} />
              </IconButton>
            </Tooltip>
          );
        })}
      </Box>
    );
  }

  function renderFolderItem(folder: Folder, isChild = false) {
    const isEditing = editingId === folder.id;
    const FIcon = getFolderIcon(folder.icon);

    if (isEditing) {
      return (
        <Box key={folder.id} sx={{ p: 1.5, ml: isChild ? 3 : 0, mb: 1, border: 1, borderColor: 'divider', borderRadius: 1 }}>
          <Box sx={{ display: 'flex', gap: 1, mb: 1, alignItems: 'center' }}>
            <TextField
              size="small"
              label={t('folders.folderName')}
              value={editName}
              onChange={(e) => setEditName(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter') handleUpdate(folder.id);
                if (e.key === 'Escape') cancelEdit();
              }}
              autoFocus
              sx={{ flex: 1 }}
            />
            <FormControl size="small" sx={{ minWidth: 140 }}>
              <InputLabel>{t('folders.parentFolder')}</InputLabel>
              <Select
                value={editParentId ?? ''}
                onChange={(e) => {
                  const val = e.target.value;
                  setEditParentId(val === '' ? null : Number(val));
                }}
                label={t('folders.parentFolder')}
              >
                <MenuItem value="">{t('folders.rootLevel')}</MenuItem>
                {topLevelFolders.map((f) => (
                  <MenuItem key={f.id} value={f.id}>{f.name}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Box>
          <Box sx={{ mb: 1 }}>
            <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: 'block' }}>
              {t('folders.color')}
            </Typography>
            {renderColorPicker(editColor, setEditColor)}
          </Box>
          <Box sx={{ mb: 1 }}>
            <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: 'block' }}>
              {t('folders.icon')}
            </Typography>
            {renderIconPicker(editIcon, setEditIcon, editColor)}
          </Box>
          <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
            <IconButton size="small" onClick={() => handleUpdate(folder.id)}>
              <CheckIcon fontSize="small" color="success" />
            </IconButton>
            <IconButton size="small" onClick={cancelEdit}>
              <CloseIcon fontSize="small" />
            </IconButton>
          </Box>
        </Box>
      );
    }

    // Check if this is a top-level folder with children
    const hasChildren = !isChild && folders.some((f) => f.id === folder.id && (f.children?.length ?? 0) > 0);

    return (
      <Box
        key={folder.id}
        sx={{
          display: 'flex',
          alignItems: 'center',
          gap: 0.5,
          py: 0.75,
          px: 1,
          ml: isChild ? 3 : 0,
          borderRadius: 1,
          '&:hover': { bgcolor: 'action.hover' },
        }}
      >
        {hasChildren ? (
          <IconButton size="small" onClick={() => toggleExpanded(folder.id)} sx={{ p: 0.25 }}>
            {expandedFolders.has(folder.id)
              ? <ExpandMoreIcon sx={{ fontSize: 18, color: 'text.secondary' }} />
              : <ChevronLeftIcon sx={{ fontSize: 18, color: 'text.secondary' }} />
            }
          </IconButton>
        ) : (
          <Box sx={{ width: 22 }} />
        )}
        <FIcon sx={{ color: folder.color || 'primary.main', fontSize: 20 }} />
        <Typography
          sx={{ flex: 1, cursor: hasChildren ? 'pointer' : 'default' }}
          variant="body2"
          onClick={() => { if (hasChildren) toggleExpanded(folder.id); }}
        >
          {folder.name}
          <Typography component="span" variant="caption" color="text.secondary" sx={{ ml: 1 }}>
            ({folder.device_count} {t('folders.devices')})
          </Typography>
        </Typography>
        <IconButton size="small" onClick={() => startEdit(folder)}>
          <EditIcon fontSize="small" />
        </IconButton>
        <IconButton size="small" onClick={() => handleDelete(folder.id, folder.name)}>
          <DeleteIcon fontSize="small" />
        </IconButton>
      </Box>
    );
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{t('folders.manage')}</DialogTitle>
      <DialogContent>
        {error && (
          <Typography color="error" variant="body2" sx={{ mb: 1 }}>
            {error}
          </Typography>
        )}

        {folders.length === 0 && !showAddForm && (
          <Typography color="text.secondary" sx={{ py: 2 }}>
            {t('folders.noFolders')}
          </Typography>
        )}

        <Box>
          {folders.map((folder) => (
            <Box key={folder.id}>
              {renderFolderItem(folder)}
              {(folder.children?.length ?? 0) > 0 && (
                <Collapse in={expandedFolders.has(folder.id)}>
                  {folder.children?.map((child) => renderFolderItem(child, true))}
                </Collapse>
              )}
            </Box>
          ))}
        </Box>

        <Collapse in={showAddForm}>
          <Box sx={{ mt: 2, p: 1.5, border: 1, borderColor: 'divider', borderRadius: 1 }}>
            <Box sx={{ display: 'flex', gap: 1, mb: 1, flexWrap: 'wrap' }}>
              <TextField
                size="small"
                label={t('folders.folderName')}
                value={newName}
                onChange={(e) => setNewName(e.target.value)}
                onKeyDown={(e) => { if (e.key === 'Enter') handleCreate(); }}
                autoFocus
                sx={{ flex: 1, minWidth: 150 }}
              />
              <FormControl size="small" sx={{ minWidth: 140 }}>
                <InputLabel>{t('folders.parentFolder')}</InputLabel>
                <Select
                  value={newParentId}
                  onChange={(e) => setNewParentId(e.target.value as number | '')}
                  label={t('folders.parentFolder')}
                >
                  <MenuItem value="">{t('folders.rootLevel')}</MenuItem>
                  {folders.map((f) => (
                    <MenuItem key={f.id} value={f.id}>{f.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Box>
            <Box sx={{ mb: 1 }}>
              <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: 'block' }}>
                {t('folders.color')}
              </Typography>
              {renderColorPicker(newColor, setNewColor)}
            </Box>
            <Box sx={{ mb: 1 }}>
              <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: 'block' }}>
                {t('folders.icon')}
              </Typography>
              {renderIconPicker(newIcon, setNewIcon, newColor)}
            </Box>
            <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
              <Button variant="contained" size="small" onClick={handleCreate}>
                {t('common.create')}
              </Button>
              <Button size="small" onClick={() => { setShowAddForm(false); setNewName(''); setNewParentId(''); setNewColor(null); setNewIcon(null); }}>
                {t('common.cancel')}
              </Button>
            </Box>
          </Box>
        </Collapse>

        {!showAddForm && (
          <Button
            startIcon={<AddIcon />}
            onClick={() => setShowAddForm(true)}
            sx={{ mt: 1 }}
          >
            {t('folders.addFolder')}
          </Button>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>{t('common.close')}</Button>
      </DialogActions>
    </Dialog>
  );
}
