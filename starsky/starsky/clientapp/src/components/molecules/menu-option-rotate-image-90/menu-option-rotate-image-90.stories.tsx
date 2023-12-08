import { Meta } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import MenuOptionRotateImage90 from "./menu-option-rotate-image-90.tsx";
import { IDetailView } from "../../../interfaces/IDetailView.ts";

export default {
  component: MenuOptionRotateImage90,
  title: "Menu/MenuOptionRotateImage90",
  decorators: [
    (Story) => (
      <div style={{ padding: "20px", maxWidth: "300px" }}>
        <Story />
      </div>
    )
  ]
} as Meta;

const Template = () => (
  <MenuOptionRotateImage90
    state={{} as IDetailView}
    setIsLoading={action("setIsLoading")}
    dispatch={() => {}}
    isMarkedAsDeleted={false}
    isReadOnly={false}
  />
);

export const Default = Template.bind({});
