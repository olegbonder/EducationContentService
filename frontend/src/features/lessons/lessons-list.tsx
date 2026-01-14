"use client";

import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { Spinner } from "@/shared/components/ui/spinner";
import { AlertCircle, Search } from "lucide-react";
import { useState } from "react";
import { CreateLessonDialog } from "./create-lesson-dialog";
import { LessonCard } from "./lesson-card";
import { PAGE_SIZE, useLessonsList } from "./model/use-lessons-list";
import { UpdateLessonDialog } from "./update-lesson-dialog";
import { useDebounce } from "use-debounce";
import { Input } from "@/shared/components/ui/input";
import {
  Select,
  SelectContent,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { LessonFilters } from "./lessons-filters";

export type LessonsFilter = {
  search?: string;
  isDeleted?: boolean;
  pageSize: number;
};

export function LessonsList() {
  const [search, setSearch] = useState("");
  const [isDeleted, setIsDeleted] = useState<boolean | undefined>(false);

  const [debouncedSearch] = useDebounce(search, 300);

  const [createOpen, setCreateOpen] = useState(false);
  const [updateOpen, setUpdateOpen] = useState(false);

  const [selectedLesson, setSelectedLesson] = useState<Lesson | undefined>(
    undefined
  );

  const { lessons, isPending, error, isError, isFetchingNextPage, cursorRef } =
    useLessonsList({ search: debouncedSearch, isDeleted, pageSize: PAGE_SIZE });

  if (isError) {
    return (
      <div className="container mx-auto py-8 px-4 flex justify-center">
        <Card className="max-w-md border-destructive/50 bg-destructive/5">
          <CardHeader className="text-center">
            <div className="mx-auto mb-2 flex h-12 w-12 items-center justify-center rounded-full bg-destructive/10">
              <AlertCircle className="h-6 w-6 text-destructive" />
            </div>
            <CardTitle className="text-destructive">
              Не удалось загрузить уроки
            </CardTitle>
            <CardDescription>
              {error?.message ?? "Произошла неизвестная ошибка"}
            </CardDescription>
          </CardHeader>
          <CardFooter className="justify-center"></CardFooter>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="mb-8">
        <h1 className="text-3xl font-bold mb-2">Уроки</h1>
        <LessonFilters />
        <Button onClick={() => setCreateOpen(true)}>Создать урок</Button>
        <p className="text-muted-foreground">
          Все доступные уроки курса по .NET разработкеs
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {isPending ? (
          <Spinner />
        ) : (
          lessons?.map((lesson) => (
            <LessonCard
              key={lesson.id}
              lesson={lesson}
              onEdit={() => {
                setSelectedLesson(lesson);
                setUpdateOpen(true);
              }}
            />
          ))
        )}
      </div>

      <CreateLessonDialog open={createOpen} onOpenChange={setCreateOpen} />

      {selectedLesson && (
        <UpdateLessonDialog
          key={selectedLesson.id}
          lesson={selectedLesson}
          open={selectedLesson !== undefined && updateOpen}
          onOpenChange={setUpdateOpen}
        />
      )}

      <div ref={cursorRef} className="flex justify-center py-4">
        {isFetchingNextPage && <Spinner />}
      </div>
    </div>
  );
}
