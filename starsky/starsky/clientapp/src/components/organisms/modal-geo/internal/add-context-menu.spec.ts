import { waitFor } from "@testing-library/react";
import L from "leaflet";
import { ILanguageLocalization } from "../../../../interfaces/ILanguageLocalization";
import { Language } from "../../../../shared/language";
import { AddContextMenu } from "./add-context-menu";
import * as nominatimModule from "./fetch-address-from-nominatim";
import * as mapElementsModule from "./fetch-osm-map-elements";

// Mock the fetch-address-from-nominatim module
jest.mock("./fetch-address-from-nominatim");
jest.mock("./fetch-osm-map-elements");

interface ILanguageMock {
  key: (obj: ILanguageLocalization) => string;
}

interface ILocalizationMock {
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

describe("AddContextMenu", () => {
  let map: L.Map;
  let mapContainer: HTMLDivElement;
  let language: ILanguageMock;
  let localization: ILocalizationMock;

  beforeEach(() => {
    // Create a map container
    mapContainer = document.createElement("div");
    mapContainer.style.width = "500px";
    mapContainer.style.height = "500px";
    document.body.appendChild(mapContainer);

    // Create a Leaflet map
    map = L.map(mapContainer).setView([52.52, 13.405], 13);

    // Mock language object
    language = {
      key: jest.fn((obj: ILanguageLocalization) => {
        if (typeof obj === "string") return obj;
        return obj.en || "";
      })
    };

    // Mock localization object
    localization = {
      MessageCoordinates: { en: "Coordinates", nl: "Coördinaten", de: "Koordinaten" },
      MessageCopyCoordinates: {
        en: "Copy coordinates",
        nl: "Coördinaten kopiëren",
        de: "Koordinaten kopieren"
      },
      MessageStreetName: { en: "Street", nl: "Straat", de: "Straße" },
      MessageCopyStreetName: {
        en: "Copy street name",
        nl: "Straatnaam kopiëren",
        de: "Straßenname kopieren"
      },
      MessageClickToCopy: {
        en: "Click to copy",
        nl: "Klik om te kopiëren",
        de: "Klicken zum Kopieren"
      },
      MessageCoordinatesCopied: {
        en: "Coordinates copied!",
        nl: "Coördinaten gekopieerd!",
        de: "Koordinaten kopiert!"
      },
      MessageStreetNameCopied: {
        en: "Street name copied!",
        nl: "Straatnaam gekopieerd!",
        de: "Straßenname kopiert!"
      },
      MessageNoStreetFound: {
        en: "No street name found",
        nl: "Geen straatnaam gevonden",
        de: "Kein Straßenname gefunden"
      },
      MessageLoadingAddress: {
        en: "Loading address...",
        nl: "Adres laden...",
        de: "Adresse wird geladen..."
      },
      MessageNearbyObjects: {
        en: "Nearby objects",
        nl: "Objecten in de buurt",
        de: "Objekte in der Nähe"
      },
      MessageNoNearbyObjects: {
        en: "No nearby objects",
        nl: "Geen objecten in de buurt",
        de: "Keine Objekte in der Nähe"
      },
      MessageEnclosingObjects: {
        en: "Enclosing objects",
        nl: "Omsluitende objecten",
        de: "Umschließende Objekte"
      },
      MessageNoEnclosingObjects: {
        en: "No enclosing objects",
        nl: "Geen omsluitende objecten",
        de: "Keine umschließenden Objekte"
      },
      MessageMapElementCopied: {
        en: "Map element copied!",
        nl: "Kaartobject gekopieerd!",
        de: "Kartenobjekt kopiert!"
      }
    };

    (mapElementsModule.FetchMapElementsFromOsm as jest.Mock).mockResolvedValue({
      nearbyObjects: [],
      enclosingObjects: []
    });

    // Mock clipboard API
    Object.assign(navigator, {
      clipboard: {
        writeText: jest.fn()
      }
    });
  });

  afterEach(() => {
    map.remove();
    document.body.removeChild(mapContainer);
    jest.resetAllMocks();

    // Clean up any context menus
    const menus = document.querySelectorAll(".leaflet-context-menu");
    menus.forEach((menu) => menu.remove());
  });

  it("should create context menu on right-click", async () => {
    const mockAddressData = {
      display_name: "Test Street 123, Test City",
      address: {
        road: "Test Street",
        house_number: "123"
      }
    };

    (nominatimModule.FetchAddressFromNominatim as jest.Mock).mockResolvedValue(mockAddressData);
    (nominatimModule.GetStreetName as jest.Mock).mockReturnValue("Test Street 123");

    AddContextMenu({
      map,
      language: language as unknown as Language,
      localization,
      setNotificationStatus: jest.fn()
    });

    // Simulate right-click event
    const event = {
      latlng: L.latLng(52.52, 13.405),
      containerPoint: L.point(100, 100)
    } as L.LeafletMouseEvent;
    map.fire("contextmenu", event);

    // Wait for async operations
    await new Promise((resolve) => setTimeout(resolve, 100));

    const contextMenu = document.querySelector(".leaflet-context-menu");
    expect(contextMenu).toBeTruthy();
  });

  it("should display coordinates in context menu", async () => {
    const mockAddressData = {
      display_name: "Test Street 123",
      address: {
        road: "Test Street",
        house_number: "123"
      }
    };

    (nominatimModule.FetchAddressFromNominatim as jest.Mock).mockResolvedValue(mockAddressData);
    (nominatimModule.GetStreetName as jest.Mock).mockReturnValue("Test Street 123");

    AddContextMenu({
      map,
      language: language as unknown as Language,
      localization,
      setNotificationStatus: jest.fn()
    });

    map.fire("contextmenu", {
      latlng: L.latLng(52.52, 13.405),
      containerPoint: L.point(100, 100)
    } as L.LeafletMouseEvent);

    await new Promise((resolve) => setTimeout(resolve, 100));

    const contextMenu = document.querySelector(".leaflet-context-menu");
    expect(contextMenu?.textContent).toContain("52.520000");
    expect(contextMenu?.textContent).toContain("13.405000");
  });

  it("should display street name when available", async () => {
    const mockAddressData = {
      display_name: "Test Street 123",
      address: {
        road: "Test Street",
        house_number: "123"
      }
    };

    (nominatimModule.FetchAddressFromNominatim as jest.Mock).mockResolvedValue(mockAddressData);
    (nominatimModule.GetStreetName as jest.Mock).mockReturnValue("Test Street 123");

    AddContextMenu({
      map,
      language: language as unknown as Language,
      localization,
      setNotificationStatus: jest.fn()
    });

    const event = {
      latlng: L.latLng(52.52, 13.405),
      containerPoint: L.point(100, 100)
    } as L.LeafletMouseEvent;
    map.fire("contextmenu", event);

    await new Promise((resolve) => setTimeout(resolve, 100));

    const contextMenu = document.querySelector(".leaflet-context-menu");
    expect(contextMenu?.textContent).toContain("Test Street 123");
  });

  it("should show no street found message when address is not available", async () => {
    (nominatimModule.FetchAddressFromNominatim as jest.Mock).mockResolvedValue(null);
    (nominatimModule.GetStreetName as jest.Mock).mockReturnValue("");

    AddContextMenu({
      map,
      language: language as unknown as Language,
      localization,
      setNotificationStatus: jest.fn()
    });

    const event = {
      latlng: L.latLng(52.52, 13.405),
      containerPoint: L.point(100, 100)
    } as L.LeafletMouseEvent;
    map.fire("contextmenu", event);

    await new Promise((resolve) => setTimeout(resolve, 100));

    const contextMenu = document.querySelector(".leaflet-context-menu");
    expect(contextMenu?.textContent).toContain("No street name found");
  });

  it("should close context menu on map click", async () => {
    (nominatimModule.FetchAddressFromNominatim as jest.Mock).mockResolvedValue(null);

    AddContextMenu({
      map,
      language: language as unknown as Language,
      localization,
      setNotificationStatus: jest.fn()
    });

    const event = {
      latlng: L.latLng(52.52, 13.405),
      containerPoint: L.point(100, 100)
    } as L.LeafletMouseEvent;
    map.fire("contextmenu", event);

    await new Promise((resolve) => setTimeout(resolve, 100));

    expect(document.querySelector(".leaflet-context-menu")).toBeTruthy();

    // Simulate click
    map.fire("click");

    expect(document.querySelector(".leaflet-context-menu")).toBeFalsy();
  });

  it("should close context menu on map move", async () => {
    (nominatimModule.FetchAddressFromNominatim as jest.Mock).mockResolvedValue(null);

    AddContextMenu({
      map,
      language: language as unknown as Language,
      localization,
      setNotificationStatus: jest.fn()
    });

    const event = {
      latlng: L.latLng(52.52, 13.405),
      containerPoint: L.point(100, 100)
    } as L.LeafletMouseEvent;
    map.fire("contextmenu", event);

    await new Promise((resolve) => setTimeout(resolve, 100));

    expect(document.querySelector(".leaflet-context-menu")).toBeTruthy();

    // Simulate move
    map.fire("movestart");

    expect(document.querySelector(".leaflet-context-menu")).toBeFalsy();
  });

  it("should remove existing context menu before creating new one", async () => {
    (nominatimModule.FetchAddressFromNominatim as jest.Mock).mockResolvedValue(null);

    AddContextMenu({
      map,
      language: language as unknown as Language,
      localization,
      setNotificationStatus: jest.fn()
    });

    // First context menu
    const event1 = {
      latlng: L.latLng(52.52, 13.405),
      containerPoint: L.point(100, 100)
    } as L.LeafletMouseEvent;
    map.fire("contextmenu", event1);

    await new Promise((resolve) => setTimeout(resolve, 100));

    const firstMenu = document.querySelector(".leaflet-context-menu");
    expect(firstMenu).toBeTruthy();

    // Second context menu
    const event2 = {
      latlng: L.latLng(52.53, 13.415),
      containerPoint: L.point(150, 150)
    } as L.LeafletMouseEvent;
    map.fire("contextmenu", event2);

    await new Promise((resolve) => setTimeout(resolve, 100));

    const menus = document.querySelectorAll(".leaflet-context-menu");
    expect(menus).toHaveLength(1);
  });

  it("should copy coordinates and call setNotificationStatus when copy-coordinates is clicked", async () => {
    const mockAddressData = {
      display_name: "Test Street 123, Test City",
      address: {
        road: "Test Street",
        house_number: "123"
      }
    };
    (nominatimModule.FetchAddressFromNominatim as jest.Mock).mockResolvedValue(mockAddressData);
    (nominatimModule.GetStreetName as jest.Mock).mockReturnValue("Test Street 123");

    const setNotificationStatus = jest.fn();
    AddContextMenu({
      map,
      language: language as unknown as Language,
      localization,
      setNotificationStatus
    });

    // Simulate right-click event
    const event = {
      latlng: L.latLng(52.52, 13.405),
      containerPoint: L.point(100, 100)
    } as L.LeafletMouseEvent;
    map.fire("contextmenu", event);

    await new Promise((resolve) => setTimeout(resolve, 100));

    const copyBtn = document.querySelector('[data-action="copy-coordinates"]') as HTMLElement;
    expect(copyBtn).toBeTruthy();
    copyBtn.click();

    expect(navigator.clipboard.writeText).toHaveBeenCalledWith("52.520000, 13.405000");

    waitFor(() => {
      expect(setNotificationStatus).toHaveBeenCalledWith("Coordinates copied!");
    });
  });

  it("should copy street and call setNotificationStatus when copy-street is clicked", async () => {
    const mockAddressData = {
      display_name: "Test Street 123, Test City",
      address: {
        road: "Test Street",
        house_number: "123"
      }
    };
    (nominatimModule.FetchAddressFromNominatim as jest.Mock).mockResolvedValue(mockAddressData);
    (nominatimModule.GetStreetName as jest.Mock).mockReturnValue("Test Street 123");

    const setNotificationStatus = jest.fn();
    AddContextMenu({
      map,
      language: language as unknown as Language,
      localization,
      setNotificationStatus
    });

    // Simulate right-click event
    const event = {
      latlng: L.latLng(52.52, 13.405),
      containerPoint: L.point(100, 100)
    } as L.LeafletMouseEvent;
    map.fire("contextmenu", event);

    await new Promise((resolve) => setTimeout(resolve, 100));

    const copyBtn = document.querySelector('[data-action="copy-street"]') as HTMLElement;
    expect(copyBtn).toBeTruthy();
    copyBtn.click();

    expect(navigator.clipboard.writeText).toHaveBeenCalledWith("Test Street 123");

    waitFor(() => {
      expect(setNotificationStatus).toHaveBeenCalledWith("Street copied!");
    });
  });

  it("should remove previous contextMenu instance before creating a new one", async () => {
    const mockAddressData = {
      display_name: "Test Street 123, Test City",
      address: {
        road: "Test Street",
        house_number: "123"
      }
    };
    (nominatimModule.FetchAddressFromNominatim as jest.Mock).mockResolvedValue(mockAddressData);
    (nominatimModule.GetStreetName as jest.Mock).mockReturnValue("Test Street 123");

    const setNotificationStatus = jest.fn();
    AddContextMenu({
      map,
      language: language as unknown as Language,
      localization,
      setNotificationStatus
    });

    // Simulate first right-click event
    const event1 = {
      latlng: L.latLng(52.52, 13.405),
      containerPoint: L.point(100, 100)
    } as L.LeafletMouseEvent;
    map.fire("contextmenu", event1);
    await new Promise((resolve) => setTimeout(resolve, 100));

    // There should be one menu
    expect(document.querySelectorAll(".leaflet-context-menu")).toHaveLength(1);

    // Simulate second right-click event
    const event2 = {
      latlng: L.latLng(52.53, 13.415),
      containerPoint: L.point(150, 150)
    } as L.LeafletMouseEvent;
    map.fire("contextmenu", event2);
    await new Promise((resolve) => setTimeout(resolve, 100));

    // There should still be only one menu
    expect(document.querySelectorAll(".leaflet-context-menu")).toHaveLength(1);
  });

  it("should render nearby and enclosing sections when map elements are available", async () => {
    (nominatimModule.FetchAddressFromNominatim as jest.Mock).mockResolvedValue(null);
    (mapElementsModule.FetchMapElementsFromOsm as jest.Mock).mockResolvedValue({
      nearbyObjects: [{ label: "Cafe", description: "amenity=cafe", copyText: "Cafe" }],
      enclosingObjects: [{ label: "City Park", description: "leisure=park", copyText: "City Park" }]
    });

    AddContextMenu({
      map,
      language: language as unknown as Language,
      localization,
      setNotificationStatus: jest.fn()
    });

    map.fire("contextmenu", {
      latlng: L.latLng(52.52, 13.405),
      containerPoint: L.point(100, 100)
    } as L.LeafletMouseEvent);

    await new Promise((resolve) => setTimeout(resolve, 100));

    const contextMenu = document.querySelector(".leaflet-context-menu");
    expect(contextMenu?.textContent).toContain("Nearby objects");
    expect(contextMenu?.textContent).toContain("Enclosing objects");
    expect(contextMenu?.textContent).toContain("Cafe");
    expect(contextMenu?.textContent).toContain("City Park");
  });

  it("should copy map element text and notify when map element is clicked", async () => {
    (nominatimModule.FetchAddressFromNominatim as jest.Mock).mockResolvedValue(null);
    (mapElementsModule.FetchMapElementsFromOsm as jest.Mock).mockResolvedValue({
      nearbyObjects: [{ label: "Bakery", description: "shop=bakery", copyText: "Bakery" }],
      enclosingObjects: []
    });

    const setNotificationStatus = jest.fn();
    AddContextMenu({
      map,
      language: language as unknown as Language,
      localization,
      setNotificationStatus
    });

    map.fire("contextmenu", {
      latlng: L.latLng(52.52, 13.405),
      containerPoint: L.point(100, 100)
    } as L.LeafletMouseEvent);

    await new Promise((resolve) => setTimeout(resolve, 100));

    const mapItem = document.querySelector('[data-action="copy-osm-item"]') as HTMLElement;
    expect(mapItem).toBeTruthy();
    mapItem.click();

    expect(navigator.clipboard.writeText).toHaveBeenCalledWith("Bakery");

    await waitFor(() => {
      expect(setNotificationStatus).toHaveBeenCalledWith("Map element copied!");
    });
  });
});
