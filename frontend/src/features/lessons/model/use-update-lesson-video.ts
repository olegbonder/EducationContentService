import { lessonsApi, lessonsQueryOptions } from "@/entities/lessons/api";
import { EnvelopeError } from "@/shared/api/errors";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

type UpdateLessonVideoParams = {
  lessonId: string;
  videoId?: string;
};

export function useUpdateLessonVideo() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: async ({ lessonId, videoId }: UpdateLessonVideoParams) => {
      await lessonsApi.updateLessonVideo({ lessonId, videoId });
    },
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
      toast.error("Ошибка при обновлении видео урока");
    },
    onSuccess: () => {
      toast.success("Видео успешно прикреплено к уроку");
    },
  });

  return {
    updateLessonVideo: mutation.mutate,
    isPending: mutation.isPending,
    error: mutation.error instanceof EnvelopeError ? mutation.error : undefined,
    isError: mutation.isError,
  };
}
