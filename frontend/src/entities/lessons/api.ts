import { apiClient } from "@/shared/api/axios-instance";
import { PaginationResponse } from "@/shared/api/types";

export type CreateLessonRequest = {
  title: string;
  description: string;
};

export type GetLessonsRequest = {
  search?: string;
  page: number;
  pageSize: number;
};

export type Envelope<T = unknown> = {
  result: T | null;
  error: ApiError | null;
  isError: boolean;
  timeGenerated: string;
};

export type ApiError = {
  messages: ErrorMessage[];
  type: ErrorType;
};

export type ErrorMessage = {
  code: string;
  message: string;
  invalidField?: string | null;
};

export type ErrorType =
  | "validation"
  | "notFound"
  | "conflict"
  | "unauthorized"
  | "forbidden"
  | "serverError";

export const lessonsApi = {
  getLessons: async (request: GetLessonsRequest) => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<{ lessons: Lesson[] }>>
    >("/lessons", {
      params: request,
    });

    return response.data.result;
  },

  createLesson: async (request: CreateLessonRequest) => {
    const response = await apiClient.post<CreateLessonRequest>(
      "/lessons",
      request
    );

    return response.data;
  },
};
