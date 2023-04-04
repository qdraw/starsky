import React from "react";
import SearchPagination from "./search-pagination";

export default {
  title: "components/molecules/search-pagination"
};

export const Default = () => {
  return <SearchPagination lastPageNumber={2} />;
};

Default.story = {
  name: "default"
};
