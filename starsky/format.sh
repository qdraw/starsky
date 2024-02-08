#!/bin/bash

# Loop over the output of the find command
while IFS= read -r file; do
    # Process each file here, for example, print the file name
    echo "Processing file: $file"
    dotnet format $file

done < <(find . -iname '*.csproj')
