import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { api } from '../api/client';
import type { DeviceUpdate } from '../types';

export default function EditDevice() {
  const { deviceId } = useParams<{ deviceId: string }>();
  const navigate = useNavigate();
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState({
    name: '',
    host: '',
    port: 80,
    username: '',
    password: '',
  });

  useEffect(() => {
    if (!deviceId) return;
    api.getDeviceDetail(deviceId)
      .then(detail => {
        setForm({
          name: detail.device.name,
          host: detail.device.host,
          port: detail.device.port,
          username: '',
          password: '',
        });
      })
      .catch(err => setError(err instanceof Error ? err.message : 'Failed to load'))
      .finally(() => setLoading(false));
  }, [deviceId]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setForm(prev => ({
      ...prev,
      [name]: name === 'port' ? parseInt(value, 10) || 0 : value,
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
        port: form.port,
      };
      if (form.username) update.username = form.username;
      if (form.password) update.password = form.password;
      await api.updateDevice(deviceId, update);
      navigate(`/devices/${deviceId}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <div className="loading">Loading...</div>;

  return (
    <div>
      <h2>Edit Device: {deviceId}</h2>
      {error && <div className="error">{error}</div>}
      <div className="card" style={{ maxWidth: 500 }}>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Name</label>
            <input name="name" value={form.name} onChange={handleChange} required />
          </div>
          <div className="form-group">
            <label>Host</label>
            <input name="host" value={form.host} onChange={handleChange} required />
          </div>
          <div className="form-group">
            <label>Port</label>
            <input name="port" type="number" value={form.port} onChange={handleChange} required />
          </div>
          <div className="form-group">
            <label>Username (leave empty to keep current)</label>
            <input name="username" value={form.username} onChange={handleChange} />
          </div>
          <div className="form-group">
            <label>Password (leave empty to keep current)</label>
            <input name="password" type="password" value={form.password} onChange={handleChange} />
          </div>
          <div className="actions">
            <button type="submit" className="btn-primary" disabled={submitting}>
              {submitting ? 'Saving...' : 'Save'}
            </button>
            <button type="button" className="btn-secondary" onClick={() => navigate(-1)}>
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
