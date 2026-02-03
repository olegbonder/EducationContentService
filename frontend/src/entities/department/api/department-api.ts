import { apiClient } from '@/shared/api/axios-instance';
import { Envelope } from '@/shared/api/envelope';
import { PaginationResponse } from '@/shared/api/types';

export type { PaginationResponse }; // Re-export PaginationResponse

export interface Department {
  id: string;
  name: string;
  parentId: string | null;
  hasMoreChildren: boolean;
  depth: number;
  path: string; // breadcrumb path
  isActive: boolean;
}

export interface DepartmentWithChildren extends Department {
  children?: DepartmentWithChildren[];
  childrenPage?: number;
  totalChildrenCount?: number;
}

// API methods for departments
export const departmentApi = {
  // Get root departments with first N children
  getRootDepartments: async (page = 1, pageSize = 10): Promise<PaginationResponse<DepartmentWithChildren>> => {
    const response = await apiClient.get<Envelope<PaginationResponse<Department>>>(`/departments/root`, {
      params: { page, pageSize }
    });

    // Transform response to include childrenPage and totalChildrenCount
    const departments = response.data.result?.items.map(dep => ({
      ...dep,
      childrenPage: 1,
      totalChildrenCount: dep.hasMoreChildren ? 0 : undefined // Will be updated when children are loaded
    })) || [];

    return {
      ...response.data.result!,
      items: departments
    };
  },

  // Get children of a department
  getDepartmentChildren: async (
    parentId: string,
    page = 1,
    pageSize = 10
  ): Promise<PaginationResponse<DepartmentWithChildren>> => {
    const response = await apiClient.get<Envelope<PaginationResponse<Department>>>(
      `/departments/${parentId}/children`,
      { params: { page, pageSize } }
    );

    // Transform response to include childrenPage and totalChildrenCount
    const departments = response.data.result?.items.map(dep => ({
      ...dep,
      childrenPage: page,
      totalChildrenCount: dep.hasMoreChildren ? 0 : undefined // Will be updated when children are loaded
    })) || [];

    return {
      ...response.data.result!,
      items: departments
    };
  }
};