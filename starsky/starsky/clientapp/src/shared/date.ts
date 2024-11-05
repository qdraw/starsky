import {SupportedLanguages} from "./language";

/**
 * Is the date Valid?
 * @param inputDateTime 2018-09-11T11:23:19 or 2018-09-11T11:23:19Z
 */
const isValidDate = (inputDateTime: string | undefined): boolean => {
  if (inputDateTime) {
    const input = new Date(inputDateTime).valueOf();
    return input > 0 && input < 7258118400 * 1000; // 01/01/2200
  }
  return false;
};

/**
 * Add 0 + 1 left Pad
 * @param n number
 */
const leftPad = (n: number) => {
  return n > 9 ? "" + n : "0" + n;
};

/**
 * Return difference in Minutes
 * @param date a Javascript Datetime stamp (unix*1000)
 * @param now Javascript now
 */
const DifferenceInDate = (date: number, now: number = new Date().valueOf()): number => {
  return (now - date) / 60000;
};

/**
 * Is the file Edited within a minute
 * @param inputDateTime Date
 */
const IsEditedNow = (inputDateTime: undefined | string): boolean | null => {
  if (!inputDateTime) return null;
  const input = new Date(inputDateTime).valueOf();
  if (!input) return null;
  const difference = DifferenceInDate(input);
  return difference <= 0.2;
};

/**
 * Get the relative date, for example 6 hour ago
 * @param inputDateTime 2018-09-11T11:23:19 or 2018-09-11T11:23:19Z
 * @param locate Language
 */
const parseRelativeDate = (
  inputDateTime: string | undefined,
  locate: SupportedLanguages
): string => {
  const date = "";

  if (!inputDateTime) return date;
  const input = new Date(`${inputDateTime}`).valueOf();

  if (!input) return date;

  const difference = DifferenceInDate(input);

  switch (true) {
    case difference <= 1:
      return "{lessThan1Minute}";
    case difference < 60:
      return difference.toFixed(0) + " {minutes}";
    case difference < 1441:
      return Math.round(difference / 60) + " {hour}";
    default:
      return parseDate(inputDateTime, locate);
  }
};

const IsIncludeTimezone = (dateTime: string): boolean => {
  const timeZoneRegex = /\+\d{2}:\d{2}/;
  return timeZoneRegex.test(dateTime);
}

/**
 * Get Date complete parsed for example: Monday, 4 May 2020
 * @param dateTime 2018-09-11T11:23:19, 2018-09-11T11:23:19Z or 2020-04-28T10:44:43.123456+01:00
 * @param locate Language
 */
const parseDate = (dateTime: string | undefined, locate: SupportedLanguages): string => {
  if (!dateTime) return "";

  // UTC DateTime already ends with Z
  const dateTimeObject = new Date(!dateTime.endsWith("Z") && !IsIncludeTimezone(dateTime) ? `${dateTime}Z` : dateTime);
  // We prefer British English, uses day-month-year order
  const locateString = locate === SupportedLanguages.en ? "en-GB" : locate.toString();
  if (dateTime.endsWith("Z")) {
    return dateTimeObject.toLocaleDateString(locateString, {
      weekday: "long",
      year: "numeric",
      month: "long",
      day: "numeric"
    });
  }
  // toLocaleDateString assumes that the input is UTC, which is usually not the case
  return dateTimeObject.toLocaleDateString(locateString, {
    timeZone: "UTC",
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric"
  });
};

/**
 * Get Day of the month (int)
 * @param dateTime 2018-09-11T11:23:19 or 2018-09-11T11:23:19Z
 */
const parseDateDate = (dateTime: string | undefined): number => {
  if (!isValidDate(dateTime) || !dateTime) {
    return 1;
  }
  const dateTimeObject = new Date(!dateTime.endsWith("Z") && !IsIncludeTimezone(dateTime) ? `${dateTime}Z` : dateTime);
  // toLocaleDateString assumes that the input is UTC, which is usually not the case
  const numberValue = dateTimeObject.toLocaleDateString([], {
    timeZone: !dateTime.endsWith("Z") ? "UTC" : undefined,
    day: "numeric"
  });
  return Number(numberValue);
};

/**
 * Get Year (int)
 * @param dateTime 2018-09-11T11:23:19 or 2018-09-11T11:23:19Z
 */
const parseDateYear = (dateTime: string | undefined): number => {
  if (!isValidDate(dateTime) || !dateTime) {
    return 1;
  }
  const dateTimeObject = new Date(!dateTime.endsWith("Z") && !IsIncludeTimezone(dateTime) ? `${dateTime}Z` : dateTime);
  // toLocaleDateString assumes that the input is UTC, which is usually not the case
  const numberValue = dateTimeObject.toLocaleDateString([], {
    timeZone: !dateTime.endsWith("Z") ? "UTC" : undefined,
    year: "numeric"
  });
  return Number(numberValue);
};

/**
 * Get Month 1-12 (int)
 * @param dateTime 2018-09-11T11:23:19 or 2018-09-11T11:23:19Z
 */
const parseDateMonth = (dateTime: string | undefined): number => {
  if (!isValidDate(dateTime) || !dateTime) {
    return 1;
  }
  const dateTimeObject = new Date(!dateTime.endsWith("Z") && !IsIncludeTimezone(dateTime) ? `${dateTime}Z` : dateTime);
  // toLocaleDateString assumes that the input is UTC, which is usually not the case
  const numberValue = dateTimeObject.toLocaleDateString([], {
    timeZone: !dateTime.endsWith("Z") ? "UTC" : undefined,
    month: "numeric"
  });
  return Number(numberValue);
};

/**
 * Get Time (for example 23:02:03)
 * @param dateTime 2018-09-11T11:23:19 or 2018-09-11T11:23:19Z
 */
const parseTime = (dateTime: string | undefined): string => {
  if (!isValidDate(dateTime) || !dateTime) {
    return "";
  }
  const dateTimeObject = new Date(!dateTime.endsWith("Z") && !IsIncludeTimezone(dateTime) ? `${dateTime}Z` : dateTime);

  // toLocaleDateString assumes that the input is UTC, which is usually not the case
  return dateTimeObject.toLocaleTimeString([], {
    timeZone: !dateTime.endsWith("Z") ? "UTC" : undefined,
    hour12: false,
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit"
  });
};

/**
 * Get Hour (for example 23) (int)
 * @param dateTime 2018-09-11T11:23:19 or 2018-09-11T11:23:19Z
 */
const parseTimeHour = (dateTime: string | undefined): number => {
  if (!isValidDate(dateTime) || !dateTime) {
    return 1;
  }
  const dateTimeObject = new Date(!dateTime.endsWith("Z") && !IsIncludeTimezone(dateTime) ? `${dateTime}Z` : dateTime);
  // toLocaleDateString assumes that the input is UTC, which is usually not the case
  const numberValue = dateTimeObject.toLocaleTimeString([], {
    timeZone: !dateTime.endsWith("Z") ? "UTC" : undefined,
    hour12: false,
    hour: "2-digit"
  });
  return Number(numberValue);
};

/**
 * Convert seconds to hours
 * @param seconds number of seconds
 */
const SecondsToHours = (seconds: number): string => {
  if (isNaN(seconds)) return "0:00";
  const time = new Date(0);
  time.setUTCSeconds(seconds);
  if (time.getUTCHours() === 0) return `${time.getUTCMinutes()}:${leftPad(time.getUTCSeconds())}`;
  return `${time.getUTCHours()}:${leftPad(time.getUTCMinutes())}:${leftPad(time.getUTCSeconds())}`;
};

export {
  DifferenceInDate,
  IsEditedNow,
  SecondsToHours,
  isValidDate,
  leftPad,
  parseDate,
  parseDateDate,
  parseDateMonth,
  parseDateYear,
  parseRelativeDate,
  parseTime,
  parseTimeHour
};
