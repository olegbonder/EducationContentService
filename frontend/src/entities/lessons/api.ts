import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
import { PaginationResponse } from "@/shared/api/types";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";

export type CreateLessonRequest = {
  title: string;
  description: string;
};

export type UpdateLessonRequest = {
  lessonId: string;
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

  deleteLesson: async (lessonId: string) => {
    const response = await apiClient.delete(`/lessons/${lessonId}`);

    return response.data;
  },

  updateLesson: async ({
    lessonId,
    title,
    description,
  }: UpdateLessonRequest) => {
    const response = await apiClient.patch<Envelope<string>>(
      `/lessons/${lessonId}`,
      { title, description }
    );

    return response.data;
  },
};

export const lessonsQueryOptions = {
  baseKey: "lessons",

  getLessonsOptions: ({
    page,
    pageSize,
  }: {
    page: number;
    pageSize: number;
  }) => {
    return queryOptions({
      queryFn: () => lessonsApi.getLessons({ page: page, pageSize: pageSize }),
      queryKey: [lessonsQueryOptions.baseKey, { page }],
    });
  },

  getLessonsInfiniteOptions: ({ pageSize }: { pageSize: number }) => {
    return infiniteQueryOptions({
      queryKey: [lessonsQueryOptions.baseKey],
      queryFn: ({ pageParam }) => {
        return lessonsApi.getLessons({
          page: pageParam,
          pageSize: pageSize,
        });
      },
      initialPageParam: 1,
      getNextPageParam: (response) => {
        if (!response || response.page >= response.totalPages) {
          return undefined;
        }
        return response.page + 1;
      },
      select: (data): PaginationResponse<Lesson> => ({
        items: data.pages.flatMap((page) => page?.items ?? []),
        totalCount: data.pages[0]?.totalCount ?? 0,
        page: data.pages[0]?.page ?? 1,
        pageSize: data.pages[0]?.pageSize ?? pageSize,
        totalPages: data.pages[0]?.totalPages ?? 0,
      }),
    });
  },
};
