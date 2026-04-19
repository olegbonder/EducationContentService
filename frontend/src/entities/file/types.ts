export type AssetType = "video" | "preview" | "avatar" | "screenshot";

export type OwnerType = "lesson" | "issue" | "module" | "profile";

export type UploadStatus =
  | "idle"
  | "uploading"
  | "processing"
  | "completed"
  | "failed";

export type FileStatus =
  | "uploading"
  | "uploaded"
  | "processing"
  | "ready"
  | "failed";

export type UploadProgress = {
  status: UploadStatus;
  progress: number;
  uploadedBytes: number;
  totalBytes: number;
  fileName?: string;
  fileSize?: number;
  error?: string;
  mediaAssetId?: string;
};

export type FileInfo = {
  id: string;
  url: string | null;
  status: FileStatus;
};

export type FileValidatorConfig = {
  maxFileSize: number; // Максимальный размер файла в байтах
  allowedTypes: string[]; // Разрешенные MIME-типы файлов
  allowedExtensions: string[]; // Разрешенные расширения файлов (без точки)
  label: string;
  description: string;
};

export type FileValidatorResult =
  | { valid: true }
  | { valid: false; error: string };
