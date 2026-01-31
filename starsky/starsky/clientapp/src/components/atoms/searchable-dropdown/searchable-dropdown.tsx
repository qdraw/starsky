import { FormEvent, FunctionComponent, useEffect, useRef, useState } from "react";
import { DropdownResult, ISearchableDropdownProps } from "./ISearableDropdownProps";

const SearchableDropdown: FunctionComponent<ISearchableDropdownProps> = ({
  fetchResults,
  defaultItems = [],
  placeholder = "Search...",
  defaultValue = "",
  maxResults = 10,
  onSelect,
  className = "",
  isLoading = false,
  noResultsText = "No results found"
}) => {
  const [query, setQuery] = useState(defaultValue);
  const [results, setResults] = useState<DropdownResult[]>([]);
  const [isOpen, setIsOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(-1);
  const inputRef = useRef<HTMLInputElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Fetch results when query changes
  useEffect(() => {
    const fetchData = async () => {
      if (!query.trim()) {
        setResults([]);
        return;
      }

      setLoading(true);
      try {
        const data = await fetchResults(query);
        setResults(data.slice(0, maxResults));
        setSelectedIndex(-1);
      } catch (error) {
        console.error("Error fetching dropdown results:", error);
        setResults([]);
      } finally {
        setLoading(false);
      }
    };

    const timeoutId = setTimeout(fetchData, 300); // Debounce search
    return () => clearTimeout(timeoutId);
  }, [query, fetchResults, maxResults]);

  // Handle click outside to close dropdown
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as HTMLElement;
      if (dropdownRef.current && !dropdownRef.current.contains(target)) {
        setIsOpen(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  // Handle keyboard navigation
  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    const items = results.length > 0 ? results : defaultItems;

    switch (e.key) {
      case "ArrowDown":
        e.preventDefault();
        setSelectedIndex((prev) => (prev < items.length - 1 ? prev + 1 : prev));
        setIsOpen(true);
        break;
      case "ArrowUp":
        e.preventDefault();
        setSelectedIndex((prev) => (prev > 0 ? prev - 1 : -1));
        break;
      case "Enter":
        e.preventDefault();
        if (selectedIndex >= 0) {
          if (results.length > 0) {
            handleSelectItem(results[selectedIndex]);
          } else {
            handleSelectItem({
              id: defaultItems[selectedIndex].value,
              displayName: defaultItems[selectedIndex].label
            });
          }
        }
        break;
      case "Escape":
        setIsOpen(false);
        setSelectedIndex(-1);
        break;
    }
  };

  const getDisplayText = (item: DropdownResult): string => {
    return typeof item === "string" ? "" : item.displayName;
  };

  const getAltText = (item: DropdownResult): string => {
    return typeof item === "string" ? "" : (item.altText ?? "");
  };

  const getSelectValue = (item: DropdownResult): string => {
    return typeof item === "string" ? item : item.id;
  };

  const handleSelectItem = (item: DropdownResult) => {
    const displayText = getDisplayText(item);
    const selectValue = getSelectValue(item);
    setQuery(displayText);
    setIsOpen(false);
    setSelectedIndex(-1);
    onSelect?.(selectValue, displayText);
  };

  const handleFormSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (selectedIndex >= 0) {
      if (results.length > 0) {
        handleSelectItem(results[selectedIndex]);
      } else {
        handleSelectItem({
          id: defaultItems[selectedIndex].value,
          displayName: defaultItems[selectedIndex].label
        });
      }
    } else if (query.trim()) {
      onSelect?.(query, "");
      setIsOpen(false);
    }
  };

  const displayItems =
    results.length > 0
      ? results
      : defaultItems.map((item) => ({ id: item.value, displayName: item.label }));

  return (
    <div
      ref={dropdownRef}
      className={`searchable-dropdown ${className}`}
      data-test="searchable-dropdown"
    >
      <form onSubmit={handleFormSubmit} className="searchable-dropdown__form">
        <input
          ref={inputRef}
          type="text"
          value={query}
          onChange={(e) => {
            setQuery(e.target.value);
            setIsOpen(true);
          }}
          onFocus={() => setIsOpen(true)}
          onKeyDown={handleKeyDown}
          placeholder={placeholder}
          className="searchable-dropdown__input"
          autoComplete="off"
          data-test="searchable-dropdown-input"
        />
        {(loading || isLoading) && <span className="searchable-dropdown__loading">...</span>}
      </form>

      {isOpen && displayItems.length > 0 && (
        <ul className="searchable-dropdown__list" data-test="searchable-dropdown-list">
          {displayItems.map((item, index) => {
            const displayText = getDisplayText(item);
            const altText = getAltText(item);
            const itemId = typeof item === "string" ? item : item.id;
            return (
              <li
                key={`${itemId}-${index}`}
                className={`searchable-dropdown__item ${
                  index === selectedIndex ? "searchable-dropdown__item--selected" : ""
                }`}
                data-test={`searchable-dropdown-item-${itemId}`}
              >
                <button
                  type="button"
                  className="searchable-dropdown__button"
                  onClick={() => handleSelectItem(item)}
                  onMouseEnter={() => setSelectedIndex(index)}
                >
                  <span>{displayText}</span>
                  {altText && <span className="searchable-dropdown__alt-text">{altText}</span>}
                </button>
              </li>
            );
          })}
        </ul>
      )}

      {isOpen &&
        displayItems.length === 0 &&
        query.trim() &&
        !loading &&
        query.trim().length >= 3 && (
          <div
            className="searchable-dropdown__no-results"
            data-test="searchable-dropdown-no-results"
          >
            {noResultsText}
          </div>
        )}
    </div>
  );
};

export default SearchableDropdown;
