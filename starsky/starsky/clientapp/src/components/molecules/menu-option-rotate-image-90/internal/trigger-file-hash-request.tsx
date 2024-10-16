import { Dispatch } from "react";
import { DetailViewAction } from "../../../../contexts/detailview-context";
import { IDetailView } from "../../../../interfaces/IDetailView";
import { RequestNewFileHash } from "./request-new-filehash";

export function TriggerFileHashRequest(
  state: IDetailView,
  setIsLoading: React.Dispatch<React.SetStateAction<boolean>>,
  dispatch: Dispatch<DetailViewAction>,
  retry: number = 0,
  delay: number = 3000
) {
  const maxRetries = 3;

  const attemptRequest = (currentRetry: number) => {
    setTimeout(() => {
      RequestNewFileHash(state, setIsLoading, dispatch).then((result) => {
        if (result === false && currentRetry < maxRetries) {
          attemptRequest(currentRetry + 1);
        } else {
          setIsLoading(false);
        }
      });
    }, delay);
  };

  return attemptRequest(retry);
}
