# Protein Parsimony

This program implements a protein parsimony algorithm
for grouping proteins with similar peptides.

## ProteinParsimony Console

The ProteinParsimony.exe program can be used to manually process 
a tab-delimited text file with columns Protein and Peptide,
or to process data in SQLite database table.

### Program syntax #1:
`ProteinParsimony.exe InputFilePath.txt [OutputFilePath]`

The input file is a tab delimited text file with columns Protein and Peptide
(column order does not matter; extra columns are ignored)

If the output file path is not defined, it will be created in the same location
as the input file, but with '_parsimony' added to the filename

### Program syntax #2:
`ProteinParsimony.exe SQLiteDatabase.db3 [TableName]`

If the input is a SQLite database file (extension .db, .db3, .sqlite,
or .sqlite3), proteins and peptides will be read from the specified table,
or from `T_Row_Metadata` if TableName is not provided. The table must have columns
Protein and Peptide. \
Results will be written to tables `T_Parsimony_Grouping` and
`T_Parsimony_Group_Members`

## Contacts

Written by Josh Aldrich for the Department of Energy (PNNL, Richland, WA) \
E-mail: proteomics@pnnl.gov
Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/

## License

The Protein Parsimony library is licensed under the Apache License, Version 2.0; you may not use this 
file except in compliance with the License.  You may obtain a copy of the 
License at https://opensource.org/licenses/Apache-2.0
