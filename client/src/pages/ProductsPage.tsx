import { useState, useEffect, useCallback, useMemo } from 'react';
import { apiFetch } from '../services/api';
import { Product } from '../types';
import CreateProductForm from '../components/CreateProductForm';
import { TableSkeleton } from '../components/Skeleton';
import EmptyState from '../components/EmptyState';
import toast from 'react-hot-toast';

export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);

  const fetchProducts = useCallback(async () => {
    setLoading(true);
    try {
      const data = await apiFetch<{ items: Product[]; totalCount: number; page: number; pageSize: number; totalPages: number }>('/api/products?pageSize=100');
      setProducts(data.items);
    } catch {
      toast.error('Failed to load products');
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

  const handleProductCreated = () => {
    fetchProducts();
    setShowForm(false);
    toast.success('Product created successfully');
  };

  return (
    <div className="page-container">
      <div className="page-header">
        <div>
          <h1 className="page-title">Products</h1>
          <p className="page-subtitle">{products.length} product{products.length !== 1 ? 's' : ''} in catalogue</p>
        </div>
        <button
          onClick={() => setShowForm(!showForm)}
          className={`btn ${showForm ? 'btn-secondary' : 'btn-primary'}`}
        >
          {showForm ? 'Cancel' : '+ New Product'}
        </button>
      </div>

      {showForm && (
        <div className="slide-in">
          <CreateProductForm onProductCreated={handleProductCreated} />
        </div>
      )}

      <div className="toolbar">
        <div className="search-input-wrapper">
          <svg className="search-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
            <circle cx="11" cy="11" r="8" />
            <line x1="21" y1="21" x2="16.65" y2="16.65" />
          </svg>
          <input
            id="search-filter"
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Search by name, colour, or description..."
            aria-label="Search products"
            className="search-input"
          />
          {searchTerm && (
            <button
              onClick={() => setSearchTerm('')}
              className="search-clear"
              aria-label="Clear search"
            >
              ×
            </button>
          )}
        </div>
      </div>

      {loading ? (
        <TableSkeleton rows={5} />
      ) : filteredProducts.length === 0 ? (
        <EmptyState
          title={searchTerm ? 'No matching products' : 'No products yet'}
          description={searchTerm ? 'Try adjusting your search terms' : 'Create your first product to get started'}
        />
      ) : (
        <>
          {/* Desktop table view */}
          <div className="table-wrapper">
            <table className="products-table" role="table">
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
                {filteredProducts.map((product) => (
                  <tr key={product.id}>
                    <td className="cell-name">{product.name}</td>
                    <td className="cell-desc">{product.description || '—'}</td>
                    <td className="cell-price">£{product.price.toFixed(2)}</td>
                    <td>
                      <span className="colour-badge">{product.colour}</span>
                    </td>
                    <td className="cell-date">{new Date(product.createdAt).toLocaleDateString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Mobile card view */}
          <div className="product-cards">
            {filteredProducts.map((product) => (
              <div key={product.id} className="product-card">
                <div className="product-card-header">
                  <h3>{product.name}</h3>
                  <span className="product-card-price">£{product.price.toFixed(2)}</span>
                </div>
                {product.description && (
                  <p className="product-card-desc">{product.description}</p>
                )}
                <div className="product-card-footer">
                  <span className="colour-badge">{product.colour}</span>
                  <span className="product-card-date">{new Date(product.createdAt).toLocaleDateString()}</span>
                </div>
              </div>
            ))}
          </div>
        </>
      )}
    </div>
  );
}
