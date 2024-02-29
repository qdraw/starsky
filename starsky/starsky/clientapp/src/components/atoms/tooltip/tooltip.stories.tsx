import Tooltip from "./tooltip";

export default {
  title: "components/atoms/tooltip"
};

export const Default = () => {
  return (
    <>
      <br />
      <div style={{ padding: 50 }}>
        <Tooltip left={true} text="Comma seperated info">
          <span className="info--small"></span>
        </Tooltip>
      </div>
    </>
  );
};

Default.storyName = "default";
