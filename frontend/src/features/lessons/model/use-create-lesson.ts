import { lessonsApi, lessonsQueryOptions } from "@/entities/lessons/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export default function useCreateLesson() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: lessonsApi.createLesson,
    onSettled: () => {
      queryClient.invalidateQueries({
        queryKey: [lessonsQueryOptions.baseKey],
      });
    },
    onError: (error) => {
      if (error instanceof EnvelopeError) {
        toast.error(error.message);
        return;
      }
      toast.error("Ошибка при создании урока");
    },
    onSuccess: () => {
      toast.success("Урок успешно создан");
    },
  });

  return {
    createLesson: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error,
    isPending: mutation.isPending,
  };
}
