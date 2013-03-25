csharp-openbinaryformat
=======================

A binary format that is version-neutral, flexible and fault-tolerant.  
BSD license.  
For version log, view the individual files.  

##What Is OpenBinaryFormat?

OpenBinaryFormat is a binary format designed for simple applications.  
It is block based and designed for apps that dumps data in same order as reading.  
This implementation supports:  

1. GZip compression on files ending with ".gz".
2. Automatic conversion to requested type.
3. Custom types.
4. Seeking.

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

TIP: Use 'StartBlock' to throw exception if the read field is not the correct block.  
TIP 2: Use 'NextId' to check the next field before reading.  

##Example: Writing Custom Data

    var points = w.StartBlock("Points");
    var bw = w.Writer;
    bw.Write((int)X.Count);
    for (int i = 0; i < n; i++) {
    	bw.Write((double)X[i]);
    	bw.Write((double)Y[i]);
    }
    w.EndBlock(points);

A block can be used to write arbitrary data.  
If you need seeking within a such block, use byte array.  

##Example: Reading Custom Data

    var points = r.StartBlock("Points");
    var br = r.Reader;
    var n = br.ReadInt32();
    for (int i = 0; i < n; i++) {
    	this.Add(new Point(br.ReadDouble(), br.ReadDouble()));
    }
    r.EndBlock(points);

A block can be contain arbitrary data.  
If you need seeking within a such block, use byte array.  

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

