import React, { useMemo, useRef } from "react";
import { IUrl } from "../../../interfaces/IUrl";
import localization from "../../../localization/localization.json";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
import SearchableDropdown from "../../atoms/searchable-dropdown";
import { DropdownResult } from "../../atoms/searchable-dropdown/ISearableDropdownProps";
import TagAutocomplete from "../tag-autocomplete/tag-autocomplete";
import useGlobalSettings from "../../../hooks/use-global-settings";

interface ISharedStructuredFilterProps {
  urlObject: IUrl;
  onChange: (nextUrl: IUrl) => void;
}

function getKeywordsText(keywords?: string[]): string {
  if (!keywords || keywords.length === 0) {
    return "";
  }
  return keywords.join(", ");
}

function parseKeywords(text: string): string[] {
  return text
    .split(",")
    .map((item) => item.trim())
    .filter((item) => item.length >= 1);
}

const SharedStructuredFilter: React.FunctionComponent<ISharedStructuredFilterProps> = ({
  urlObject,
  onChange
}) => {
  const settings = useGlobalSettings();
  const language = new Language(settings.language);

  const keywordsRef = useRef<HTMLDivElement>(null);

  const MessageSearchOrSelect = language.key(localization.MessageSearchOrSelect);
  const MessageNoResultsFound = language.key(localization.MessageNoResultsFound);
  const MessageDate = language.key(localization.MessageDate);
  const MessageFilters = language.key(localization.MessageFilters);
  const MessageShowFilters = language.key(localization.MessageShowFilters);
  const MessageHideFilters = language.key(localization.MessageHideFilters);
  const MessageFileType = language.key(localization.MessageFileType);
  const MessageCamera = language.key(localization.MessageCamera);
  const MessageKeywords = language.key(localization.MessageKeywords);
  const MessageColorClassColourResetFilter = language.key(localization.ColorClassColourResetFilter);
  const MessageItemsOutsideFilter = language.key(localization.MessageItemsOutsideFilter);

  const hasActiveFilters =
    !!urlObject.imageFormat ||
    !!urlObject.camera ||
    (urlObject.keywords?.length ?? 0) >= 1 ||
    !!urlObject.dateFrom ||
    !!urlObject.dateTo;

  const isOpen = urlObject.filtersOpen === true || hasActiveFilters;

  const keywordsText = useMemo(() => {
    return getKeywordsText(urlObject.keywords);
  }, [urlObject.keywords]);

  const updateFilters = (next: IUrl) => {
    delete next.p;
    onChange(next);
  };

  const clearFilters = () => {
    const next = { ...urlObject };
    delete next.imageFormat;
    delete next.camera;
    delete next.keywords;
    delete next.dateFrom;
    delete next.dateTo;
    next.filtersOpen = false;
    updateFilters(next);
  };

  const fetchCameraSuggest = async (query: string): Promise<DropdownResult[]> => {
    if (!query || query.trim().length < 2) {
      return [];
    }

    const response = await fetch(new UrlQuery().UrlSearchSuggestCameraApi(encodeURIComponent(query)));
    const responseJson = await response.json();
    if (!Array.isArray(responseJson)) {
      return [];
    }

    return responseJson.map((item) => ({ id: item, displayName: item }));
  };

  if (!isOpen && !hasActiveFilters) {
    return (
      <div className="structured-filter structured-filter--collapsed">
        <button
          type="button"
          data-test="shared-filter-toggle"
          className="btn btn--default"
          onClick={() => updateFilters({ ...urlObject, filtersOpen: true })}
        >
          {MessageFilters}
        </button>
      </div>
    );
  }

  return (
    <div className="structured-filter" data-test="shared-filter-panel">
      <div className="structured-filter__header">
        <button
          type="button"
          data-test="shared-filter-toggle"
          className="btn"
          onClick={() => updateFilters({ ...urlObject, filtersOpen: !isOpen })}
        >
          {isOpen ? MessageHideFilters : MessageShowFilters}
        </button>

        {hasActiveFilters ? (
          <button
            type="button"
            data-test="shared-filter-reset"
            className="btn btn--default"
            onClick={clearFilters}
          >
            {MessageColorClassColourResetFilter}
          </button>
        ) : null}
      </div>

      {isOpen ? (
        <>
          <div className="structured-filter__grid">
            <div className="structured-filter__group">
              <label>{MessageFileType}</label>
              <div className="structured-filter__chips">
                {[
                  { key: "RAW", value: "raw" },
                  { key: "JPG", value: "jpg" },
                  { key: "PNG", value: "png" }
                ].map((item) => (
                  <button
                    type="button"
                    key={item.key}
                    data-test={`shared-filter-filetype-${item.value}`}
                    className={
                      urlObject.imageFormat?.toLowerCase() === item.value
                        ? "btn btn--default active"
                        : "btn"
                    }
                    onClick={() => {
                      const next = { ...urlObject };
                      if (urlObject.imageFormat?.toLowerCase() === item.value) {
                        delete next.imageFormat;
                      } else {
                        next.imageFormat = item.value;
                      }
                      next.filtersOpen = true;
                      updateFilters(next);
                    }}
                  >
                    {item.key}
                  </button>
                ))}
              </div>
            </div>

            <div className="structured-filter__group">
              <label>{MessageCamera}</label>
              <SearchableDropdown
                key={urlObject.camera ?? "camera-empty"}
                fetchResults={fetchCameraSuggest}
                placeholder={MessageSearchOrSelect}
                noResultsText={MessageNoResultsFound}
                defaultValue={urlObject.camera ?? ""}
                onSelect={(id) => {
                  const next: IUrl = { ...urlObject, filtersOpen: true };
                  next.camera = id;
                  if (!id) {
                    delete next.camera;
                  }
                  updateFilters(next);
                }}
              />
            </div>

            <div className="structured-filter__group">
              <label>{MessageKeywords}</label>
              <TagAutocomplete
                key={keywordsText}
                spellcheck={true}
                reference={keywordsRef}
                onInput={(event) => {
                  const text = event.currentTarget.textContent ?? "";
                  const keywords = parseKeywords(text);
                  const next: IUrl = { ...urlObject, filtersOpen: true };
                  next.keywords = keywords;
                  if (keywords.length === 0) {
                    delete next.keywords;
                  }
                  updateFilters(next);
                }}
                name="keywords"
                contentEditable={true}
                data-test="shared-filter-keywords"
              >
                {keywordsText}
              </TagAutocomplete>
            </div>

            <div className="structured-filter__group structured-filter__group--dates">
              <label>{MessageDate}</label>
              <div className="structured-filter__dates">
                <input
                  type="date"
                  data-test="shared-filter-date-from"
                  value={urlObject.dateFrom ?? ""}
                  onChange={(event) => {
                    const value = event.currentTarget.value;
                    const next: IUrl = { ...urlObject, filtersOpen: true };
                    next.dateFrom = value;
                    if (!value) {
                      delete next.dateFrom;
                    }
                    updateFilters(next);
                  }}
                />
                <input
                  type="date"
                  data-test="shared-filter-date-to"
                  value={urlObject.dateTo ?? ""}
                  onChange={(event) => {
                    const value = event.currentTarget.value;
                    const next: IUrl = { ...urlObject, filtersOpen: true };
                    next.dateTo = value;
                    if (!value) {
                      delete next.dateTo;
                    }
                    updateFilters(next);
                  }}
                />
              </div>
            </div>
          </div>

          {hasActiveFilters ? (
            <div className="structured-filter__active-message">{MessageItemsOutsideFilter}</div>
          ) : null}
        </>
      ) : null}
    </div>
  );
};

export default SharedStructuredFilter;
