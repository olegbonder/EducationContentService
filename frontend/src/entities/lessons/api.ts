import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
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

export const lessonsApi = {
  getLessons: async (request: GetLessonsRequest) => {
    const response = await apiClient.get<Envelope<PaginationResponse<Lesson>>>(
      "/lessons",
      {
        params: request,
      }
    );

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
