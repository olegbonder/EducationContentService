import { lessonsApi, lessonsQueryOptions } from "@/entities/lessons/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { toast } from "sonner";

export function useDeleteLesson() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: lessonsApi.deleteLesson,
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
      toast.error("Ошибка при удалении урока");
    },
    onSuccess: () => {
      toast.success("Урок успешно удален");
    },
  });

  return {
    deleteLesson: mutation.mutate,
    isPending: mutation.isPending,
  };
}
