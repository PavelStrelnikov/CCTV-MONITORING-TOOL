import { useEffect, useState } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import MenuItem from '@mui/material/MenuItem';
import Button from '@mui/material/Button';
import Alert from '@mui/material/Alert';
import Paper from '@mui/material/Paper';
import SaveIcon from '@mui/icons-material/Save';
import { useTranslation } from 'react-i18next';
import { api } from '../api/client.ts';

const INTERVAL_OPTIONS = [
  { value: 0, labelKey: 'settings.disabled' },
  { value: 60, labelKey: 'settings.1min' },
  { value: 300, labelKey: 'settings.5min' },
  { value: 600, labelKey: 'settings.10min' },
  { value: 900, labelKey: 'settings.15min' },
  { value: 1800, labelKey: 'settings.30min' },
  { value: 3600, labelKey: 'settings.1hour' },
];

export default function Settings() {
  const { t, i18n } = useTranslation();
  const [interval, setInterval] = useState(900);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleLanguageChange = (lang: string) => {
    i18n.changeLanguage(lang);
    localStorage.setItem('cctv-lang', lang);
  };

  useEffect(() => {
    api.getSettings()
      .then((s) => setInterval(s.default_poll_interval))
      .catch((err) => setError(err instanceof Error ? err.message : t('settings.failedLoad')))
      .finally(() => setLoading(false));
  }, []);

  const handleSave = async () => {
    setSaving(true);
    setError('');
    setSuccess('');
    try {
      const updated = await api.updateSettings({ default_poll_interval: interval });
      setInterval(updated.default_poll_interval);
      setSuccess(t('settings.saved'));
    } catch (err) {
      setError(err instanceof Error ? err.message : t('settings.failedSave'));
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <Typography>{t('common.loading')}</Typography>;

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        {t('settings.title')}
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
          {error}
        </Alert>
      )}
      {success && (
        <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess('')}>
          {success}
        </Alert>
      )}

      <Paper sx={{ p: 3, maxWidth: 500 }}>
        <Typography variant="h6" gutterBottom>
          {t('settings.polling')}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {t('settings.pollingDesc')}
        </Typography>

        <TextField
          select
          fullWidth
          label={t('settings.defaultPollInterval')}
          value={interval}
          onChange={(e) => setInterval(Number(e.target.value))}
          sx={{ mb: 3 }}
        >
          {INTERVAL_OPTIONS.map((opt) => (
            <MenuItem key={opt.value} value={opt.value}>
              {t(opt.labelKey)}
            </MenuItem>
          ))}
        </TextField>

        <Button
          variant="contained"
          startIcon={<SaveIcon />}
          onClick={handleSave}
          disabled={saving}
        >
          {saving ? t('settings.saving') : t('settings.save')}
        </Button>
      </Paper>

      <Paper sx={{ p: 3, maxWidth: 500, mt: 3 }}>
        <Typography variant="h6" gutterBottom>
          {t('settings.language')}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {t('settings.languageDesc')}
        </Typography>
        <TextField
          select
          fullWidth
          label={t('settings.language')}
          value={i18n.language}
          onChange={(e) => handleLanguageChange(e.target.value)}
        >
          <MenuItem value="en">{t('settings.langEnglish')}</MenuItem>
          <MenuItem value="he">{t('settings.langHebrew')}</MenuItem>
        </TextField>
      </Paper>
    </Box>
  );
}
