import React, { useEffect, useState } from "react";
import { ArchiveAction } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IArchiveProps } from "../../../interfaces/IArchiveProps";
import { IConnectionDefault } from "../../../interfaces/IConnectionDefault";
import { CastToInterface } from "../../../shared/cast-to-interface";
import FetchGet from "../../../shared/fetch-get";
import FetchPost from "../../../shared/fetch-post";
import { FileListCache } from "../../../shared/filelist-cache";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import Preloader from "../../atoms/preloader/preloader";

type ForceSyncWaitButtonPropTypes = {
  propsParentFolder?: string;
  historyLocationSearch: string;
  callback(): void;
  dispatch: React.Dispatch<ArchiveAction>;
};

export const ForceSyncWaitTime = 10000;

/**
 * Helper to get new content in the current view
 * @param param0 where to fetch to, dispatch and callback to close
 */
export async function ForceSyncRequestNewContent({
  historyLocationSearch,
  dispatch,
  callback
}: ForceSyncWaitButtonPropTypes): Promise<void> {
  const url = new UrlQuery().UrlIndexServerApi(
    new URLPath().StringToIUrl(historyLocationSearch)
  );
  FetchGet(url).then((connectionResult) => {
    if (connectionResult.statusCode !== 200) {
      console.log("request failed");
      console.error(connectionResult);
      callback();
      return;
    }
    const forceSyncResult = new CastToInterface().MediaArchive(
      connectionResult.data
    );
    const payload = forceSyncResult.data as IArchiveProps;
    if (payload.fileIndexItems) {
      dispatch({ type: "force-reset", payload });
    }
    callback();
  });
}

const ForceSyncWaitButton: React.FunctionComponent<
  ForceSyncWaitButtonPropTypes
> = ({ propsParentFolder, historyLocationSearch, callback, dispatch }) => {
  function forceSync(): Promise<IConnectionDefault> {
    const parentFolder = propsParentFolder ? propsParentFolder : "/";
    setIsLoading(true);
    new FileListCache().CacheCleanEverything();

    var urlSync = new UrlQuery().UrlSync(parentFolder);
    return FetchPost(urlSync, "");
  }

  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageForceSync = language.text(
    "Handmatig synchroniseren van huidige map",
    "Synchronize current directory manually"
  );

  const [startCounter, setStartCounter] = useState(0);
  // preloading icon
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (startCounter <= 0) return;

    let timeout: NodeJS.Timeout;
    forceSync().then(() => {
      timeout = setTimeout(() => {
        ForceSyncRequestNewContent({
          dispatch,
          historyLocationSearch,
          callback
        });
      }, ForceSyncWaitTime);
    });

    return () => {
      clearTimeout(timeout);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [startCounter]);

  function startForceSync() {
    setStartCounter((currCount) => currCount + 1);
  }

  return (
    <>
      {isLoading ? <Preloader isWhite={false} isOverlay={true} /> : ""}
      <button
        className="btn btn--default"
        data-test="force-sync"
        onClick={() => startForceSync()}
      >
        {MessageForceSync}
      </button>
    </>
  );
};

export default ForceSyncWaitButton;
