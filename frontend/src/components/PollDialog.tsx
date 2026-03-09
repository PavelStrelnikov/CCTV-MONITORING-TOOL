import { useState, useEffect, useCallback, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import Button from '@mui/material/Button';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import CircularProgress from '@mui/material/CircularProgress';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorIcon from '@mui/icons-material/Error';
import SkipNextIcon from '@mui/icons-material/SkipNext';
import NetworkCheckIcon from '@mui/icons-material/NetworkCheck';
import RefreshIcon from '@mui/icons-material/Refresh';

interface PollStep {
  step: string;
  status: 'pending' | 'running' | 'success' | 'error' | 'skipped';
  detail: string;
}

function getStepLabel(step: string, t: (key: string) => string): string {
  const labels: Record<string, string> = {
    web_port: t('poll.webPort'),
    sdk_port: t('poll.sdkPort'),
    connect: t('poll.connect'),
    device_info: t('poll.deviceInfo'),
    cameras: t('poll.cameras'),
    disks: t('poll.disks'),
    recording: t('poll.recording'),
    time_check: t('poll.timeCheck'),
  };
  return labels[step] || step;
}

const INITIAL_STEPS: PollStep[] = [
  { step: 'web_port', status: 'pending', detail: '' },
  { step: 'sdk_port', status: 'pending', detail: '' },
  { step: 'connect', status: 'pending', detail: '' },
  { step: 'device_info', status: 'pending', detail: '' },
  { step: 'cameras', status: 'pending', detail: '' },
  { step: 'disks', status: 'pending', detail: '' },
  { step: 'recording', status: 'pending', detail: '' },
  { step: 'time_check', status: 'pending', detail: '' },
];

interface PollDialogProps {
  open: boolean;
  onClose: () => void;
  deviceId: string;
  deviceName?: string;
  onPollComplete?: () => void;
}

export default function PollDialog({ open, onClose, deviceId, deviceName, onPollComplete }: PollDialogProps) {
  const { t } = useTranslation();
  const [steps, setSteps] = useState<PollStep[]>(INITIAL_STEPS);
  const [done, setDone] = useState(false);
  const [finalStatus, setFinalStatus] = useState<'success' | 'error' | null>(null);
  const [finalDetail, setFinalDetail] = useState('');
  const onPollCompleteRef = useRef(onPollComplete);
  onPollCompleteRef.current = onPollComplete;

  const startPoll = useCallback(async () => {
    setSteps(INITIAL_STEPS.map(s => ({ ...s })));
    setDone(false);
    setFinalStatus(null);
    setFinalDetail('');

    try {
      const response = await fetch(`/api/devices/${deviceId}/poll-stream`);
      if (!response.ok || !response.body) {
        setDone(true);
        setFinalStatus('error');
        setFinalDetail(`HTTP ${response.status}`);
        return;
      }

      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = '';

      while (true) {
        const { done: streamDone, value } = await reader.read();
        if (streamDone) break;

        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split('\n');
        buffer = lines.pop() || '';

        for (const line of lines) {
          if (!line.startsWith('data: ')) continue;
          try {
            const data = JSON.parse(line.slice(6));
            if (data.step === 'done') {
              setDone(true);
              setFinalStatus(data.status as 'success' | 'error');
              setFinalDetail(data.detail || '');
              onPollCompleteRef.current?.();
            } else {
              setSteps(prev =>
                prev.map(s =>
                  s.step === data.step
                    ? { ...s, status: data.status, detail: data.detail || '' }
                    : s,
                ),
              );
            }
          } catch {
            // ignore parse errors
          }
        }
      }
    } catch (err) {
      setDone(true);
      setFinalStatus('error');
      setFinalDetail(err instanceof Error ? err.message : 'Connection failed');
    }
  }, [deviceId]); // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => {
    if (open) {
      startPoll();
    }
  }, [open]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleClose = () => {
    if (done) onClose();
  };

  const getStepIcon = (status: string) => {
    switch (status) {
      case 'running':
        return <CircularProgress size={20} />;
      case 'success':
        return <CheckCircleIcon color="success" fontSize="small" />;
      case 'error':
        return <ErrorIcon color="error" fontSize="small" />;
      case 'skipped':
        return <SkipNextIcon color="disabled" fontSize="small" />;
      default:
        return <Box sx={{ width: 20, height: 20, borderRadius: '50%', border: '2px solid', borderColor: '#334155' }} />;
    }
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <NetworkCheckIcon color="primary" />
        <Typography variant="h6" component="span">
          {t('deviceDetail.devicePoll')}
        </Typography>
        {deviceName && (
          <Typography variant="body2" color="text.secondary" sx={{ ml: 1 }}>
            — {deviceName}
          </Typography>
        )}
      </DialogTitle>

      <DialogContent>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5, py: 1 }}>
          {steps.map((s) => (
            <Box
              key={s.step}
              sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 1.5,
                p: 1,
                borderRadius: 1,
                bgcolor: s.status === 'running' ? 'rgba(59, 130, 246, 0.08)' : 'transparent',
              }}
            >
              {getStepIcon(s.status)}
              <Box sx={{ flex: 1, minWidth: 0 }}>
                <Typography
                  variant="body2"
                  fontWeight={s.status === 'running' ? 600 : 400}
                  color={s.status === 'skipped' ? 'text.disabled' : 'text.primary'}
                >
                  {getStepLabel(s.step, t)}
                </Typography>
                {s.detail && (
                  <Typography variant="caption" color="text.secondary" noWrap>
                    {s.detail}
                  </Typography>
                )}
              </Box>
            </Box>
          ))}
        </Box>

        {done && finalStatus && (
          <Box
            sx={{
              mt: 2,
              p: 1.5,
              borderRadius: 1,
              bgcolor: finalStatus === 'success' ? 'rgba(34, 197, 94, 0.15)' : 'rgba(239, 68, 68, 0.15)',
              border: '1px solid',
              borderColor: finalStatus === 'success' ? 'success.main' : 'error.main',
              color: finalStatus === 'success' ? 'success.main' : 'error.main',
            }}
          >
            <Typography variant="body2" fontWeight={600}>
              {finalDetail}
            </Typography>
          </Box>
        )}
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 2 }}>
        {done && finalStatus === 'error' && (
          <Button onClick={startPoll} startIcon={<RefreshIcon />} color="primary">
            {t('poll.retry')}
          </Button>
        )}
        {done && (
          <Button onClick={onClose} color="inherit">
            {t('poll.close')}
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
}
