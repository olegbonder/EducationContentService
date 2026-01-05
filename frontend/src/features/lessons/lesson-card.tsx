import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
} from "@/shared/components/ui/card";
import { Pencil, Play, Trash2 } from "lucide-react";
import Link from "next/link";
import { useDeleteLesson } from "./model/use-delete-lesson";

type Props = {
  lesson: Lesson;
  onEdit: (lesson: Lesson) => void;
};

export function LessonCard({ lesson, onEdit }: Props) {
  const { deleteLesson, isPending } = useDeleteLesson();

  const handleDelete = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();

    deleteLesson(lesson.id);
  };

  const handleEdit = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();

    onEdit(lesson);
  };

  return (
    <Link href={`/lessons/${lesson.id}`}>
      <Card className="h-full hover:shadow-lg transition-shadow cursor-pointer">
        <CardHeader className="p-0">
          <div className="relative aspect-video bg-muted flex items-center justify-center rounded-t-lg overflow-hidden">
            {lesson.video ? (
              <Play className="w-12 h-12 text-muted-foreground" />
            ) : (
              <div className="text-muted-foreground text-sm">
                Видео отсутствует
              </div>
            )}
          </div>
        </CardHeader>
        <CardContent className="pt-4">
          <h3 className="font-semibold text-lg mb-2 line-clamp-2">
            {lesson.title}
          </h3>
          <p className="text-sm text-muted-foreground line-clamp-3">
            {lesson.description}
          </p>
        </CardContent>
        <CardFooter className="flex justify-between items-center text-xs text-muted-foreground">
          <span>Обновлено {lesson.updatedAt.toLocaleString()}</span>
          <div className="flex gap-1">
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 text-muted-foreground hover:text-primary hover:bg-primary/10 transition-colors"
              onClick={handleEdit}
            >
              <Pencil className="h-4 w-4" />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              className="h-8 w-8 text-destructive hover:text-white! hover:bg-red-500! transition-colors"
              onClick={handleDelete}
              disabled={isPending}
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        </CardFooter>
      </Card>
    </Link>
  );
}
