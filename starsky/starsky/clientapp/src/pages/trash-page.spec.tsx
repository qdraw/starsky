import { render } from "@testing-library/react";
import * as Preloader from "../components/atoms/preloader/preloader";
import * as ApplicationException from "../components/organisms/application-exception/application-exception";
import * as ArchiveContextWrapper from "../contexts-wrappers/archive-wrapper";
import * as useSearchList from "../hooks/use-searchlist";
import { ISearchList } from "../hooks/use-searchlist";
import { newIArchive } from "../interfaces/IArchive";
import { PageType } from "../interfaces/IDetailView";
import { TrashPage } from "./trash-page";

describe("TrashPage", () => {
  it("default error case", () => {
    jest.spyOn(useSearchList, "default").mockImplementationOnce(() => {
      return {} as ISearchList;
    });

    const error = render(<TrashPage />);

    expect(error.container.innerHTML).toBe("Something went wrong");
  });

  it("check if context is called", () => {
    const contextSpy = jest.spyOn(ArchiveContextWrapper, "default").mockImplementationOnce(() => {
      return <></>;
    });

    jest.spyOn(useSearchList, "default").mockImplementationOnce(() => {
      return {
        archive: newIArchive(),
        pageType: PageType.Trash
      } as ISearchList;
    });

    const trashPage = render(<TrashPage />);

    expect(contextSpy).toHaveBeenCalled();

    trashPage.unmount();
  });

  it("Internal Error null", () => {
    jest
      .spyOn(useSearchList, "default")
      .mockImplementationOnce(() => null as unknown as ISearchList);

    const component = render(<TrashPage />);
    expect(component.container.innerHTML).toBe("Something went wrong");

    component.unmount();
  });

  it("Internal Error plain object", () => {
    jest
      .spyOn(useSearchList, "default")
      .mockImplementationOnce(() => ({}) as unknown as ISearchList);

    const component = render(<TrashPage />);
    expect(component.container.innerHTML).toBe("Something went wrong");

    component.unmount();
  });

  it("App exception", () => {
    const applicationExceptionSpy = jest
      .spyOn(ApplicationException, "default")
      .mockImplementationOnce(() => null);

    jest.spyOn(useSearchList, "default").mockImplementationOnce(() => {
      return {
        pageType: PageType.ApplicationException
      } as ISearchList;
    });

    const component = render(<TrashPage />);

    expect(applicationExceptionSpy).toHaveBeenCalled();

    component.unmount();
  });

  it("Loading should display preloader", () => {
    const preloaderSpy = jest.spyOn(Preloader, "default");

    jest.spyOn(useSearchList, "default").mockImplementationOnce(() => {
      return {
        archive: {},
        pageType: PageType.Loading
      } as ISearchList;
    });

    jest.spyOn(ArchiveContextWrapper, "default").mockImplementationOnce(() => {
      return <></>;
    });

    const component = render(<TrashPage />);

    expect(preloaderSpy).toHaveBeenCalled();

    component.unmount();
  });
});
