import React, { useEffect } from 'react';
import Preloader from '../components/atoms/preloader/preloader';
import DetailViewGpx from '../components/organisms/detail-view-media/detail-view-gpx';
import DetailViewMp4 from '../components/organisms/detail-view-media/detail-view-mp4';
import DetailViewSidebar from '../components/organisms/detail-view-sidebar/detail-view-sidebar';
import MenuDetailView from '../components/organisms/menu-detail-view/menu-detail-view';
import { DetailViewContext } from '../contexts/detailview-context';
import useKeyboardEvent from '../hooks/use-keyboard-event';
import useLocation from '../hooks/use-location';
import { IDetailView, newDetailView, newIRelativeObjects } from '../interfaces/IDetailView';
import { ImageFormat, Orientation } from '../interfaces/IFileIndexItem';
import { INavigateState } from '../interfaces/INavigateState';
import DocumentTitle from '../shared/document-title';
import FetchGet from '../shared/fetch-get';
import { Keyboard } from '../shared/keyboard';
import { OrientationHelper } from '../shared/orientation-helper';
import { URLPath } from '../shared/url-path';
import { UrlQuery } from '../shared/url-query';

const DetailView: React.FC<IDetailView> = () => {

  var history = useLocation();

  let { state } = React.useContext(DetailViewContext);

  // if there is no state
  if (!state) {
    state = { relativeObjects: newIRelativeObjects(), fileIndexItem: { fileHash: '' }, ...newDetailView() };
  }

  // next + prev state
  const [relativeObjects, setRelativeObjects] = React.useState(state.relativeObjects);

  // when in some cases the relative urls are not updated by a state change
  useEffect(() => {
    setRelativeObjects(state.relativeObjects);
  }, [state.relativeObjects]);

  // boolean to get the details-side menu on or off
  const [isDetails, setDetails] = React.useState(new URLPath().StringToIUrl(history.location.search).details);
  useEffect(() => {
    var details = new URLPath().StringToIUrl(history.location.search).details;
    setDetails(details);
  }, [history.location.search]);

  // update the browser title
  useEffect(() => {
    if (!state) return;
    new DocumentTitle().SetDocumentTitle(state);
  }, [state]);

  const [isAutomaticRotated, setAutomaticRotated] = React.useState(true);
  useEffect(() => {
    new OrientationHelper().DetectAutomaticRotation().then((result) => setAutomaticRotated(result))
  }, []);

  // To Get the rotation update
  const [translateRotation, setTranslateRotation] = React.useState(Orientation.Horizontal);
  useEffect(() => {
    if (!state) return;
    if (!state.fileIndexItem.orientation) return;

    console.log('isAutomaticRotated', isAutomaticRotated);

    if (isAutomaticRotated) {
      return;
    }
    // know if the thumbnail is ready, if not rotate the image clientside
    FetchGet(new UrlQuery().UrlThumbnailJsonApi(state.fileIndexItem.fileHash)).then((result) => {
      if (!state.fileIndexItem.orientation) return;
      if (result.statusCode === 202) {
        // result from API is: "Thumbnail is not ready yet"
        setTranslateRotation(state.fileIndexItem.orientation);
      }
      else if (result.statusCode === 200) {
        // thumbnail is alreay rotated (but need to be called due change of image)
        setTranslateRotation(Orientation.Horizontal);
      }
    }).catch((e) => {
      console.log(e);
    });

    // disable to prevent duplicate api calls
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state.fileIndexItem.fileHash, isAutomaticRotated]);

  // know if you searching ?t= in url
  const [isSearchQuery, setIsSearchQuery] = React.useState(!!new URLPath().StringToIUrl(history.location.search).t);
  useEffect(() => {
    setIsSearchQuery(!!new URLPath().StringToIUrl(history.location.search).t);
  }, [history.location.search]);

  // update relative next prev buttons for search queries
  useEffect(() => {
    if (state.subPath === "/" || !isSearchQuery) return;
    FetchGet(new UrlQuery().UrlSearchRelativeApi(state.subPath,
      new URLPath().StringToIUrl(history.location.search).t,
      new URLPath().StringToIUrl(history.location.search).p)
    ).then((result) => {
      if (result.statusCode !== 200) return;
      setRelativeObjects(result.data);
    }).catch((err) => {
      console.log(err);
    });
  }, [history.location.search, isSearchQuery, state.subPath]);

  // previous item
  useKeyboardEvent(/ArrowLeft/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    if (!relativeObjects) return;
    if (!relativeObjects.prevFilePath) return;
    prev();
  }, [relativeObjects]);

  // next item
  useKeyboardEvent(/ArrowRight/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    if (!relativeObjects) return;
    if (!relativeObjects.nextFilePath) return;
    next();
  }, [relativeObjects]);

  // parent item or to search page
  useKeyboardEvent(/Escape/, (event: KeyboardEvent) => {
    if (!history.location) return;
    if (new Keyboard().isInForm(event)) return;

    var url = isSearchQuery ? new UrlQuery().HashSearchPage(history.location.search) :
      new UrlQuery().updateFilePathHash(history.location.search, state.fileIndexItem.parentDirectory);

    history.navigate(url, {
      state: {
        filePath: state.fileIndexItem.filePath
      } as INavigateState
    });
  }, [state.fileIndexItem]);

  // toggle details side menu
  function toggleLabels() {
    var urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.details = !isDetails;
    setDetails(urlObject.details);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true })
  }

  // short key to toggle sidemenu
  useKeyboardEvent(/^(d)$/, (event: KeyboardEvent) => {
    if (new Keyboard().isInForm(event)) return;
    toggleLabels();
  }, [history.location.search]);

  // Reset Error after changing page
  const [isError, setError] = React.useState(false);
  useEffect(() => {
    setError(false);
  }, [state.subPath]);

  // Reset Loading after changing page
  const [isLoading, setIsLoading] = React.useState(true);

  /**
  * navigation function to go to next photo
  */
  function next() {
    if (!relativeObjects) return;
    var nextPath = new UrlQuery().updateFilePathHash(history.location.search, relativeObjects.nextFilePath, false);
    // Prevent keeps loading forever
    if (relativeObjects.nextHash !== state.fileIndexItem.fileHash) {
      setIsLoading(true)
    }
    history.navigate(nextPath, { replace: true });
  }

  /**
   * navigation function to go to previous photo
   */
  function prev() {
    if (!relativeObjects) return;
    var prevPath = new UrlQuery().updateFilePathHash(history.location.search, relativeObjects.prevFilePath, false);
    // Prevent keeps loading forever
    if (relativeObjects.prevHash !== state.fileIndexItem.fileHash) {
      setIsLoading(true)
    }
    history.navigate(prevPath, { replace: true });
  }

  if (!state.fileIndexItem || !relativeObjects) {
    return (<Preloader parent={"/"} isDetailMenu={true} isOverlay={true} />)
  }

  return (<>
    <MenuDetailView />
    <div className={isDetails ? "detailview detailview--edit" : "detailview"}>
      {isLoading ? <Preloader parent={state.fileIndexItem.parentDirectory} isDetailMenu={true} isOverlay={true} /> : ""}

      {isDetails && state.fileIndexItem.status ? <DetailViewSidebar
        status={state.fileIndexItem.status} filePath={state.fileIndexItem.filePath} /> : null}

      {state.fileIndexItem.imageFormat === ImageFormat.gpx ? <DetailViewGpx /> : null}
      {state.fileIndexItem.imageFormat === ImageFormat.mp4 ? <DetailViewMp4 /> : null}

      <div className={isError ? "main main--error main--" + state.fileIndexItem.imageFormat : "main main--" + state.fileIndexItem.imageFormat}>

        {!isError && state.fileIndexItem.fileHash ? <img alt={state.fileIndexItem.tags}
          className={"image--default " + translateRotation}
          onLoad={() => {
            setError(false);
            setIsLoading(false);
          }}
          onError={() => {
            setError(true);
            setIsLoading(false);
          }} src={new UrlQuery().UrlThumbnailImage(state.fileIndexItem.fileHash, true)} /> : null}

        {relativeObjects.nextFilePath ?
          <div onClick={() => next()} className="nextprev nextprev--next"><div className="icon" /></div>
          : ""}

        {relativeObjects.prevFilePath ?
          <div onClick={() => prev()}
            className="nextprev nextprev--prev"><div className="icon" /></div>
          : <div className="nextprev nextprev" />}

      </div>
    </div>
  </>)
};

export default DetailView;
