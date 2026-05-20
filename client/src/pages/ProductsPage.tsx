import { useState, useEffect, useCallback, useMemo } from 'react';
import { apiFetch } from '../services/api';
import { Product } from '../types';
import CreateProductForm from '../components/CreateProductForm';
import EditProductModal from '../components/EditProductModal';
import { TableSkeleton } from '../components/Skeleton';
import EmptyState from '../components/EmptyState';
import toast from 'react-hot-toast';

export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);

  const fetchProducts = useCallback(async () => {
    setLoading(true);
    try {
      const data = await apiFetch<{ items: Product[]; totalCount: number }>('/api/products?pageSize=100');
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

  const handleDelete = async (product: Product) => {
    if (!confirm(`Delete "${product.name}"? This cannot be undone.`)) return;

    try {
      await apiFetch(`/api/products/${product.id}`, { method: 'DELETE' });
      toast.success('Product deleted');
      fetchProducts();
    } catch {
      toast.error('Failed to delete product');
    }
  };

  const handleEditSaved = () => {
    setEditingProduct(null);
    toast.success('Product updated');
    fetchProducts();
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
            <button onClick={() => setSearchTerm('')} className="search-clear" aria-label="Clear search">×</button>
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
          <div className="table-wrapper">
            <table className="products-table" role="table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Description</th>
                  <th>Price</th>
                  <th>Colour</th>
                  <th>Created</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredProducts.map((product) => (
                  <tr key={product.id}>
                    <td className="cell-name">{product.name}</td>
                    <td className="cell-desc">{product.description || '—'}</td>
                    <td className="cell-price">£{product.price.toFixed(2)}</td>
                    <td><span className="colour-badge">{product.colour}</span></td>
                    <td className="cell-date">{new Date(product.createdAt).toLocaleDateString()}</td>
                    <td className="cell-actions">
                      <button onClick={() => setEditingProduct(product)} className="btn-icon" aria-label="Edit product" title="Edit">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                          <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                          <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
                        </svg>
                      </button>
                      <button onClick={() => handleDelete(product)} className="btn-icon btn-icon-danger" aria-label="Delete product" title="Delete">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                          <polyline points="3 6 5 6 21 6" />
                          <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                        </svg>
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="product-cards">
            {filteredProducts.map((product) => (
              <div key={product.id} className="product-card">
                <div className="product-card-header">
                  <h3>{product.name}</h3>
                  <span className="product-card-price">£{product.price.toFixed(2)}</span>
                </div>
                {product.description && <p className="product-card-desc">{product.description}</p>}
                <div className="product-card-footer">
                  <span className="colour-badge">{product.colour}</span>
                  <div className="card-actions">
                    <button onClick={() => setEditingProduct(product)} className="btn-icon" aria-label="Edit">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                        <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                        <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
                      </svg>
                    </button>
                    <button onClick={() => handleDelete(product)} className="btn-icon btn-icon-danger" aria-label="Delete">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                        <polyline points="3 6 5 6 21 6" />
                        <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                      </svg>
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </>
      )}

      {editingProduct && (
        <EditProductModal
          product={editingProduct}
          onSaved={handleEditSaved}
          onCancel={() => setEditingProduct(null)}
        />
      )}
    </div>
  );
}
