import React, { memo } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useHotKeys from "../../../hooks/use-keyboard/use-hotkeys";
import useKeyboardEvent from "../../../hooks/use-keyboard/use-keyboard-event";
import useLocation from "../../../hooks/use-location/use-location";
import { IRelativeObjects } from "../../../interfaces/IDetailView";
import localization from "../../../localization/localization.json";
import { Keyboard } from "../../../shared/keyboard/keyboard";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
import Link from "../../atoms/link/link";

interface IRelativeLink {
  relativeObjects: IRelativeObjects;
}

/**
 * Only for Archive pages (used to be RelativeLink)
 */
const ArchivePagination: React.FunctionComponent<IRelativeLink> = memo((props) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessagePrevious = language.key(localization.MessagePrevious);
  const MessageNext = language.key(localization.MessageNext);

  // used for reading current location
  const history = useLocation();

  const { relativeObjects } = props;

  // to the next/prev relative object
  // when in select mode and navigate next to the select mode is still on but there are no items selected
  const prevUrl = relativeObjects
    ? new UrlQuery().updateFilePathHash(history.location.search, relativeObjects.prevFilePath, false, true)
    : null;
  const nextUrl = relativeObjects
    ? new UrlQuery().updateFilePathHash(history.location.search, relativeObjects.nextFilePath, false, true)
    : null;

  // previous page
  useKeyboardEvent(
    /ArrowLeft/,
    (event: KeyboardEvent) => {
      if (new Keyboard().isInForm(event)) return;
      if (relativeObjects?.prevFilePath == null) return;
      history.navigate(prevUrl as string, { replace: true });
    },
    [relativeObjects, prevUrl]
  );

  // next page
  useKeyboardEvent(
    /ArrowRight/,
    (event: KeyboardEvent) => {
      if (new Keyboard().isInForm(event)) return;
      if (relativeObjects?.nextFilePath == null) return;
      history.navigate(nextUrl as string, { replace: true });
    },
    [relativeObjects, nextUrl]
  );

  // previous page (Cmd/Ctrl+[)
  useHotKeys(
    { key: "[", ctrlKeyOrMetaKey: true },
    () => {
      if (relativeObjects?.prevFilePath == null) return;
      history.navigate(prevUrl as string, { replace: true });
    },
    [relativeObjects, prevUrl]
  );

  // next page (Cmd/Ctrl+])
  useHotKeys(
    { key: "]", ctrlKeyOrMetaKey: true },
    () => {
      if (relativeObjects?.nextFilePath == null) return;
      history.navigate(nextUrl as string, { replace: true });
    },
    [relativeObjects, nextUrl]
  );

  if (!relativeObjects) return <div className="relativelink" />;

  const prev =
    relativeObjects.prevFilePath === null ? null : (
      <Link className="prev" data-test="archive-pagination-prev" to={prevUrl as string}>
        {MessagePrevious}
      </Link>
    );
  const next =
    relativeObjects.nextFilePath === null ? null : (
      <Link className="next" data-test="archive-pagination-next" to={nextUrl as string}>
        {MessageNext}
      </Link>
    );

  return (
    <div className="relativelink">
      <h4 className="nextprev">
        {prev}
        {next}
      </h4>
    </div>
  );
});
export default ArchivePagination;
