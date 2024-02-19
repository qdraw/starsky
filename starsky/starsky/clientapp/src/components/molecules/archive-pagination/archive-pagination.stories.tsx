import { MemoryRouter } from "react-router-dom";
import { newIRelativeObjects } from "../../../interfaces/IDetailView";
import ArchivePagination from "./archive-pagination";

export default {
  title: "components/molecules/archive-pagination"
};

export const Default = () => {
  return (
    <MemoryRouter>
      <ArchivePagination relativeObjects={newIRelativeObjects()} />
    </MemoryRouter>
  );
};

Default.storyName = "default";
