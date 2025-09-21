export enum TimeRecordType {
  ClockIn = 1,
  ClockOut = 2,
  BreakStart = 3,
  BreakEnd = 4
}

export interface CreateTimeRecordRequest {
  entryTime: Date;
  description?: string;
}

export interface UpdateTimeRecordRequest {
  exitTime: Date;
  description?: string;
}

export interface TimeRecord {
  id: number;
  date: string;
  entryTime: Date;
  exitTime?: Date;
  totalHours?: number;
  description?: string;
  userId: string;
  createdAt: string;
  updatedAt: string;
}

export interface TimeRecordCreate {
  type: TimeRecordType;
  description?: string;
  timestamp?: string;
}

export interface TimeRecordResponse {
  id: number;
  timestamp: string;
  type: TimeRecordType;
  typeDescription: string;
  description?: string;
  createdAt: string;
}

export interface TimeRecordList {
  id: number;
  timestamp: string;
  type: TimeRecordType;
  typeDescription: string;
  description?: string;
}

export interface TimeRecordFilter {
  startDate?: string;
  endDate?: string;
  type?: TimeRecordType;
  page: number;
  pageSize: number;
}

export interface TimeRecordSummary {
  date: string;
  records: TimeRecordList[];
  totalWorkedTime?: string;
  isComplete: boolean;
}