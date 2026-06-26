export const BookStatus = {
  Uploaded: 0,
  Processing: 1,
  Ready: 2,
  Failed: 3,
} as const;

export const BookStatusName = {
  [BookStatus.Uploaded]: "Uploaded",
  [BookStatus.Processing]: "Processing",
  [BookStatus.Ready]: "Ready",
  [BookStatus.Failed]: "Failed",
} as const;