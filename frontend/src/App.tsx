import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout';
import DeviceList from './pages/DeviceList';
import AddDevice from './pages/AddDevice';
import DeviceDetail from './pages/DeviceDetail';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<DeviceList />} />
          <Route path="/devices/add" element={<AddDevice />} />
          <Route path="/devices/:deviceId" element={<DeviceDetail />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
