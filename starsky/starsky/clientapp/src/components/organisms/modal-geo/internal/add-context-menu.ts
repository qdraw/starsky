import L from "leaflet";
import React from "react";
import { ILanguageLocalization } from "../../../../interfaces/ILanguageLocalization";
import { Language } from "../../../../shared/language";
import { FetchAddressFromNominatim, GetStreetName } from "./fetch-address-from-nominatim";

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

    // Fetch address from Nominatim
    const addressData = await FetchAddressFromNominatim(currentLat, currentLng);
    streetName = addressData ? GetStreetName(addressData.address) : "";

    // Update menu with data
    contextMenu.innerHTML = `
      <div class="leaflet-context-menu__section-title leaflet-context-menu__section-title--bottom">
        ${language.key(localization.MessageCoordinates)}
      </div>
      <div class="leaflet-context-menu__coords">
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
        <div class="leaflet-context-menu__street">
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
    `;

    // Add click handlers
    contextMenu.querySelectorAll('[data-action="copy-coordinates"]').forEach((el) => {
      el.addEventListener("click", async (event) => {
        event.preventDefault();
        const coordinates = `${currentLat.toFixed(6)}, ${currentLng.toFixed(6)}`;
        console.log("Coordinates copied:", coordinates);

        await copyToClipboard(coordinates);
        setNotificationStatus(language.key(localization.MessageCoordinatesCopied));
        closeContextMenu();
      });
    });

    contextMenu.querySelectorAll('[data-action="copy-street"]').forEach((el) => {
      el.addEventListener("click", async (event) => {
        event.preventDefault();
        console.log("Street name copied:", streetName);
        await copyToClipboard(streetName);
        setNotificationStatus(language.key(localization.MessageStreetNameCopied));
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

  // Copy to clipboard helper
  async function copyToClipboard(text: string) {
    await navigator.clipboard.writeText(text);
  }
}
