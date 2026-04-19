import { useState } from "react";
import { AssetType, OwnerType, UploadProgress } from "../types";
import {
  ChunkUploadUrl,
  fileApi,
  PartETag,
  StartMultipartUploadResponse,
} from "../api";
import { isEnvelopeError } from "@/shared/api/errors";
import { is } from "zod/v4/locales";
import { validateFile } from "../lib/validators";
import { error } from "console";

export type Props = {
  ownerId: string;
  ownerType: OwnerType;
  assetType: AssetType;
};

export function useFileUpload({ ownerId, ownerType, assetType }: Props) {
  const [uploadState, setUploadState] = useState<UploadProgress>({
    status: "idle",
    progress: 0,
    uploadedBytes: 0,
    totalBytes: 0,
  });

  const upload = async (file: File): Promise<string | undefined> => {
    const validation = validateFile(file, assetType);
    if (!validation.valid) {
      setUploadState({
        status: "failed",
        progress: 0,
        uploadedBytes: 0,
        totalBytes: file.size,
        fileName: file.name,
        fileSize: file.size,
        error: validation.error,
      });
      return undefined;
    }
    try {
      setUploadState({
        status: "uploading",
        progress: 0,
        uploadedBytes: 0,
        totalBytes: file.size,
        fileName: file.name,
        fileSize: file.size,
      });

      const uploadData: StartMultipartUploadResponse =
        await fileApi.startMultipartUpload({
          fileName: file.name,
          contentType: file.type,
          size: file.size,
          assetType,
          ownerType,
          ownerId,
        });
      const { mediaAssetId, chunkUploadUrls, chunkSize } = uploadData;

      const partETags = await uploadChunks(file, chunkUploadUrls, chunkSize);

      await fileApi.completeMultipartUpload({
        mediaAssetId,
        uploadId: uploadData.uploadId,
        partETags,
      });

      setUploadState((prev) => ({
        ...prev,
        status: "completed",
        progress: 100,
        uploadedBytes: file.size,
        fileName: file.name,
        fileSize: file.size,
      }));

      return mediaAssetId;
    } catch (error) {
      const errorMessage = isEnvelopeError(error)
        ? error.firstMessage
        : error instanceof Error
          ? error.message
          : "Ошибка загрузки файла";

      setUploadState((prev) => ({
        ...prev,
        status: "failed",
        error: errorMessage,
      }));
      return undefined;
    }
  };

  const uploadChunks = async (
    file: File,
    chunks: ChunkUploadUrl[],
    chunkSize: number,
  ): Promise<PartETag[]> => {
    const partETags: PartETag[] = [];

    for (let i = 0; i < chunks.length; i++) {
      const chunkInfo = chunks[i];
      const start = i * chunkSize;
      const end = Math.min(start + chunkSize, file.size);

      const chunk = file.slice(start, end);
      const eTag = await fileApi.uploadChunk(chunkInfo.uploadUrl, chunk);
      partETags.push({
        partNumber: chunkInfo.partNumber,
        eTag,
      });

      const progress = Math.round(((i + 1) / chunks.length) * 100);
      const uploadedBytes = Math.min((i + 1) * chunkSize, file.size);

      setUploadState((prev) => ({
        ...prev,
        progress,
        uploadedBytes,
      }));
    }

    return partETags;
  };

  return {
    upload,
    uploadState,
    isIdle: uploadState.status === "idle",
    isUploading: uploadState.status === "uploading",
    isCompleted: uploadState.status === "completed",
    isFailed: uploadState.status === "failed",
    error: uploadState.error,
  };
}
