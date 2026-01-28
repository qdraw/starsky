import { useState } from "react";
import { ITimezone, ITimezoneShiftResult } from "../../../interfaces/ITimezone";

export function useTimezoneState() {
  const [timezones, setTimezones] = useState<ITimezone[]>([]);
  const [recordedTimezone, setRecordedTimezone] = useState("");
  const [correctTimezone, setCorrectTimezone] = useState("");

  const reset = () => {
    setRecordedTimezone("");
    setCorrectTimezone("");
  };

  return {
    timezones,
    setTimezones,
    recordedTimezone,
    setRecordedTimezone,
    correctTimezone,
    setCorrectTimezone,
    reset
  };
}
