import { Button } from "@/shared/components/ui/button";
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
} from "@/shared/components/ui/card";
import { Pencil, Trash2 } from "lucide-react";
import Link from "next/link";
import { useDeleteLesson } from "./model/use-delete-lesson";
import "@vidstack/react/player/styles/default/theme.css";
import "@vidstack/react/player/styles/default/layouts/video.css";
import { MediaPlayer, MediaProvider } from "@vidstack/react";
import {
  defaultLayoutIcons,
  DefaultVideoLayout,
} from "@vidstack/react/player/layouts/default";

type Props = {
  lesson: Lesson;
  onEdit: (lesson: Lesson) => void;
  onOpenVideoUpload: (lesson: Lesson) => void;
};

export function LessonCard({ lesson, onEdit, onOpenVideoUpload }: Props) {
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

  const handleOpenVideoUpload = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();

    onOpenVideoUpload(lesson);
  };

  return (
    <Card className="h-full hover:shadow-lg transition-shadow cursor-pointer">
      <CardHeader className="p-0">
        <div className="relative aspect-video bg-muted flex items-center justify-center rounded-t-lg overflow-hidden">
          {/*<video
              src={lesson.video.url}
              className="object-contain w-full h-full"
              controls
              preload="metadata"
            />*/}
          {lesson.video?.url ? (
            <MediaPlayer
              title={lesson.title}
              src={lesson.video.url}
            >
              <MediaProvider />
              <DefaultVideoLayout                
                icons={defaultLayoutIcons}
              />
            </MediaPlayer>
          ) : (
            <Link href={`/lessons/${lesson.id}`}>
              <div className="text-muted-foreground text-sm">
                Видео отсутствует
              </div>
            </Link>
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
        {(!lesson.isDeleted && (!lesson.video ||
          lesson.video.status === "failed" ||
          lesson.video.status === "deleted" ||
          !lesson.video.status)) && (
          <Button onClick={handleOpenVideoUpload} className="mt-4 w-full">
            Загрузить видео
          </Button>
        )}
        {lesson.video?.status === "uploading" ||
          (lesson.video?.status === "processing" && (
            <div className="mt-4 text-sm text-muted-foreground text-center">
              Загрузка видео...
            </div>
          ))}
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
  );
}
