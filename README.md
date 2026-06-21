# Net

⚠️ **DISCLAIMER:** This software is provided "as is" for **educational and authorized security testing purposes only**. The author does not condone, support, or encourage any malicious behavior or unauthorized access to computer systems. Utilizing this software against targets without prior explicit written consent is illegal and punishable by law. By downloading or analyzing this code, you agree that the author cannot be held liable for any misuse, damage, or legal consequences caused by this software. Shortly: CREATOR IS NOT RESPONSIBLE FOR ANY DAMAGES MADE BY/USING THIS MALWARE.
---
## Overview & Capabilities
The architecture demonstrates several persistence and get system privileges to execute commands:
* **Persistence Mechanisms:** Modifies the CurrentUser Registry Run keys (`SOFTWARE\Microsoft\Windows\CurrentVersion\Run`) to ensure execution upon system startup.
* **Self-Replication Analysis:** Copies itself to user folder and documents folder. alongside monitoring for removable media (`Win32_VolumeChangeEvent`) to demonstrate physical vector propagation.
* **Command Execution:** Queries a predefined remote resource (C2 endpoint via HTTP) to parse custom configuration commands (`C2Config`) and executes them silently using `cmd.exe`.
* **Evasion Technique:** Employs low-level Windows APIs (`ShowWindow` with `SW_HIDE`) to suppress console visibility during runtime.
* **How it escalates privileges:** It drops a exe file and executes it. That file copies the system token to its token. And it starts the main malware. it should create a loop but it doesnt for some reason.

* ## Components Analysed

* **Language:** C# (.NET Framework / .NET Core compatibility)
* **Key Libraries Used:**
    * `System.Diagnostics` & `System.IO` (Process management and file system tasks)
    * `System.Management` (WQL Event querying for USB monitoring)
    * `Newtonsoft.Json` (C2 configuration parsing)
    * `System.Runtime.InteropServices` (Win32 API Interop)

## Building
Just run the solution on visual studio and install Newtonsoft.Json and compile.
