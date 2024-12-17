import React, { useState } from "react";
import useGlobalSettings from "../../../hooks/use-global-settings";
import { IFileIndexItem } from "../../../interfaces/IFileIndexItem";
import localization from "../../../localization/localization.json";
import {
  isValidDate,
  leftPad,
  parseDate,
  parseDateDate,
  parseDateMonth,
  parseDateYear,
  parseTime,
  parseTimeHour
} from "../../../shared/date";
import FetchPost from "../../../shared/fetch/fetch-post";
import { Language } from "../../../shared/language";
import { UrlQuery } from "../../../shared/url/url-query";
import FormControl from "../../atoms/form-control/form-control";
import Modal from "../../atoms/modal/modal";

interface IModalDatetimeProps {
  isOpen: boolean;
  handleExit: (result: IFileIndexItem[] | null) => void;
  subPath: string;
  dateTime?: string;
}

const ModalEditDatetime: React.FunctionComponent<IModalDatetimeProps> = (props) => {
  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageModalDatetime = language.key(localization.MessageModalDatetime);
  const MessageYear = language.key(localization.MessageYear);
  const MessageMonth = language.key(localization.MessageMonth);
  const MessageDate = language.key(localization.MessageDate);
  const MessageTime = language.key(localization.MessageTime);
  const MessageErrorDatetime = language.key(localization.MessageErrorDatetime);

  const isFormEnabled = true;

  const [fullYear, setFullYear] = useState(parseDateYear(props.dateTime));
  const [month, setMonth] = useState(parseDateMonth(props.dateTime));
  const [date, setDate] = useState(parseDateDate(props.dateTime));
  const [hour, setHour] = useState(parseTimeHour(props.dateTime));
  const [minute, setMinute] = useState(props.dateTime ? new Date(props.dateTime).getMinutes() : 1);
  const [seconds, setSeconds] = useState(
    props.dateTime ? new Date(props.dateTime).getSeconds() : 1
  );

  function getDates() {
    return (
      `${fullYear}-${leftPad(month)}-${leftPad(date)}` +
      `T${leftPad(hour)}:${leftPad(minute)}:${leftPad(seconds)}`
    );
  }

  function updateDateTime() {
    if (!isFormEnabled) return;

    const updateApiUrl = new UrlQuery().UrlUpdateApi();

    const bodyParams = new URLSearchParams();
    bodyParams.append("f", props.subPath);
    bodyParams.append("datetime", getDates());

    FetchPost(updateApiUrl, bodyParams.toString()).then((result) => {
      if (result.statusCode !== 200) return;
      props.handleExit(result.data as IFileIndexItem[]);
    });
  }

  return (
    <Modal
      id="modal-archive-mkdir"
      isOpen={props.isOpen}
      handleExit={() => {
        props.handleExit(null);
      }}
    >
      <div data-test="modal-edit-datetime" className="content">
        <div className="modal content--subheader">
          <b>{MessageModalDatetime}</b>
          {isValidDate(getDates()) ? (
            <>
              <br />
              {parseDate(getDates(), settings.language)} {parseTime(getDates())}{" "}
            </>
          ) : null}
        </div>
        <div className="modal content--text">
          <div className="date">
            <p>{MessageYear}</p>
            <FormControl
              name="year"
              maxlength={4}
              onBlur={(e) => {
                setFullYear(Number(e.target.textContent));
              }}
              className="inline-block"
              warning={false}
              contentEditable={isFormEnabled}
            >
              {fullYear}
            </FormControl>
          </div>

          <div className="date">
            <p>{MessageMonth}</p>
            <FormControl
              name="month"
              maxlength={2}
              onBlur={(e) => {
                setMonth(Number(e.target.textContent));
              }}
              className="inline-block"
              contentEditable={isFormEnabled}
              warning={false}
            >
              {month}
            </FormControl>
          </div>

          <div className="date">
            <p>{MessageDate}</p>
            <FormControl
              name="date"
              maxlength={2}
              onBlur={(e) => {
                setDate(Number(e.target.textContent));
              }}
              className="inline-block"
              contentEditable={isFormEnabled}
              warning={false}
            >
              {date}
            </FormControl>
          </div>

          <div className="date-spacer"></div>

          <div className="date">
            <p>{MessageTime}</p>
            <FormControl
              name="hour"
              maxlength={2}
              onBlur={(e) => {
                setHour(Number(e.target.textContent));
              }}
              className="inline-block"
              contentEditable={isFormEnabled}
              warning={false}
            >
              {hour}
            </FormControl>
            <b>&nbsp;:&nbsp;</b>
            <FormControl
              name="minute"
              maxlength={2}
              onBlur={(e) => {
                setMinute(Number(e.target.textContent));
              }}
              className="inline-block"
              contentEditable={isFormEnabled}
              warning={false}
            >
              {minute}
            </FormControl>
            <b>&nbsp;:&nbsp;</b>
            <FormControl
              name="sec"
              maxlength={2}
              onBlur={(e) => {
                setSeconds(Number(e.target.textContent));
              }}
              className="inline-block"
              contentEditable={isFormEnabled}
              warning={false}
            >
              {seconds}
            </FormControl>
          </div>

          {!isValidDate(getDates()) ? (
            <div data-test="modal-edit-datetime-non-valid" className="warning-box">
              {MessageErrorDatetime}
            </div>
          ) : null}

          <button
            disabled={!isValidDate(getDates())}
            className="btn btn--default"
            onClick={updateDateTime}
            data-test="modal-edit-datetime-btn-default"
          >
            {MessageModalDatetime}
          </button>
        </div>
      </div>
    </Modal>
  );
};

export default ModalEditDatetime;
