export interface ApiResponse<T> {
  exitoso: boolean;
  mensaje?: string;
  data?: T;
  errores?: string[];
}
