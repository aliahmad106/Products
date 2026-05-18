import { useState, useEffect, useCallback, useMemo } from 'react';
import { useAuth } from '../hooks/useAuth';
import { apiFetch } from '../services/api';
import { Product } from '../types';
import CreateProductForm from '../components/CreateProductForm';

export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [loading, setLoading] = useState(false);
  const { logout } = useAuth();

  const fetchProducts = useCallback(async () => {
    setLoading(true);
    try {
      const data = await apiFetch<Product[]>('/api/products');
      setProducts(data);
    } catch {
      // 401 is handled by apiFetch redirect
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const filteredProducts = useMemo(() => {
    const term = searchTerm.trim().toLowerCase();
    if (!term) return products;
    return products.filter(
      (p) =>
        p.name.toLowerCase().includes(term) ||
        p.colour.toLowerCase().includes(term) ||
        (p.description && p.description.toLowerCase().includes(term))
    );
  }, [products, searchTerm]);

  return (
    <div className="products-container">
      <header className="app-header">
        <h1>Products</h1>
        <button onClick={logout} className="logout-btn">Logout</button>
      </header>

      <CreateProductForm onProductCreated={fetchProducts} />

      <div className="filter-section">
        <label htmlFor="search-filter">Search products:</label>
        <input
          id="search-filter"
          type="text"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          placeholder="Search by name, colour, or description..."
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
            {filteredProducts.length === 0 ? (
              <tr>
                <td colSpan={5} className="empty-state">No products found</td>
              </tr>
            ) : (
              filteredProducts.map((product) => (
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
