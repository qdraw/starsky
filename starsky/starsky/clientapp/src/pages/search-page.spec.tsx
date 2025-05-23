import { render } from "@testing-library/react";
import * as Preloader from "../components/atoms/preloader/preloader";
import * as ApplicationException from "../components/organisms/application-exception/application-exception";
import * as ArchiveContextWrapper from "../contexts-wrappers/archive-wrapper";
import * as useSearchList from "../hooks/use-searchlist";
import { ISearchList } from "../hooks/use-searchlist";
import { PageType } from "../interfaces/IDetailView";
import { SearchPage } from "./search-page";

describe("SearchPage", () => {
  it("default check if MenuSearch + context is called", () => {
    jest
      .spyOn(window, "scrollTo")
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {});

    const contextSpy = jest.spyOn(ArchiveContextWrapper, "default").mockImplementationOnce(() => {
      return <></>;
    });
    const component = render(<SearchPage />);
    expect(contextSpy).toHaveBeenCalled();
    component.unmount();
  });

  it("Internal Error null", () => {
    jest
      .spyOn(useSearchList, "default")
      .mockImplementationOnce(() => null as unknown as ISearchList);

    const component = render(<SearchPage />);
    expect(component.container.innerHTML).toBe("Something went wrong");

    component.unmount();
  });

  it("Internal Error plain object", () => {
    jest
      .spyOn(useSearchList, "default")
      .mockImplementationOnce(() => ({}) as unknown as ISearchList);

    const component = render(<SearchPage />);
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
      } as unknown as ISearchList;
    });

    const component = render(<SearchPage />);

    expect(applicationExceptionSpy).toHaveBeenCalled();

    component.unmount();
  });

  it("Loading should display preloader", () => {
    const preloaderSpy = jest.spyOn(Preloader, "default");

    jest.spyOn(useSearchList, "default").mockImplementationOnce(() => {
      return {
        archive: {},
        pageType: PageType.Loading
      } as unknown as ISearchList;
    });

    jest.spyOn(ArchiveContextWrapper, "default").mockImplementationOnce(() => {
      return <></>;
    });

    const component = render(<SearchPage />);

    expect(preloaderSpy).toHaveBeenCalled();

    component.unmount();
  });
});
