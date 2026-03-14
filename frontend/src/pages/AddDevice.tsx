import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import Button from '@mui/material/Button';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Alert from '@mui/material/Alert';
import FormControlLabel from '@mui/material/FormControlLabel';
import Checkbox from '@mui/material/Checkbox';
import { api } from '../api/client.ts';
import type { DeviceCreate, FolderTree } from '../types.ts';

export default function AddDevice() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const presetFolder = searchParams.get('folder');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [folders, setFolders] = useState<FolderTree[]>([]);
  const [form, setForm] = useState<Omit<DeviceCreate, 'device_id'>>({
    name: '',
    vendor: 'hikvision',
    host: '',
    web_port: null,
    sdk_port: null,
    web_protocol: 'http',
    username: 'admin',
    password: '',
    transport_mode: 'isapi',
    poll_interval_seconds: null,
    folder_id: presetFolder ? parseInt(presetFolder, 10) : null,
  });

  useEffect(() => {
    api.getFolders().then(setFolders).catch(() => {});
  }, []);

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
    setSubmitting(true);
    setError('');
    try {
      await api.createDevice(form);
      navigate('/devices');
    } catch (err) {
      setError(err instanceof Error ? err.message : t('addDevice.failed'));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        {t('addDevice.title')}
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Paper sx={{ maxWidth: { xs: '100%', sm: 520 }, p: { xs: 2, sm: 3 } }}>
        <form onSubmit={handleSubmit}>
          <Stack spacing={2}>
            <TextField
              label={t('addDevice.name')}
              name="name"
              value={form.name}
              onChange={handleChange}
              placeholder="Building 1 NVR"
              required
              size="small"
              fullWidth
            />
            <TextField
              select
              label={t('addDevice.vendor')}
              name="vendor"
              value={form.vendor}
              onChange={handleChange}
              size="small"
              fullWidth
            >
              <MenuItem value="hikvision">Hikvision</MenuItem>
              <MenuItem value="dahua">Dahua</MenuItem>
              <MenuItem value="provision">Provision</MenuItem>
            </TextField>
            <TextField
              label={t('addDevice.host')}
              name="host"
              value={form.host}
              onChange={handleChange}
              placeholder="192.168.1.100"
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
              required
              size="small"
              fullWidth
            />
            <TextField
              label={t('addDevice.password')}
              name="password"
              type="password"
              value={form.password}
              onChange={handleChange}
              required
              size="small"
              fullWidth
            />
            <Box display="flex" gap={1}>
              <Button type="submit" variant="contained" disabled={submitting}>
                {submitting ? t('addDevice.submitting') : t('addDevice.submit')}
              </Button>
              <Button variant="outlined" onClick={() => navigate(-1)}>
                {t('addDevice.cancel')}
              </Button>
            </Box>
          </Stack>
        </form>
      </Paper>
    </Box>
  );
}
