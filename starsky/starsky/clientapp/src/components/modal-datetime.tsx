import React from 'react';
import useGlobalSettings from '../hooks/use-global-settings';
import { IFileIndexItem } from '../interfaces/IFileIndexItem';
import { isValidDate, leftPad, parseDate, parseTime, parseTimeHour } from '../shared/date';
import FetchPost from '../shared/fetch-post';
import { Language } from '../shared/language';
import { UrlQuery } from '../shared/url-query';
import FormControl from './form-control';
import Modal from './modal';

interface IModalDatetimeProps {
  isOpen: boolean;
  handleExit: (result: IFileIndexItem[] | null) => void;
  subPath: string;
  dateTime?: string;
}

const ModalDatetime: React.FunctionComponent<IModalDatetimeProps> = (props) => {

  // content
  const settings = useGlobalSettings();
  const language = new Language(settings.language);
  const MessageModalDatetime = language.text("Datum en tijd bewerken", "Edit date and time");
  const MessageYear = language.text("Jaar", "Year");
  const MessageMonth = language.text("Maand", "Month");
  const MessageDate = language.text("Dag", "Day");
  const MessageTime = language.text("Tijd", "Time");
  const MessageErrorDatetime = language.text("De datum en tijd zijn incorrect ingegeven", "The date and time were entered incorrectly");

  const [isFormEnabled] = React.useState(true);

  const [fullYear, setFullYear] = React.useState(props.dateTime ? new Date(!props.dateTime.endsWith("Z") ? `${props.dateTime}Z` : props.dateTime).getFullYear() : 1);
  const [month, setMonth] = React.useState(props.dateTime ? new Date(!props.dateTime.endsWith("Z") ? `${props.dateTime}Z` : props.dateTime).getMonth() + 1 : 1);
  const [date, setDate] = React.useState(props.dateTime ? new Date(props.dateTime).getDate() : 1);
  const [hour, setHour] = React.useState(parseTimeHour(props.dateTime));
  const [minute, setMinute] = React.useState(props.dateTime ? new Date(props.dateTime).getMinutes() : 1);
  const [seconds, setSeconds] = React.useState(props.dateTime ? new Date(props.dateTime).getSeconds() : 1);

  function getDates() {
    return `${fullYear}-${leftPad(month)}-${leftPad(date)}` +
      `T${leftPad(hour)}:${leftPad(minute)}:${leftPad(seconds)}`;
  }

  function updateDateTime() {
    if (!isFormEnabled) return;

    var updateApiUrl = new UrlQuery().UrlUpdateApi();

    var bodyParams = new URLSearchParams();
    bodyParams.append("f", props.subPath);
    bodyParams.append("datetime", getDates());

    FetchPost(updateApiUrl, bodyParams.toString()).then(result => {
      if (result.statusCode !== 200) return;
      props.handleExit(result.data);
    });
  }

  return <Modal
    id="modal-archive-mkdir"
    isOpen={props.isOpen}
    handleExit={() => {
      props.handleExit(null)
    }}>
    <div className="content">
      <div className="modal content--subheader"><b>{MessageModalDatetime}</b>
        {isValidDate(getDates()) ? <>
          <br />{parseDate(getDates(), settings.language)} {parseTime(getDates())} </> : null}</div>
      <div className="modal content--text">

        <div className="date">
          <p>{MessageYear}</p>
          <FormControl name="year"
            maxlength={4}
            onBlur={e => { setFullYear(Number(e.target.textContent)) }}
            className="inline-block"
            warning={false}
            contentEditable={isFormEnabled}>
            {fullYear}
          </FormControl>
        </div>

        <div className="date">
          <p>{MessageMonth}</p>
          <FormControl name="month"
            maxlength={2}
            onBlur={e => { setMonth(Number(e.target.textContent)) }}
            className="inline-block"
            contentEditable={isFormEnabled}
            warning={false}>
            {month}
          </FormControl>
        </div>

        <div className="date">
          <p>{MessageDate}</p>
          <FormControl name="date"
            maxlength={2}
            onBlur={e => { setDate(Number(e.target.textContent)) }}
            className="inline-block"
            contentEditable={isFormEnabled}
            warning={false}>
            {date}
          </FormControl>
        </div>

        <div className="date-spacer">
        </div>

        <div className="date">
          <p>{MessageTime}</p>
          <FormControl name="hour"
            maxlength={2}
            onBlur={e => { setHour(Number(e.target.textContent)) }}
            className="inline-block"
            contentEditable={isFormEnabled}
            warning={false}>
            {hour}
          </FormControl>
          <b>&nbsp;:&nbsp;</b>
          <FormControl name="minute"
            maxlength={2}
            onBlur={e => { setMinute(Number(e.target.textContent)) }}
            className="inline-block"
            contentEditable={isFormEnabled}
            warning={false}>
            {minute}
          </FormControl>
          <b>&nbsp;:&nbsp;</b>
          <FormControl name="sec"
            maxlength={2}
            onBlur={e => { setSeconds(Number(e.target.textContent)) }}
            className="inline-block"
            contentEditable={isFormEnabled}
            warning={false}>
            {seconds}
          </FormControl>
        </div>

        {!isValidDate(getDates()) ? <div className="warning-box">{MessageErrorDatetime}</div> : null}

        <button
          disabled={!isValidDate(getDates())}
          className="btn btn--default"
          onClick={updateDateTime}>
          {MessageModalDatetime}
        </button>

      </div>
    </div>
  </Modal>
};

export default ModalDatetime