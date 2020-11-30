import React, { memo } from "react";
import useLocation from "../../../hooks/use-location";
import { URLPath } from "../../../shared/url-path";
import { UrlQuery } from "../../../shared/url-query";
import NavigateLink from "../../atoms/navigate-link/navigate-link";

/**
 * subPath is child folder
 * Breadcrumb variable should only contain parent Folders
 */
interface IBreadcrumbProps {
	subPath: string;
	breadcrumb: Array<string>;
}

const Breadcrumbs: React.FunctionComponent<IBreadcrumbProps> = memo((props) => {
	// used for reading current location
	var history = useLocation();

	if (!props.subPath || !props.breadcrumb) return <div className="breadcrumb" />;
	return (
		<div className={props.subPath.length >= 28 ? "breadcrumb breadcrumb--long" : "breadcrumb"}>
			{props.breadcrumb.map((item, index) => {
				let name = item.split("/")[item.split("/").length - 1];

				// instead of nothing
				if (index === 0) {
					name = "Home";
				}

				// For the home page
				if (item === props.subPath) {
					return (
						<span key={item}>
							<NavigateLink to={new UrlQuery().updateFilePathHash(history.location.search, item)}>
								{name}
							</NavigateLink>
						</span>
					);
				}

				return (
					<span key={item}>
						<NavigateLink to={new UrlQuery().updateFilePathHash(history.location.search, item)}>
							{name}
						</NavigateLink>{" "}
						<span> »</span>{" "}
					</span>
				);
			})}
			{new URLPath().FileNameBreadcrumb(props.subPath)}
		</div>
	);
});

export default Breadcrumbs;
