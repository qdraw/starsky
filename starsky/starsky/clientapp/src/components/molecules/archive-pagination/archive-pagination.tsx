import React, { memo } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location/use-location";
import { IRelativeObjects } from "../../../interfaces/IDetailView";
import localization from "../../../localization/localization.json";
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

  if (!relativeObjects) return <div className="relativelink" />;

  // to the next/prev relative object
  // when in select mode and navigate next to the select mode is still on but there are no items selected
  const prevUrl = new UrlQuery().updateFilePathHash(
    history.location.search,
    relativeObjects.prevFilePath,
    false,
    true
  );
  const nextUrl = new UrlQuery().updateFilePathHash(
    history.location.search,
    relativeObjects.nextFilePath,
    false,
    true
  );

  const prev =
    relativeObjects.prevFilePath !== null ? (
      <Link className="prev" data-test="archive-pagination-prev" to={prevUrl}>
        {MessagePrevious}
      </Link>
    ) : null;
  const next =
    relativeObjects.nextFilePath !== null ? (
      <Link className="next" data-test="archive-pagination-next" to={nextUrl}>
        {MessageNext}
      </Link>
    ) : null;

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
