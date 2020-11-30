import React, { memo } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location";
import { IRelativeObjects } from "../../../interfaces/IDetailView";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url-query";
import NavigateLink from "../../atoms/navigate-link/navigate-link";

export interface IRelativeLink {
	relativeObjects: IRelativeObjects;
}

/**
 * Only for Archive pages (used to be RelativeLink)
 */
const ArchivePagination: React.FunctionComponent<IRelativeLink> = memo((props) => {
	// content
	const settings = useGlobalSettings();
	const language = new Language(settings.language);
	const MessagePrevious = language.text("Vorige", "Previous");
	const MessageNext = language.text("Volgende", "Next");

	// used for reading current location
	var history = useLocation();

	let { relativeObjects } = props;

	if (!relativeObjects) return <div className="relativelink" />;

	// to the next/prev relative object
	// when in select mode and navigate next to the select mode is still on but there are no items selected
	var prevUrl = new UrlQuery().updateFilePathHash(
		history.location.search,
		relativeObjects.prevFilePath,
		false,
		true
	);
	var nextUrl = new UrlQuery().updateFilePathHash(
		history.location.search,
		relativeObjects.nextFilePath,
		false,
		true
	);

	let prev =
		relativeObjects.prevFilePath !== null ? (
			<NavigateLink className="prev" to={prevUrl}>
				{MessagePrevious}
			</NavigateLink>
		) : null;
	let next =
		relativeObjects.nextFilePath !== null ? (
			<NavigateLink className="next" to={nextUrl}>
				{MessageNext}
			</NavigateLink>
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
