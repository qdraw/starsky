import { render } from "@testing-library/react";
import React from "react";
import Breadcrumb from "./breadcrumbs";

describe("Breadcrumb", () => {
  it("renders", () => {
    render(<Breadcrumb subPath="/" breadcrumb={["/"]} />);
  });

  it("disabled", () => {
    var wrapper = render(<Breadcrumb subPath="" breadcrumb={[]} />);

    const spans = wrapper.queryAllByTestId("breadcrumb-span");
    expect(spans).toHaveLength(0);
  });

  it("check Length for breadcrumbs", () => {
    const breadcrumbs = ["/", "/test"];
    const wrapper = render(
      <Breadcrumb subPath="/test/01" breadcrumb={breadcrumbs} />
    );
    const spans = wrapper.queryAllByTestId("breadcrumb-span");
    expect(spans).toHaveLength(2);
  });

  it("check 3 Length for breadcrumbs", () => {
    const breadcrumbs = ["/", "/test", "/01"];
    const wrapper = render(
      <Breadcrumb subPath="/test/01/01" breadcrumb={breadcrumbs} />
    );
    const spans = wrapper.queryAllByTestId("breadcrumb-span");
    expect(spans).toHaveLength(3);
  });
});
