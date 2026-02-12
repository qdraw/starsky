import L from "leaflet";
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
  localization: ILocalization;
}

/**
 * Adds a right-click context menu to the map
 * Shows coordinates and street name with copy functionality
 */
export function AddContextMenu({ map, language, localization }: IContextMenuOptions) {
  // Remove any existing context menu
  const existingMenu = document.querySelector(".leaflet-context-menu");
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
    contextMenu.style.position = "absolute";
    contextMenu.style.zIndex = "1000";

    // Position menu at click location
    const containerPoint = map.latLngToContainerPoint(event.latlng);
    contextMenu.style.left = `${containerPoint.x}px`;
    contextMenu.style.top = `${containerPoint.y}px`;

    // Add loading message
    contextMenu.innerHTML = `
      <div style="padding: 4px 8px; color: #666;">
        ${language.key(localization.MessageLoadingAddress)}
      </div>
    `;

    document.body.appendChild(contextMenu);

    // Fetch address from Nominatim
    const addressData = await FetchAddressFromNominatim(currentLat, currentLng);
    streetName = addressData ? GetStreetName(addressData.address) : "";

    // Update menu with data
    contextMenu.innerHTML = `
      <div style="padding: 4px 8px; border-bottom: 1px solid #eee; margin-bottom: 4px;">
        <strong>${language.key(localization.MessageCoordinates)}</strong>
      </div>
      <div style="padding: 4px 8px; font-family: monospace; font-size: 12px; margin-bottom: 8px;">
        ${currentLat.toFixed(6)}, ${currentLng.toFixed(6)}
      </div>
      <div style="padding: 4px 8px; cursor: pointer; hover: background: #f5f5f5;" class="context-menu-item" data-action="copy-coordinates" title="${language.key(localization.MessageClickToCopy)}">
        📋 ${language.key(localization.MessageCopyCoordinates)}
      </div>
      ${
        streetName
          ? `
        <div style="padding: 4px 8px; border-top: 1px solid #eee; margin-top: 4px; padding-top: 8px;">
          <strong>${language.key(localization.MessageStreetName)}</strong>
        </div>
        <div style="padding: 4px 8px; margin-bottom: 8px;">
          ${streetName}
        </div>
        <div style="padding: 4px 8px; cursor: pointer;" class="context-menu-item" data-action="copy-street" title="${language.key(localization.MessageClickToCopy)}">
          📋 ${language.key(localization.MessageCopyStreetName)}
        </div>
      `
          : `
        <div style="padding: 4px 8px; color: #999; font-style: italic;">
          ${language.key(localization.MessageNoStreetFound)}
        </div>
      `
      }
    `;

    // Add hover effects
    const menuItems = contextMenu.querySelectorAll(".context-menu-item");
    menuItems.forEach((item) => {
      const htmlItem = item as HTMLElement;
      htmlItem.addEventListener("mouseenter", function () {
        htmlItem.style.backgroundColor = "#f5f5f5";
      });
      htmlItem.addEventListener("mouseleave", function () {
        htmlItem.style.backgroundColor = "";
      });
    });

    // Add click handlers
    contextMenu.querySelectorAll('[data-action="copy-coordinates"]').forEach((el) => {
      el.addEventListener("click", async () => {
        const coordinates = `${currentLat.toFixed(6)}, ${currentLng.toFixed(6)}`;
        await copyToClipboard(coordinates);
        showCopyNotification(language.key(localization.MessageCoordinatesCopied));
        closeContextMenu();
      });
    });

    contextMenu.querySelectorAll('[data-action="copy-street"]').forEach((el) => {
      el.addEventListener("click", async () => {
        await copyToClipboard(streetName);
        showCopyNotification(language.key(localization.MessageStreetNameCopied));
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
    try {
      await navigator.clipboard.writeText(text);
    } catch (err) {
      console.error("Failed to copy to clipboard:", err);
      // Fallback for older browsers
      const textArea = document.createElement("textarea");
      textArea.value = text;
      textArea.style.position = "fixed";
      textArea.style.left = "-999999px";
      document.body.appendChild(textArea);
      textArea.select();
      try {
        document.execCommand("copy");
      } catch (copyErr) {
        console.error("Fallback copy failed:", copyErr);
      }
      document.body.removeChild(textArea);
    }
  }

  // Show copy notification
  function showCopyNotification(message: string) {
    const notification = document.createElement("div");
    notification.textContent = message;
    notification.style.position = "fixed";
    notification.style.top = "20px";
    notification.style.right = "20px";
    notification.style.backgroundColor = "#4CAF50";
    notification.style.color = "white";
    notification.style.padding = "12px 24px";
    notification.style.borderRadius = "4px";
    notification.style.boxShadow = "0 2px 8px rgba(0,0,0,0.2)";
    notification.style.zIndex = "10000";
    notification.style.fontSize = "14px";
    notification.style.fontWeight = "500";

    document.body.appendChild(notification);

    setTimeout(() => {
      notification.remove();
    }, 2000);
  }
}
