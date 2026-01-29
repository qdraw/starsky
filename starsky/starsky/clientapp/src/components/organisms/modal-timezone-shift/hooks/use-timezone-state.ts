import { useState } from "react";
import { ITimezone } from "../../../../interfaces/ITimezone";

export interface ITimezoneState {
  timezones: ITimezone[];
  setTimezones: React.Dispatch<React.SetStateAction<ITimezone[]>>;
  recordedTimezoneId: string;
  setRecordedTimezoneId: React.Dispatch<React.SetStateAction<string>>;
  correctTimezoneId: string;
  setCorrectTimezoneId: React.Dispatch<React.SetStateAction<string>>;
  recordedTimezoneDisplayName: string;
  setRecordedTimezoneDisplayName: React.Dispatch<React.SetStateAction<string>>;
  correctTimezoneDisplayName: string;
  setCorrectTimezoneDisplayName: React.Dispatch<React.SetStateAction<string>>;
  reset: () => void;
}

export function useTimezoneState(): ITimezoneState {
  const [timezones, setTimezones] = useState<ITimezone[]>([]);
  const [recordedTimezoneId, setRecordedTimezoneId] = useState("");
  const [correctTimezoneId, setCorrectTimezoneId] = useState("");
  const [recordedTimezoneDisplayName, setRecordedTimezoneDisplayName] = useState("");
  const [correctTimezoneDisplayName, setCorrectTimezoneDisplayName] = useState("");

  const reset = () => {
    setRecordedTimezoneId("");
    setCorrectTimezoneId("");
    setRecordedTimezoneDisplayName("");
    setCorrectTimezoneDisplayName("");
  };

  return {
    timezones,
    setTimezones,
    recordedTimezoneId,
    setRecordedTimezoneId,
    correctTimezoneId,
    setCorrectTimezoneId,
    recordedTimezoneDisplayName,
    setRecordedTimezoneDisplayName,
    correctTimezoneDisplayName,
    setCorrectTimezoneDisplayName,
    reset
  };
}
