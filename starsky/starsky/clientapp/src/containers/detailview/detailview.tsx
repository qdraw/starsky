import { FC, useContext, useEffect, useRef, useState } from "react";
import FileHashImage from "../../components/atoms/file-hash-image/file-hash-image";
import Preloader from "../../components/atoms/preloader/preloader";
import ColorClassSelectKeyboard from "../../components/molecules/color-class-select/color-class-select-keyboard";
import DetailViewGpx from "../../components/organisms/detail-view-media/detail-view-gpx";
import DetailViewMp4 from "../../components/organisms/detail-view-media/detail-view-mp4";
import DetailViewSidebar from "../../components/organisms/detail-view-sidebar/detail-view-sidebar";
import { DetailViewContext } from "../../contexts/detailview-context";
import { useGestures } from "../../hooks/use-gestures/use-gestures";
import useKeyboardEvent from "../../hooks/use-keyboard/use-keyboard-event";
import useLocation from "../../hooks/use-location/use-location";
import { IDetailView, newDetailView } from "../../interfaces/IDetailView";
import { ImageFormat } from "../../interfaces/IFileIndexItem";
import { DocumentTitle } from "../../shared/document-title";
import { Keyboard } from "../../shared/keyboard";
import { UpdateRelativeObject } from "../../shared/update-relative-object";
import { URLPath } from "../../shared/url/url-path";
import MenuDetailViewContainer from "../menu-detailview-container/menu-detailview-container";
import { moveFolderUp } from "./helpers/move-folder-up";
import { PrevNext } from "./helpers/prev-next";
import { statusRemoved } from "./helpers/status-removed";

const DetailView: FC<IDetailView> = () => {
  const history = useLocation();

  // eslint-disable-next-line prefer-const
  let { state, dispatch } = useContext(DetailViewContext);

  // if there is no state
  if (!state) {
    state = { ...newDetailView() };
  }

  // next + prev state
  const [relativeObjects, setRelativeObjects] = useState(state.relativeObjects);

  // in normal detailview the state isn't updated (so without search query)
  useEffect(() => {
    setRelativeObjects(state.relativeObjects);
  }, [state.relativeObjects]);

  // boolean to get the details-side menu on or off
  const [details, setDetails] = useState(
    new URLPath().StringToIUrl(history?.location?.search)?.details
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
  const [isSearchQuery, setIsSearchQuery] = useState(
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
    // es_lint-disable-next-line react-hooks/exhaustive-deps // https://github.com/facebook/react/pull/30774
  }, [history.location.search, isSearchQuery, state.subPath]);

  // previous item
  useKeyboardEvent(
    /ArrowLeft/,
    (event: KeyboardEvent) => {
      if (new Keyboard().isInForm(event)) return;
      new PrevNext(
        relativeObjects,
        state,
        isSearchQuery,
        history,
        setRelativeObjects,
        setIsLoading
      ).prev();
    },
    [relativeObjects]
  );

  // next item
  useKeyboardEvent(
    /ArrowRight/,
    (event: KeyboardEvent) => {
      if (new Keyboard().isInForm(event)) return;
      new PrevNext(
        relativeObjects,
        state,
        isSearchQuery,
        history,
        setRelativeObjects,
        setIsLoading
      ).next();
    },
    [relativeObjects]
  );

  // parent item or to search page
  useKeyboardEvent(
    /Escape/,
    (event: KeyboardEvent) => {
      moveFolderUp(event, history, isSearchQuery, state);
    },
    [state.fileIndexItem]
  );

  // toggle details side menu
  function toggleLabels() {
    const urlObject = new URLPath().StringToIUrl(history.location.search);
    urlObject.details = !details;
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
  const [isError, setIsError] = useState(false);
  useEffect(() => {
    setIsError(false);
    setIsUseGestures(true);
  }, [state.subPath]);

  // // When item is removed
  useEffect(() => {
    statusRemoved(state, relativeObjects, isSearchQuery, history, setRelativeObjects, setIsLoading);
    // es_lint-disable-next-line react-hooks/exhaustive-deps // https://github.com/facebook/react/pull/30774
  }, [state.fileIndexItem?.status]);

  // Reset Loading after changing page
  const [isLoading, setIsLoading] = useState(true);

  const mainRef = useRef<HTMLDivElement>(null);
  const [isUseGestures, setIsUseGestures] = useState(true);

  useGestures(mainRef, {
    onSwipeLeft: () => {
      if (isUseGestures) {
        new PrevNext(
          relativeObjects,
          state,
          isSearchQuery,
          history,
          setRelativeObjects,
          setIsLoading
        ).next();
      }
    },
    onSwipeRight: () => {
      if (isUseGestures) {
        new PrevNext(
          relativeObjects,
          state,
          isSearchQuery,
          history,
          setRelativeObjects,
          setIsLoading
        ).prev();
      }
    }
  });

  if (!state.fileIndexItem || !relativeObjects) {
    return <Preloader parent={"/"} isWhite={true} isOverlay={true} />;
  }

  return (
    <>
      <MenuDetailViewContainer />
      <div className={details ? "detailview detailview--edit" : "detailview"}>
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

        {details && state.fileIndexItem.status ? (
          <DetailViewSidebar
            state={state}
            dispatch={dispatch}
            status={state.fileIndexItem.status}
            filePath={state.fileIndexItem.filePath}
          />
        ) : null}

        {state.fileIndexItem.imageFormat === ImageFormat.gpx ? <DetailViewGpx /> : null}
        {state.fileIndexItem.imageFormat === ImageFormat.mp4 ? <DetailViewMp4 /> : null}

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
              setError={setIsError}
              alt={state.fileIndexItem.title + " " + state.fileIndexItem.tags}
              id={state.fileIndexItem.filePath}
              setIsLoading={setIsLoading}
              fileHash={state.fileIndexItem.fileHash}
              orientation={state.fileIndexItem.orientation}
              onWheelCallback={() => {
                if (isUseGestures) setIsUseGestures(false);
              }}
              onResetCallback={() => {
                setIsUseGestures(true);
              }}
            />
          ) : null}

          {relativeObjects.nextFilePath ? (
            <button
              onClick={() =>
                new PrevNext(
                  relativeObjects,
                  state,
                  isSearchQuery,
                  history,
                  setRelativeObjects,
                  setIsLoading
                ).next()
              }
              onKeyDown={(event) => {
                // eslint-disable-next-line @typescript-eslint/no-unused-expressions
                event.key === "Enter" &&
                  new PrevNext(
                    relativeObjects,
                    state,
                    isSearchQuery,
                    history,
                    setRelativeObjects,
                    setIsLoading
                  ).next();
              }}
              data-test="detailview-next"
              className="nextprev nextprev--next"
            >
              <div className="icon" />
            </button>
          ) : (
            ""
          )}

          {relativeObjects.prevFilePath ? (
            <button
              onClick={() =>
                new PrevNext(
                  relativeObjects,
                  state,
                  isSearchQuery,
                  history,
                  setRelativeObjects,
                  setIsLoading
                ).prev()
              }
              onKeyDown={(event) => {
                // eslint-disable-next-line @typescript-eslint/no-unused-expressions
                event.key === "Enter" &&
                  new PrevNext(
                    relativeObjects,
                    state,
                    isSearchQuery,
                    history,
                    setRelativeObjects,
                    setIsLoading
                  ).next();
              }}
              data-test="detailview-prev"
              className="nextprev nextprev--prev"
            >
              <div className="icon" />
            </button>
          ) : (
            <div className="nextprev nextprev" />
          )}
        </div>
      </div>
    </>
  );
};

export default DetailView;
