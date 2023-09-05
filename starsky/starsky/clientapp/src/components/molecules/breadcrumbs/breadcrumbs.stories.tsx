import { MemoryRouter } from "react-router-dom";
import Breadcrumb from "./breadcrumbs";

export default {
  title: "components/molecules/breadcrumbs"
};

export const Default = () => {
  const breadcrumbs = ["/", "/test"];
  return (
    <MemoryRouter>
      <Breadcrumb subPath="/test/01" breadcrumb={breadcrumbs} />
    </MemoryRouter>
  );
};

Default.story = {
  name: "default"
};
