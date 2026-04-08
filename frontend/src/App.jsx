import { useState, useEffect } from 'react';
import axios from 'axios';

const API_URL = 'https://localhost:7000/api'; // Sesuaikan port backend-mu

function App() {
  const [token, setToken] = useState(localStorage.getItem('token') || '');
  const [products, setProducts] = useState([]);
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('password123');

  // 1. Fungsi Login
  const handleLogin = async (e) => {
    e.preventDefault();
    try {
      const res = await axios.post(`${API_URL}/Auth/login`, { username, password });
      const jwt = res.data.token;
      setToken(jwt);
      localStorage.setItem('token', jwt);
      alert('Login Berhasil!');
    } catch (err) { alert('Login Gagal! Cek API/Auth'); }
  };

  // 2. Load Data Produk
  const fetchProducts = async () => {
    try {
      const res = await axios.get(`${API_URL}/Products`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      setProducts(res.data);
    } catch (err) { console.error("Unauthorized atau API mati"); }
  };

  useEffect(() => { if (token) fetchProducts(); }, [token]);

  // 3. UI Login
  if (!token) {
    return (
      <div style={{ padding: '20px', textAlign: 'center' }}>
        <h2>Product System - Login</h2>
        <form onSubmit={handleLogin}>
          <input type="text" value={username} onChange={e => setUsername(e.target.value)} /><br/>
          <input type="password" value={password} onChange={e => setPassword(e.target.value)} /><br/><br/>
          <button type="submit">Login</button>
        </form>
      </div>
    );
  }

  // 4. UI Dashboard Produk
  return (
    <div style={{ padding: '20px' }}>
      <h2>Dashboard Produk</h2>
      <button onClick={() => { localStorage.clear(); setToken(''); }}>Logout</button>
      <hr/>
      <table border="1" cellPadding="10" style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr>
            <th>ID</th>
            <th>Nama</th>
            <th>Harga</th>
            <th>Stok</th>
          </tr>
        </thead>
        <tbody>
          {products.map(p => (
            <tr key={p.id}>
              <td>{p.id}</td>
              <td>{p.name}</td>
              <td>{p.price}</td>
              <td>{p.stock}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default App;