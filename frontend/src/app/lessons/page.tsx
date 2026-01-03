"use client";

import { Spinner } from "@/shared/components/ui/spinner";
import { lessonsApi } from "@/entities/lessons/api";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
} from "@/shared/components/ui/card";
import { Play } from "lucide-react";
import { useState } from "react";
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/shared/components/ui/pagination";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Button } from "@/shared/components/ui/button";
import { EnvelopeError } from "@/shared/api/errors";
import { toast } from "sonner";

const PAGE_SIZE = 2;

export default function LessonsPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const {
    data,
    isPending: getIsPending,
    error,
    isError,
    isFetching,
  } = useQuery({
    queryFn: () => lessonsApi.getLessons({ page: page, pageSize: PAGE_SIZE }),
    queryKey: ["lessons", { page }],
  });

  const {
    mutate: createLesson,
    isPending: createIsPending,
    error: createLessonError,
  } = useMutation({
    mutationFn: () =>
      lessonsApi.createLesson({
        title: "Новый урок 2",
        description: "Описание нового урока",
      }),
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ["lessons"] });
    },
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }
      toast.error("Ошибка при создании урока");
    },
  });

  if (getIsPending) {
    return <Spinner />;
  }

  if (error) {
    return <div className="text-red-500">Ошибка: {error.message}</div>;
  }

  if (createIsPending) {
    return <Spinner />;
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="mb-8">
        <h1 className="text-3xl font-bold mb-2">Уроки</h1>
        <Button onClick={() => createLesson()} disabled={getIsPending}>
          Создать урок
        </Button>
        {createLessonError && (
          <div className="text-red-500 mt-2">
            Ошибка: {createLessonError.message}
          </div>
        )}
        <p className="text-muted-foreground">
          Все доступные уроки по .NET разработке
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {data?.items?.map((lesson) => (
          <Card
            key={lesson.id}
            className="h-full hover:shadow-lg transition-shadow"
          >
            <CardHeader className="p-0">
              <div className="relative aspect-video bg-muted flex items-center justify-center rounded-t-lg">
                {lesson.video ? (
                  <Play className="w-12 h-12 text-muted-foreground" />
                ) : (
                  <div className="text-muted-foreground text-sm">
                    Видео отсутствует
                  </div>
                )}
              </div>
            </CardHeader>
            <CardContent className="p-4 flex-1">
              <h3 className="text-lg font-semibold mb-2">{lesson.title}</h3>
              <p className="text-sm text-muted-foreground">
                {lesson.description}
              </p>
            </CardContent>
            <CardFooter className="text-xs text-muted-foreground">
              Обновлено: {new Date(lesson.updatedAt).toLocaleDateString()}
            </CardFooter>
          </Card>
        ))}
      </div>
      {data && data.totalPages > 1 && (
        <div className="mt-8 flex justify-center">
          <Pagination>
            <PaginationContent>
              <PaginationItem>
                <PaginationPrevious
                  onClick={() => setPage((prev) => Math.max(1, prev - 1))}
                  className={
                    page === 1
                      ? "pointer-events-none opacity-50"
                      : "cursor-pointer"
                  }
                />
              </PaginationItem>
              {Array.from(
                { length: data.totalPages },
                (_, index) => index + 1
              ).map((pageNumber) => (
                <PaginationItem key={pageNumber}>
                  <PaginationLink
                    className="cursor-pointer"
                    onClick={() => setPage(pageNumber)}
                    isActive={pageNumber === page}
                  >
                    {pageNumber}
                  </PaginationLink>
                </PaginationItem>
              ))}

              <PaginationItem>
                <PaginationNext
                  onClick={() =>
                    setPage((prev) => Math.min(data.totalPages, prev + 1))
                  }
                  className={
                    page === data.totalPages
                      ? "pointer-events-none opacity-50"
                      : "cursor-pointer"
                  }
                />
              </PaginationItem>
            </PaginationContent>
          </Pagination>
        </div>
      )}
    </div>
  );
}
