csharp-openbinaryformat
=======================

A binary format that is version-neutral, flexible and fault-tolerant.  
BSD license.  
For version log, view the individual files.  

##What Is OpenBinaryFormat?

OpenBinaryFormat is a binary format designed for simple applications.  
For reading it is forward-only.  
It attempts to convert the data type if the type is different from expected.  

##Example: Writing Data

    var f = OpenBinaryFormat.ToFile("data.obf");

    var person = f.StartBlock("person");
    
    f.WriteString("name", "Luke Skywalker");
    f.WriteDouble("age", 20);
    f.WriteString("comments", "He is one of the good guys.");
    
    f.EndBlock(person);
    
    f.Close();
    
##Example: Reading data

    var f = OpenBinaryFormat.FromFile("data.obf");

    var person = f.SeekBlock("person");
    
    this.name = f.Seek<string>("name", null, person);
    this.age = f.Seek<int>("age", 0, person);
    this.comments = f.Seek<string>("comments", "no comments", person);
    
    f.EndBlock(person);
    
    f.Close();

The 'Seek' method searches through the file looking for a block with that name.  
The search stops when it reaches the position given by the third argument.  
You can use 'StartBlock' to throw exception if the read field is not the correct block.  

##Example: Writing Compressed File

    var f = OpenBinaryFormat.ToFile("data.obf.gz");
    ...
    
When the file name ends with ".gz", it will compress the data in-memory with gzip compression.  

##Example: Reading Compressed File

    var f = OpenBinaryFormat.FromFile("data.obf.gz");
    ...

When the file name ends with ".gz", it will decompress the data in-memory with gzip compression.  

##Field Binary Layout

    <id>      type: string
    <type>      type: int32, little endian order
    <data>      type: unknown
    
##Block Binary Layout

A block tells how many bytes to skip if there is an error or unknown fields.  
It is also used to constrain the search for fields with a specific name.  
Blocks can be nested, but not exceed a parent block.  

    <id>      type: string
    -1          type: int32, little endian order
    <length>    type: long
    ...         (data)
    _______     (end of block)

##Native Types

All native types have negative type index, to make the format easily expandable for custom types.  
The native types are:  

    -1      Block
    -100    Long        Int64               Little endian order
    -101    Int         Int32               Little endian order
    -200    Double      Float64             Little endian order
    -201    Float       Float32             Little endian order
    -300    String      Length + char[]     UTF-8
    -400    Bytes       Length + byte[]

To make your application maximum compatible with other OpenBinaryFormat applications,  
please support the data types above.  

