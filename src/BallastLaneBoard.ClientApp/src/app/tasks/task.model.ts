export interface TaskResponse {
  id: string;
  title: string;
  description: string | null;
  status: TaskItemStatus;
  dueDate: string | null;
  userId: string;
  createdAt: string;
  updatedAt: string | null;
}

export type TaskItemStatus = 'Pending' | 'InProgress' | 'Completed';

export interface CreateTaskRequest {
  title: string;
  description?: string;
  dueDate?: string;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  dueDate?: string;
}

export interface ChangeTaskStatusRequest {
  status: TaskItemStatus;
}
