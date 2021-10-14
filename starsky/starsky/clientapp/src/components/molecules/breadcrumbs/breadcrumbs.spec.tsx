import { render } from "@testing-library/react";
import React from "react";
import Breadcrumb from "./breadcrumbs";

describe("Breadcrumb", () => {
  it("renders", () => {
    render(<Breadcrumb subPath="/" breadcrumb={["/"]} />);
  });

  it("disabled", () => {
    var wrapper = render(<Breadcrumb subPath="" breadcrumb={[]} />);
    expect(wrapper.find("span")).toHaveLength(0);
  });

  it("check Length for breadcrumbs", () => {
    const breadcrumbs = ["/", "/test"];
    const wrapper = render(
      <Breadcrumb subPath="/test/01" breadcrumb={breadcrumbs} />
    );
    console.log(wrapper.container.innerHTML);

    expect(wrapper.find("span")).toHaveLength(4);
  });
});
