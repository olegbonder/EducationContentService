import { DropZone } from "@/entities/videos/drop-zone";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { useCallback, useRef, useState } from "react";
import { useVideoUpload } from "./model/use-video-upload";
import { Progress } from "@/shared/components/ui/progress";

type Props = {
  lesson: Lesson;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};
export function LessonVideoUploadDialog({ lesson, open, onOpenChange }: Props) {
  const { uploadVideo, uploadState } = useVideoUpload();

  const [isDragging, setIsDragging] = useState(false);

  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  }, []);

  const handleFileSelected = useCallback(
    async (file: File) => {
      if (file.type.startsWith("video/")) {
        await uploadVideo(file, lesson.id);
      }
    },
    [uploadVideo, lesson.id],
  );

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      setIsDragging(false);
      const file = e.dataTransfer.files[0];
      if (file) {
        handleFileSelected(file);
      }
    },
    [handleFileSelected],
  );

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    handleFileSelected(file);
  };

  const openFilePicker = () => fileInputRef.current?.click();

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Загрузка видео урока {lesson.title}</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          {uploadState.status === "idle" && (
            <DropZone
              isDragging={isDragging}
              onDragOver={handleDragOver}
              onDragLeave={handleDragLeave}
              onDrop={handleDrop}
              onClick={openFilePicker}
            />
          )}

          {uploadState.status === "uploading" && (
            <div className="space-y-1.5">
              <Progress value={uploadState.progress} className="h-2" />
              <div className="flex justify-between text-xs text-muted-foreground">
                <span>Загрузка...</span>
                <span>{uploadState.progress}%</span>
              </div>
            </div>
          )}
          <input
            ref={fileInputRef}
            type="file"
            accept="video/*"
            className="hidden"
            onChange={handleFileSelect}
            aria-label="Выбор видео файла"
          />
        </div>
      </DialogContent>
    </Dialog>
  );
}
