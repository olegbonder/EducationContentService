import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { Textarea } from "@/shared/components/ui/textarea";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { useUpdateLesson } from "./model/use-update-lesson";

const updateLessonSchema = z.object({
  title: z
    .string()
    .min(1, "Название урока обязательно")
    .min(3, "Название должно содержать минимум 3 символа")
    .max(200, "Название не должно превышать 200 символов"),
  description: z
    .string()
    .min(1, "Описание урока обязательно")
    .min(10, "Описание должно содержать минимум 10 символов")
    .max(1000, "Описание не должно превышать 1000 символов"),
});

type UpdateLessonData = z.infer<typeof updateLessonSchema>;

type Props = {
  lesson: Lesson;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function UpdateLessonDialog({ lesson, open, onOpenChange }: Props) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<UpdateLessonData>({
    defaultValues: {
      title: lesson.title,
      description: lesson.description,
    },
    resolver: zodResolver(updateLessonSchema),
  });

  const { updateLesson, isPending, error, isError } = useUpdateLesson();

  const onSubmit = (data: UpdateLessonData) => {
    updateLesson(
      { lessonId: lesson.id, ...data },
      {
        onSuccess: () => {
          onOpenChange(false);
        },
      }
    );
  };

  const getErrorMessage = (): string => {
    if (isError) {
      return error ? error.message : "Неизвестная ошибка";
    }

    return "";
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Редактирование урока</DialogTitle>
          <DialogDescription>Измените данные урока</DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="title">Название урока</Label>
              <Input
                id="title"
                placeholder="Введите название урока"
                className={`w-full ${
                  errors.title
                    ? "border-destructive focus-visible:ring-destructive"
                    : ""
                }`}
                {...register("title")}
              />
              {/* <FormError message={errors.title?.message} /> */}
            </div>

            <div className="grid gap-2">
              <Label htmlFor="description">Описание</Label>
              <Textarea
                id="description"
                placeholder="Введите описание урока"
                className={`w-full min-h-[100px] ${
                  errors.description
                    ? "border-destructive focus-visible:ring-destructive"
                    : ""
                }`}
                {...register("description")}
              />
              {/* <FormError message={errors.description?.message} /> */}
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
              >
                Отмена
              </Button>
              <Button type="submit" disabled={isPending}>
                Сохранить
              </Button>
              {error && <div className="text-red-500">{getErrorMessage()}</div>}
            </DialogFooter>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
