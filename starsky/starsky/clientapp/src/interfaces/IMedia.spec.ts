import { IMedia, newIMedia } from "./IMedia";

describe("IMedia", () => {
  it("newIMedia", () => {
    const media = newIMedia();
    expect(media).toStrictEqual({} as IMedia);
  });
});
