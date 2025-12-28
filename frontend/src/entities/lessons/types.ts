type Lesson = {
  id: string;
  title: string;
  description: string;
  video?: MediaDto;
  createdAt: Date;
  updatedAt: Date;
};

type MediaDto = {
  id: string;
  url: string;
  status: MediaStatus;
};

type MediaStatus = "uploading" | "uploaded" | "ready" | "failed" | "deleted";

/*enum MediaStatus {
  UPLOADING = "UPLOADING",
  UPLOADED = "UPLOADED",
  READY = "READY",
  FAILED = "FAILED",
  DELETED = "DELETED",
}*/
