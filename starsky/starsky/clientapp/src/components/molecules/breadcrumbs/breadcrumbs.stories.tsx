import React from "react";
import Breadcrumb from "./breadcrumbs";

export default {
  title: "components/molecules/breadcrumbs"
};

export const Default = () => {
  const breadcrumbs = ["/", "/test"];
  return <Breadcrumb subPath="/test/01" breadcrumb={breadcrumbs} />;
};

Default.story = {
  name: "default"
};
