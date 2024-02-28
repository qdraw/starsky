import { fireEvent, render } from "@testing-library/react";
import { IConnectionDefault } from "../../../../interfaces/IConnectionDefault";
import InlineSearchSuggest, { IInlineSearchSuggestProps } from "./inline-search-suggest";

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
      const { getByText } = render(<InlineSearchSuggest {...props} suggest={[]} />);
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

    it("hides logout menu item when useLocalDesktop is enabled", () => {
      const props2 = {
        suggest: [],
        setFormFocus: jest.fn(),
        inputFormControlReference: {
          current: {
            value: "default text"
          } as any
        },
        featuresResult: {
          data: { useLocalDesktop: true },
          statusCode: 200
        } as IConnectionDefault,
        defaultText: "default text",
        callback: jest.fn()
      };
      const { queryByText } = render(<InlineSearchSuggest {...props2} />);
      expect(queryByText("Logout")).toBeNull();
    });
  });

  describe("max suggestions", () => {
    it("renders up to 9 suggestions", () => {
      const suggest = [
        "suggestion1",
        "suggestion2",
        "suggestion3",
        "suggestion4",
        "suggestion5",
        "suggestion6",
        "suggestion7",
        "suggestion8",
        "suggestion9",
        "suggestion10"
      ];
      const { queryAllByTestId } = render(
        <InlineSearchSuggest
          suggest={suggest}
          setFormFocus={jest.fn()}
          inputFormControlReference={{ current: null }}
          featuresResult={{} as IConnectionDefault}
          callback={jest.fn()}
        />
      );
      expect(queryAllByTestId(/^menu-inline-search-suggest-/).length).toBe(9);
    });

    it("renders all suggestions when there are less than 9", () => {
      const suggest = ["suggestion1", "suggestion2", "suggestion3"];
      const { queryAllByTestId } = render(
        <InlineSearchSuggest
          suggest={suggest}
          setFormFocus={jest.fn()}
          inputFormControlReference={{ current: null }}
          featuresResult={{} as IConnectionDefault}
          callback={jest.fn()}
        />
      );
      expect(queryAllByTestId(/^menu-inline-search-suggest-/).length).toBe(3);
    });
  });
});
