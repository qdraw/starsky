import XCTest


final class RoutePersistenceServiceTests: XCTestCase {
    private var tempDir: URL!
    private var settingsService: SettingsService!
    private var service: RoutePersistenceService!

    override func setUp() {
        super.setUp()
        tempDir = FileManager.default.temporaryDirectory.appendingPathComponent(UUID().uuidString)
        try? FileManager.default.createDirectory(at: tempDir, withIntermediateDirectories: true)
        settingsService = SettingsService(settingsFile: tempDir.appendingPathComponent("settings.json"))
        settingsService.load()
        service = RoutePersistenceService(settingsService: settingsService)
    }

    override func tearDown() {
        try? FileManager.default.removeItem(at: tempDir)
        super.tearDown()
    }

    func testEmptyListByDefault() {
        XCTAssertTrue(service.getRoutes().isEmpty)
    }

    func testSaveEntry() {
        service.saveRoute(index: 0, route: "?f=/photos")
        XCTAssertEqual(service.getRoutes().count, 1)
        XCTAssertEqual(service.getRoutes()[0].route, "?f=/photos")
    }

    func testSaveWithGeometry() {
        let geo = SavedWindowState(route: "?f=/", x: 200, y: 150, width: 1100, height: 700, isMaximized: false)
        service.saveRoute(index: 0, route: "?f=/photos", geometry: geo)
        let routes = service.getRoutes()
        XCTAssertEqual(routes[0].x, 200)
        XCTAssertEqual(routes[0].width, 1100)
    }

    func testListExpansionWithBlanks() {
        service.saveRoute(index: 2, route: "?f=/third")
        let routes = service.getRoutes()
        XCTAssertEqual(routes.count, 3)
        XCTAssertEqual(routes[2].route, "?f=/third")
        XCTAssertEqual(routes[0].route, "?f=/")
    }

    func testRemoveEntry() {
        service.saveRoute(index: 0, route: "?f=/first")
        service.saveRoute(index: 1, route: "?f=/second")
        service.removeRoute(index: 0)
        let routes = service.getRoutes()
        XCTAssertEqual(routes.count, 1)
        XCTAssertEqual(routes[0].route, "?f=/second")
    }

    func testClearAll() {
        service.saveRoute(index: 0, route: "?f=/a")
        service.saveRoute(index: 1, route: "?f=/b")
        service.clearAll()
        XCTAssertTrue(service.getRoutes().isEmpty)
    }
}
