export interface PagedAndSortedRequest {
  page: number;
  pageSize: number;
  sortBy: string | null;
  sortDirection: 'asc' | 'desc';
  searchKeyword: string | null;
}
