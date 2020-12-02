import { storiesOf } from "@storybook/react";
import React from "react";
import PreferencesUsername from "./preferences-username";

storiesOf("components/organisms/preferences-username", module).add(
	"default",
	() => {
		return <PreferencesUsername />;
	}
);
