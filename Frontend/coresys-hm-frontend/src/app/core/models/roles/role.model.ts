export interface Role {
  id: number;
  name: string;
  description: string | null;
  isActive: boolean;
  isSystem: boolean;
  isSeeded: boolean;
  permissions: string[];
  cantidadUsuarios: number;
  createdAt: string;
}

export interface CreateRoleRequest {
  name: string;
  description?: string;
  permissions: string[];
}

export interface UpdateRoleRequest {
  name: string;
  description?: string;
  isActive: boolean;
  permissions: string[];
}
