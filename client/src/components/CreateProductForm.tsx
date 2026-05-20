import { useState, FormEvent } from 'react';
import { apiFetch } from '../services/api';
import { Product, CreateProductRequest } from '../types';

interface Props {
  onProductCreated: () => void;
}

export default function CreateProductForm({ onProductCreated }: Props) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [price, setPrice] = useState('');
  const [colour, setColour] = useState('');
  const [errors, setErrors] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErrors([]);
    setLoading(true);

    const request: CreateProductRequest = {
      name,
      description,
      price: parseFloat(price) || 0,
      colour,
    };

    try {
      await apiFetch<Product>('/api/products', {
        method: 'POST',
        body: JSON.stringify(request),
      });
      setName('');
      setDescription('');
      setPrice('');
      setColour('');
      onProductCreated();
    } catch (err: unknown) {
      const apiErr = err as { errors?: string[]; message?: string };
      if (apiErr.errors) {
        setErrors(apiErr.errors);
      } else if (apiErr.message) {
        setErrors([apiErr.message]);
      } else {
        setErrors(['An unexpected error occurred']);
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="create-form">
      <h2>New Product</h2>

      {errors.length > 0 && (
        <div className="alert alert-error" role="alert">
          {errors.map((err, i) => (
            <p key={i}>{err}</p>
          ))}
        </div>
      )}

      <div className="form-grid">
        <div className="form-group">
          <label htmlFor="product-name">Name</label>
          <input
            id="product-name"
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="e.g. Widget Pro"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="product-colour">Colour</label>
          <input
            id="product-colour"
            type="text"
            value={colour}
            onChange={(e) => setColour(e.target.value)}
            placeholder="e.g. Blue"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="product-price">Price (£)</label>
          <input
            id="product-price"
            type="number"
            step="0.01"
            min="0"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
            placeholder="0.00"
            required
          />
        </div>
      </div>

      <div className="form-group">
        <label htmlFor="product-description">Description <span className="label-optional">(optional)</span></label>
        <input
          id="product-description"
          type="text"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Brief product description"
        />
      </div>

      <button type="submit" className="btn btn-primary" disabled={loading}>
        {loading ? (
          <span className="btn-loading">
            <span className="spinner" aria-hidden="true" />
            Creating...
          </span>
        ) : (
          'Create Product'
        )}
      </button>
    </form>
  );
}
