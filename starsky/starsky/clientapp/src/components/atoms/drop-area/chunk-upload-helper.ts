import { IExifStatus } from "../../../interfaces/IExifStatus";
import { IFileIndexItem, newIFileIndexItem } from "../../../interfaces/IFileIndexItem";
import FetchPost from "../../../shared/fetch/fetch-post";
import { GetCookie } from "../../../shared/cookie/get-cookie";

// 95MB chunk size to stay well under Cloudflare's 100MB limit
const CHUNK_SIZE = 95 * 1024 * 1024;

interface ChunkUploadInitResponse {
  uploadId: string;
  expiresAt: string;
}

const CastFileIndexItem = (element: {
  fileHash: string;
  filePath: string;
  fileName: string;
  status: IExifStatus;
}): IFileIndexItem => {
  const uploadFileObject = newIFileIndexItem();
  uploadFileObject.fileHash = element.fileHash;
  uploadFileObject.filePath = element.filePath;
  uploadFileObject.isDirectory = false;
  uploadFileObject.fileName = element.fileName;
  uploadFileObject.lastEdited = new Date().toISOString();
  uploadFileObject.status = element.status;
  return uploadFileObject;
};

export class ChunkUploadHelper {
  private readonly endpoint: string;
  private readonly folderPath: string | undefined;
  private setNotificationStatus: React.Dispatch<React.SetStateAction<string>>;

  constructor(
    endpoint: string,
    folderPath: string | undefined,
    setNotificationStatus: React.Dispatch<React.SetStateAction<string>>
  ) {
    this.endpoint = endpoint;
    this.folderPath = folderPath;
    this.setNotificationStatus = setNotificationStatus;
  }

  async uploadFileInChunks(
    file: File,
    fileIndex: number,
    totalFiles: number
  ): Promise<IFileIndexItem[]> {
    try {
      // Step 1: Initialize chunk session
      const initResponse = await this.initChunkSession(file);
      if (!initResponse) {
        return this.createErrorResult(file);
      }

      const uploadId = initResponse.uploadId;
      const totalChunks = Math.ceil(file.size / CHUNK_SIZE);

      // Step 2: Upload chunks
      const success = await this.uploadChunks(file, uploadId, totalChunks, fileIndex, totalFiles);
      if (!success) {
        await this.deleteChunkSession(uploadId);
        return this.createErrorResult(file);
      }

      // Step 3: Complete upload
      const results = await this.completeChunkUpload(uploadId);
      if (!results) {
        await this.deleteChunkSession(uploadId);
        return this.createErrorResult(file);
      }

      return results;
    } catch (error) {
      console.error("Chunk upload failed:", error);
      return this.createErrorResult(file);
    }
  }

  private async initChunkSession(file: File): Promise<ChunkUploadInitResponse | null> {
    const totalChunks = Math.ceil(file.size / CHUNK_SIZE);
    const params = new URLSearchParams({
      fileName: file.name,
      totalChunks: totalChunks.toString(),
      totalSize: file.size.toString()
    });

    const response = await FetchPost(
      `${this.endpoint}/chunk/init?${params}`,
      "",
      "post",
      { to: this.folderPath }
    );

    if (response.statusCode === 200 && response.data) {
      return response.data as ChunkUploadInitResponse;
    }

    console.error("Failed to initialize chunk upload:", response);
    return null;
  }

  private async uploadChunks(
    file: File,
    uploadId: string,
    totalChunks: number,
    fileIndex: number,
    totalFiles: number
  ): Promise<boolean> {
    for (let chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++) {
      const start = chunkIndex * CHUNK_SIZE;
      const end = Math.min(start + CHUNK_SIZE, file.size);
      const chunk = file.slice(start, end);

      this.setNotificationStatus(
        `Uploading chunk ${chunkIndex + 1}/${totalChunks} of file ${fileIndex + 1}/${totalFiles} (${file.name})`
      );

      const success = await this.uploadSingleChunk(uploadId, chunkIndex, chunk);
      if (!success) {
        return false;
      }
    }

    return true;
  }

  private async uploadSingleChunk(
    uploadId: string,
    chunkIndex: number,
    chunk: Blob
  ): Promise<boolean> {
    try {
      const url = `${this.endpoint}/chunk/${uploadId}?chunkIndex=${chunkIndex}`;

      const settings: RequestInit = {
        method: "PUT",
        body: chunk,
        credentials: "include" as RequestCredentials,
        headers: {
          "X-XSRF-TOKEN": GetCookie("X-XSRF-TOKEN"),
          "Content-Type": "application/octet-stream"
        }
      };

      const response = await fetch(url, settings);
      if (!response.ok) {
        console.error(`Chunk upload failed for index ${chunkIndex}:`, response);
        return false;
      }

      return true;
    } catch (error) {
      console.error(`Error uploading chunk ${chunkIndex}:`, error);
      return false;
    }
  }

  private async completeChunkUpload(uploadId: string): Promise<IFileIndexItem[] | null> {
    this.setNotificationStatus("Finalizing upload...");

    const response = await FetchPost(
      `${this.endpoint}/chunk/${uploadId}/complete`,
      "",
      "post",
      { to: this.folderPath }
    );

    if (!response.data) {
      console.error("Failed to complete chunk upload:", response);
      return null;
    }

    const dataArr = response.data as {
      fileIndexItem?: IFileIndexItem;
      filePath?: string;
      fileHash?: string;
      status: IExifStatus;
    }[];

    const results: IFileIndexItem[] = [];
    for (const dataItem of dataArr) {
      if (!dataItem) {
        results.push(this.createErrorResult({ name: "unknown" } as File)[0]);
      } else if (dataItem.fileIndexItem && dataItem.status !== IExifStatus.Ok) {
        results.push(CastFileIndexItem(dataItem.fileIndexItem));
      } else if (!dataItem.fileIndexItem && dataItem.status !== IExifStatus.Ok) {
        results.push({
          filePath: dataItem.filePath,
          fileName: "unknown",
          isDirectory: false,
          fileHash: dataItem.fileHash,
          status: dataItem.status
        } as IFileIndexItem);
      } else if (dataItem.fileIndexItem) {
        dataItem.fileIndexItem.lastEdited = new Date().toISOString();
        results.push(dataItem.fileIndexItem);
      }
    }

    return results;
  }

  private async deleteChunkSession(uploadId: string): Promise<void> {
    try {
      await fetch(`${this.endpoint}/chunk/${uploadId}`, {
        method: "DELETE",
        credentials: "include" as RequestCredentials,
        headers: {
          "X-XSRF-TOKEN": GetCookie("X-XSRF-TOKEN")
        }
      });
    } catch (error) {
      console.error("Error deleting chunk session:", error);
    }
  }

  private createErrorResult(file: File): IFileIndexItem[] {
    return [
      {
        filePath: file.name,
        fileName: file.name,
        status: IExifStatus.ServerError
      } as IFileIndexItem
    ];
  }
}

export function getChunkThreshold(): number {
  // Use 95MB as threshold for chunk upload (stays under 100MB Cloudflare limit)
  return 95 * 1024 * 1024;
}



