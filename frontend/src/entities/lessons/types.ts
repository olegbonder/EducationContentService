type Lesson = {
  id: string;
  title: string;
  description: string;
  video?: MediaDto;
  createdAt: Date;
  updatedAt: Date;
  isDeleted: boolean;
};

type MediaDto = {
  id: string;
  url: string;
  status: MediaStatus;
};

type MediaStatus =
  | "uploading"
  | "uploaded"
  | "ready"
  | "processing"
  | "failed"
  | "deleted";
