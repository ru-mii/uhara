## Syntax
```c#
IntPtr Add(
[in] string _assembly
[in] string _namespace
[in] string _class
[in] string _method
[in] short paramCount
[in] short hookOffset
[in] short overwriteSize
[in] byte[] bytes
);
```   
## Parameters
**[in] _assembly**   
Name of the assembly file that holds used method, usually "Assembly-CSharpd.ll".
<br>
**[in] _namespace**   
Namespace that method belongs to.   
<br>
**[in] _class**   
Class that method belongs to.   
<br>
**[in] paramCount**   
Parameter count of the method.   
<br>
**[in] hookOffset**   
Hook offset for the method for the long jump.   
<br>
**[in] overwriteSize**   
Minimum required byte steal size to preserve correct code execution with long jump, can't be less than 14.   
<br>
**[in] bytes**   
Injected asm code.   
## Return value
Address that points at the injected code minus 8 bytes.
Returns IntPtr.Zero if failed.   
## Remarks
Compile method with mono and hook.   
First 8 bytes below the "bytes" code are reserved for your use, usually saving instance.
