grep the first 60 kb from a file

```
{ head -c 60000 >head_part.arw; cat >tail_part.arw;} < DSC01028.ARW
```