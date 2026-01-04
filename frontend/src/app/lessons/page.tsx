"use client";

import { Spinner } from "@/shared/components/ui/spinner";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
} from "@/shared/components/ui/card";
import { Play } from "lucide-react";
import { useState } from "react";
import { Button } from "@/shared/components/ui/button";
import { CreateLessonDialog } from "@/features/lessons/create-lesson-dialog";
import LessonsPagination from "@/features/lessons/lessons-pagination";
import { useLessonsList } from "@/features/lessons/model/use-lessons-list";

export default function LessonsPage() {
  const [open, setOpen] = useState(false);
  const [page, setPage] = useState(1);

  const { lessons, isPending, error, totalPages } = useLessonsList({ page });

  if (isPending) {
    return <Spinner />;
  }

  if (error) {
    return <div className="text-red-500">Ошибка: {error.message}</div>;
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="mb-8">
        <h1 className="text-3xl font-bold mb-2">Уроки</h1>
        <Button onClick={() => setOpen(true)}>Создать урок</Button>
        <p className="text-muted-foreground">
          Все доступные уроки по .NET разработке
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {lessons?.map((lesson) => (
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
      {totalPages && (
        <LessonsPagination
          currentPage={page}
          totalPages={totalPages}
          onPageChange={setPage}
        />
      )}

      <CreateLessonDialog open={open} onOpenChange={setOpen} />
    </div>
  );
}
