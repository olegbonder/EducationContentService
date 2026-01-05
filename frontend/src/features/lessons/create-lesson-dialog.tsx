import { Button } from "@/shared/components/ui/button";
import {
  DialogHeader,
  DialogFooter,
  Dialog,
  DialogContent,
  DialogTitle,
  DialogDescription,
} from "@/shared/components/ui/dialog";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@radix-ui/react-label";
import { useState } from "react";
import useCreateLesson from "./model/use-create-lesson";
import { z } from "zod";
import { useForm, SubmitHandler } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

const createLessonSchema = z.object({
  title: z
    .string()
    .min(1, "Название урока обязательно")
    .min(3, "Название должно содержать минимум 3 символа")
    .max(200, "Название не должно превышать 200 символов"),
  description: z
    .string()
    .min(10, "Описание должно содержать минимум 10 символов")
    .max(1000, "Описание не должно превышать 1000 символов")
    .optional(),
});

type CreateLessonData = z.infer<typeof createLessonSchema>;

type CreateLessonDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function CreateLessonDialog({
  open,
  onOpenChange,
}: CreateLessonDialogProps) {
  const initialData: CreateLessonData = {
    title: "",
    description: "",
  };
  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
    reset,
  } = useForm<CreateLessonData>({
    defaultValues: initialData,
    resolver: zodResolver(createLessonSchema),
  });

  const { createLesson, isPending, error } = useCreateLesson();

  const onSubmit = async (data: CreateLessonData) => {
    createLesson(data, {
      onSuccess: () => {
        onOpenChange(false);
        reset(initialData);
      },
    });
  };
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Создать урок</DialogTitle>
          <DialogDescription>
            Заполните форму ниже, чтобы создать новый урок.
          </DialogDescription>
        </DialogHeader>
        <form className="grid gap-4 py-4" onSubmit={handleSubmit(onSubmit)}>
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
            {errors.title && (
              <p className="text-sm text-destructive">{errors.title.message}</p>
            )}
          </div>
          <div className="grid gap-2">
            <Label htmlFor="description">Описание</Label>
            <Input
              id="description"
              placeholder="Введите описание урока"
              className={`w-full ${
                errors.description
                  ? "border-destructive focus-visible:ring-destructive"
                  : ""
              }`}
              {...register("description")}
            />
            {errors.description && (
              <p className="text-sm text-destructive">
                {errors.description.message}
              </p>
            )}
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Отмена
            </Button>
            <Button type="submit" disabled={isPending || !isValid}>
              Создать урок
            </Button>
            {error && <div className="text-red-500">{error.message}</div>}
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
