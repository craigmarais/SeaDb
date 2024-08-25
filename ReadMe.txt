Using a Memory mapped file is has a more stable throughput to the file than the FileStream. 

It also allows reads and writes to occur simultaniously on the file without
needing to open and close streams. 

I create a new file when the capacity is reached instead of growing it as the I would then need to close the stream 