
const isValidDate = (inputDateTime: string | undefined): boolean => {
  if (inputDateTime) {
    let input = new Date(inputDateTime).valueOf();
    return input > 0;
  }
  return false;
}


const leftPad = (n: Number) => {
  return n > 9 ? "" + n : "0" + n;
}

const parseRelativeDate = (inputDateTime: string | undefined): string => {
  let date = "";

  if (inputDateTime) {
    let input = new Date(inputDateTime).valueOf();
    if (input) {

      let now = new Date().valueOf();

      let difference = (now - input) / 60000;

      switch (true) {
        case (difference <= 1):
          return "minder dan 1 minuut";
        case (difference < 60):
          return difference.toFixed(0) + " minuten";
        case (difference < 1440):
          return "gisteren";
        default:
          return parseDate(inputDateTime);
      }

    }
  }
  return date;
}

const parseDate = (dateTime: string | undefined): string => {

  let date = "";
  if (dateTime) {
    date += new Date(dateTime).getDate();
    date += "-";
    date += new Date(dateTime).getMonth() + 1;
    date += "-";
    date += new Date(dateTime).getFullYear();
  }
  return date;
}

const parseTime = (dateTime: string | undefined): string => {

  let date = "";
  if (dateTime) {
    date += leftPad(new Date(dateTime).getHours());
    date += ":";
    date += leftPad(new Date(dateTime).getMinutes());
    date += ":";
    date += leftPad(new Date(dateTime).getSeconds());
  }
  return date;
}

export { isValidDate, parseRelativeDate, parseDate, parseTime };

