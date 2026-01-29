# SearchableDropdown Component

A generic, reusable searchable dropdown atom component that fetches results from a backend API.

## Features

- 🔍 Real-time search with debouncing
- ⌨️ Keyboard navigation (Arrow Up/Down, Enter, Escape)
- 🎯 Click outside to close
- 🔄 Customizable result fetching from backend
- 📋 Default items when no search query
- ⏳ Loading indicator
- 🎨 Fully styled and customizable

## Usage

### Basic Example

```tsx
import SearchableDropdown from "@/components/atoms/searchable-dropdown/searchable-dropdown";

const MyComponent = () => {
  const fetchResults = async (query: string) => {
    const response = await fetch(`/api/search?q=${query}`);
    const data = await response.json();
    return data.results; // Must return string[]
  };

  return (
    <SearchableDropdown
      fetchResults={fetchResults}
      placeholder="Search items..."
      onSelect={(value) => console.log("Selected:", value)}
    />
  );
};
```

### With Default Items

```tsx
<SearchableDropdown
  fetchResults={fetchResults}
  placeholder="Search or select..."
  defaultItems={[
    { label: "Home", value: "home" },
    { label: "Settings", value: "settings" },
    { label: "About", value: "about" }
  ]}
  onSelect={(value) => handleSelection(value)}
/>
```

### With Custom Configuration

```tsx
<SearchableDropdown
  fetchResults={fetchResults}
  placeholder="Search products..."
  defaultValue="Apple"
  maxResults={8}
  isLoading={false}
  className="custom-dropdown-class"
  onSelect={(value) => handleSelection(value)}
/>
```

## Props

### `ISearchableDropdownProps`

| Prop | Type | Required | Default | Description |
|------|------|----------|---------|-------------|
| `fetchResults` | `(query: string) => Promise<string[]>` | Yes | - | Function to fetch results from backend. Takes search query and returns array of options. |
| `defaultItems` | `Array<{ label: string; value: string }>` | No | `[]` | Default items shown when no search query is entered. |
| `placeholder` | `string` | No | `"Search..."` | Placeholder text for the input field. |
| `defaultValue` | `string` | No | `""` | Default selected value. |
| `maxResults` | `number` | No | `10` | Maximum number of results to display. |
| `onSelect` | `(value: string) => void` | No | - | Callback function when an item is selected. |
| `className` | `string` | No | `""` | Additional CSS class name for custom styling. |
| `isLoading` | `boolean` | No | `false` | Loading state (can be controlled from parent if needed). |

## Keyboard Navigation

- **Arrow Down**: Move down to next item
- **Arrow Up**: Move up to previous item
- **Enter**: Select the highlighted item
- **Escape**: Close the dropdown

## Features Explained

### Debounced Search
The component automatically debounces search requests by 300ms to avoid excessive API calls while typing.

### Click Outside Detection
When the user clicks outside the dropdown, it automatically closes.

### Loading State
Shows a loading indicator (`...`) while fetching results from the backend.

### No Results Message
Displays a "No results found" message when search returns no items.

### Mouse & Keyboard Integration
Users can navigate items using both mouse (hover/click) and keyboard (arrow keys/enter).

## Styling

The component comes with default styles in `searchable-dropdown.css`. You can customize it by:

1. **Adding custom CSS**: Import and override the classes
2. **Using the `className` prop**: Pass additional class names for custom styling

### Available CSS Classes

```css
.searchable-dropdown /* Main container */
.searchable-dropdown__form /* Form wrapper */
.searchable-dropdown__input /* Input field */
.searchable-dropdown__loading /* Loading indicator */
.searchable-dropdown__list /* Dropdown list */
.searchable-dropdown__item /* Individual item */
.searchable-dropdown__item--selected /* Selected item */
.searchable-dropdown__button /* Item button */
.searchable-dropdown__no-results /* No results message */
```

## Backend API Integration

The `fetchResults` function should return a Promise that resolves to an array of strings:

```tsx
// Example with fetch
const fetchResults = async (query: string) => {
  const response = await fetch(`/api/items?search=${encodeURIComponent(query)}`);
  if (!response.ok) throw new Error("Failed to fetch");
  const data = await response.json();
  return data.items; // Array of strings
};

// Example with axios
import axios from "axios";

const fetchResults = async (query: string) => {
  const { data } = await axios.get("/api/items", { params: { search: query } });
  return data.items;
};
```

## Testing

The component includes comprehensive tests in `searchable-dropdown.spec.tsx`:

```bash
npm test -- searchable-dropdown.spec.tsx
```

Tests cover:
- Rendering
- User interactions (typing, clicking, keyboard navigation)
- API integration
- Edge cases (no results, empty input, etc.)

## Accessibility

- Uses semantic HTML elements
- Supports keyboard navigation for power users
- Includes `data-test` attributes for testing
- Has proper focus management

## Migration from MenuInlineSearch

If you're migrating from `MenuInlineSearch`, the main difference is:

```tsx
// Before - MenuInlineSearch (tightly coupled to search API)
<MenuInlineSearch callback={(query) => handleSearch(query)} />

// After - SearchableDropdown (generic, any backend)
<SearchableDropdown
  fetchResults={async (query) => {
    const response = await fetch(`/api/search?q=${query}`);
    return response.json();
  }}
  onSelect={(value) => handleSelection(value)}
/>
```

## Performance Considerations

- **Debounced Search**: 300ms debounce delay prevents excessive API calls
- **Max Results**: Limits the number of items rendered (default: 10)
- **Virtual Scrolling**: For very large lists, consider wrapping in a virtualized list library
