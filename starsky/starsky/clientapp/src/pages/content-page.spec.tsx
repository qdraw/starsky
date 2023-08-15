import { render } from "@testing-library/react";
import * as MediaContent from "../containers/media-content";
import ContentPage from "./content-page";

describe("ContentPage", () => {
  it("default", () => {
    const mediaContentSpy = jest
      .spyOn(MediaContent, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    render(<ContentPage />);
    expect(mediaContentSpy).toBeCalledTimes(0);
  });

  it("with navigate and location", () => {
    const mediaContentSpy = jest
      .spyOn(MediaContent, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    render(<ContentPage />);
    expect(mediaContentSpy).toBeCalled();
  });
});
