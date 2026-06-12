import { Meta } from "@storybook/react-vite";
import { IDetailView } from "../../../interfaces/IDetailView.ts";
import MenuOptionRotateImage90 from "./menu-option-rotate-image-90.tsx";

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
    setIsLoading={() => console.log("setIsLoading")}
    dispatch={() => console.log("dispatch")}
    isMarkedAsDeleted={false}
    isReadOnly={false}
  />
);

export const Default = Template.bind({});
