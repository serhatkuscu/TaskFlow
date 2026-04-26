export type TaskStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled';

export interface TaskItem {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  createdAt: string;
}

export interface PagedResult {
  items: TaskItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
