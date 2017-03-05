DirectoryDiff
This C# code is written to recursively compare the files in a "new" directory against an older version of itself. The compare essentially extracts all the new files from the new directory into a separate "delta" directory. This is useful when having to transit large files across an air-gap environment as WSUS directories can be quite large and can take hours to properly transit all the security controls from a connected to disconnected network.
It is written in C# so that the code can be compiled and protected against edits where file-signing is not implemented.
The code does not properly compare and I haven't had the time to figure this out. Use at your own risk.

The test is fairly simple: the delta folder is a proper diff if it can be merged with the "old" folder in the compare.
In math terms: "Old" + "delta" = "New"
