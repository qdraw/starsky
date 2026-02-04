import { MemoryRouter } from "react-router-dom";
import SearchPagination from "./search-pagination";

export default {
  title: "components/molecules/search-pagination"
};

export const Default = () => {
  return (
    <MemoryRouter>
      <SearchPagination lastPageNumber={2} />
    </MemoryRouter>
  );
};

Default.storyName = "default";
