const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5008';

export const authService = {
  async login(username: string, password: string): Promise<void> {
    const res = await fetch(`${API_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password }),
    });

    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.message ?? 'Login failed.');
    }

    const data = await res.json();
    localStorage.setItem('token', data.token);
  },

  async register(username: string, password: string): Promise<void> {
    const res = await fetch(`${API_URL}/api/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password }),
    });

    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      throw new Error(err.message ?? 'Registration failed.');
    }
  },

  logout(): void {
    localStorage.removeItem('token');
  },

  getToken(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('token');
  },

  isAuthenticated(): boolean {
    return Boolean(this.getToken());
  },
};
