# SearchableDropdown Component

A generic, reusable searchable dropdown atom component that fetches results from a backend API. Supports both simple string arrays and object arrays with id/displayName from the backend.

## Features

- 🔍 Real-time search with debouncing
- ⌨️ Keyboard navigation (Arrow Up/Down, Enter, Escape)
- 🎯 Click outside to close
- 🔄 Customizable result fetching from backend
- 📋 Default items when no search query
- ⏳ Loading indicator
- 🎨 Fully styled and customizable
- 🆗 Accepts both `["Banana"]` and `[{ id: "banana", displayName: "Banana" }]` as backend results

## Usage

### Example 1: Simple String Results

```tsx
<SearchableDropdown
  fetchResults={async (query) => {
    // Returns ["Banana", "Apple"]
    const response = await fetch(`/api/fruits?q=${query}`);
    return response.json();
  }}
  onSelect={(value) => console.log(value)} // value is "Banana"
  placeholder="Search fruits..."
/>
```

### Example 2: Object Results

```tsx
<SearchableDropdown
  fetchResults={async (query) => {
    // Returns [{ id: "banana", displayName: "Banana" }]
    const response = await fetch(`/api/fruits?q=${query}`);
    return response.json();
  }}
  onSelect={(id) => console.log(id)} // id is "banana"
  placeholder="Search fruits..."
/>
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

## Props

### `ISearchableDropdownProps`

| Prop           | Type                                                                          | Required | Default       | Description                                                                                          |
| -------------- | ----------------------------------------------------------------------------- | -------- | ------------- | ---------------------------------------------------------------------------------------------------- |
| `fetchResults` | `(query: string) => Promise<string[] \| {id: string, displayName: string}[]>` | Yes      | -             | Function to fetch results from backend. Can return array of strings or array of objects.             |
| `defaultItems` | `Array<{ label: string; value: string }>`                                     | No       | `[]`          | Default items shown when no search query is entered.                                                 |
| `placeholder`  | `string`                                                                      | No       | `"Search..."` | Placeholder text for the input field.                                                                |
| `defaultValue` | `string`                                                                      | No       | `""`          | Default selected value.                                                                              |
| `maxResults`   | `number`                                                                      | No       | `10`          | Maximum number of results to display.                                                                |
| `onSelect`     | `(value: string) => void`                                                     | No       | -             | Callback function when an item is selected. Returns the id (for objects) or the value (for strings). |
| `className`    | `string`                                                                      | No       | `""`          | Additional CSS class name for custom styling.                                                        |
| `isLoading`    | `boolean`                                                                     | No       | `false`       | Loading state (can be controlled from parent if needed).                                             |

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

The `fetchResults` function can return either a string array or an array of objects with `id` and `displayName`:

```tsx
// Example 1: Return array of strings
const fetchResults = async (query: string) => {
  const response = await fetch(`/api/items?search=${encodeURIComponent(query)}`);
  if (!response.ok) throw new Error("Failed to fetch");
  return await response.json(); // ["Banana", "Apple"]
};

// Example 2: Return array of objects
const fetchResults = async (query: string) => {
  const response = await fetch(`/api/items?search=${encodeURIComponent(query)}`);
  if (!response.ok) throw new Error("Failed to fetch");
  return await response.json(); // [{ id: "banana", displayName: "Banana" }]
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
