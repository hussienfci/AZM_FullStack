/**
 * Base URL for the UserManagementApi backend.
 * In development, `/api` is forwarded to the .NET API by `proxy.conf.json`,
 * which avoids CORS and self-signed certificate issues.
 */
export const API_BASE_URL = '/api';

/** Endpoints that must never carry an Authorization header or trigger a logout on 401. */
export const PUBLIC_ENDPOINTS: readonly string[] = [
  `${API_BASE_URL}/users/login`,
  `${API_BASE_URL}/users/register`,
];
