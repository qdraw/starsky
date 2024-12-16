import { render, RenderResult } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import * as DetailView from "../containers/detailview/detailview";
import * as useDetailViewContext from "../contexts/detailview-context";
import { mountReactHook } from "../hooks/___tests___/test-hook";
import { useSocketsEventName } from "../hooks/realtime/use-sockets.const";
import { IDetailView, newDetailView } from "../interfaces/IDetailView";
import { newIFileIndexItem } from "../interfaces/IFileIndexItem";
import DetailViewWrapper, { DetailViewEventListenerUseEffect } from "./detailview-wrapper";

describe("DetailViewWrapper", () => {
  it("renders", () => {
    render(
      <MemoryRouter>
        <DetailViewWrapper {...newDetailView()} />
      </MemoryRouter>
    );
  });

  describe("with mount", () => {
    it("check if DetailView is mounted", () => {
      const args = { ...newDetailView() } as IDetailView;
      const detailView = jest.spyOn(DetailView, "default").mockImplementationOnce(() => {
        return <></>;
      });

      const component = render(<DetailViewWrapper {...args} />);
      expect(detailView).toHaveBeenCalled();
      component.unmount();
    });

    it("check if dispatch is called", () => {
      const contextValues = {
        state: { fileIndexItem: newIFileIndexItem() },
        dispatch: jest.fn()
      } as unknown as useDetailViewContext.IDetailViewContext;

      jest
        .spyOn(useDetailViewContext, "useDetailViewContext")
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues)
        .mockImplementationOnce(() => contextValues);

      const args = {
        ...newDetailView(),
        fileIndexItem: newIFileIndexItem()
      } as IDetailView;

      const detailView = jest.spyOn(DetailView, "default").mockImplementationOnce(() => {
        return <></>;
      });

      const component = render(<DetailViewWrapper {...args} />);

      expect(contextValues.dispatch).toHaveBeenCalled();
      expect(detailView).toHaveBeenCalled();

      component.unmount();
    });
  });

  describe("no context", () => {
    it("[detail view] No context if used", () => {
      const contextValues = {
        state: null,
        dispatch: jest.fn()
      } as unknown as useDetailViewContext.IDetailViewContext;

      jest
        .spyOn(useDetailViewContext, "useDetailViewContext")
        .mockReset()
        .mockImplementationOnce(() => contextValues);

      const args = { ...newDetailView() } as IDetailView;
      const component = render(<DetailViewWrapper {...args} />);

      expect(component.container.innerHTML).toBe("");
      component.unmount();
    });
  });

  describe("DetailViewEventListenerUseEffect", () => {
    const { location } = window;
    /**
     * Mock the location feature
     * @see: https://wildwolf.name/jest-how-to-mock-window-location-href/
     */
    beforeAll(() => {
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      delete window.location;
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      window.location = {
        search: "/?f=/test.jpg"
      };
    });

    afterAll((): void => {
      window.location = location;
    });

    it("Check if event is received", () => {
      const dispatch = jest.fn();
      document.body.innerHTML = "";
      const result = mountReactHook(
        DetailViewEventListenerUseEffect as (...args: unknown[]) => unknown,
        [dispatch]
      );

      const detail = {
        data: [
          {
            ...newIFileIndexItem()
            // should ignore this one
          },
          {
            colorclass: undefined,
            ...newIFileIndexItem(),
            filePath: "/test.jpg",
            type: "update"
          }
        ]
      };
      const event = new CustomEvent(useSocketsEventName, {
        detail
      });

      document.body.dispatchEvent(event);

      expect(dispatch).toHaveBeenCalled();
      expect(dispatch).toHaveBeenCalledWith(detail.data[1]);

      const element = result.componentMount as unknown as RenderResult;
      element.unmount();
    });
  });
});
