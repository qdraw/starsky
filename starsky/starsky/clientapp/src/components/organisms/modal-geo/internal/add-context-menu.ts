import L from "leaflet";
import React from "react";
import { ILanguageLocalization } from "../../../../interfaces/ILanguageLocalization";
import { Language } from "../../../../shared/language";
import { FetchAddressFromNominatim, GetStreetName } from "./fetch-address-from-nominatim";
import { FetchMapElementsFromOsm, IOsmMapElementItem } from "./fetch-osm-map-elements";

export interface ILocalization {
  MessageCoordinates: ILanguageLocalization;
  MessageCopyCoordinates: ILanguageLocalization;
  MessageStreetName: ILanguageLocalization;
  MessageCopyStreetName: ILanguageLocalization;
  MessageClickToCopy: ILanguageLocalization;
  MessageCoordinatesCopied: ILanguageLocalization;
  MessageStreetNameCopied: ILanguageLocalization;
  MessageNoStreetFound: ILanguageLocalization;
  MessageLoadingAddress: ILanguageLocalization;
  MessageNearbyObjects: ILanguageLocalization;
  MessageNoNearbyObjects: ILanguageLocalization;
  MessageEnclosingObjects: ILanguageLocalization;
  MessageNoEnclosingObjects: ILanguageLocalization;
  MessageMapElementCopied: ILanguageLocalization;
  [key: string]: ILanguageLocalization;
}

interface IContextMenuOptions {
  map: L.Map;
  language: Language;
  setNotificationStatus: React.Dispatch<React.SetStateAction<string | null>>;
  localization: ILocalization;
}

/**
 * Adds a right-click context menu to the map
 * Shows coordinates and street name with copy functionality
 */
export function AddContextMenu({
  map,
  language,
  localization,
  setNotificationStatus
}: IContextMenuOptions) {
  // Remove any existing context menu
  const mapContainer = map.getContainer();
  const existingMenu = mapContainer.querySelector(".leaflet-context-menu");
  if (existingMenu) {
    existingMenu.remove();
  }

  let contextMenu: HTMLDivElement | null = null;
  let currentLat = 0;
  let currentLng = 0;
  let streetName = "";
  let nearbyObjects: IOsmMapElementItem[] = [];
  let enclosingObjects: IOsmMapElementItem[] = [];

  // Create context menu on right-click
  map.on("contextmenu", async function (event: L.LeafletMouseEvent) {
    currentLat = event.latlng.lat;
    currentLng = event.latlng.lng;

    // Remove existing menu if any
    if (contextMenu) {
      contextMenu.remove();
    }

    // Create context menu element
    contextMenu = document.createElement("div");
    contextMenu.className = "leaflet-context-menu";

    // Position menu at click location (relative to map container)
    const containerPoint = map.latLngToContainerPoint(event.latlng);
    contextMenu.style.left = `${containerPoint.x}px`;
    contextMenu.style.top = `${containerPoint.y}px`;

    // Add loading message
    contextMenu.replaceChildren(
      createDiv("leaflet-context-menu__loading", language.key(localization.MessageLoadingAddress))
    );

    mapContainer.appendChild(contextMenu);

    // Fetch address and OSM map elements in parallel.
    const [addressData, mapElementsData] = await Promise.all([
      FetchAddressFromNominatim(currentLat, currentLng),
      FetchMapElementsFromOsm(currentLat, currentLng)
    ]);
    streetName = addressData ? GetStreetName(addressData.address) : "";
    nearbyObjects = mapElementsData?.nearbyObjects || [];
    enclosingObjects = mapElementsData?.enclosingObjects || [];

    // Update menu with data
    // textContent/title are used because streetName comes from OpenStreetMap, which anyone can edit
    const coordinatesText = `${currentLat.toFixed(6)}, ${currentLng.toFixed(6)}`;
    const clickToCopy = language.key(localization.MessageClickToCopy);
    const children: HTMLElement[] = [
      createDiv(
        "leaflet-context-menu__section-title leaflet-context-menu__section-title--bottom",
        language.key(localization.MessageCoordinates)
      ),
      createDiv("leaflet-context-menu__coords", coordinatesText, "copy-coordinates"),
      createDiv(
        "context-menu-item",
        `📋 ${language.key(localization.MessageCopyCoordinates)}`,
        "copy-coordinates",
        clickToCopy
      )
    ];

    if (streetName) {
      children.push(
        createDiv(
          "leaflet-context-menu__section-title leaflet-context-menu__section-title--top",
          language.key(localization.MessageStreetName)
        ),
        createDiv("leaflet-context-menu__street", streetName, "copy-street"),
        createDiv(
          "context-menu-item",
          `📋 ${language.key(localization.MessageCopyStreetName)}`,
          "copy-street",
          clickToCopy
        )
      );
    } else {
      children.push(
        createDiv(
          "leaflet-context-menu__no-street",
          language.key(localization.MessageNoStreetFound)
        )
      );
    }

    children.push(
      ...renderOsmSection(
        language.key(localization.MessageNearbyObjects),
        language.key(localization.MessageNoNearbyObjects),
        nearbyObjects
      ),
      ...renderOsmSection(
        language.key(localization.MessageEnclosingObjects),
        language.key(localization.MessageNoEnclosingObjects),
        enclosingObjects
      )
    );
    contextMenu.replaceChildren(...children);

    // Add click handlers
    contextMenu.querySelectorAll('[data-action="copy-coordinates"]').forEach((el) => {
      el.addEventListener("click", async (event) => {
        event.preventDefault();
        event.stopPropagation();
        const coordinates = `${currentLat.toFixed(6)}, ${currentLng.toFixed(6)}`;
        await copyToClipboard(coordinates);
        setNotificationStatus(language.key(localization.MessageCoordinatesCopied));
        closeContextMenu();
      });
    });

    contextMenu.querySelectorAll('[data-action="copy-street"]').forEach((el) => {
      el.addEventListener("click", async (event) => {
        event.preventDefault();
        event.stopPropagation();
        await copyToClipboard(streetName);
        setNotificationStatus(language.key(localization.MessageStreetNameCopied));
        closeContextMenu();
      });
    });

    contextMenu.querySelectorAll('[data-action="copy-osm-item"]').forEach((el) => {
      el.addEventListener("click", async (event) => {
        event.preventDefault();
        event.stopPropagation();
        const target = event.currentTarget as HTMLElement;
        const copyText = target.dataset.copyText || "";
        if (!copyText) {
          return;
        }

        await copyToClipboard(copyText);
        setNotificationStatus(language.key(localization.MessageMapElementCopied));
        closeContextMenu();
      });
    });
  });

  // Close menu when clicking elsewhere
  function closeContextMenu() {
    if (contextMenu) {
      contextMenu.remove();
      contextMenu = null;
    }
  }

  map.on("click", closeContextMenu);
  map.on("movestart", closeContextMenu);
}

function createDiv(
  className: string,
  text: string,
  action?: string,
  title?: string
): HTMLDivElement {
  const div = document.createElement("div");
  div.className = className;
  div.textContent = text;
  if (action) div.dataset.action = action;
  if (title) div.title = title;
  return div;
}

function renderOsmSection(
  title: string,
  emptyMessage: string,
  items: IOsmMapElementItem[]
): HTMLElement[] {
  const elements: HTMLElement[] = [
    createDiv(
      "leaflet-context-menu__section-title leaflet-context-menu__section-title--top",
      title
    )
  ];

  const validItems = items.filter((item) => !!item);
  if (validItems.length === 0) {
    elements.push(createDiv("leaflet-context-menu__no-osm-items", emptyMessage));
    return elements;
  }

  const container = createDiv("leaflet-context-menu__osm-items", "");
  for (const item of validItems) {
    const row = createDiv("context-menu-item leaflet-context-menu__osm-item", "", "copy-osm-item");
    row.dataset.copyText = item.copyText || item.label || "";
    row.appendChild(
      createDiv("leaflet-context-menu__osm-item-label", item.label || item.copyText || "")
    );
    if (item.description) {
      row.appendChild(createDiv("leaflet-context-menu__osm-item-description", item.description));
    }
    container.appendChild(row);
  }
  elements.push(container);
  return elements;
}

// Copy to clipboard helper
async function copyToClipboard(text: string) {
  await navigator.clipboard.writeText(text);
}
