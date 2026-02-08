import React, { useEffect, useState } from "react";
import { UrlQuery } from "../../../shared/url/url-query";
import FormControl from "../../atoms/form-control/form-control";

interface ITagAutocompleteProps {
  name: string;
  className?: string;
  contentEditable: boolean;
  spellcheck?: boolean;
  reference?: React.RefObject<HTMLDivElement>;
  onInput?: (
    event: React.ChangeEvent<HTMLDivElement> | React.KeyboardEvent<HTMLDivElement>
  ) => void;
  children?: React.ReactNode;
  onBlur?(event: React.ChangeEvent<HTMLDivElement>): void;
  maxlength?: number;
}

function normalizeTagText(value: string): string {
  // Replace Unicode non-breaking space (U+00A0) with normal space
  return value.replaceAll(/\u00a0/g, " ");
}

export function setCaretToEnd(element: HTMLDivElement) {
  const range = document.createRange();
  range.selectNodeContents(element);
  range.collapse(false);
  const selection = globalThis.getSelection();
  if (selection) {
    selection.removeAllRanges();
    selection.addRange(range);
  }
}

function getTagQuery(value: string): string {
  const cleanValue = normalizeTagText(value);
  const parts = cleanValue.split(",");
  return parts.at(-1)?.trim() ?? "";
}

const TagAutocomplete: React.FunctionComponent<ITagAutocompleteProps> = (props) => {
  const [tagSuggest, setTagSuggest] = useState<string[]>([]);
  const [tagQuery, setTagQuery] = useState("");
  const [tagSuggestOpen, setTagSuggestOpen] = useState(false);
  const [tagKeyDownIndex, setTagKeyDownIndex] = useState(-1);

  function applyTagSuggestion(value: string) {
    const element = props.reference?.current;
    if (!element) return;

    const currentText = normalizeTagText(element.textContent ?? "");
    const parts = currentText
      .split(",")
      .slice(0, -1)
      .map((item) => item.trim())
      .filter((item) => item.length > 0);

    const newText = parts.length > 0 ? `${parts.join(", ")}, ${value}, ` : `${value}, `;

    element.textContent = newText;
    setCaretToEnd(element);
    setTagQuery("");
    setTagSuggest([]);
    setTagSuggestOpen(false);
    setTagKeyDownIndex(-1);

    props.onInput?.({
      currentTarget: element
    } as React.ChangeEvent<HTMLDivElement>);
  }

  function handleTagInput(event: React.ChangeEvent<HTMLDivElement>) {
    props.onInput?.(event);
    const query = getTagQuery(event.currentTarget.textContent ?? "");
    setTagQuery(query);
    if (query.length === 0) {
      setTagSuggest([]);
      setTagKeyDownIndex(-1);
      setTagSuggestOpen(false);
    } else {
      setTagSuggestOpen(true);
    }
  }

  function handleTagKeyDown(event: React.KeyboardEvent<HTMLDivElement>) {
    if (!tagSuggestOpen || tagSuggest.length === 0) return;

    if (event.key === "ArrowDown") {
      event.preventDefault();
      const nextIndex = Math.min(tagSuggest.length - 1, tagKeyDownIndex + 1);
      setTagKeyDownIndex(nextIndex);
      return;
    }

    if (event.key === "ArrowUp") {
      event.preventDefault();
      const nextIndex = Math.max(0, tagKeyDownIndex - 1);
      setTagKeyDownIndex(nextIndex);
      return;
    }

    if (event.key === "Enter") {
      if (tagKeyDownIndex >= 0) {
        event.preventDefault();
        applyTagSuggestion(tagSuggest[tagKeyDownIndex]);
      }
      return;
    }

    if (event.key === "Escape") {
      setTagSuggestOpen(false);
      setTagKeyDownIndex(-1);
    }
  }

  useEffect(() => {
    if (!tagSuggestOpen) return;
    const query = tagQuery.trim();
    if (query.length === 0) return;

    const controller = new AbortController();
    const timeout = setTimeout(() => {
      const url = `${new UrlQuery().UrlSearchSuggestApi(encodeURIComponent(query), false)}`;

      fetch(url, { signal: controller.signal })
        .then((response) => response.json())
        .then((data) => {
          if (Array.isArray(data)) {
            setTagSuggest(data);
            return;
          }
          setTagSuggest([]);
        })
        .catch(() => {
          setTagSuggest([]);
        });
    }, 200);

    return () => {
      controller.abort();
      clearTimeout(timeout);
    };
  }, [tagQuery, tagSuggestOpen]);

  return (
    <div className="tag-autocomplete">
      <FormControl
        spellcheck={props.spellcheck}
        onInput={handleTagInput}
        onKeyDown={handleTagKeyDown}
        onFocus={() => setTagSuggestOpen(true)}
        onBlur={(event) => {
          setTimeout(() => setTagSuggestOpen(false), 150);
          props.onBlur?.(event);
        }}
        reference={props.reference}
        maxlength={props.maxlength}
        name={props.name}
        className={props.className}
        contentEditable={props.contentEditable}
      >
        {props.children}
      </FormControl>

      {tagSuggestOpen && tagSuggest.length > 0 ? (
        <div className="tag-suggest-list" data-test="tag-suggest-list">
          {tagSuggest.map((suggestion, index) => (
            <button
              key={suggestion}
              data-selected={index === tagKeyDownIndex}
              className={
                index === tagKeyDownIndex
                  ? "tag-suggest-item tag-suggest-item--active"
                  : "tag-suggest-item"
              }
              onMouseDown={(event) => {
                event.preventDefault();
                applyTagSuggestion(suggestion);
              }}
            >
              {suggestion}
            </button>
          ))}
        </div>
      ) : null}
    </div>
  );
};

export default TagAutocomplete;
