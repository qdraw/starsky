import React, { useEffect, useState } from 'react';
import { ArchiveContext } from '../../../contexts/archive-context';
import useGlobalSettings from '../../../hooks/use-global-settings';
import useInterval from '../../../hooks/use-interval';
import useLocation from '../../../hooks/use-location';
import { IArchiveProps } from '../../../interfaces/IArchiveProps';
import { CastToInterface } from '../../../shared/cast-to-interface';
import FetchGet from '../../../shared/fetch-get';
import FetchPost from '../../../shared/fetch-post';
import { Language } from '../../../shared/language';
import { URLPath } from '../../../shared/url-path';
import { UrlQuery } from '../../../shared/url-query';
import Modal from '../../atoms/modal/modal';
import Preloader from '../../atoms/preloader/preloader';

interface IModalDisplayOptionsProps {
  isOpen: boolean;
  handleExit: Function;
  parentFolder?: string;
}

const ModalArchiveSynchronizeManually: React.FunctionComponent<IModalDisplayOptionsProps> = (props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageSynchronizeManually = language.text("Handmatig synchroniseren", "Synchronize manually");
  const MessageForceSync = language.text("Handmatig synchroniseren van huidige map", "Synchronize current directory manually");
  const MessageRemoveCache = language.text("Verwijder cache van huidige map", "Refresh cache of current directory");
  const MessageGeoSync = language.text("Voeg geolocatie automatisch toe", "Automatically add geolocation");
  const MessageGeoSyncExplainer = language.text("De locatie wordt afgeleid van een gpx bestand die zich in de huidige map bevind " +
    "en op basis van de locatie worden er plaatsnamen bij de afbeeldingen gevoegd", "The location is derived from a gpx file located in " +
  " the current folder and based on the location place names are appended to the images")
  const MessageManualThumbnailSync = language.text("Thumbnail afbeeldingen generen", "Generate thumbnail images");
  const MessageManualThumbnailSyncExplainer = language.text("Deze actie genereert op de achtergrond veel miniatuurafbeeldingen, dit heeft invloed op de prestaties",
    "This action generate on the background lots of thumbnail images, this does impact the performance");

  // preloading icon
  const [isLoading, setIsLoading] = useState(false);

  var history = useLocation();
  let { dispatch } = React.useContext(ArchiveContext);

  // the default is true
  const [collections, setCollections] = React.useState(new URLPath().StringToIUrl(history.location.search).collections !== false);

  /** update when changing values and search */
  useEffect(() => {
    setCollections(new URLPath().StringToIUrl(history.location.search).collections !== false);
  }, [collections, history.location.search])

  /**
   * Remove Folder cache
   */
  function removeCache() {
    setIsLoading(true);

    var parentFolder = props.parentFolder ? props.parentFolder : "/";
    FetchGet(new UrlQuery().UrlRemoveCache(new URLPath().encodeURI(parentFolder))).then((_) => {
      setTimeout(() => {
        FetchGet(new UrlQuery().UrlIndexServerApiPath(new URLPath().encodeURI(parentFolder))).then((connectionResult) => {
          var removeCacheResult = new CastToInterface().MediaArchive(connectionResult.data);
          var payload = removeCacheResult.data as IArchiveProps;
          if (payload.fileIndexItems) {
            dispatch({ type: 'force-reset', payload });
          }
          props.handleExit();
        });
      }, 500);
    });
  }

  const [geoSyncPercentage, setGeoSyncPercentage] = useState(0);

  function geoSync() {
    console.log('--geo');

    var parentFolder = props.parentFolder ? props.parentFolder : "/";
    var bodyParams = new URLSearchParams();
    bodyParams.set("f", parentFolder);

    FetchPost(new UrlQuery().UrlGeoSync(), bodyParams.toString()).then((anyData) => {
    });
  }

  function fetchGeoSyncStatus() {
    var parentFolder = props.parentFolder ? props.parentFolder : "/";
    FetchGet(new UrlQuery().UrlGeoStatus(new URLPath().encodeURI(parentFolder))).then((anyData) => {

      if (anyData.statusCode !== 200 || !anyData.data) {
        setGeoSyncPercentage(-1);
        return;
      }

      if (anyData.data.current === 0 && anyData.data.total === 0) {
        setGeoSyncPercentage(0);
        return;
      }
      setGeoSyncPercentage(anyData.data.current / anyData.data.total * 100);
    });
  }

  useInterval(() => fetchGeoSyncStatus(), 4333);

  function forceSync() {
    var parentFolder = props.parentFolder ? props.parentFolder : "/";
    setIsLoading(true);
    FetchGet(new UrlQuery().UrlSync(new URLPath().encodeURI(parentFolder))).then((_) => {
      setTimeout(() => {
        FetchGet(new UrlQuery().UrlIndexServerApiPath(new URLPath().encodeURI(parentFolder))).then((connectionResult) => {
          var forceSyncResult = new CastToInterface().MediaArchive(connectionResult.data);
          var payload = forceSyncResult.data as IArchiveProps;
          if (payload.fileIndexItems) {
            dispatch({ type: 'force-reset', payload });
          }
          props.handleExit();
        });
      }, 10000);
    });
  }

  function manualThumbnailSync() {
    var parentFolder = props.parentFolder ? props.parentFolder : "/";
    var bodyParams = new URLSearchParams();
    bodyParams.set("f", parentFolder);
    setIsLoading(true);

    FetchPost(new UrlQuery().UrlThumbnailGeneration(), bodyParams.toString()).then((anyData) => {
      setTimeout(() => {
        setIsLoading(false);
        props.handleExit();
      }, 10000);
    });
  }

  return (<Modal
    id="detailview-export-modal"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit()
    }}>
    {isLoading ? <Preloader isDetailMenu={false} isOverlay={true} /> : ""}

    <div className="modal content--subheader">{MessageSynchronizeManually}</div>
    <div className="modal content--text">
      <button className="btn btn--default" data-test="force-sync" onClick={() => forceSync()}>{MessageForceSync}</button>
      <button className="btn btn--default" data-test="remove-cache" onClick={() => removeCache()}>{MessageRemoveCache}</button>
      <button className="btn btn--info btn--percentage" data-test="geo-sync" onClick={() => geoSync()}>{MessageGeoSync} {geoSyncPercentage}%</button>
      <p>{MessageGeoSyncExplainer}</p>
      <button className="btn btn--info" data-test="thumbnail-generation" onClick={() => manualThumbnailSync()}>{MessageManualThumbnailSync}</button>
      <p>
        {MessageManualThumbnailSyncExplainer}
      </p>
    </div>
  </Modal>)
}

export default ModalArchiveSynchronizeManually;
