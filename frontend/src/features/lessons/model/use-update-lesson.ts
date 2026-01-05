import { lessonsApi, lessonsQueryOptions } from "@/entities/lessons/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export function useUpdateLesson() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: lessonsApi.updateLesson,
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
      toast.error("Ошибка при обновлении урока");
    },
    onSuccess: () => {
      toast.success("Урок успешно обновлен");
    },
  });

  return {
    updateLesson: mutation.mutate,
    isError: mutation.isError,
    error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
    isPending: mutation.isPending,
  };
}
