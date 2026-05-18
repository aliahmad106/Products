export interface Product {
  id: string;
  name: string;
  description: string | null;
  price: number;
  colour: string;
  createdAt: string;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
  colour: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
}

export interface ApiError {
  traceId: string;
  statusCode: number;
  message: string;
  errors: string[] | null;
}
