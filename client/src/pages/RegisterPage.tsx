import { useState, FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import toast from 'react-hot-toast';

export default function RegisterPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();

    if (password !== confirmPassword) {
      toast.error('Passwords do not match');
      return;
    }

    if (password.length < 8) {
      toast.error('Password must be at least 8 characters');
      return;
    }

    if (!/[A-Z]/.test(password)) {
      toast.error('Password must contain at least one uppercase letter');
      return;
    }

    if (!/[a-z]/.test(password)) {
      toast.error('Password must contain at least one lowercase letter');
      return;
    }

    if (!/[0-9]/.test(password)) {
      toast.error('Password must contain at least one digit');
      return;
    }

    if (!/[^a-zA-Z0-9]/.test(password)) {
      toast.error('Password must contain at least one special character');
      return;
    }

    if (!/^[a-zA-Z0-9_]+$/.test(username)) {
      toast.error('Username can only contain letters, numbers, and underscores');
      return;
    }

    setLoading(true);

    try {
      await register({ username, password });
      toast.success('Account created! Welcome.');
      navigate('/');
    } catch (err: unknown) {
      const apiErr = err as { message?: string };
      toast.error(apiErr?.message || 'Registration failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <div className="login-header">
          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" aria-hidden="true">
            <rect x="3" y="3" width="7" height="7" rx="1.5" fill="var(--primary)" opacity="0.9" />
            <rect x="14" y="3" width="7" height="7" rx="1.5" fill="var(--primary)" opacity="0.7" />
            <rect x="3" y="14" width="7" height="7" rx="1.5" fill="var(--primary)" opacity="0.7" />
            <rect x="14" y="14" width="7" height="7" rx="1.5" fill="var(--primary)" opacity="0.5" />
          </svg>
          <h1>Products</h1>
          <p>Create an account to get started</p>
        </div>

        <form onSubmit={handleSubmit} className="login-form">
          <div className="form-group">
            <label htmlFor="username">Username</label>
            <input
              id="username"
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="Letters, numbers, underscores (3-30 chars)"
              required
              minLength={3}
              maxLength={30}
              pattern="[a-zA-Z0-9_]+"
              autoComplete="username"
              autoFocus
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Min 8 chars, upper, lower, digit, special"
              required
              minLength={8}
              maxLength={128}
              autoComplete="new-password"
            />
          </div>

          <div className="form-group">
            <label htmlFor="confirm-password">Confirm Password</label>
            <input
              id="confirm-password"
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              placeholder="Confirm your password"
              required
              minLength={8}
              autoComplete="new-password"
            />
          </div>

          <button type="submit" className="btn btn-primary btn-full" disabled={loading}>
            {loading ? (
              <span className="btn-loading">
                <span className="spinner" aria-hidden="true" />
                Creating account...
              </span>
            ) : (
              'Create Account'
            )}
          </button>
        </form>

        <div className="login-footer">
          <div className="password-requirements">
            <p><strong>Password requirements:</strong></p>
            <ul>
              <li>At least 8 characters</li>
              <li>One uppercase letter</li>
              <li>One lowercase letter</li>
              <li>One digit</li>
              <li>One special character</li>
            </ul>
          </div>
          <p>Already have an account? <Link to="/login" className="auth-link">Sign in</Link></p>
        </div>
      </div>
    </div>
  );
}
