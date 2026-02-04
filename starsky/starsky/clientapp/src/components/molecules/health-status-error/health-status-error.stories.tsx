import HealthStatusError from "./health-status-error";

export default {
  title: "components/molecules/health-status-error"
};

export const Default = () => {
  return (
    <>
      <b>There nothing shown yet, only if the api returns a error code</b>
      <HealthStatusError />
    </>
  );
};

Default.storyName = "default";
