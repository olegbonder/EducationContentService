import { lessonsApi, lessonsQueryOptions } from "@/entities/lessons/api";
import { EnvelopeError } from "@/shared/api/errors";
import { queryClient } from "@/shared/api/query-client";
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
import { useMutation, useQueryClient } from "@tanstack/react-query";
import React, { useState } from "react";
import { toast } from "sonner";
import useCreateLesson from "./model/use-create-lesson";

type CreateLessonDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

type CreateFormData = {
  title: string;
  description: string;
};

export function CreateLessonDialog({
  open,
  onOpenChange,
}: CreateLessonDialogProps) {
  const initialData: CreateFormData = {
    title: "",
    description: "",
  };
  const [createFormData, setCreateFormData] =
    useState<CreateFormData>(initialData);

  const { createLesson, isPending, error } = useCreateLesson();

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    console.log("create lesson");
    createLesson(createFormData, {
      onSuccess: () => {
        setCreateFormData(initialData);
        onOpenChange(false);
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
        <form className="grid gap-4 py-4" onSubmit={(e) => handleSubmit(e)}>
          <div className="grid gap-2">
            <Label htmlFor="title">Название урока</Label>
            <Input
              id="title"
              placeholder="Введите название урока"
              className="w-full"
              value={createFormData.title}
              onChange={(e) =>
                setCreateFormData({
                  ...createFormData,
                  title: e.target.value,
                })
              }
            />
          </div>
          <div className="grid gap-2">
            <Label htmlFor="description">Описание</Label>
            <Input
              id="description"
              placeholder="Введите описание урока"
              className="w-full"
              value={createFormData.description}
              onChange={(e) =>
                setCreateFormData({
                  ...createFormData,
                  description: e.target.value,
                })
              }
            />
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Отмена
            </Button>
            <Button type="submit" disabled={isPending}>
              Создать урок
            </Button>
            {error && <div className="text-red-500">{error.message}</div>}
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
