import { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../hooks/useAuth';
import { apiFetch } from '../services/api';
import { Product } from '../types';
import CreateProductForm from '../components/CreateProductForm';

export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [colourFilter, setColourFilter] = useState('');
  const [loading, setLoading] = useState(false);
  const { logout } = useAuth();

  const fetchProducts = useCallback(async () => {
    setLoading(true);
    try {
      const query = colourFilter.trim() ? `?colour=${encodeURIComponent(colourFilter.trim())}` : '';
      const data = await apiFetch<Product[]>(`/api/products${query}`);
      setProducts(data);
    } catch {
      // 401 is handled by apiFetch redirect
    } finally {
      setLoading(false);
    }
  }, [colourFilter]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  return (
    <div className="products-container">
      <header className="app-header">
        <h1>Products</h1>
        <button onClick={logout} className="logout-btn">Logout</button>
      </header>

      <CreateProductForm onProductCreated={fetchProducts} />

      <div className="filter-section">
        <label htmlFor="colour-filter">Filter by colour:</label>
        <input
          id="colour-filter"
          type="text"
          value={colourFilter}
          onChange={(e) => setColourFilter(e.target.value)}
          placeholder="e.g. Red, Blue"
        />
      </div>

      {loading && <div className="loading" aria-live="polite">Loading products...</div>}

      {!loading && (
        <table className="products-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>Price</th>
              <th>Colour</th>
              <th>Created</th>
            </tr>
          </thead>
          <tbody>
            {products.length === 0 ? (
              <tr>
                <td colSpan={5} className="empty-state">No products found</td>
              </tr>
            ) : (
              products.map((product) => (
                <tr key={product.id}>
                  <td>{product.name}</td>
                  <td>{product.description || '—'}</td>
                  <td>£{product.price.toFixed(2)}</td>
                  <td>{product.colour}</td>
                  <td>{new Date(product.createdAt).toLocaleDateString()}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
