import { useState, FormEvent } from 'react';
import { apiFetch } from '../services/api';
import { Product } from '../types';

interface Props {
  product: Product;
  onSaved: () => void;
  onCancel: () => void;
}

export default function EditProductModal({ product, onSaved, onCancel }: Props) {
  const [name, setName] = useState(product.name);
  const [description, setDescription] = useState(product.description || '');
  const [price, setPrice] = useState(product.price.toString());
  const [colour, setColour] = useState(product.colour);
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErrors([]);
    setLoading(true);

    try {
      await apiFetch(`/api/products/${product.id}`, {
        method: 'PUT',
        body: JSON.stringify({
          name,
          description,
          price: parseFloat(price) || 0,
          colour,
        }),
      });
      onSaved();
    } catch (err: unknown) {
      const apiErr = err as { errors?: string[]; message?: string };
      if (apiErr.errors) setErrors(apiErr.errors);
      else if (apiErr.message) setErrors([apiErr.message]);
      else setErrors(['An unexpected error occurred']);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onCancel}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Edit Product</h2>
          <button onClick={onCancel} className="modal-close" aria-label="Close">×</button>
        </div>

        {errors.length > 0 && (
          <div className="alert alert-error" role="alert">
            {errors.map((err, i) => <p key={i}>{err}</p>)}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div className="form-grid">
            <div className="form-group">
              <label htmlFor="edit-name">Name</label>
              <input id="edit-name" type="text" value={name} onChange={(e) => setName(e.target.value)} required />
            </div>
            <div className="form-group">
              <label htmlFor="edit-colour">Colour</label>
              <input id="edit-colour" type="text" value={colour} onChange={(e) => setColour(e.target.value)} required />
            </div>
            <div className="form-group">
              <label htmlFor="edit-price">Price (£)</label>
              <input id="edit-price" type="number" step="0.01" min="0" value={price} onChange={(e) => setPrice(e.target.value)} required />
            </div>
          </div>
          <div className="form-group">
            <label htmlFor="edit-description">Description</label>
            <input id="edit-description" type="text" value={description} onChange={(e) => setDescription(e.target.value)} />
          </div>
          <div className="modal-actions">
            <button type="button" onClick={onCancel} className="btn btn-secondary">Cancel</button>
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
