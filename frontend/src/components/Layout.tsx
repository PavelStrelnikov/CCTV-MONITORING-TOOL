import { Link, Outlet } from 'react-router-dom';

export default function Layout() {
  return (
    <div className="app">
      <header className="header">
        <h1><Link to="/">CCTV Monitor</Link></h1>
        <nav>
          <Link to="/">Devices</Link>
          <Link to="/devices/add">Add Device</Link>
        </nav>
      </header>
      <main className="main">
        <Outlet />
      </main>
    </div>
  );
}
