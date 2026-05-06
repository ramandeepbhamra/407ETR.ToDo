export interface ApiLog {
  id: string;
  userId: string | null;
  userName: string | null;
  httpMethod: string;
  serviceName: string;
  methodName: string | null;
  parameters: string | null;
  executionTime: string;
  executionDuration: number;
  clientIpAddress: string | null;
  userAgent: string | null;
  statusCode: number;
  exceptionMessage: string | null;
  createdDate: string;
}
