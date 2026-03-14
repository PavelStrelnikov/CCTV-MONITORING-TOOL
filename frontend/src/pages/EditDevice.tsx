import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import Button from '@mui/material/Button';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Alert from '@mui/material/Alert';
import CircularProgress from '@mui/material/CircularProgress';
import FormControlLabel from '@mui/material/FormControlLabel';
import Checkbox from '@mui/material/Checkbox';
import IconButton from '@mui/material/IconButton';
import InputAdornment from '@mui/material/InputAdornment';
import VisibilityIcon from '@mui/icons-material/Visibility';
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff';
import { api } from '../api/client.ts';
import type { DeviceUpdate, FolderTree } from '../types.ts';

export default function EditDevice() {
  const { t } = useTranslation();
  const { deviceId } = useParams<{ deviceId: string }>();
  const navigate = useNavigate();
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [folders, setFolders] = useState<FolderTree[]>([]);
  const [form, setForm] = useState({
    name: '',
    host: '',
    web_port: null as number | null,
    sdk_port: null as number | null,
    web_protocol: 'http',
    username: '',
    password: '',
    transport_mode: 'isapi',
    poll_interval_seconds: null as number | null,
    folder_id: null as number | null,
  });

  useEffect(() => {
    if (!deviceId) return;
    Promise.all([
      api.getDeviceDetail(deviceId),
      api.getCredentials(deviceId).catch(() => null),
      api.getFolders().catch(() => [] as FolderTree[]),
    ])
      .then(([detail, creds, foldersData]) => {
        setFolders(foldersData);
        setForm({
          name: detail.device.name,
          host: detail.device.host,
          web_port: detail.device.web_port,
          sdk_port: detail.device.sdk_port,
          web_protocol: detail.device.web_protocol || 'http',
          username: creds?.username || '',
          password: creds?.password || '',
          transport_mode: detail.device.transport_mode || 'isapi',
          poll_interval_seconds: detail.device.poll_interval_seconds ?? null,
          folder_id: detail.device.folder_id,
        });
      })
      .catch((err) => setError(err instanceof Error ? err.message : 'Failed to load'))
      .finally(() => setLoading(false));
  }, [deviceId]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    const portFields = ['web_port', 'sdk_port', 'poll_interval_seconds'];
    setForm((prev) => ({
      ...prev,
      [name]: portFields.includes(name) ? (value ? parseInt(value, 10) || null : null) : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!deviceId) return;
    setSubmitting(true);
    setError('');
    try {
      const update: DeviceUpdate = {
        name: form.name,
        host: form.host,
        web_port: form.web_port,
        sdk_port: form.sdk_port,
        web_protocol: form.web_protocol,
        username: form.username,
        password: form.password,
        transport_mode: form.transport_mode,
        poll_interval_seconds: form.poll_interval_seconds,
        folder_id: form.folder_id,
      };
      await api.updateDevice(deviceId, update);
      navigate(`/devices/${deviceId}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : t('editDevice.failed'));
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" p={4}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        {`${t('editDevice.title')}: ${deviceId}`}
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Paper sx={{ maxWidth: { xs: '100%', sm: 520 }, p: { xs: 2, sm: 3 } }}>
        <form onSubmit={handleSubmit} autoComplete="off">
          <Stack spacing={2}>
            <TextField
              label={t('addDevice.name')}
              name="name"
              value={form.name}
              onChange={handleChange}
              required
              size="small"
              fullWidth
            />
            <TextField
              label={t('addDevice.host')}
              name="host"
              value={form.host}
              onChange={handleChange}
              required
              size="small"
              fullWidth
            />
            <Box display="flex" alignItems="center" gap={1}>
              <TextField
                label={t('addDevice.webPort')}
                name="web_port"
                type="number"
                value={form.web_port ?? ''}
                onChange={handleChange}
                placeholder="8080"
                size="small"
                sx={{ flex: 1 }}
              />
              <FormControlLabel
                control={
                  <Checkbox
                    checked={form.web_protocol === 'https'}
                    onChange={(e) => setForm((prev) => ({ ...prev, web_protocol: e.target.checked ? 'https' : 'http' }))}
                    size="small"
                  />
                }
                label="HTTPS"
                sx={{ mr: 0 }}
              />
            </Box>
            <TextField
              label={t('addDevice.sdkPort')}
              name="sdk_port"
              type="number"
              value={form.sdk_port ?? ''}
              onChange={handleChange}
              placeholder="8000"
              size="small"
              fullWidth
            />
            <TextField
              select
              label={t('addDevice.transport')}
              name="transport_mode"
              value={form.transport_mode}
              onChange={handleChange}
              size="small"
              fullWidth
            >
              <MenuItem value="isapi">{t('addDevice.transportIsapi')}</MenuItem>
              <MenuItem value="sdk">{t('addDevice.transportSdk')}</MenuItem>
              <MenuItem value="auto">{t('addDevice.transportAuto')}</MenuItem>
            </TextField>
            <TextField
              label={t('addDevice.pollInterval')}
              name="poll_interval_seconds"
              type="number"
              value={form.poll_interval_seconds ?? ''}
              onChange={handleChange}
              placeholder="120"
              size="small"
              fullWidth
              helperText={t('addDevice.pollIntervalHelp')}
            />
            <TextField
              select
              label={t('folders.folder')}
              name="folder_id"
              value={form.folder_id ?? ''}
              onChange={(e) => {
                const val = e.target.value;
                setForm((prev) => ({ ...prev, folder_id: val === '' ? null : Number(val) }));
              }}
              size="small"
              fullWidth
            >
              <MenuItem value="">{t('folders.noFolder')}</MenuItem>
              {folders.map((f) => [
                <MenuItem key={f.id} value={f.id}>{f.name}</MenuItem>,
                ...(f.children || []).map((c) => (
                  <MenuItem key={c.id} value={c.id} sx={{ pl: 4 }}>
                    {f.name} / {c.name}
                  </MenuItem>
                )),
              ])}
            </TextField>
            <TextField
              label={t('addDevice.username')}
              name="username"
              value={form.username}
              onChange={handleChange}
              autoComplete="off"
              size="small"
              fullWidth
            />
            <TextField
              label={t('addDevice.password')}
              name="password"
              type={showPassword ? 'text' : 'password'}
              value={form.password}
              onChange={handleChange}
              autoComplete="new-password"
              size="small"
              fullWidth
              slotProps={{
                input: {
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        size="small"
                        onClick={() => setShowPassword((prev) => !prev)}
                        edge="end"
                      >
                        {showPassword ? <VisibilityOffIcon fontSize="small" /> : <VisibilityIcon fontSize="small" />}
                      </IconButton>
                    </InputAdornment>
                  ),
                },
              }}
            />
            <Box display="flex" gap={1}>
              <Button type="submit" variant="contained" disabled={submitting}>
                {submitting ? t('editDevice.saving') : t('editDevice.save')}
              </Button>
              <Button variant="outlined" onClick={() => navigate(-1)}>
                {t('editDevice.cancel')}
              </Button>
            </Box>
          </Stack>
        </form>
      </Paper>
    </Box>
  );
}
