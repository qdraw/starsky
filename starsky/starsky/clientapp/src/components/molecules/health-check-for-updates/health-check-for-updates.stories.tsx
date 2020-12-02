import { storiesOf } from "@storybook/react";
import React from "react";
import HealthCheckForUpdates, {
	CheckForUpdatesLocalStorageName
} from "./health-check-for-updates";

storiesOf("components/molecules/health-check-for-updates", module)
	.add("default", () => {
		(window as any).isElectron = undefined;
		return (
			<>
				<button
					onClick={() => {
						localStorage.removeItem(CheckForUpdatesLocalStorageName);
						window.location.reload();
					}}
				>
					Clean sessie
				</button>
				T<b>There nothing shown yet, only if the api returns a error code</b>
				<HealthCheckForUpdates />
			</>
		);
	})
	.add("Electron", () => {
		(window as any).isElectron = true;
		return (
			<>
				<b>
					<button
						className={"b"}
						onClick={() => {
							localStorage.removeItem(CheckForUpdatesLocalStorageName);
							window.location.reload();
						}}
					>
						Clean sessie
					</button>
					There nothing shown yet, only if the api returns a error code
				</b>
				<HealthCheckForUpdates />
			</>
		);
	});
