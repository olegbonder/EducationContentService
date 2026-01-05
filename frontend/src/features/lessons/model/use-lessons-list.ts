import { lessonsQueryOptions } from "@/entities/lessons/api";
import { useQuery } from "@tanstack/react-query";

const PAGE_SIZE = 3;

export function useLessonsList({ page }: { page: number }) {
  const { data, isPending, error, isError } = useQuery(
    lessonsQueryOptions.getLessonsOptions({ page, pageSize: PAGE_SIZE })
  );

  return {
    lessons: data?.items,
    totalPages: data?.totalPages,
    totalCount: data?.totalCount,
    currentPage: data?.page,
    isPending,
    error,
    isError,
  };
}
