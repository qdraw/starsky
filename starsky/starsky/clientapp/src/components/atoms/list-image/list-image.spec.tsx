import { createEvent, fireEvent, render } from "@testing-library/react";
import React from "react";
import useIntersection from "../../../hooks/use-intersection-observer";
import { ImageFormat } from "../../../interfaces/IFileIndexItem";
import { UrlQuery } from "../../../shared/url-query";
import ListImage from "./list-image";

jest.mock("../../../hooks/use-intersection-observer");

describe("ListImageTest", () => {
  it("renders", () => {
    render(
      <ListImage alt={"alt"} fileHash={"src"} imageFormat={ImageFormat.jpg} />
    );
  });

  it("useIntersection = true", () => {
    (useIntersection as jest.Mock<any>).mockImplementation(() => true);
    var element = render(
      <ListImage fileHash={"test.jpg"} imageFormat={ImageFormat.jpg}>
        test
      </ListImage>
    );

    const img = element.queryAllByTestId(
      "list-image-img"
    )[0] as HTMLImageElement;

    const keyDownEvent = createEvent.load(img, {
      key: "x",
      code: "x"
    });

    fireEvent(img, keyDownEvent);

    expect(img).not.toBeNull();
    expect(img.src).toContain(
      new UrlQuery().UrlThumbnailImage("test.jpg", false)
    );
  });

  it("img-box--error null", () => {
    var element = render(
      <ListImage imageFormat={ImageFormat.jpg} fileHash={"null"} />
    );

    const img = element.queryAllByTestId(
      "list-image-img-error"
    )[0] as HTMLImageElement;

    expect(img).not.toBeNull();
    expect(img.className).toContain("img-box--error");
  });

  it("img-box--error null 2", () => {
    var element = render(
      <ListImage imageFormat={ImageFormat.jpg} fileHash={"null"} />
    );

    const img = element.queryAllByTestId(
      "list-image-img-error"
    )[0] as HTMLImageElement;

    expect(img).not.toBeNull();
    expect(img.className).toContain("img-box--error");
  });
});
