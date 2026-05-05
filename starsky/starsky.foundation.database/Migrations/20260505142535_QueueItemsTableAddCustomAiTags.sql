DROP TABLE QueueItems;
ALTER TABLE FileIndex
DROP
COLUMN ImageClassificationGeneratedAt;
ALTER TABLE FileIndex
DROP
COLUMN ImageClassificationModel;
ALTER TABLE FileIndex
DROP
COLUMN RejectedTags;
ALTER TABLE FileIndex
DROP
COLUMN SuggestedTags;