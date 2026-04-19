import { fsApiClient } from "@/shared/api/axios-instance";
import { AssetType, OwnerType } from "./types";
import { Envelope } from "@/shared/api/evelope";
import axios from "axios";

export type StartMultipartUploadRequest = {
  fileName: string;
  contentType: string;
  size: number;
  assetType: AssetType;
  ownerType: OwnerType;
  ownerId: string;
};

export type ChunkUploadUrl = {
  partNumber: number;
  uploadUrl: string;
};

export type StartMultipartUploadResponse = {
  mediaAssetId: string;
  uploadId: string;
  chunkUploadUrls: ChunkUploadUrl[];
  chunkSize: number;
};

export type PartETag = {
  partNumber: number;
  eTag: string;
};

export type CompleteMultipartUploadRequest = {
  mediaAssetId: string;
  uploadId: string;
  partETags: PartETag[];
};

export const fileApi = {
  startMultipartUpload: async (
    request: StartMultipartUploadRequest,
  ): Promise<StartMultipartUploadResponse> => {
    const response = await fsApiClient.post<
      Envelope<StartMultipartUploadResponse>
    >("/files/multipart-upload", request);
    return response.data.result!;
  },
  uploadChunk: async (
    uploadUrl: string,
    chunk: Blob,
    signal?: AbortSignal,
  ): Promise<string> => {
    const response = await axios.put(uploadUrl, chunk, {
      headers: {
        "Content-Type": chunk.type,
      },
      signal,
    });

    const eTag = response.headers.etag?.replace(/"/g, "") || ""; // Удаляем кавычки из eTag
    return eTag;
  },
  completeMultipartUpload: async (
    request: CompleteMultipartUploadRequest,
  ): Promise<void> => {
    const response = await fsApiClient.post<Envelope<void>>(
      "/files/complete-upload",
      request,
    );
    return response.data.result!;
  },
};
