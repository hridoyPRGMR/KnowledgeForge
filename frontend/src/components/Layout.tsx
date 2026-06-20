import { NavLink, Outlet } from 'react-router-dom';

export default function Layout() {
  return (
    <div className="app">
      <nav className="sidebar">
        <div className="logo">KnowledgeForge</div>
        <NavLink to="/" end>Dashboard</NavLink>
        <NavLink to="/books/upload">Upload</NavLink>
        <NavLink to="/cross-book">Cross-Book</NavLink>
        <NavLink to="/profile">Profile</NavLink>
      </nav>
      <main className="content">
        <Outlet />
      </main>
    </div>
  );
}
