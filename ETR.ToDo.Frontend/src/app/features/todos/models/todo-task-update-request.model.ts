export interface TodoTaskUpdateRequest {
  title: string;
  listId: string;
  dueDate?: string | null;
}
