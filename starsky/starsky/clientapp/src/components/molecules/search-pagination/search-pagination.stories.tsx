import { storiesOf } from "@storybook/react";
import React from "react";
import SearchPagination from './search-pagination';

storiesOf("components/molecules/search-pagination", module)
  .add("default", () => {
    return <SearchPagination lastPageNumber={2} />
  })
