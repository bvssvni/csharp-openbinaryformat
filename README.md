csharp-openbinaryformat
=======================

A binary format that is version-neutral, flexible and fault-tolerant.  
BSD license (for more information about license and update log, see "version.md").  

##Introduction to OpenBinaryFormat  

OpenBinaryFormat is a binary data format designed for flexibility and backward compability.  
By default, it supports common data types and automatic conversion of: 

- Double
- Float
- Long
- Int
- String
- Byte array

Example of file layout, in corresponding XML:

    <block name="v1.0">
      <string name="First Name">John</string>
      <string name="Last Name">Smith</string>
      <double name="Age">80</double>
      <block name="v1.0 CRM plugin">
        <int name="CustomerId">42</int>
      </block>
    </block>
    
The program jumps to end of block when not recognizing one or more fields.  
This makes it possible to extend the format with new types of fields while  
maintaining maximum backward compability.  

The block hierarchy does not affect the data model, which gives many advantages.  
One can use blocks for many tricks, for example reading more than one data model from same file.  
Since the blocks are skipped when encountering an unexpected field, one can simply join  
files together and read it twice, one for each "type" of file.  
This makes it easy to combine multiple document formats in the same file.  

In traditional formats, the responsibility of compability is put on the program reading the file.  
OpenBinaryFormat pushes this responsibility to the program writing the file.  
This solves the problem of predicting future needs for a format.  

