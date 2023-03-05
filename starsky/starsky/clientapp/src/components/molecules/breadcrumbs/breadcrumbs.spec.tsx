import { render, screen } from "@testing-library/react";
import Breadcrumb from "./breadcrumbs";

describe("Breadcrumb", () => {
  it("renders", () => {
    render(<Breadcrumb subPath="/" breadcrumb={["/"]} />);
  });

  it("disabled", () => {
    const wrapper = render(<Breadcrumb subPath="" breadcrumb={[]} />);

    const spans = screen.queryAllByTestId("breadcrumb-span");
    expect(spans).toHaveLength(0);

    wrapper.unmount();
  });

  it("check Length for breadcrumbs", () => {
    const breadcrumbs = ["/", "/test"];
    const wrapper = render(
      <Breadcrumb subPath="/test/01" breadcrumb={breadcrumbs} />
    );
    const spans = screen.queryAllByTestId("breadcrumb-span");
    expect(spans).toHaveLength(2);

    wrapper.unmount();
  });

  it("check 3 Length for breadcrumbs", () => {
    const breadcrumbs = ["/", "/test", "/01"];
    const wrapper = render(
      <Breadcrumb subPath="/test/01/01" breadcrumb={breadcrumbs} />
    );
    const spans = screen.queryAllByTestId("breadcrumb-span");
    expect(spans).toHaveLength(3);

    wrapper.unmount();
  });
});
