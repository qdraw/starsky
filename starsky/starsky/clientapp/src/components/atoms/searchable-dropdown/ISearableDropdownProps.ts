export interface IDropdownItem {
  id: string;
  displayName: string;
  altText?: string;
}

export type DropdownResult = string | IDropdownItem;

export interface ISearchableDropdownProps {
  /**
   * Function to fetch results from backend
   * Can return either string[] or IDropdownItem[]
   * Example 1: ["Banana", "Apple"]
   * Example 2: [{"id": "banana", "displayName": "Banana"}, {"id": "apple", "displayName": "Apple"}]
   */
  fetchResults: (query: string) => Promise<DropdownResult[]>;

  /**
   * Default items shown when no search query
   */
  defaultItems?: Array<{ label: string; value: string; altText?: string }>;

  /**
   * Placeholder text for input
   */
  placeholder?: string;

  /**
   * Default selected value
   */
  defaultValue?: string;

  /**
   * Max number of results to display
   */
  maxResults?: number;

  /**
   * Callback when item is selected
   * Returns the id (for objects) or the value (for strings)
   */
  onSelect?: (id: string, displayName: string) => void;

  /**
   * Class name for custom styling
   */
  className?: string;

  /**
   * Loading state (controlled from parent if needed)
   */
  isLoading?: boolean;
}
