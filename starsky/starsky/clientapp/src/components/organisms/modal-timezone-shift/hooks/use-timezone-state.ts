import { useState } from "react";
import { ITimezone } from "../../../../interfaces/ITimezone";

export interface ITimezoneState {
  timezones: ITimezone[];
  setTimezones: React.Dispatch<React.SetStateAction<ITimezone[]>>;
  recordedTimezone: string;
  setRecordedTimezone: React.Dispatch<React.SetStateAction<string>>;
  correctTimezone: string;
  setCorrectTimezone: React.Dispatch<React.SetStateAction<string>>;
  reset: () => void;
}

export function useTimezoneState(): ITimezoneState {
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
