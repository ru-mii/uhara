## Setup
```c#
vars.JitSave = vars.Uhara.CreateTool("UnityCS", "JitSave");
```
Functions would then be used through vars.JitSave   

## Description
Compiles and hooks method, returns address for rip-8 of injected asm.   
Default methods like Awake, Start, Update, LateUpdate hold instance address in RDI register at the beginning of their execution creating grat hook target.
