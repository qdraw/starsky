DELETE FROM FileIndex WHERE `ParentDirectory` = '/2018/03/2018_03_18'

DELETE FROM FileIndex WHERE `IsDirectory` = 'true'

DELETE FROM FileIndex WHERE `FilePath` LIKE '%/2017/11%'
