export interface TodoTaskCreateRequest {
  title: string;
  listId: string;
  dueDate?: string | null;
}
