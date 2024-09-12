import { Dispatch } from "react";
import { DetailViewAction } from "../../../../contexts/detailview-context";
import { IDetailView } from "../../../../interfaces/IDetailView";
import { RequestNewFileHash } from "./request-new-filehash";

// Delay function to wrap setTimeout in a Promise for better async handling
function delay(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

// Function to request a new file hash with retry support
async function requestFileHashWithRetry(
  state: IDetailView,
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>,
  dispatch: Dispatch<DetailViewAction>,
  retries: number = 0,
  maxRetries: number = 3
): Promise<void> {
  const result = await RequestNewFileHash(state, setIsLoading, dispatch);

  if (result === false && retries < maxRetries) {
    // Retry after a delay if the attempt failed and max retries not reached
    await delay(7000);
    await requestFileHashWithRetry(state, setIsLoading, dispatch, retries + 1, maxRetries);
  } else {
    setIsLoading(false);
  }
}

// Function to trigger the whole process with an initial delay
export async function TriggerFileHashRequest(
  state: IDetailView,
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>,
  dispatch: Dispatch<DetailViewAction>,
  maxRetries: number = 3
): Promise<void> {
  await delay(3000);
  await requestFileHashWithRetry(state, setIsLoading, dispatch, 0, maxRetries);
}
