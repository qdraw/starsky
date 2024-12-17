import HealthCheckForUpdates, { CheckForUpdatesLocalStorageName } from "./health-check-for-updates";

export default {
  title: "components/molecules/health-check-for-updates"
};

export const Default = () => {
  (window as unknown as { isElectron: unknown }).isElectron = undefined;
  return (
    <>
      <button
        onClick={() => {
          localStorage.removeItem(CheckForUpdatesLocalStorageName);
          window.location.reload();
        }}
      >
        Clean sessie
      </button>
      T<b>There nothing shown yet, only if the api returns a error code</b>
      <HealthCheckForUpdates />
    </>
  );
};

Default.storyName = "default";

export const Electron = () => {
  (window as unknown as { isElectron: boolean }).isElectron = true;
  return (
    <>
      <b>
        <button
          className={"b"}
          onClick={() => {
            localStorage.removeItem(CheckForUpdatesLocalStorageName);
            window.location.reload();
          }}
        >
          Clean sessie
        </button>
        There nothing shown yet, only if the api returns a error code
      </b>
      <HealthCheckForUpdates />
    </>
  );
};
