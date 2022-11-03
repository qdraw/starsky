import { FSWatcher } from "fs";

export const FileWatcherObjects = new Set<[FSWatcher, string]>();
