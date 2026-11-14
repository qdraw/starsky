import { render, screen } from "@testing-library/react";
import { ImageFormat } from "../../../interfaces/IFileIndexItem";
import DetailViewPlayer from "./detail-view-player";

describe("DetailViewPlayer", () => {
  it("renders the video player for mp4", () => {
    render(<DetailViewPlayer imageFormat={ImageFormat.mp4} />);
    expect(screen.getByTestId("video")).toBeTruthy();
  });

  it("renders the audio player for m4a", () => {
    render(<DetailViewPlayer imageFormat={ImageFormat.m4a} />);
    expect(screen.getByTestId("audio-figure")).toBeTruthy();
  });

  it("renders nothing for other formats", () => {
    const { container } = render(<DetailViewPlayer imageFormat={ImageFormat.jpg} />);
    expect(container.innerHTML).toBe("");
  });
});
