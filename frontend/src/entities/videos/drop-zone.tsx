import { cn } from "@/shared/lib/utils";
import { Upload } from "lucide-react";

export function DropZone({
  isDragging,
  onDragOver,
  onDragLeave,
  onDrop,
  onClick,
}: {
  isDragging: boolean;
  onDragOver: (e: React.DragEvent) => void;
  onDragLeave: (e: React.DragEvent) => void;
  onDrop: (e: React.DragEvent) => void;
  onClick?: () => void;
}) {
  return (
    <div
      className={cn(
        "border-2 border-dashed rounded-lg p-8 text-center transition-colors cursor-pointer",
        isDragging
          ? "border-primary bg-primary/50"
          : "border-muted-foreground/25 hover:border-primary/50",
      )}
      onDragOver={onDragOver}
      onDragLeave={onDragLeave}
      onDrop={onDrop}
      onClick={onClick}
    >
      <Upload className="w-10 h-10 mx-auto mb-3 text-muted-foreground" />
      <p className="text-sm font-medium">Перетащите видео</p>
      <p className="text-xs text-muted-foreground mt-1">
        или нажмите, чтобы загрузить
      </p>
      <p className="text-xs text-muted-foreground mt-3">
        MP4, WebM, MDV до 5 ГБ
      </p>
    </div>
  );
}
