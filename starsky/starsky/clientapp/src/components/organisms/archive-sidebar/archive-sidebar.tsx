import React, { memo, useEffect, useLayoutEffect } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import useLocation from "../../../hooks/use-location";
import { PageType } from "../../../interfaces/IDetailView";
import { Language } from "../../../shared/language";
import { URLPath } from "../../../shared/url-path";
import ArchiveSidebarColorClass from "../../molecules/archive-sidebar/archive-sidebar-color-class";
import ArchiveSidebarLabelEdit from "../../molecules/archive-sidebar/archive-sidebar-label-edit";
import ArchiveSidebarSelectionList from "../../molecules/archive-sidebar/archive-sidebar-selection-list";
import { IArchiveSidebarProps } from "./archive-sidebar.types";

const ArchiveSidebar: React.FunctionComponent<IArchiveSidebarProps> = memo(
	(archive) => {
		// Content
		const settings = useGlobalSettings();
		const language = new Language(settings.language);
		const MessageSelectionName = language.text("Selectie", "Selection");
		const MessageReadOnlyFolder = language.text(
			"Alleen lezen map",
			"Read only folder"
		);
		const MessageUpdateLabels = language.text(
			"Labels wijzigingen",
			"Update labels"
		);
		const MessageColorClassification = language.text(
			"Kleur-Classificatie",
			"Color Classification"
		);

		// Update view based on url parameters
		var history = useLocation();
		const [isSidebar, setIsSidebar] = React.useState(
			new URLPath().StringToIUrl(history.location.search).sidebar
		);
		useEffect(() => {
			var sidebarLocal = new URLPath().StringToIUrl(history.location.search)
				.sidebar;
			setIsSidebar(sidebarLocal);
		}, [history.location.search]);

		/**
		 * toggle when activate &sidebar=true
		 */
		useEffect(() => {
			if (isSidebar) {
				document.body.classList.add("lock-screen");
			} else {
				document.body.classList.remove("lock-screen");
				window.scrollTo(0, parseInt(document.body.style.top || "0") * -1);
			}
			return () => {
				document.body.classList.remove("lock-screen");
				window.scrollTo(0, parseInt(document.body.style.top || "0") * -1);
			};
		}, [isSidebar]);

		/**
		 * to avoid changes in location when scrolling while the sidebar is open
		 */
		const listener = (e: Event) => {
			if (!window.scrollY) return;
			document.body.style.top = `-${window.scrollY}px`;
		};

		useLayoutEffect(() => {
			window.addEventListener("scroll", listener);
			return () => {
				window.removeEventListener("scroll", listener);
			};
		});

		// /** to avoid wrong props passed */
		if (archive.pageType === PageType.Loading || !isSidebar) {
			return <></>;
		}

		return (
			<div className="sidebar">
				{archive.isReadOnly ? (
					<>
						<div className="content--header">Status</div>
						<div className="content content--text">
							<div className="warning-box">{MessageReadOnlyFolder}</div>
						</div>{" "}
					</>
				) : null}

				<div className="content--header">{MessageSelectionName}</div>
				<ArchiveSidebarSelectionList {...archive} />

				<div className="content--header">{MessageUpdateLabels}</div>
				<ArchiveSidebarLabelEdit />

				<div className="content--header">{MessageColorClassification}</div>
				<div className="content--text">
					<ArchiveSidebarColorClass {...archive} />
				</div>
			</div>
		);
	}
);

export default ArchiveSidebar;
