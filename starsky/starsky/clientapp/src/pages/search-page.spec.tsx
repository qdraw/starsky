import React from "react";
import * as Preloader from "../components/atoms/preloader/preloader";
import * as ApplicationException from "../components/organisms/application-exception/application-exception";
import * as ArchiveContextWrapper from "../contexts-wrappers/archive-wrapper";
import * as useSearchList from "../hooks/use-searchlist";
import { PageType } from "../interfaces/IDetailView";
import SearchPage from "./search-page";

describe("SearchPage", () => {
  it("default check if MenuSearch + context is called", () => {
    jest
      .spyOn(window, "scrollTo")
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {})
      .mockImplementationOnce(() => {});

    var contextSpy = jest
      .spyOn(ArchiveContextWrapper, "default")
      .mockImplementationOnce(() => {
        return <></>;
      });
    const component = render(<SearchPage>t</SearchPage>);
    expect(contextSpy).toBeCalled();
    component.unmount();
  });

  it("Internal Error null", () => {
    jest
      .spyOn(useSearchList, "default")
      .mockImplementationOnce(() => null as any);

    const component = render(<SearchPage>t</SearchPage>);
    expect(component.html()).toBe("Something went wrong");

    component.unmount();
  });

  it("Internal Error plain object", () => {
    jest
      .spyOn(useSearchList, "default")
      .mockImplementationOnce(() => ({} as any));

    const component = render(<SearchPage>t</SearchPage>);
    expect(component.html()).toBe("Something went wrong");

    component.unmount();
  });

  it("App exception", () => {
    const applicationExceptionSpy = jest
      .spyOn(ApplicationException, "default")
      .mockImplementationOnce(() => null);

    jest.spyOn(useSearchList, "default").mockImplementationOnce(() => {
      return {
        pageType: PageType.ApplicationException
      } as any;
    });

    const component = render(<SearchPage>t</SearchPage>);

    expect(applicationExceptionSpy).toBeCalled();

    component.unmount();
  });

  it("Loading should display preloader", () => {
    const preloaderSpy = jest.spyOn(Preloader, "default");

    jest.spyOn(useSearchList, "default").mockImplementationOnce(() => {
      return {
        archive: {},
        pageType: PageType.Loading
      } as any;
    });

    jest.spyOn(ArchiveContextWrapper, "default").mockImplementationOnce(() => {
      return <></>;
    });

    const component = render(<SearchPage>t</SearchPage>);

    expect(preloaderSpy).toBeCalled();

    component.unmount();
  });
});
