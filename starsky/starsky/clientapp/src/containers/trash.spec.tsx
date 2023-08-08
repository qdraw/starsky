import { render } from "@testing-library/react";
import * as useFetch from "../hooks/use-fetch";
import { newIArchive } from "../interfaces/IArchive";
import { newIConnectionDefault } from "../interfaces/IConnectionDefault";
import Trash from "./trash";

describe("Trash", () => {
  it("renders", () => {
    render(<Trash {...newIArchive()} />);
  });

  it("check if warning exist with no items in the list", () => {
    // usage ==> import * as useFetch from '../hooks/use-fetch';
    const spyGet = jest
      .spyOn(useFetch, "default")
      .mockImplementationOnce(() => {
        return newIConnectionDefault();
      });

    jest.spyOn(window, "scrollTo").mockImplementationOnce(() => {});

    const container = render(
      <Trash
        {...newIArchive()}
        colorClassActiveList={[]}
        colorClassUsage={[]}
        fileIndexItems={[]}
      />
    );
    expect(container.container.innerHTML).toContain("warning-box");

    expect(spyGet).toBeCalled();
  });
});
