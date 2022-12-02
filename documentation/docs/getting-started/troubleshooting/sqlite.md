# Troubleshooting SQLite Problems

## Bad Performance

If you have only a few images, concurrent users, and CPU cores, [SQLite](https://www.sqlite.org/) may seem faster compared to full-fledged database servers like [MariaDB](https://mariadb.com/).

This changes as the index grows and the number of concurrent requests increases. The way MariaDB handles multiple queries is completely different and optimized for high concurrency, while SQLite, for example, locks the index on updates so that other operations have to wait. In the worst case, this can lead to locking errors and timeouts during indexing - especially when combined with a slow disk or network storage.

The biggest advantage of SQLite is that you don't need to run a separate database server. This can be very useful for testing and works well if you only have a few thousand files to index. If you are looking for scalability and high performance, it is not a good choice.

[Get MariaDB Performance Tips ›](performance.md#mariadb)

## Locking Errors

If you use [traditional hard drives instead of SSDs](performance.md#storage), you will find that Starsky frequently runs into locking issues with SQLite because your CPU is many times faster than the mechanical heads of your disks. To some extent, this may also happen with solid-state drives, but it is much more likely with slow storage.

You may be able to optimize the behavior and reduce locking errors with SQLite parameters that you can set with the [database config option](../config-options.md#database-connection), but ultimately you should use an SSD if you want to keep SQLite or switch to MariaDB. Please note that our team cannot provide support otherwise.

## Server Crashes

If the server crashes unexpectedly or your database files get corrupted frequently, it is usually because they are stored on an unreliable device such as a USB flash drive, an SD card, or a shared network folder mounted via NFS or CIFS. These may also have [unexpected file size limitations](https://thegeekpage.com/fix-the-file-size-exceeds-the-limit-allowed-and-cannot-be-saved/), which is especially problematic for databases that do not split data into smaller files.

- [ ] Never use the same database files with more than one server instance
- [ ] Use SSDs instead of traditional hard drives, never use network storage
- [ ] Consider using MariaDB instead of SQLite

## Corrupted Files

↪ [Server Crashes](#server-crashes)