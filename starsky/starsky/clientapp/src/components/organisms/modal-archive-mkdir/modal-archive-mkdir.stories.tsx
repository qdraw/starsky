import { storiesOf } from "@storybook/react";
import React from "react";
import ModalArchiveMkdir from "./modal-archive-mkdir";

storiesOf("components/organisms/modal-archive-mkdir", module).add(
	"default",
	() => {
		return (
			<ModalArchiveMkdir
				state={{} as any}
				dispatch={() => {}}
				isOpen={true}
				handleExit={() => {}}
			></ModalArchiveMkdir>
		);
	}
);
