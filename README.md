# SavioMacedo.MaoDesign.EmbroideryFormat
[![.NET](https://github.com/SavioMacedo/SavioMacedo.MaoDesign.EmbroideryFormat/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/SavioMacedo/SavioMacedo.MaoDesign.EmbroideryFormat/actions/workflows/dotnet.yml)

Reader and writer for embroidery files in .net
This package can read:
* DST
* HUS
* JEF
* PEC
* PES
* XXX

### Implementations
Can you see examples in test projects but it looks simple like this:

```csharp
Stream fileStream;
DstFile resultFile = DstFile.Read(fileStream, false, false, 2.0f);
 ```
 
 or

```csharp
byte[] fileBytes;
DstFile resultFile = DstFile.Read(fileBytes, false, false, 2.0f);
 ```
 
 The read methods need to be passed the arguments: bytes or a file stream, if allow transparency, if hide the machine paths and the point thinckness.
