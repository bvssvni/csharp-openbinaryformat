csharp-openbinaryformat
=======================

A binary format that is version-neutral, flexible and fault-tolerant.  
BSD license (for more information about license and update log, see "version.md").  

##Philosophy Behind OpenBinaryFormat

OpenBinaryFormat is based on abstracting a data "block" from the actual data.  
By controlling blocks, the exporter program can control when and how the importer fails if incompatible.  
A block tells the importer to "ignore the rest if you don't understand it".  
Some importer algorithms may try to read multiple versions of the same data until they "get it".  
This makes it easy to expand a format with custom types and create files that are compatible with multiple importers.  

##Field Binary Layout

    <name>      type: string
    <type>      type: int32, little endian order
    <data>      type: unknown
    
##Block Binary Layout

A block tells how many bytes to skip if there is an error or unknown fields.  
It is specifically designed to handle incompability and partial loading, it _does not affect the data model_.  
Blocks can be nested, which gives the exporter algorithms full control over compability behavior.  

    <name>      type: string
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
    -400    Bytes

To make your application maximum compatible with other OpenBinaryFormat applications,  
please support the data types above.  

##Example

If the binary format was written in XML format, it may look something like this:  

    <block name="v1.0">
      <string name="First Name">John</string>
      <string name="Last Name">Smith</string>
      <double name="Age">80</double>
      <block name="v1.0 CRM plugin">
        <int name="CustomerId">42</int>
      </block>
    </block>

Here we see 3 fields controlled by 2 blocks.  
The 2 first fields are written by the application while the 3rd is written by a plugin.  
