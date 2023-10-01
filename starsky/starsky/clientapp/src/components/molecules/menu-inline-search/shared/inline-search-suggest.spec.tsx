import { fireEvent, render } from "@testing-library/react";
import { IConnectionDefault } from "../../../../interfaces/IConnectionDefault";
import InlineSearchSuggest, {
  IInlineSearchSuggestProps
} from "./inline-search-suggest";

describe("inline-search-suggest", () => {
  it("renders", () => {
    const props: IInlineSearchSuggestProps = {
      suggest: [],
      setFormFocus: jest.fn(),
      inputFormControlReference: { current: null },
      featuresResult: {} as IConnectionDefault
    };
    render(<InlineSearchSuggest {...props} />);
  });

  describe("with Context", () => {
    const props: IInlineSearchSuggestProps = {
      suggest: ["query1", "query2", "query3"],
      setFormFocus: jest.fn(),
      inputFormControlReference: {
        current: {
          value: "default text"
        } as any
      },
      featuresResult: { data: { systemTrashEnabled: true }, statusCode: 200 },
      defaultText: "default text",
      callback: jest.fn()
    };

    it("renders default menu items when suggest is empty", () => {
      const { getByText } = render(
        <InlineSearchSuggest {...props} suggest={[]} />
      );
      expect(getByText("Home")).toBeTruthy();
      expect(getByText("Photos of this week")).toBeTruthy();
      expect(getByText("Import")).toBeTruthy();
      expect(getByText("Preferences")).toBeTruthy();
      expect(getByText("Logout")).toBeTruthy();
    });

    it("renders search results when suggest is not empty", () => {
      const { getByText } = render(<InlineSearchSuggest {...props} />);
      expect(getByText("query1")).toBeTruthy();
      expect(getByText("query2")).toBeTruthy();
      expect(getByText("query3")).toBeTruthy();
    });

    it("calls Navigate function when search result button is clicked", () => {
      const { getByText } = render(<InlineSearchSuggest {...props} />);
      fireEvent.click(getByText("query1"));
      expect(props.callback).toHaveBeenCalledWith("query1");
    });

    it("hides trash menu item when system trash is enabled", () => {
      const props2 = {
        suggest: [],
        setFormFocus: jest.fn(),
        inputFormControlReference: {
          current: {
            value: "default text"
          } as any
        },
        featuresResult: {
          data: { systemTrashEnabled: true },
          statusCode: 200
        } as IConnectionDefault,
        defaultText: "default text",
        callback: jest.fn()
      };
      const { queryByText } = render(<InlineSearchSuggest {...props2} />);
      expect(queryByText("Trash")).toBeNull();
    });

    it("hides logout menu item when useLocalDesktopUi is enabled", () => {
      const props2 = {
        suggest: [],
        setFormFocus: jest.fn(),
        inputFormControlReference: {
          current: {
            value: "default text"
          } as any
        },
        featuresResult: {
          data: { useLocalDesktopUi: true },
          statusCode: 200
        } as IConnectionDefault,
        defaultText: "default text",
        callback: jest.fn()
      };
      const { queryByText } = render(<InlineSearchSuggest {...props2} />);
      expect(queryByText("Logout")).toBeNull();
    });
  });
});
