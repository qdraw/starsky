import { IMedia, newIMedia } from './IMedia';

describe("IMedia", () => {
  it("newIMedia", () => {
    var media = newIMedia();
    expect(media).toStrictEqual({} as IMedia);
  });
});