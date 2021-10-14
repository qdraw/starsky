import { Link } from "@reach/router";
import React, { memo, useEffect } from "react";
import useLocation from "../../../hooks/use-location";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import Preloader from "../../atoms/preloader/preloader";

interface IListImageBox {
  item: IFileIndexItem;
  /**
   * When selecting and pressing shift
   * @param filePath the entire path (subPath style)
   */
  onSelectionCallback?(filePath: string): void;
  className?: string;
  children?: React.ReactNode;
}

const ListImageViewSelectContainer: React.FunctionComponent<IListImageBox> = memo(
  (props) => {
    const item = props.item;
    if (item.isDirectory === undefined) item.isDirectory = false;

    const [className] = React.useState(
      !props.className ? "list-image-box" : props.className
    );

    var history = useLocation();

    // Check if select exist or Length 0 or more
    const [select, setSelect] = React.useState(
      new URLPath().StringToIUrl(history.location.search).select
    );
    useEffect(() => {
      setSelect(new URLPath().StringToIUrl(history.location.search).select);
    }, [history.location.search]);

    function toggleSelection(fileName: string): void {
      var urlObject = new URLPath().toggleSelection(
        fileName,
        history.location.search
      );
      history.navigate(new URLPath().IUrlToString(urlObject), {
        replace: true
      });
      setSelect(urlObject.select);
    }

    var preloader = <Preloader isOverlay={true} isWhite={false} />;
    const [isPreloaderState, setPreloaderState] = React.useState(false);

    function preloaderStateOnClick(event: React.MouseEvent) {
      // Command (mac) or ctrl click means open new window
      // event.button = is only trigged in safari
      if (event.metaKey || event.ctrlKey || event.button === 1) return;
      setPreloaderState(true);
    }

    // selected state
    if (select) {
      return (
        <div
          className={`${className} ${className}--select`}
          data-filepath={item.filePath}
        >
          <button
            onClick={(event) => {
              // multiple select using the shift key
              if (!event.shiftKey) {
                toggleSelection(item.fileName);
              } else if (event.shiftKey && props.onSelectionCallback) {
                props.onSelectionCallback(item.filePath);
              }
            }}
            className={
              select.indexOf(item.fileName) === -1
                ? "box-content colorclass--" +
                  item.colorClass +
                  " isDirectory-" +
                  item.isDirectory
                : "box-content box-content--selected colorclass--" +
                  item.colorClass +
                  " isDirectory-" +
                  item.isDirectory
            }
          >
            {props.children}
          </button>
        </div>
      );
    }

    // default state
    // data-filepath is needed to scroll to
    return (
      <div
        data-test="list-image-view-select-container"
        className={`${className} box--view`}
        data-filepath={item.filePath}
      >
        {/* for slow connections show preloader icon */}
        {isPreloaderState ? preloader : null}
        {/* the a href to the child page */}
        <Link
          onClick={preloaderStateOnClick}
          title={item.fileName}
          to={new UrlQuery().updateFilePathHash(
            history.location.search,
            item.filePath
          )}
          className={
            "box-content colorclass--" +
            item.colorClass +
            " isDirectory-" +
            item.isDirectory
          }
        >
          {props.children}
        </Link>
      </div>
    );
  }
);

export default ListImageViewSelectContainer;
