## Setup
```c#
vars.JitSave = vars.Uhara.CreateTool("UnityCS", "JitSave");
```
Functions would then be used through vars.JitSave   

## Description
Compiles and hooks method, returns address for RIP-0x8 of injected asm.   
8 bytes at RIP-0x8 are reserved for your use.
