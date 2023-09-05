import { MemoryRouter } from "react-router-dom";
import ColorClassFilter from "./color-class-filter";

export default {
  title: "components/molecules/color-class-filter"
};

export const Default = () => {
  return (
    <MemoryRouter>
      <ColorClassFilter
        itemsCount={1}
        subPath={"/test"}
        colorClassActiveList={[1, 2, 3, 4, 5, 6, 7, 8]}
        colorClassUsage={[1, 2, 3, 4, 5, 6, 7, 8]}
        sticky={true}
      ></ColorClassFilter>
    </MemoryRouter>
  );
};

Default.story = {
  name: "default"
};
