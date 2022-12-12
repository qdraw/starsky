import React, { useEffect, useRef } from "react";
import FileHashImage from "../../components/atoms/file-hash-image/file-hash-image";
import Preloader from "../../components/atoms/preloader/preloader";
import ColorClassSelectKeyboard from "../../components/molecules/color-class-select/color-class-select-keyboard";
import DetailViewGpx from "../../components/organisms/detail-view-media/detail-view-gpx";
import DetailViewMp4 from "../../components/organisms/detail-view-media/detail-view-mp4";
import DetailViewSidebar from "../../components/organisms/detail-view-sidebar/detail-view-sidebar";
import { DetailViewContext } from "../../contexts/detailview-context";
import useGestures from "../../hooks/use-gestures/use-gestures";
import useKeyboardEvent from "../../hooks/use-keyboard/use-keyboard-event";
import useLocation from "../../hooks/use-location";
import {
  IDetailView,
  IRelativeObjects,
  newDetailView
} from "../../interfaces/IDetailView";
import { ImageFormat } from "../../interfaces/IFileIndexItem";
import { INavigateState } from "../../interfaces/INavigateState";
import DocumentTitle from "../../shared/document-title";
import { Keyboard } from "../../shared/keyboard";
import { UpdateRelativeObject } from "../../shared/update-relative-object";
import { URLPath } from "../../shared/url-path";
import { UrlQuery } from "../../shared/url-query";
import MenuDetailViewContainer from "../menu-detailview-container/menu-detailview-container";

const DetailView: React.FC<IDetailView> = () => {
  const history = useLocation();

  let { state, dispatch } = React.useContext(DetailViewContext);

  // if there is no state
  if (!state) {
    state = { ...newDetailView() };
  }

  // next + prev state
  const [relativeObjects, setRelativeObjects] = React.useState(
    state.relativeObjects
  );

  // in normal detailview the state isn't updated (so without search query)
  useEffect(() => {
    setRelativeObjects(state.relativeObjects);
  }, [state.relativeObjects]);

  // boolean to get the details-side menu on or off
  const [isDetails, setDetails] = React.useState(
    new URLPath().StringToIUrl(history.location.search).details
  );
  useEffect(() => {
    const details = new URLPath().StringToIUrl(history.location.search).details;
    setDetails(details);
  }, [history.location.search]);

  // update the browser title
  useEffect(() => {
    if (!state) return;
    new DocumentTitle().SetDocumentTitle(state);
  }, [state]);

  // know if you searching ?t= in url
  const [isSearchQuery, setIsSearchQuery] = React.useState(
    !!new URLPath().StringToIUrl(history.location.search).t
  );
  useEffect(() => {
    setIsSearchQuery(!!new URLPath().StringToIUrl(history.location.search).t);
  }, [history.location.search]);

  // update relative next prev buttons for search queries
  useEffect(() => {
    new UpdateRelativeObject()
      .Update(state, isSearchQuery, history.location.search, setRelativeObjects)
      .catch(() => {
        // do nothing on catch error
      });
    // function UpdateRelativeObject  is not subject to change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [history.location.search, isSearchQuery, state.subPath]);

  // previous item
  useKeyboardEvent(
    /ArrowLeft/,
    (event: KeyboardEvent) => {
      if (new Keyboard().isInForm(event)) return;
      prev();
    },
    [relativeObjects]
  );

  // next item
  useKeyboardEvent(
    /ArrowRight/,
    (event: KeyboardEvent) => {
      if (new Keyboard().isInForm(event)) return;
      next();
    },
    [relativeObjects]
  );

  // parent item or to search page
  useKeyboardEvent(
    /Escape/,
    (event: KeyboardEvent) => {
      if (!history.location) return;
      if (new Keyboard().isInForm(event)) return;

      const url = isSearchQuery
        ? new UrlQuery().HashSearchPage(history.location.search)
        : new UrlQuery().updateFilePathHash(
            history.location.search,
            state.fileIndexItem.parentDirectory
          );

      history.navigate(url, {
        state: {
          filePath: state.fileIndexItem.filePath
        } as INavigateState
      });
    },
    [state.fileIndexItem]
  );

  // toggle details side menu
  function toggleLabels() {
    const urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.details = !isDetails;
    setDetails(urlObject.details);
    history.navigate(new URLPath().IUrlToString(urlObject), { replace: true });
  }

  // short key to toggle sidemenu
  useKeyboardEvent(
    /^(d)$/,
    (event: KeyboardEvent) => {
      if (new Keyboard().isInForm(event)) return;
      toggleLabels();
    },
    [history.location.search]
  );

  // Reset Error after changing page
  const [isError, setError] = React.useState(false);
  useEffect(() => {
    setError(false);
    setUseGestures(true);
  }, [state.subPath]);

  // Reset Loading after changing page
  const [isLoading, setIsLoading] = React.useState(true);

  /**
   * navigation function to go to next photo
   */
  function next() {
    if (!relativeObjects) return;
    if (!relativeObjects.nextFilePath) return;
    if (relativeObjects.nextFilePath === state.subPath) {
      // when changing next very fast it might skip a check
      new UpdateRelativeObject()
        .Update(
          state,
          isSearchQuery,
          history.location.search,
          setRelativeObjects
        )
        .then((data) => {
          navigateNext(data);
        })
        .catch(() => {
          // do nothing on catch error
        });
      return;
    }
    navigateNext(relativeObjects);
  }

  /**
   * Navigate to Next
   * @param relative object to move from
   */
  function navigateNext(relative: IRelativeObjects) {
    const nextPath = new UrlQuery().updateFilePathHash(
      history.location.search,
      relative.nextFilePath,
      false
    );
    // Prevent keeps loading forever
    if (relative.nextHash !== state.fileIndexItem.fileHash) {
      setIsLoading(true);
    }

    history.navigate(nextPath, { replace: true }).then(() => {
      setIsLoading(false);
    });
  }

  /**
   * navigation function to go to prev photo
   */
  function prev() {
    if (!relativeObjects) return;
    if (!relativeObjects.prevFilePath) return;
    if (relativeObjects.prevFilePath === state.subPath) {
      // when changing prev very fast it might skip a check
      new UpdateRelativeObject()
        .Update(
          state,
          isSearchQuery,
          history.location.search,
          setRelativeObjects
        )
        .then((data) => {
          navigatePrev(data);
        });
      return;
    }
    navigatePrev(relativeObjects);
  }

  /**
   * Navigate to previous
   * @param relative object to move from
   */
  function navigatePrev(relative: IRelativeObjects) {
    const prevPath = new UrlQuery().updateFilePathHash(
      history.location.search,
      relativeObjects.prevFilePath,
      false
    );

    // Prevent keeps loading forever
    if (relative.prevHash !== state.fileIndexItem.fileHash) {
      setIsLoading(true);
    }

    history.navigate(prevPath, { replace: true }).then(() => {
      // when the re-render happens un-expected
      // window.location.search === history.location.search
      setIsLoading(false);
    });
  }

  const mainRef = useRef<HTMLDivElement>(null);
  const [isUseGestures, setUseGestures] = React.useState(true);

  useGestures(mainRef, {
    onSwipeLeft: () => {
      if (isUseGestures) next();
    },
    onSwipeRight: () => {
      if (isUseGestures) prev();
    }
  });

  if (!state.fileIndexItem || !relativeObjects) {
    return <Preloader parent={"/"} isWhite={true} isOverlay={true} />;
  }

  return (
    <>
      <MenuDetailViewContainer />
      <div className={isDetails ? "detailview detailview--edit" : "detailview"}>
        {isLoading ? (
          <Preloader
            parent={state.fileIndexItem.parentDirectory}
            isWhite={true}
            isOverlay={false}
          />
        ) : (
          ""
        )}

        <ColorClassSelectKeyboard
          currentColorClass={state.fileIndexItem.colorClass}
          collections={state.collections === true}
          isEnabled={true}
          filePath={state.fileIndexItem.filePath}
          onToggle={() => {
            // do nothing when press toggle
          }}
        />

        {isDetails && state.fileIndexItem.status ? (
          <DetailViewSidebar
            state={state}
            dispatch={dispatch}
            status={state.fileIndexItem.status}
            filePath={state.fileIndexItem.filePath}
          />
        ) : null}

        {state.fileIndexItem.imageFormat === ImageFormat.gpx ? (
          <DetailViewGpx />
        ) : null}
        {state.fileIndexItem.imageFormat === ImageFormat.mp4 ? (
          <DetailViewMp4 />
        ) : null}

        <div
          ref={mainRef}
          className={
            isError
              ? "main main--error main--" + state.fileIndexItem.imageFormat
              : "main main--" + state.fileIndexItem.imageFormat
          }
        >
          {!isError && state.fileIndexItem.fileHash ? (
            <FileHashImage
              setError={setError}
              id={state.fileIndexItem.filePath}
              isError={isError}
              setIsLoading={setIsLoading}
              fileHash={state.fileIndexItem.fileHash}
              orientation={state.fileIndexItem.orientation}
              onWheelCallback={() => {
                if (isUseGestures) setUseGestures(false);
              }}
              onResetCallback={() => {
                setUseGestures(true);
              }}
            />
          ) : null}

          {relativeObjects.nextFilePath ? (
            <div
              onClick={() => next()}
              data-test="detailview-next"
              className="nextprev nextprev--next"
            >
              <div className="icon" />
            </div>
          ) : (
            ""
          )}

          {relativeObjects.prevFilePath ? (
            <div
              onClick={() => prev()}
              data-test="detailview-prev"
              className="nextprev nextprev--prev"
            >
              <div className="icon" />
            </div>
          ) : (
            <div className="nextprev nextprev" />
          )}
        </div>
      </div>
    </>
  );
};

export default DetailView;
