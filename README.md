# Protein Parsimony

This program implements a protein parsimony algorithm
for grouping proteins with similar peptides.

## ProteinParsimony Console

The ProteinParsimony.exe program can be used to manually process 
a tab-delimited text file with columns Protein and Peptide,
or to process a SQLite file with table `T_Row_Metadata`

```
ProteinParsimony.exe InputFile.txt [OutputFilePath]
 or
ProteinParsimony.exe SQLiteDatabase.db3
```

The input file is a tab delimited text file with columns Protein and Peptide
(column order does not matter; extra columns are ignored)

If the output file path is not defined, it will be created in the same location
as the input file, but with `_parsimony` added to the filename

Alternatively, the input file can be a SQLite database file (extension .db, .db3, .sqlite, or .sqlite3)
Proteins and peptides will be read from table `T_Row_Metadata` and results will be
written to tables `T_Row_Metadata_parsimony` and `T_Row_Metadata_parsimony_groups`

## Contacts

Written by Josh Aldrich for the Department of Energy (PNNL, Richland, WA) \
E-mail: proteomics@pnnl.gov
Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/

## License

The Protein Parsimony library is licensed under the Apache License, Version 2.0; you may not use this 
file except in compliance with the License.  You may obtain a copy of the 
License at https://opensource.org/licenses/Apache-2.0
