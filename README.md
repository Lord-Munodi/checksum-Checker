# Checksum-Checker

A very basic WPF app to find the checksum of a file.  
Supports SHA1, SHA265, SHA512 and MD5, as these are popular or recommended checksum hashes. Note however that MD5 is vulnerable and should not relied on if malicious tampering is suspected.
It is simple to add another hash algorithm, just edit the constructor of MainWindow.cs, and if .NET does not provide an implementation for the algorithm, add a new class. If adding a class you may use CRC32.cs as a template.

More features will be added, while preserving the goal of a simple utility. If you'd like a feature, send me an email (or a pull request).