import {
  PartETag,
  StartMultipartUploadRequest,
  videoApi,
} from "@/entities/videos/api";
import { useState } from "react";

export type UploadProgress = {
  status: "idle" | "uploading" | "completed" | "error";
  progress: number;
  error?: string;
};

export function useVideoUpload() {
  const [uploadState, setUploadState] = useState<UploadProgress>({
    status: "idle",
    progress: 0,
  });
  const uploadVideo = async (file: File, lessonId: string) => {
    try {
      setUploadState({ status: "uploading", progress: 0 });
      // Step 1: Start multipart upload
      const startUploadRequest: StartMultipartUploadRequest = {
        fileName: file.name,
        contentType: file.type,
        size: file.size,
        assetType: "video",
        ownerType: "lesson",
        ownerId: lessonId,
      };
      const { mediaAssetId, uploadId, chunkUploadUrls, chunkSize } =
        await videoApi.startMultipartUpload(startUploadRequest);

      // Step 2: Upload chunks
      const partETags: PartETag[] = [];

      const totalChunks = chunkUploadUrls.length;
      for (let i = 0; i < totalChunks; i++) {
        const chunkInfo = chunkUploadUrls[i];
        const start = i * chunkSize;
        const end = Math.min(start + chunkSize, file.size);

        const chunk = file.slice(start, end);

        const eTag = await videoApi.uploadChunk(chunkInfo.uploadUrl, chunk);
        partETags.push({
          partNumber: chunkInfo.partNumber,
          eTag,
        });

        const progress = Math.round(((i + 1) / totalChunks) * 100);

        setUploadState((prev) => ({ ...prev, progress }));
      }

      // Step 3: Complete multipart upload
      await videoApi.completeMultipartUpload({
        mediaAssetId,
        uploadId,
        partETags: partETags,
      });

      setUploadState((prev) => ({
        ...prev,
        status: "completed",
        progress: 100,
      }));
    } catch (error) {
      console.error("Error uploading video:", error);
      setUploadState({
        status: "error",
        progress: 0,
        error: error instanceof Error ? error.message : "Unknown error",
      });
    }
  };

  return { uploadVideo, uploadState };
}
