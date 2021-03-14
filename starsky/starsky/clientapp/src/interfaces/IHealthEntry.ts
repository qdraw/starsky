/**
 * duration is for example:  "00:00:00.0011496"
 * name could be "ApplicationDbContext"
 */
export interface IHealthEntry {
  duration: string;
  isHealthy: boolean;
  name: string;
}
