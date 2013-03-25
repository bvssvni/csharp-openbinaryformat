csharp-openbinaryformat
=======================

A binary format that is version-neutral, flexible and fault-tolerant.  
BSD license.  
For version log, view the individual files.  

##What Is OpenBinaryFormat?

OpenBinaryFormat is a binary format that lets you read and write data in a safe but straight-forward way.  
It is designed for advanced applications that require maximum compability and fault-tolerance.  
The format uses block abstractions for compability control and can convert types automatically.  
Unlike XML and JSON, OpenBinaryFormat supports custom data types and allows reading and writing in the same manner  
as typical when "dumping data" to binary files.  

The philosophy is that blocks required when reading will throw exception if not there.  
If an optional block is saved within a block it should not be requested,  
but read the fields in a straight-forward-manner.  

When reading from a block, the order of the fields does not matter.  
If an unknown type is encountered, it will jump to the end of the block.  
The format supports nested blocks, so one can have complete control over "extensions" to an existing document format.  

Since the format is binary, it requires no parsing of numbers or enconding of binary blobs.  
The only trade-off is an id (string) + type (int) for each field.  
Blocks uses long (int64) for length, which allows files much larger than 2GB.  

##Example

Writing data:

    var f = OpenBinaryFormat.ToFile("data.txt");

    var person = f.StartBlock("person");
    
    f.WriteString("name", "Luke Skywalker");
    f.WriteDouble("age", 20);
    f.WriteString("comments", "He is one of the good guys.");
    
    f.EndBlock(person);
    
    f.Close();
    
Reading data:

    var f = OpenBinaryFormat.FromFile("data.txt");

    var person = f.StartBlock("person");
    
    this.name = f.Read<string>("name", null, person);
    this.age = f.Read<int>("age", 0, person);
    this.comments = f.Read<string>("comments", "no comments", person);
    
    f.EndBlock(person);
    
    f.Close();

##Compression

When OpenBinaryFormat reads or writes to a file ending with ".gz", it uses gzip compression.  
The compression happens in-memory, so saved changed will not take affect until calling '.Close()'.  

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

