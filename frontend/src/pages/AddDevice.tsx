import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../api/client';
import type { DeviceCreate } from '../types';

export default function AddDevice() {
  const navigate = useNavigate();
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState<DeviceCreate>({
    device_id: '',
    name: '',
    vendor: 'hikvision',
    host: '',
    port: 80,
    username: 'admin',
    password: '',
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setForm(prev => ({
      ...prev,
      [name]: name === 'port' ? parseInt(value, 10) || 0 : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError('');
    try {
      await api.createDevice(form);
      navigate('/');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add device');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <h2>Add Device</h2>
      {error && <div className="error">{error}</div>}
      <div className="card" style={{ maxWidth: 500 }}>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Device ID</label>
            <input name="device_id" value={form.device_id} onChange={handleChange}
              placeholder="nvr-building-1" required />
          </div>
          <div className="form-group">
            <label>Name</label>
            <input name="name" value={form.name} onChange={handleChange}
              placeholder="Building 1 NVR" required />
          </div>
          <div className="form-group">
            <label>Vendor</label>
            <select name="vendor" value={form.vendor} onChange={handleChange}>
              <option value="hikvision">Hikvision</option>
              <option value="dahua">Dahua</option>
              <option value="provision">Provision</option>
            </select>
          </div>
          <div className="form-group">
            <label>Host</label>
            <input name="host" value={form.host} onChange={handleChange}
              placeholder="192.168.1.100" required />
          </div>
          <div className="form-group">
            <label>Port</label>
            <input name="port" type="number" value={form.port} onChange={handleChange} required />
          </div>
          <div className="form-group">
            <label>Username</label>
            <input name="username" value={form.username} onChange={handleChange} required />
          </div>
          <div className="form-group">
            <label>Password</label>
            <input name="password" type="password" value={form.password} onChange={handleChange} required />
          </div>
          <button type="submit" className="btn-primary" disabled={submitting}>
            {submitting ? 'Adding...' : 'Add Device'}
          </button>
        </form>
      </div>
    </div>
  );
}
