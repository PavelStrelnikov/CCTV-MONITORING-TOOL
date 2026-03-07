export interface HealthSummary {
  reachable: boolean;
  camera_count: number;
  online_cameras: number;
  offline_cameras: number;
  disk_ok: boolean;
  response_time_ms: number;
  checked_at: string;
}

export interface Device {
  device_id: string;
  name: string;
  vendor: string;
  host: string;
  port: number;
  is_active: boolean;
  last_health: HealthSummary | null;
}

export interface CameraChannel {
  channel_id: string;
  channel_name: string;
  status: string;
  ip_address: string | null;
  checked_at: string;
}

export interface Disk {
  disk_id: string;
  status: string;
  capacity_bytes: number;
  free_bytes: number;
  health_status: string;
  checked_at: string;
}

export interface Alert {
  id: number;
  alert_type: string;
  severity: string;
  message: string;
  status: string;
  created_at: string;
  resolved_at: string | null;
}

export interface DeviceDetail {
  device: Device;
  cameras: CameraChannel[];
  disks: Disk[];
  alerts: Alert[];
}

export interface DeviceCreate {
  device_id: string;
  name: string;
  vendor: string;
  host: string;
  port: number;
  username: string;
  password: string;
}

export interface PollResult {
  device_id: string;
  health: HealthSummary;
}

export interface Overview {
  total_devices: number;
  reachable_devices: number;
  total_cameras: number;
  online_cameras: number;
  disks_ok: boolean;
}
