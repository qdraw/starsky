import XCTest
import Combine
@testable import starsky

final class FakeLauncher: BackendLaunching {
    let portToReturn: UInt16?
    var terminated = false

    init(port: UInt16?) {
        self.portToReturn = port
    }

    func launch() -> UInt16? {
        return portToReturn
    }

    func terminate() {
        terminated = true
    }
}

final class FakeChecker: ReadinessChecking {
    let ready: Bool
    init(ready: Bool) { self.ready = ready }

    func waitUntilReady(localUrl: URL, timeout: TimeInterval) -> Bool {
        return ready
    }
}

final class AppViewModelTests: XCTestCase {
    var cancellables = Set<AnyCancellable>()

    override func tearDown() {
        cancellables.removeAll()
        super.tearDown()
    }

    func testStartSuccessSetsWebUrl() throws {
        let fakeLauncher = FakeLauncher(port: 12345)
        let fakeChecker = FakeChecker(ready: true)
        let vm = AppViewModel(launcher: fakeLauncher, checker: fakeChecker)

        let expect = expectation(description: "webUrl set")

        vm.$webUrl
            .sink { url in
                if url != nil { expect.fulfill() }
            }
            .store(in: &cancellables)

        vm.start()

        wait(for: [expect], timeout: 2.0)
        XCTAssertEqual(vm.webUrl?.absoluteString, "http://localhost:12345/")
        XCTAssertFalse(vm.isLoading)

        // cleanup: stop
        vm.stop()
    }

    func testLaunchFailureSetsLoadingFalse() throws {
        let fakeLauncher = FakeLauncher(port: nil)
        let fakeChecker = FakeChecker(ready: false)
        let vm = AppViewModel(launcher: fakeLauncher, checker: fakeChecker)

        let expect = expectation(description: "loading false")

        vm.$isLoading
            .sink { loading in
                if loading == false { expect.fulfill() }
            }
            .store(in: &cancellables)

        vm.start()

        wait(for: [expect], timeout: 2.0)
        XCTAssertNil(vm.webUrl)
        XCTAssertFalse(vm.isLoading)
    }

    func testStartTimeoutResultsInNoWebUrl() throws {
        let fakeLauncher = FakeLauncher(port: 54321)
        let fakeChecker = FakeChecker(ready: false)
        let vm = AppViewModel(launcher: fakeLauncher, checker: fakeChecker)

        vm.start()

        // Poll until isLoading becomes false or timeout
        let timeout: TimeInterval = 2.0
        let deadline = Date().addingTimeInterval(timeout)
        while Date() < deadline {
            if !vm.isLoading { break }
            RunLoop.current.run(until: Date().addingTimeInterval(0.01))
        }

        XCTAssertNil(vm.webUrl)
        XCTAssertFalse(vm.isLoading)
    }
}
