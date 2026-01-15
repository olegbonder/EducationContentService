import { lessonsQueryOptions } from "@/entities/lessons/api";
import { useInfiniteQuery } from "@tanstack/react-query";
import { RefCallback, useCallback } from "react";
import { LessonsFilterState } from "./lessons-filters-store";

export const PAGE_SIZE = 3;

export function useLessonsList(filter: LessonsFilterState) {
  const {
    data,
    isPending,
    error,
    isError,
    refetch,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
  } = useInfiniteQuery({
    ...lessonsQueryOptions.getLessonsInfiniteOptions(filter),
  });

  const cursorRef: RefCallback<HTMLDivElement> = useCallback(
    (el) => {
      const observer = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting) {
            fetchNextPage();
          }
        },
        {
          threshold: 0.5,
        }
      );

      if (el) {
        observer.observe(el);

        return () => observer.disconnect();
      }
    },
    [fetchNextPage, hasNextPage, isFetchingNextPage]
  );

  return {
    lessons: data?.items,
    totalPages: data?.totalPages,
    totalCount: data?.totalCount,
    currentPage: data?.page,
    isPending,
    error,
    isError,
    refetch,
    isFetchingNextPage,
    cursorRef,
  };
}
