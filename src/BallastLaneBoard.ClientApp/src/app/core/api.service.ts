import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TaskResponse, CreateTaskRequest, UpdateTaskRequest, ChangeTaskStatusRequest } from '../tasks/task.model';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = '/api';

  constructor(private http: HttpClient) {}

  getTasks(): Observable<TaskResponse[]> {
    return this.http.get<TaskResponse[]>(`${this.baseUrl}/tasks`);
  }

  getTask(id: string): Observable<TaskResponse> {
    return this.http.get<TaskResponse>(`${this.baseUrl}/tasks/${id}`);
  }

  createTask(request: CreateTaskRequest): Observable<TaskResponse> {
    return this.http.post<TaskResponse>(`${this.baseUrl}/tasks`, request);
  }

  updateTask(id: string, request: UpdateTaskRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/tasks/${id}`, request);
  }

  changeTaskStatus(id: string, request: ChangeTaskStatusRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/tasks/${id}/status`, request);
  }

  deleteTask(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/tasks/${id}`);
  }

  register(username: string, email: string, password: string): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/auth/register`, { username, email, password });
  }
}
