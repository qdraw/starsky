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
    contextMenu.innerHTML = `
      <div class="leaflet-context-menu__loading">
        ${language.key(localization.MessageLoadingAddress)}
      </div>
    `;

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
    contextMenu.innerHTML = `
      <div class="leaflet-context-menu__section-title leaflet-context-menu__section-title--bottom">
        ${language.key(localization.MessageCoordinates)}
      </div>
      <div class="leaflet-context-menu__coords" data-action="copy-coordinates">
        ${currentLat.toFixed(6)}, ${currentLng.toFixed(6)}
      </div>
      <div class="context-menu-item" data-action="copy-coordinates" title="${language.key(localization.MessageClickToCopy)}">
        📋 ${language.key(localization.MessageCopyCoordinates)}
      </div>
      ${
        streetName
          ? `
        <div class="leaflet-context-menu__section-title leaflet-context-menu__section-title--top">
          ${language.key(localization.MessageStreetName)}
        </div>
        <div class="leaflet-context-menu__street" data-action="copy-street">
          ${streetName}
        </div>
        <div class="context-menu-item" data-action="copy-street" title="${language.key(localization.MessageClickToCopy)}">
          📋 ${language.key(localization.MessageCopyStreetName)}
        </div>
      `
          : `
        <div class="leaflet-context-menu__no-street">
          ${language.key(localization.MessageNoStreetFound)}
        </div>
      `
      }
      ${renderOsmSection(
        language.key(localization.MessageNearbyObjects),
        language.key(localization.MessageNoNearbyObjects),
        nearbyObjects
      )}
      ${renderOsmSection(
        language.key(localization.MessageEnclosingObjects),
        language.key(localization.MessageNoEnclosingObjects),
        enclosingObjects
      )}
    `;

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
        const copyText = target.getAttribute("data-copy-text") || "";
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

function renderOsmSection(title: string, emptyMessage: string, items: IOsmMapElementItem[]): string {
  const itemsMarkup = items
    .filter((item) => !!item)
    .map((item) => {
      const label = escapeHtml(item.label || item.copyText || "");
      const description = escapeHtml(item.description || "");
      const copyText = escapeHtml(item.copyText || item.label || "");

      return `
        <div class="context-menu-item leaflet-context-menu__osm-item" data-action="copy-osm-item" data-copy-text="${copyText}">
          <div class="leaflet-context-menu__osm-item-label">${label}</div>
          ${description ? `<div class="leaflet-context-menu__osm-item-description">${description}</div>` : ""}
        </div>
      `;
    })
    .join("");

  return `
    <div class="leaflet-context-menu__section-title leaflet-context-menu__section-title--top">
      ${escapeHtml(title)}
    </div>
    ${
      itemsMarkup
        ? `<div class="leaflet-context-menu__osm-items">${itemsMarkup}</div>`
        : `<div class="leaflet-context-menu__no-osm-items">${escapeHtml(emptyMessage)}</div>`
    }
  `;
}

function escapeHtml(input: string): string {
  return input
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
}

// Copy to clipboard helper
async function copyToClipboard(text: string) {
  await navigator.clipboard.writeText(text);
}
