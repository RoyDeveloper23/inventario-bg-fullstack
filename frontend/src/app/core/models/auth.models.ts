export interface LoginRequest {
  email: string;
  password: string;
}

export interface Usuario {
  id: string;
  nombre: string;
  email: string;
}

export interface LoginResponse {
  accessToken: string;
  tokenType: string;
  expiresAtUtc: string;
  usuario: Usuario;
}
