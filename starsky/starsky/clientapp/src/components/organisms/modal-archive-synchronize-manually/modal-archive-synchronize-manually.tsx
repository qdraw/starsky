import React, { useEffect, useState } from "react";
import { ArchiveContext } from "../../../contexts/archive-context";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useInterval from "../../../hooks/use-interval";
import useLocation from "../../../hooks/use-location/use-location";
import localization from "../../../localization/localization.json";
import FetchGet from "../../../shared/fetch/fetch-get";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url/url-path";
import { UrlQuery } from "../../../shared/url/url-query";
import Modal from "../../atoms/modal/modal";
import Preloader from "../../atoms/preloader/preloader";
import ForceSyncWaitButton from "../../molecules/force-sync-wait-button/force-sync-wait-button";
import { RemoveCache } from "./internal/remove-cache.ts";

interface IModalDisplayOptionsProps {
  isOpen: boolean;
  handleExit: () => void;
  parentFolder?: string;
}

const ModalArchiveSynchronizeManually: React.FunctionComponent<IModalDisplayOptionsProps> = (
  props
) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageSynchronizeManually = language.key(localization.MessageSynchronizeManually);
  const MessageRemoveCache = language.key(localization.MessageRemoveCache);
  const MessageGeoSync = language.key(localization.MessageGeoSync);
  const MessageGeoSyncExplainer = language.key(localization.MessageGeoSyncExplainer);
  const MessageManualThumbnailSync = language.key(localization.MessageManualThumbnailSync);
  const MessageManualThumbnailSyncExplainer = language.key(
    localization.MessageManualThumbnailSyncExplainer
  );

  // preloading icon
  const [isLoading, setIsLoading] = useState(false);

  const history = useLocation();
  const { dispatch } = React.useContext(ArchiveContext);

  // the default is true
  const [collections, setCollections] = React.useState(
    new URLPath().StringToIUrl(history.location.search).collections !== false
  );

  /** update when changing values and search */
  useEffect(() => {
    setCollections(new URLPath().StringToIUrl(history.location.search).collections !== false);
  }, [collections, history.location.search]);

  const [geoSyncPercentage, setGeoSyncPercentage] = useState(0);

  function geoSync() {
    const parentFolder = props.parentFolder ?? "/";

    const bodyParams = new URLSearchParams();
    bodyParams.set("f", parentFolder);
    FetchPost(new UrlQuery().UrlGeoSync(), bodyParams.toString()).then(() => {
      // do nothing with result
    });
  }

  function fetchGeoSyncStatus() {
    const parentFolder = props.parentFolder ?? "/";
    FetchGet(new UrlQuery().UrlGeoStatus(new URLPath().encodeURI(parentFolder))).then((anyData) => {
      if (anyData.statusCode !== 200 || !anyData.data) {
        setGeoSyncPercentage(-1);
        return;
      }

      if (anyData.data.current === 0 && anyData.data.total === 0) {
        setGeoSyncPercentage(0);
        return;
      }
      setGeoSyncPercentage((anyData.data.current / anyData.data.total) * 100);
    });
  }

  useInterval(() => fetchGeoSyncStatus(), 10000);

  function manualThumbnailSync() {
    const parentFolder = props.parentFolder ?? "/";
    const bodyParams = new URLSearchParams();
    bodyParams.set("f", parentFolder);
    setIsLoading(true);

    FetchPost(new UrlQuery().UrlThumbnailGeneration(), bodyParams.toString()).then(() => {
      setTimeout(() => {
        setIsLoading(false);
        props.handleExit();
      }, 10000);
    });
  }

  return (
    <Modal
      id="modal-archive-synchronize-manually"
      isOpen={props.isOpen}
      handleExit={() => {
        props.handleExit();
      }}
    >
      {isLoading ? <Preloader isWhite={false} isOverlay={true} /> : ""}

      <div className="modal content--subheader">{MessageSynchronizeManually}</div>
      <div className="modal content--text">
        <ForceSyncWaitButton
          propsParentFolder={props.parentFolder}
          historyLocationSearch={history.location.search}
          callback={() => props.handleExit()}
          dispatch={dispatch}
        ></ForceSyncWaitButton>
        <button
          className="btn btn--default"
          data-test="remove-cache"
          onClick={() =>
            RemoveCache(
              setIsLoading,
              props.parentFolder ?? "",
              history.location.search,
              dispatch,
              props.handleExit
            )
          }
        >
          {MessageRemoveCache}
        </button>
        <button
          className="btn btn--info btn--percentage"
          data-test="geo-sync"
          onClick={() => geoSync()}
        >
          {MessageGeoSync} {geoSyncPercentage}%
        </button>
        <p>{MessageGeoSyncExplainer}</p>
        <button
          className="btn btn--info"
          data-test="thumbnail-generation"
          onClick={() => manualThumbnailSync()}
        >
          {MessageManualThumbnailSync}
        </button>
        <p>{MessageManualThumbnailSyncExplainer}</p>
      </div>
    </Modal>
  );
};

export default ModalArchiveSynchronizeManually;
