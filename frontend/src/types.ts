export interface TimeCheck {
  device_time?: string;
  server_time?: string;
  drift_seconds?: number;
  timezone?: string | null;
  time_mode?: string | null;
}

export interface HealthSummary {
  reachable: boolean;
  camera_count: number;
  online_cameras: number;
  offline_cameras: number;
  disk_ok: boolean;
  response_time_ms: number;
  checked_at: string;
  web_port_open?: boolean | null;
  sdk_port_open?: boolean | null;
  time_check?: TimeCheck | null;
}

export interface Tag {
  name: string;
  color: string;
}

export interface Device {
  device_id: string;
  name: string;
  vendor: string;
  host: string;
  web_port: number | null;
  sdk_port: number | null;
  transport_mode: string;
  is_active: boolean;
  last_health: HealthSummary | null;
  model?: string | null;
  serial_number?: string | null;
  firmware_version?: string | null;
  last_poll_at?: string | null;
  poll_interval_seconds?: number | null;
  tags: Tag[];
  ignored_channels: string[];
}

export interface CameraChannel {
  channel_id: string;
  channel_name: string;
  status: string;
  ip_address: string | null;
  recording?: string | null;
  checked_at: string;
}

export interface Disk {
  disk_id: string;
  status: string;
  capacity_bytes: number;
  free_bytes: number;
  health_status: string;
  checked_at: string;
  temperature?: number | null;
  power_on_hours?: number | null;
  smart_status?: string | null;
}

export interface Alert {
  id: number;
  device_id: string;
  device_name: string;
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
  health: HealthSummary | null;
}

export interface HealthLogEntry {
  reachable: boolean;
  camera_count: number;
  online_cameras: number;
  offline_cameras: number;
  disk_ok: boolean;
  response_time_ms: number;
  checked_at: string;
}

export interface DeviceCreate {
  device_id: string;
  name: string;
  vendor: string;
  host: string;
  web_port: number | null;
  sdk_port: number | null;
  username: string;
  password: string;
  transport_mode: string;
  poll_interval_seconds: number | null;
}

export interface DeviceUpdate {
  name?: string;
  host?: string;
  web_port?: number | null;
  sdk_port?: number | null;
  username?: string;
  password?: string;
  is_active?: boolean;
  transport_mode?: string;
  poll_interval_seconds?: number | null;
}

export interface PollResult {
  device_id: string;
  health: HealthSummary;
}

export interface PollLogEntry {
  device_id: string;
  device_name: string;
  reachable: boolean;
  camera_count: number;
  online_cameras: number;
  offline_cameras: number;
  disk_ok: boolean;
  response_time_ms: number;
  checked_at: string;
}

export interface OverviewDeviceSummary {
  device_id: string;
  name: string;
  reachable: boolean;
  camera_count: number;
  online_cameras: number;
  offline_cameras: number;
  disk_ok: boolean;
  recording_total: number;
  recording_ok: number;
  time_drift: number | null;
  last_poll_at: string | null;
}

export interface Overview {
  total_devices: number;
  reachable_devices: number;
  unreachable_devices: number;
  total_cameras: number;
  online_cameras: number;
  offline_cameras: number;
  total_disks: number;
  disks_ok_count: number;
  disks_error_count: number;
  disks_ok: boolean;
  recording_total: number;
  recording_ok: number;
  time_drift_issues: number;
  devices: OverviewDeviceSummary[];
}

export interface SystemSettings {
  default_poll_interval: number;
}
