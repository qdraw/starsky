import { StoryObj } from "@storybook/react";
import { DropdownResult } from "./ISearableDropdownProps";
import SearchableDropdown from "./searchable-dropdown";

const meta = {
  title: "components/atoms/searchable-dropdown",
  component: SearchableDropdown,
  parameters: {
    layout: "centered"
  },
  tags: ["autodocs"]
};

export default meta;
type Story = StoryObj<typeof meta>;

// Mock fetch function - returns simple strings
const mockFetchStrings = async (query: string): Promise<string[]> => {
  const allItems = [
    "Apple",
    "Apricot",
    "Avocado",
    "Banana",
    "Blackberry",
    "Blueberry",
    "Cherry",
    "Coconut",
    "Cranberry",
    "Date",
    "Dragon Fruit",
    "Elderberry",
    "Fig",
    "Grapefruit",
    "Grape",
    "Guava",
    "Honeydew",
    "Kiwi",
    "Lemon",
    "Lime",
    "Mango",
    "Melon",
    "Nectarine",
    "Orange",
    "Papaya",
    "Peach",
    "Pear",
    "Pineapple",
    "Plum",
    "Pomegranate",
    "Raspberry",
    "Strawberry",
    "Tangerine",
    "Watermelon"
  ];

  // Simulate network delay
  await new Promise((resolve) => setTimeout(resolve, 300));

  if (!query.trim()) {
    return [];
  }

  return allItems.filter((item) => item.toLowerCase().includes(query.toLowerCase()));
};

// Mock fetch function - returns objects with id and displayName
const mockFetchObjects = async (query: string): Promise<DropdownResult[]> => {
  const allItems = [
    { id: "apple", displayName: "Apple" },
    { id: "apricot", displayName: "Apricot" },
    { id: "avocado", displayName: "Avocado" },
    { id: "banana", displayName: "Banana" },
    { id: "blackberry", displayName: "Blackberry" },
    { id: "blueberry", displayName: "Blueberry" },
    { id: "cherry", displayName: "Cherry" },
    { id: "coconut", displayName: "Coconut" },
    { id: "cranberry", displayName: "Cranberry" },
    { id: "date", displayName: "Date" },
    { id: "dragon-fruit", displayName: "Dragon Fruit" },
    { id: "elderberry", displayName: "Elderberry" },
    { id: "fig", displayName: "Fig" },
    { id: "grapefruit", displayName: "Grapefruit" },
    { id: "grape", displayName: "Grape" },
    { id: "guava", displayName: "Guava" },
    { id: "honeydew", displayName: "Honeydew" },
    { id: "kiwi", displayName: "Kiwi" },
    { id: "lemon", displayName: "Lemon" },
    { id: "lime", displayName: "Lime" },
    { id: "mango", displayName: "Mango" },
    { id: "melon", displayName: "Melon" },
    { id: "nectarine", displayName: "Nectarine" },
    { id: "orange", displayName: "Orange" },
    { id: "papaya", displayName: "Papaya" },
    { id: "peach", displayName: "Peach" },
    { id: "pear", displayName: "Pear" },
    { id: "pineapple", displayName: "Pineapple" },
    { id: "plum", displayName: "Plum" },
    { id: "pomegranate", displayName: "Pomegranate" },
    { id: "raspberry", displayName: "Raspberry" },
    { id: "strawberry", displayName: "Strawberry" },
    { id: "tangerine", displayName: "Tangerine" },
    { id: "watermelon", displayName: "Watermelon" }
  ];

  // Simulate network delay
  await new Promise((resolve) => setTimeout(resolve, 300));

  if (!query.trim()) {
    return [];
  }

  return allItems.filter((item) => item.displayName.toLowerCase().includes(query.toLowerCase()));
};

export const WithStrings: Story = {
  args: {
    fetchResults: mockFetchStrings,
    placeholder: "Search fruits (strings)...",
    onSelect: (value) => console.log("Selected (string value):", value)
  }
};

export const WithObjects: Story = {
  args: {
    fetchResults: mockFetchObjects,
    placeholder: "Search fruits (objects)...",
    onSelect: (value) => console.log("Selected (object id):", value)
  }
};
export const WithDefaultItems: Story = {
  args: {
    fetchResults: mockFetchStrings,
    placeholder: "Search or select...",
    defaultItems: [
      { label: "Home", value: "home" },
      { label: "Settings", value: "settings" },
      { label: "About", value: "about" }
    ],
    onSelect: (value) => console.log("Selected:", value)
  }
};

export const WithCustomClassName: Story = {
  args: {
    fetchResults: mockFetchStrings,
    placeholder: "Custom styled dropdown...",
    className: "custom-dropdown",
    onSelect: (value) => console.log("Selected:", value)
  }
};
