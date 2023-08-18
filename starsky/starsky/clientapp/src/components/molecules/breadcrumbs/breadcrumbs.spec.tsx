import { render, screen } from "@testing-library/react";
import * as Link from "../../atoms/link/link";
import Breadcrumb from "./breadcrumbs";

describe("Breadcrumb", () => {
  it("renders", () => {
    jest.spyOn(Link, "default").mockImplementationOnce(() => <a></a>);
    render(<Breadcrumb subPath="/" breadcrumb={["/"]} />);
  });

  it("disabled", () => {
    const wrapper = render(<Breadcrumb subPath="" breadcrumb={[]} />);

    const spans = screen.queryAllByTestId("breadcrumb-span");
    expect(spans).toHaveLength(0);

    wrapper.unmount();
  });

  it("check Length for breadcrumbs", () => {
    jest
      .spyOn(Link, "default")
      .mockImplementationOnce(() => <></>)
      .mockImplementationOnce(() => <></>);

    const breadcrumbs = ["/", "/test"];
    const wrapper = render(
      <Breadcrumb subPath="/test/01" breadcrumb={breadcrumbs} />
    );
    const spans = screen.queryAllByTestId("breadcrumb-span");
    expect(spans).toHaveLength(2);

    wrapper.unmount();
  });

  it("check 3 Length for breadcrumbs", () => {
    jest
      .spyOn(Link, "default")
      .mockImplementationOnce(() => <></>)
      .mockImplementationOnce(() => <></>)
      .mockImplementationOnce(() => <></>);

    const breadcrumbs = ["/", "/test", "/01"];
    const wrapper = render(
      <Breadcrumb subPath="/test/01/01" breadcrumb={breadcrumbs} />
    );
    const spans = screen.queryAllByTestId("breadcrumb-span");
    expect(spans).toHaveLength(3);

    wrapper.unmount();
  });
});
