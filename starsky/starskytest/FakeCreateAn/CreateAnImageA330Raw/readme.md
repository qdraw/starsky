grep the first 50 kb from a file

```
{ head -c 50000 >head_part; cat >tail_part;} < DSC01028.ARW
```