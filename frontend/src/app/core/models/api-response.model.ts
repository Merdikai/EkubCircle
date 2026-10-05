// RFC 7807 ProblemDetails and Result Pattern matching ASP.NET Core API

export interface ProblemDetails {
  type?: string;
  title: string;
  status: number;
  detail: string;
  instance?: string;
  errors?: Record<string, string[]>;
  errorCode?: string;
}

export interface ApiResult<T> {
  isSuccess: boolean;
  value?: T;
  error?: ProblemDetails;
}

export interface PagedList<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
