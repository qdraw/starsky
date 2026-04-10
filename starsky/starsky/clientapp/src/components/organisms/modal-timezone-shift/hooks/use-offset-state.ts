import { useState } from "react";

export interface OffsetState {
  years: number;
  months: number;
  days: number;
  hours: number;
  minutes: number;
  seconds: number;
}

export function useOffsetState() {
  const [offsetYears, setOffsetYears] = useState(0);
  const [offsetMonths, setOffsetMonths] = useState(0);
  const [offsetDays, setOffsetDays] = useState(0);
  const [offsetHours, setOffsetHours] = useState(0);
  const [offsetMinutes, setOffsetMinutes] = useState(0);
  const [offsetSeconds, setOffsetSeconds] = useState(0);

  const reset = () => {
    setOffsetYears(0);
    setOffsetMonths(0);
    setOffsetDays(0);
    setOffsetHours(0);
    setOffsetMinutes(0);
    setOffsetSeconds(0);
  };

  const getOffset = (): OffsetState => ({
    years: offsetYears,
    months: offsetMonths,
    days: offsetDays,
    hours: offsetHours,
    minutes: offsetMinutes,
    seconds: offsetSeconds
  });

  return {
    offsetYears,
    setOffsetYears,
    offsetMonths,
    setOffsetMonths,
    offsetDays,
    setOffsetDays,
    offsetHours,
    setOffsetHours,
    offsetMinutes,
    setOffsetMinutes,
    offsetSeconds,
    setOffsetSeconds,
    reset,
    getOffset
  };
}
